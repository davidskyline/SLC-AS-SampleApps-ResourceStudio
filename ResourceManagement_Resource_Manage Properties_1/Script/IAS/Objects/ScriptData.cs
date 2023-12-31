﻿namespace Script.IAS
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

		private Dictionary<Guid, PropertyData> propertiesById;

		private List<ConfiguredProperty> configuredProperties;

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

		public IReadOnlyCollection<PropertyData> Properties
		{
			get { return propertiesById.Values; }
		}

		public IReadOnlyCollection<ConfiguredProperty> ConfiguredProperties
		{
			get { return configuredProperties; }
		}

		public IReadOnlyCollection<ReservationInstance> OngoingAndFutureReservations
		{
			get { return ongoingAndFutureReservations.Value; }
		}
		#endregion

		#region Methods
		public UpdatePropertyResult TryUpdateResourceProperties()
		{
			var added = configuredProperties.Where(x => !resourceData.Properties.Select(y => y.PropertyId).Contains(x.PropertyData.Instance.ID.Id)).ToList();
			var updated = GetUpdatedProperties(configuredProperties.Except(added).ToList());
			var removed = resourceData.Properties.Where(x => !configuredProperties.Select(y => y.PropertyData.Instance.ID.Id).Contains(x.PropertyId)).ToList();

			if ((updated.Any() || removed.Any()) && OngoingAndFutureReservations.Any())
			{
				return new UpdatePropertyResult
				{
					Succeeded = false,
					ErrorReason = ErrorReason.ResourceInUse,
				};
			}

			UpdateDomInstances(added, updated, removed);

			return new UpdatePropertyResult
			{
				Succeeded = true,
				ErrorReason = ErrorReason.None,
			};
		}

		public void SetConfiguredProperties(List<ConfiguredProperty> configuredProperties)
		{
			this.configuredProperties = configuredProperties ?? throw new ArgumentNullException(nameof(configuredProperties));
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);

			resourceData = resourceManagerHandler.Resources.Single(x => x.Instance.ID.Id == domInstanceId);
			propertiesById = resourceManagerHandler.Properties.ToDictionary(x => x.Instance.ID.Id, x => x);

			LoadConfiguredProperties();

			ongoingAndFutureReservations = new Lazy<List<ReservationInstance>>(() => GetOngoingAndFutureReservations());
		}

		private void LoadConfiguredProperties()
		{
			configuredProperties = new List<ConfiguredProperty>();

			foreach (var resourceProperty in resourceData.Properties)
			{
				if (!propertiesById.TryGetValue(resourceProperty.PropertyId, out var propertyData))
				{
					continue;
				}

				var configuredProperty = new ConfiguredProperty(propertyData, resourceProperty.Value);
				configuredProperties.Add(configuredProperty);
			}
		}

		private void UpdateDomInstances(List<ConfiguredProperty> added, List<ConfiguredProperty> updated, List<ResourceProperty> removed)
		{
			var hasChangedData = false;

			foreach (var configuredProperty in added)
			{
				if (string.IsNullOrEmpty(configuredProperty.Value))
				{
					continue;
				}

				var resourcePropertiesSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.Id);
				resourcePropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.Property, new ValueWrapper<Guid>(configuredProperty.PropertyData.Instance.ID.Id)));
				resourcePropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.PropertyValue, new ValueWrapper<string>(configuredProperty.Value)));

				resourceData.Instance.Sections.Add(resourcePropertiesSection);

				hasChangedData = true;
			}

			foreach (var configuredProperty in updated)
			{
				var resourcePropertiesSections = resourceData.Instance.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.Id.Id && x.FieldValues.Any());

				var resourcePropertiesSection = resourcePropertiesSections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.Property)?.Value.Value == configuredProperty.PropertyData.Instance.ID.Id);
				if (resourcePropertiesSection == null)
				{
					continue;
				}

				resourcePropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.PropertyValue, new ValueWrapper<string>(configuredProperty.Value)));

				hasChangedData = true;
			}

			foreach (var resourceProperty in removed)
			{
				var resourcePropertiesSections = resourceData.Instance.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.Id.Id && x.FieldValues.Any());

				var sectionToRemove = resourcePropertiesSections.SingleOrDefault(x => (Guid)x.GetFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceProperties.Property)?.Value.Value == resourceProperty.PropertyId);

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

		private List<ConfiguredProperty> GetUpdatedProperties(List<ConfiguredProperty> propertiesToVerify)
		{
			var updatedProperties = new List<ConfiguredProperty>();

			foreach (var property in propertiesToVerify)
			{
				var resourceProperty = resourceData.Properties.Single(x => x.PropertyId == property.PropertyData.Instance.ID.Id);
				if (resourceProperty.Value == property.Value)
				{
					continue;
				}

				updatedProperties.Add(property);
			}

			return updatedProperties;
		}
		#endregion
	}
}
