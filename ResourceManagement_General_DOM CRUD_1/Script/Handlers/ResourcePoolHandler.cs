namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.DOM.DomIds;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.ResourceManager;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Net.SRM.Capabilities;

	public class ResourcePoolHandler
	{
		#region Fields
		private readonly IEngine engine;

		private readonly DomHelper domHelper;

		private readonly DomInstance domInstance;

		private Lazy<ResourceManagerHandler> resourceManagerHandler;

		private Lazy<SrmHelpers> srmHelpers;

		private Lazy<ResourcePoolData> resourcePoolData;
		#endregion

		public ResourcePoolHandler(IEngine engine, DomHelper domHelper, DomInstance instance)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domHelper = domHelper ?? throw new ArgumentNullException(nameof(domHelper));
			this.domInstance = instance ?? throw new ArgumentNullException(nameof(instance));

			Init();
		}

		#region Properties
		private ResourceManagerHandler ResourceManagerHandler => resourceManagerHandler.Value;

		private SrmHelpers SrmHelpers => srmHelpers.Value;

		private ResourcePoolData ResourcePoolData => resourcePoolData.Value;
		#endregion

		#region Methods
		public bool ValidateNameChange()
		{
			var existingResourcePool = SrmHelpers.ResourceManagerHelper.GetResourcePoolByName(ResourcePoolData.Name);
			if (existingResourcePool != null)
			{
				if (existingResourcePool.ID != ResourcePoolData.PoolId)
				{
					SetErrorMessage($"Resource pool '{ResourcePoolData.Name}' already exists with ID '{existingResourcePool.ID}'.");

					return false;
				}

				if (existingResourcePool.Name == ResourcePoolData.Name)
				{
					return true;
				}
			}

			var resourcePool = SrmHelpers.ResourceManagerHelper.GetResourcePool(ResourcePoolData.PoolId);
			if (resourcePool == null)
			{
				SetErrorMessage($"Resource pool '{ResourcePoolData.Name}' with ID '{ResourcePoolData.PoolId}' does not exist.");

				return false;
			}

			resourcePool.Name = ResourcePoolData.Name;
			SrmHelpers.ResourceManagerHelper.AddOrUpdateResourcePools(resourcePool);

			return true;
		}

		public bool ValidateResources()
		{
			var resourceDataMappingsByResourceId = GetResourceDataMappings();

			var resourceFilter = new ORFilterElement<Resource>(resourceDataMappingsByResourceId.Keys.Select(id => ResourceExposers.ID.Equal(id)).ToArray());
			var resources = new List<Resource>();

			foreach (var resource in SrmHelpers.ResourceManagerHelper.GetResources(resourceFilter))
			{
				if (resource == null || !resourceDataMappingsByResourceId.TryGetValue(resource.ID, out var resourceDataMapping))
				{
					continue;
				}

				resourceDataMapping.Resource = resource;
			}

			var resourcesToUpdate = ApplyChanges(resourceDataMappingsByResourceId.Values);
			SrmHelpers.ResourceManagerHelper.AddOrUpdateResources(resourcesToUpdate.ToArray());

			return true;
		}

		private void Init()
		{
			resourceManagerHandler = new Lazy<ResourceManagerHandler>(() => new ResourceManagerHandler(domHelper));
			srmHelpers = new Lazy<SrmHelpers>(() => new SrmHelpers(engine));

			resourcePoolData = new Lazy<ResourcePoolData>(() => ResourceManagerHandler.ResourcePools.Single(x => x.Instance.ID.Id == domInstance.ID.Id));
		}

		private void SetErrorMessage(string errorMessage)
		{
			var resourcePoolInfoSection = domInstance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourcePoolInfo.Id.Id);
			resourcePoolInfoSection.AddOrReplaceFieldValue(new FieldValue(Resourcemanagement.Sections.ResourcePoolInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> {errorMessage}")));

			domHelper.DomInstances.Update(domInstance);
			domHelper.DomInstances.DoStatusTransition(domInstance.ID, Resourcemanagement.Behaviors.Resourcepool_Behavior.Transitions.Complete_To_Error);
		}

		private Dictionary<Guid, ResourceDataMapping> GetResourceDataMappings()
		{
			var resourceDomInstanceIds = ResourcePoolData.ResourceIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x)).ToList() ?? new List<Guid>();
			var resourceDomInstances = ResourceManagerHandler.Resources.Where(x => resourceDomInstanceIds.Contains(x.Instance.ID.Id) && x.ResourceId != Guid.Empty).ToList();

			var poolCapabilitiesByPoolDomInstanceId = ResourceManagerHandler.ResourcePools.ToDictionary(x => x.Instance.ID.Id, x => x.Capabilities);
			var capabilitiesById = ResourceManagerHandler.Capabilities.ToDictionary(x => x.Instance.ID.Id, x => x);
			var capabilityValuesById = ResourceManagerHandler.CapabilityValues.ToDictionary(x => x.Instance.ID.Id, x => x);

			var resourceDataMappingsByResourceId = new Dictionary<Guid, ResourceDataMapping>();
			foreach (var resourceDomInstance in resourceDomInstances)
			{
				var resourceDataMapping = new ResourceDataMapping
				{
					ResourceData = resourceDomInstance,
				};

				if (resourceDomInstance.ResourceId == Guid.Empty)
				{
					continue;
				}

				resourceDataMappingsByResourceId.Add(resourceDomInstance.ResourceId, resourceDataMapping);

				foreach (var poolDomInstanceId in resourceDomInstance.PoolIds.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x)))
				{
					if (!poolCapabilitiesByPoolDomInstanceId.TryGetValue(poolDomInstanceId, out var capabilities))
					{
						continue;
					}

					foreach (var poolCapability in capabilities)
					{
						if (!capabilitiesById.TryGetValue(poolCapability.CapabilityId, out var capabilityData))
						{
							continue;
						}

						ConfiguredCapability configuredCapability;
						if (capabilityData.CapabilityType == Resourcemanagement.Enums.CapabilityType.String)
						{
							configuredCapability = new ConfiguredStringCapability(capabilityData, poolCapability.CapabilityStringValue);
						}
						else
						{
							var discretes = new List<string>();

							poolCapability.CapabilityEnumValueIds.ForEach(x =>
							{
								if (capabilityValuesById.TryGetValue(x, out var capabilityValueData) && capabilityValueData.CapabilityId.Equals(capabilityData.Instance.ID.Id))
								{
									discretes.Add(capabilityValueData.Value);
								}
							});

							configuredCapability = new ConfiguredEnumCapability(capabilityData, discretes);
						}

						resourceDataMapping.ConfiguredCapabilities.Add(configuredCapability);
					}
				}
			}

			return resourceDataMappingsByResourceId;
		}

		private List<Resource> ApplyChanges(IEnumerable<ResourceDataMapping> resourceDataMappings)
		{
			var capabilityNamesManagedByDom = ResourceManagerHandler.Capabilities.Select(x => x.Name).ToList();

			var resourcesToUpdate = new List<Resource>();
			foreach (var resourceDataMapping in resourceDataMappings)
			{
				var requiredCapabilities = GetRequiredResourceCapabilities(resourceDataMapping.ConfiguredCapabilities);

				var added = requiredCapabilities.Where(x => !resourceDataMapping.Resource.Capabilities.Select(y => y.CapabilityProfileID).Contains(x.CapabilityProfileID)).ToList();
				var updated = requiredCapabilities.Except(added).ToList();
				var removed = resourceDataMapping.Resource.Capabilities.Where(x => !requiredCapabilities.Select(y => y.CapabilityProfileID).Contains(x.CapabilityProfileID) && capabilityNamesManagedByDom.Contains(SrmHelpers.ProfileHelper.GetProfileParameterById(x.CapabilityProfileID).Name)).ToList();

				if (ResourceHasChangedData(resourceDataMapping.Resource, added, updated, removed))
				{
					resourcesToUpdate.Add(resourceDataMapping.Resource);
				}
			}

			return resourcesToUpdate;
		}

		private List<ResourceCapability> GetRequiredResourceCapabilities(List<ConfiguredCapability> configuredCapabilities)
		{
			var resourceCapabilitiesByCapabilityName = new Dictionary<string, ResourceCapability>();

			foreach (var configuredCapability in configuredCapabilities)
			{
				if (configuredCapability is ConfiguredStringCapability configuredStringCapability)
				{
					continue;
				}

				if (configuredCapability is ConfiguredEnumCapability configuredEnumCapability)
				{
					if (!resourceCapabilitiesByCapabilityName.TryGetValue(configuredEnumCapability.CapabilityData.Name, out var resourceCapability))
					{
						var profileParameter = SrmHelpers.ProfileHelper.GetProfileParameterByName(configuredEnumCapability.CapabilityData.Name);
						if (profileParameter == null)
						{
							continue;
						}

						resourceCapability = new ResourceCapability(profileParameter.ID);

						resourceCapabilitiesByCapabilityName.Add(configuredEnumCapability.CapabilityData.Name, resourceCapability);
					}

					if (resourceCapability.Value == null)
					{
						resourceCapability.Value = new Skyline.DataMiner.Net.Profiles.CapabilityParameterValue(configuredEnumCapability.Discretes);
					}
					else
					{
						configuredEnumCapability.Discretes.ForEach(x =>
						{
							if (!resourceCapability.Value.Discreets.Contains(x))
							{
								resourceCapability.Value.Discreets.Add(x);
							}
						});
					}
				}
			}

			return resourceCapabilitiesByCapabilityName.Values.ToList();
		}

		private bool ResourceHasChangedData(Resource resource, List<ResourceCapability> added, List<ResourceCapability> updated, List<ResourceCapability> removed)
		{
			var hasChangedData = false;

			foreach (var resourceCapability in removed)
			{
				resource.Capabilities.Remove(resourceCapability);

				hasChangedData = true;
			}

			foreach (var resourceCapability in updated)
			{
				var capability = resource.Capabilities.Single(x => x.CapabilityProfileID == resourceCapability.CapabilityProfileID);
				if (!capability.Value.Discreets.Except(resourceCapability.Value.Discreets).Any() && !resourceCapability.Value.Discreets.Except(capability.Value.Discreets).Any())
				{
					continue;
				}

				capability.Value.Discreets = resourceCapability.Value.Discreets;

				hasChangedData = true;
			}

			foreach (var resourceCapability in added)
			{
				resource.Capabilities.Add(resourceCapability);

				hasChangedData = true;
			}

			return hasChangedData;
		}
		#endregion
	}
}
