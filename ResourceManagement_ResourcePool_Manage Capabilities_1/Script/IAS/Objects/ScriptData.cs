namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Sections.Fields;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private readonly Guid domInstanceId;

		private ResourceManagerHandler resourceManagerHandler;

		private ResourcePoolData resourcePool;

		private Dictionary<Guid, CapabilityData> capabilitiesById;

		private Dictionary<Guid, CapabilityValueData> capabilityValuesById;

		private List<ConfiguredCapability> configuredCapabilities;

		private Lazy<List<ReservationInstance>> ongoingAndFutureReservations;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string PoolName
		{
			get { return resourcePool.Name; }
		}

		public IReadOnlyCollection<CapabilityData> Capabilities
		{
			get { return capabilitiesById.Values; }
		}

		public IReadOnlyCollection<ConfiguredCapability> ConfiguredCapabilities
		{
			get { return configuredCapabilities; }
		}

		public ConfiguredCapability CapabilityToConfigure { get; private set; }

		public IReadOnlyCollection<ReservationInstance> OngoingAndFutureReservations
		{
			get { return ongoingAndFutureReservations.Value; }
		}
		#endregion

		#region Methods
		public UpdateCapabilityResult UpdatePoolCapabilities()
		{
			var added = configuredCapabilities.Where(x => !resourcePool.Capabilities.Select(y => y.CapabilityId).Contains(x.CapabilityData.Instance.ID.Id)).ToList();
			var updated = GetUpdatedCapabilities(configuredCapabilities.Except(added).ToList());
			var removed = resourcePool.Capabilities.Where(x => !configuredCapabilities.Select(y => y.CapabilityData.Instance.ID.Id).Contains(x.CapabilityId)).ToList();

			if ((updated.Any() || removed.Any()) && OngoingAndFutureReservations.Any())
			{
				return new UpdateCapabilityResult
				{
					Succeeded = false,
					ErrorReason = ErrorReason.ResourceInUse,
				};
			}

			UpdateDomInstances(added, updated, removed);

			return new UpdateCapabilityResult
			{
				Succeeded = true,
				ErrorReason = ErrorReason.None,
			};
		}

		public List<string> GetCapabilityValuesByCapabilityId(Guid capabilityId)
		{
			return capabilityValuesById.Values.Where(x => x.CapabilityId == capabilityId).Select(x => x.Value).ToList();
		}

		public void SetCapabilityToConfigure(CapabilityData capabilityData)
		{
			var capabilityToConfigure = configuredCapabilities.FirstOrDefault(x => x.CapabilityData.Instance.ID.Id.Equals(capabilityData.Instance.ID.Id));
			if (capabilityToConfigure == null)
			{
				if (capabilityData.CapabilityType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String)
				{
					capabilityToConfigure = new ConfiguredStringCapability(capabilityData);
				}
				else
				{
					capabilityToConfigure = new ConfiguredEnumCapability(capabilityData);
				}

				configuredCapabilities.Add(capabilityToConfigure);
			}

			CapabilityToConfigure = capabilityToConfigure;
		}

		public void SetConfiguredCapabilities(List<CapabilityData> capabilities)
		{
			configuredCapabilities.RemoveAll(x => !capabilities.Select(y => y.Instance.ID.Id).Contains(x.CapabilityData.Instance.ID.Id));
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);

			resourcePool = resourceManagerHandler.ResourcePools.Single(x => x.Instance.ID.Id == domInstanceId);
			capabilitiesById = resourceManagerHandler.Capabilities.ToDictionary(x => x.Instance.ID.Id, x => x);
			capabilityValuesById = resourceManagerHandler.CapabilityValues.ToDictionary(x => x.Instance.ID.Id, x => x);

			LoadConfiguredCapabilities();

			ongoingAndFutureReservations = new Lazy<List<ReservationInstance>>(() => GetOngoingAndFutureReservations());
		}

		private void LoadConfiguredCapabilities()
		{
			configuredCapabilities = new List<ConfiguredCapability>();

			foreach (var poolCapability in resourcePool.Capabilities)
			{
				if (!capabilitiesById.TryGetValue(poolCapability.CapabilityId, out var capabilityData))
				{
					continue;
				}

				ConfiguredCapability configuredCapability;
				if (capabilityData.CapabilityType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String)
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
		}

		private void UpdateDomInstances(List<ConfiguredCapability> added, List<ConfiguredCapability> updated, List<ResourcePoolCapability> removed)
		{
			var hasChangedData = false;

			foreach (var configuredCapability in added)
			{
				var resourcePoolCapabilitiesSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Id);
				resourcePoolCapabilitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability, new ValueWrapper<Guid>(configuredCapability.CapabilityData.Instance.ID.Id)));

				if (configuredCapability is ConfiguredStringCapability configuredStringCapability)
				{
					if (string.IsNullOrEmpty(configuredStringCapability.Value))
					{
						continue;
					}

					resourcePoolCapabilitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_String_Value, new ValueWrapper<string>(configuredStringCapability.Value)));

					hasChangedData = true;
				}
				else if (configuredCapability is ConfiguredEnumCapability configuredEnumCapability)
				{
					if (!configuredEnumCapability.Discretes.Any())
					{
						continue;
					}

					var capabilityValueIds = capabilityValuesById.Values.Where(x => x.CapabilityId.Equals(configuredEnumCapability.CapabilityData.Instance.ID.Id) && configuredEnumCapability.Discretes.Contains(x.Value)).Select(x => x.Instance.ID.Id).ToList();
					resourcePoolCapabilitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_Enum_Values, new ListValueWrapper<Guid>(capabilityValueIds)));

					hasChangedData = true;
				}
				else
				{
					continue;
				}

				resourcePool.Instance.Sections.Add(resourcePoolCapabilitiesSection);
			}

			foreach (var configuredCapability in updated)
			{
				var resourcePoolCapabilitiesSections = resourcePool.Instance.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Id.Id && x.FieldValues.Any());

				var resourcePoolCapabilitiesSection = resourcePoolCapabilitiesSections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability)?.Value.Value == configuredCapability.CapabilityData.Instance.ID.Id);
				if (resourcePoolCapabilitiesSection == null)
				{
					continue;
				}

				if (configuredCapability is ConfiguredStringCapability configuredStringCapability)
				{
					resourcePoolCapabilitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_String_Value, new ValueWrapper<string>(configuredStringCapability.Value)));

					hasChangedData = true;
				}
				else if (configuredCapability is ConfiguredEnumCapability configuredEnumCapability)
				{
					var capabilityValueIds = capabilityValuesById.Values.Where(x => x.CapabilityId.Equals(configuredEnumCapability.CapabilityData.Instance.ID.Id) && configuredEnumCapability.Discretes.Contains(x.Value)).Select(x => x.Instance.ID.Id).ToList();

					resourcePoolCapabilitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_Enum_Values, new ListValueWrapper<Guid>(capabilityValueIds)));

					hasChangedData = true;
				}
				else
				{
					continue;
				}
			}

			foreach (var poolCapability in removed)
			{
				var resourcePoolCapabilitiesSections = resourcePool.Instance.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Id.Id && x.FieldValues.Any());

				var sectionToRemove = resourcePoolCapabilitiesSections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability)?.Value.Value == poolCapability.CapabilityId);

				//var resourcePoolCapabilitiesSection = resourcePool.Instance.Sections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolCapabilities.Capability)?.Value.Value == poolCapability.CapabilityId);

				if (sectionToRemove != null)
				{
					resourcePool.Instance.Sections.Remove(sectionToRemove);

					hasChangedData = true;
				}
			}

			if (hasChangedData)
			{
				resourceManagerHandler.DomHelper.DomInstances.Update(resourcePool.Instance);
			}
		}

		private List<ConfiguredCapability> GetUpdatedCapabilities(List<ConfiguredCapability> capabilitiesToVerify)
		{
			var updatedCapabilities = new List<ConfiguredCapability>();

			foreach (var capability in capabilitiesToVerify)
			{
				var poolCapability = resourcePool.Capabilities.Single(x => x.CapabilityId == capability.CapabilityData.Instance.ID.Id);

				if (capability is ConfiguredStringCapability configuredStringCapability)
				{
					if (poolCapability.CapabilityStringValue == configuredStringCapability.Value)
					{
						continue;
					}
				}
				else if (capability is ConfiguredEnumCapability configuredEnumCapability)
				{
					var capabilityValueIds = capabilityValuesById.Values.Where(x => x.CapabilityId.Equals(configuredEnumCapability.CapabilityData.Instance.ID.Id) && configuredEnumCapability.Discretes.Contains(x.Value)).Select(x => x.Instance.ID.Id).ToList();

					if (!capabilityValueIds.Any() || (!poolCapability.CapabilityEnumValueIds.Except(capabilityValueIds).Any() && !capabilityValueIds.Except(poolCapability.CapabilityEnumValueIds).Any()))
					{
						continue;
					}
				}
				else
				{
					continue;
				}

				updatedCapabilities.Add(capability);
			}

			return updatedCapabilities;
		}

		private List<ReservationInstance> GetOngoingAndFutureReservations()
		{
			var poolResourceIds = resourcePool.ResourceIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
			if (!poolResourceIds.Any())
			{
				return new List<ReservationInstance>();
			}

			var resourcesData = resourceManagerHandler.Resources.Where(x => poolResourceIds.Contains(Convert.ToString(x.Instance.ID.Id))).ToList();
			if (!resourcesData.Any())
			{
				return new List<ReservationInstance>();
			}

			var srmHelpers = new SrmHelpers(engine);

			var resourceFilter = new ORFilterElement<Resource>(resourcesData.Where(x => x.ResourceId != Guid.Empty).Select(x => ResourceExposers.ID.Equal(x.ResourceId)).ToArray());

			var resources = new List<Resource>();
			foreach (var resource in srmHelpers.ResourceManagerHelper.GetResources(resourceFilter))
			{
				if (resource == null)
				{
					continue;
				}

				resources.Add(resource);
			}

			if (!resources.Any())
			{
				return new List<ReservationInstance>();
			}

			var reservationFilter = new ORFilterElement<ReservationInstance>(resources.Select(x => ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(x.ResourceId.Id)).ToArray()).AND(ReservationInstanceExposers.End.GreaterThan(DateTime.UtcNow));
			return srmHelpers.ResourceManagerHelper.GetReservationInstances(reservationFilter).ToList();
		}
		#endregion
	}
}
