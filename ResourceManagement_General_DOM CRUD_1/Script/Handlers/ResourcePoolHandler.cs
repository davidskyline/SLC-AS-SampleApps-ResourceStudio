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

		private Dictionary<Guid, List<ConfiguredCapability>> configuredCapabilitiesByPoolDomInstanceId = new Dictionary<Guid, List<ConfiguredCapability>>();
		#endregion

		public ResourcePoolHandler(IEngine engine, DomHelper domHelper, DomInstance domInstance)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domHelper = domHelper ?? throw new ArgumentNullException(nameof(domHelper));
			this.domInstance = domInstance ?? throw new ArgumentNullException(nameof(domInstance));

			Init();
		}

		#region Properties
		private ResourceManagerHandler ResourceManagerHandler => resourceManagerHandler.Value;

		private SrmHelpers SrmHelpers => srmHelpers.Value;

		private ResourcePoolData ResourcePoolData => resourcePoolData.Value;
		#endregion

		#region Methods
		public void Handle()
		{
			var resourcesResult = HandleDataOnPoolResources();
			if (!resourcesResult.Success)
			{
				SetErrorMessage(resourcesResult.ErrorMessage);

				return;
			}

			var poolNameResult = HandlePoolName();
			if (!poolNameResult.Success)
			{
				SetErrorMessage(poolNameResult.ErrorMessage);

				return;
			}

			var poolResourceResult = HandlePoolResource(poolNameResult.HasChangedName, resourcesResult.ResourcesInPool);
			if (!poolResourceResult.Success)
			{
				SetErrorMessage(poolResourceResult.ErrorMessage);

				//return;
			}
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

		private PoolNameResult HandlePoolName()
		{
			var existingResourcePool = SrmHelpers.ResourceManagerHelper.GetResourcePoolByName(ResourcePoolData.Name);
			if (existingResourcePool != null)
			{
				if (existingResourcePool.ID != ResourcePoolData.PoolId)
				{
					return new PoolNameResult
					{
						ErrorMessage = $"Resource pool '{ResourcePoolData.Name}' already exists with ID '{existingResourcePool.ID}'.",
					};
				}

				if (existingResourcePool.Name == ResourcePoolData.Name)
				{
					return new PoolNameResult
					{
						HasChangedName = false,
					};
				}
			}

			var resourcePool = SrmHelpers.ResourceManagerHelper.GetResourcePool(ResourcePoolData.PoolId);
			if (resourcePool == null)
			{
				return new PoolNameResult
				{
					ErrorMessage = $"Resource pool '{ResourcePoolData.Name}' with ID '{ResourcePoolData.PoolId}' does not exist.",
				};
			}

			resourcePool.Name = ResourcePoolData.Name;

			resourcePool = SrmHelpers.ResourceManagerHelper.AddOrUpdateResourcePools(resourcePool).First();

			return new PoolNameResult
			{
				HasChangedName = true,
				ResourcePool = resourcePool,
			};
		}

		private ResourcesResult HandleDataOnPoolResources()
		{
			var result = new ResourcesResult();

			var resourceDataMappingsByResourceId = GetResourceDataMappings();

			var resourceFilter = new ORFilterElement<Resource>(resourceDataMappingsByResourceId.Keys.Select(id => ResourceExposers.ID.Equal(id)).ToArray());
			foreach (var resource in SrmHelpers.ResourceManagerHelper.GetResources(resourceFilter))
			{
				if (resource == null || !resourceDataMappingsByResourceId.TryGetValue(resource.ID, out var resourceDataMapping))
				{
					continue;
				}

				resourceDataMapping.Resource = resource;
				result.ResourcesInPool.Add(resource);
			}

			var resourcesToUpdate = ApplyChanges(resourceDataMappingsByResourceId.Values);
			result.UpdatedResources = SrmHelpers.ResourceManagerHelper.AddOrUpdateResources(resourcesToUpdate.ToArray()).ToList();

			return result;
		}

		private PoolResourceResult HandlePoolResource(bool poolNameIsChanged, List<Resource> poolResources)
		{
			var result = new PoolResourceResult();

			if (!ResourcePoolData.IsBookable)
			{
				if (ResourcePoolData.ResourceId == Guid.Empty)
				{
					return result;
				}

				var poolResource = SrmHelpers.ResourceManagerHelper.GetResource(ResourcePoolData.ResourceId);
				if (poolResource == null)
				{
					return result;
				}

				SrmHelpers.ResourceManagerHelper.RemoveResources(poolResource);

				var resourcePoolInternalPropertiesSection = ResourcePoolData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourcePoolInternalProperties.Id.Id);
				resourcePoolInternalPropertiesSection.RemoveFieldValueById(Resourcemanagement.Sections.ResourcePoolInternalProperties.Pool_Resource_Id);

				ResourceManagerHandler.DomHelper.DomInstances.Update(ResourcePoolData.Instance);

				return result;
			}

			var resourceName = $"{ResourcePoolData.Name} - Pool Resource";
			var isNew = ResourcePoolData.ResourceId == Guid.Empty;
			var existingResource = SrmHelpers.ResourceManagerHelper.GetResourceByName(resourceName);
			if (existingResource != null && (isNew || existingResource.ID != ResourcePoolData.ResourceId))
			{
				return new PoolResourceResult
				{
					ErrorMessage = $"Resource '{resourceName}' already exists with ID '{existingResource.ID}'.",
				};
			}

			result.Resource = isNew ? new Resource() : SrmHelpers.ResourceManagerHelper.GetResource(ResourcePoolData.ResourceId) ?? new Resource();
			result.Resource.Name = resourceName;

			result.Resource.PoolGUIDs.Clear();
			result.Resource.PoolGUIDs.Add(ResourcePoolData.PoolId);

			if (poolResources.Any())
			{
				result.Resource.MaxConcurrency = poolResources.Sum(x => x.MaxConcurrency);
				result.Resource.Mode = ResourceMode.Unavailable;
			}
			else
			{
				result.Resource.MaxConcurrency = 1;
				result.Resource.Mode = ResourceMode.Unavailable;
			}

			var requiredCapabilities = GetRequiredResourceCapabilities(configuredCapabilitiesByPoolDomInstanceId[ResourcePoolData.Instance.ID.Id]);
			if (isNew)
			{
				result.Resource.Capabilities = requiredCapabilities;

				result.Resource = SrmHelpers.ResourceManagerHelper.AddOrUpdateResources(result.Resource).First();

				var resourcePoolInternalPropertiesSection = ResourcePoolData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourcePoolInternalProperties.Id.Id);
				resourcePoolInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Resourcemanagement.Sections.ResourcePoolInternalProperties.Pool_Resource_Id, new ValueWrapper<Guid>(result.Resource.ID)));

				ResourceManagerHandler.DomHelper.DomInstances.Update(ResourcePoolData.Instance);
			}
			else
			{
				var added = requiredCapabilities.Where(x => !result.Resource.Capabilities.Select(y => y.CapabilityProfileID).Contains(x.CapabilityProfileID)).ToList();
				var updated = requiredCapabilities.Except(added).ToList();
				var removed = result.Resource.Capabilities.Where(x => !requiredCapabilities.Select(y => y.CapabilityProfileID).Contains(x.CapabilityProfileID)).ToList();

				if (ResourceHasChangedData(result.Resource, added, updated, removed) || poolNameIsChanged)
				{
					result.Resource = SrmHelpers.ResourceManagerHelper.AddOrUpdateResources(result.Resource).First();
				}
			}

			return result;
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
					if (!TryGetConfiguredCapabilities(poolDomInstanceId, out var configuredCapabilities))
					{
						continue;
					}

					resourceDataMapping.ConfiguredCapabilities.AddRange(configuredCapabilities);
				}
			}

			return resourceDataMappingsByResourceId;

			bool TryGetConfiguredCapabilities(Guid poolDomInstanceId, out List<ConfiguredCapability> configuredCapabilities)
			{
				if (configuredCapabilitiesByPoolDomInstanceId.TryGetValue(poolDomInstanceId, out configuredCapabilities))
				{
					return true;
				}
				
				if (!poolCapabilitiesByPoolDomInstanceId.TryGetValue(poolDomInstanceId, out var capabilities))
				{
					return false;
				}

				configuredCapabilities = new List<ConfiguredCapability>();
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

					configuredCapabilities.Add(configuredCapability);
				}

				configuredCapabilitiesByPoolDomInstanceId.Add(poolDomInstanceId, configuredCapabilities);
				
				return true;
			}
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

		#region Classes
		private abstract class Result
		{
			public string ErrorMessage { get; set; }

			public bool Success => string.IsNullOrEmpty(ErrorMessage);
		}

		private class PoolResourceResult : Result
		{
			public Resource Resource { get; set; }
		}

		private class PoolNameResult : Result
		{
			public ResourcePool ResourcePool { get; set; }

			public bool HasChangedName { get; set; }
		}

		private class ResourcesResult : Result
		{
			public List<Resource> ResourcesInPool { get; set; } = new List<Resource>();

			public List<Resource> UpdatedResources { get; set; } = new List<Resource>();
		}
		#endregion
	}
}
