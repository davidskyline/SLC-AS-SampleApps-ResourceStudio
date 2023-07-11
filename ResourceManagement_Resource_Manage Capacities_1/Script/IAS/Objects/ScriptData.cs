namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private readonly Guid domInstanceId;

		private ResourceManagerHandler resourceManagerHandler;

		private ResourceData resourceData;

		private Dictionary<Guid, CapacityData> capacitiesById;

		private List<ConfiguredCapacity> configuredCapacities;

		private Lazy<List<ReservationInstance>> ongoingAndFutureReservations;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string ResourceName
		{
			get
			{
				return resourceData.Name;
			}
		}

		public IReadOnlyCollection<CapacityData> Capacities
		{
			get { return capacitiesById.Values; }
		}

		public IReadOnlyCollection<ConfiguredCapacity> ConfiguredCapacities
		{
			get { return configuredCapacities; }
		}

		public ConfiguredCapacity CapacityToConfigure { get; private set; }

		public IReadOnlyCollection<ReservationInstance> OngoingAndFutureReservations
		{
			get { return ongoingAndFutureReservations.Value; }
		}
		#endregion

		#region Methods
		public UpdateCapacityResult TryUpdateResourceCapacities()
		{
			var added = configuredCapacities.Where(x => !resourceData.Capacities.Select(y => y.CapacityId).Contains(x.CapacityData.Instance.ID.Id)).ToList();
			var updated = GetUpdatedCapacities(configuredCapacities.Except(added).ToList());
			var removed = resourceData.Capacities.Where(x => !configuredCapacities.Select(y => y.CapacityData.Instance.ID.Id).Contains(x.CapacityId)).ToList();

			if ((updated.Any() || removed.Any()) && OngoingAndFutureReservations.Any())
			{
				return new UpdateCapacityResult
				{
					Succeeded = false,
					ErrorReason = ErrorReason.ResourceInUse,
				};
			}

			UpdateDomInstances(added, updated, removed);

			return new UpdateCapacityResult
			{
				Succeeded = true,
				ErrorReason = ErrorReason.None,
			};
		}

		public void SetCapacityToConfigure(CapacityData capacityData)
		{
			var capacityToConfigure = configuredCapacities.FirstOrDefault(x => x.CapacityData.Instance.ID.Id.Equals(capacityData.Instance.ID.Id));
			if (capacityToConfigure == null)
			{
				capacityToConfigure = new ConfiguredCapacity(capacityData);

				configuredCapacities.Add(capacityToConfigure);
			}

			CapacityToConfigure = capacityToConfigure;
		}

		public void SetConfiguredCapacities(List<CapacityData> capacities)
		{
			configuredCapacities.RemoveAll(x => !capacities.Select(y => y.Instance.ID.Id).Contains(x.CapacityData.Instance.ID.Id));
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);

			resourceData = resourceManagerHandler.Resources.Single(x => x.Instance.ID.Id == domInstanceId);
			capacitiesById = resourceManagerHandler.Capacities.ToDictionary(x => x.Instance.ID.Id, x => x);

			LoadConfiguredCapacities();

			ongoingAndFutureReservations = new Lazy<List<ReservationInstance>>(() => GetOngoingAndFutureReservations());
		}

		private void LoadConfiguredCapacities()
		{
			configuredCapacities = new List<ConfiguredCapacity>();

			foreach (var resourceCapacity in resourceData.Capacities)
			{
				if (!capacitiesById.TryGetValue(resourceCapacity.CapacityId, out var capacityData))
				{
					continue;
				}

				var configuredCapacity = new ConfiguredCapacity(capacityData, resourceCapacity.Value);
				configuredCapacities.Add(configuredCapacity);
			}
		}

		private void UpdateDomInstances(List<ConfiguredCapacity> added, List<ConfiguredCapacity> updated, List<ResourceCapacity> removed)
		{
			var hasChangedData = false;

			foreach (var configuredCapacity in added)
			{
				var resourceCapacitiesSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.Id);
				resourceCapacitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.Capacity, new ValueWrapper<Guid>(configuredCapacity.CapacityData.Instance.ID.Id)));
				resourceCapacitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.CapacityValue, new ValueWrapper<double>(configuredCapacity.Value)));

				resourceData.Instance.Sections.Add(resourceCapacitiesSection);

				hasChangedData = true;
			}

			foreach (var configuredCapacity in updated)
			{
				var resourceCapacitiesSections = resourceData.Instance.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.Id.Id && x.FieldValues.Any());

				var resourceCapacitiesSection = resourceCapacitiesSections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.Capacity)?.Value.Value == configuredCapacity.CapacityData.Instance.ID.Id);
				if (resourceCapacitiesSection == null)
				{
					continue;
				}

				resourceCapacitiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.CapacityValue, new ValueWrapper<double>(configuredCapacity.Value)));

				hasChangedData = true;
			}

			foreach (var resourceCapacity in removed)
			{
				var resourceCapacitiesSections = resourceData.Instance.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.Id.Id && x.FieldValues.Any());

				var sectionToRemove = resourceCapacitiesSections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCapacities.Capacity)?.Value.Value == resourceCapacity.CapacityId);

				if (sectionToRemove != null)
				{
					resourceData.Instance.Sections.Remove(sectionToRemove);

					hasChangedData = true;
				}
			}

			if (hasChangedData)
			{
				resourceManagerHandler.DomHelper.DomInstances.Update(resourceData.Instance);
			}
		}

		private List<ReservationInstance> GetOngoingAndFutureReservations()
		{
			if (resourceData.ResourceId == Guid.Empty)
			{
				return new List<ReservationInstance>();
			}

			var srmHelpers = new SrmHelpers(engine);

			var resource = srmHelpers.ResourceManagerHelper.GetResource(resourceData.ResourceId);
			if (resource == null)
			{
				return new List<ReservationInstance>();
			}

			var filter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID).AND(ReservationInstanceExposers.End.GreaterThan(DateTime.UtcNow));
			return srmHelpers.ResourceManagerHelper.GetReservationInstances(filter).ToList();
		}

		private List<ConfiguredCapacity> GetUpdatedCapacities(List<ConfiguredCapacity> capacitiesToVerify)
		{
			var updatedCapacities = new List<ConfiguredCapacity>();

			foreach (var capacity in capacitiesToVerify)
			{
				var resourceCapacity = resourceData.Capacities.Single(x => x.CapacityId == capacity.CapacityData.Instance.ID.Id);
				if (resourceCapacity.Value.Equals(capacity.Value))
				{
					continue;
				}

				updatedCapacities.Add(capacity);
			}

			return updatedCapacities;
		}
		#endregion
	}
}
