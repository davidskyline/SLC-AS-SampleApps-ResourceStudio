namespace Skyline.Automation.DOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM.DomIds;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	internal class ResourceDataLoader
	{
		#region Fields
		private readonly DomHelper domHelper;

		private readonly Dictionary<Guid, Action<ResourceData, object>> genericMapper = new Dictionary<Guid, Action<ResourceData, object>>
		{
			[Resourcemanagement.Sections.ResourceInfo.Name.Id] = (data, value) => data.Name = Convert.ToString(value),
			[Resourcemanagement.Sections.ResourceInfo.Type.Id] = (data, value) => data.ResourceType = (Resourcemanagement.Enums.Type)value,
			[Resourcemanagement.Sections.ResourceInfo.Element.Id] = (data, value) => data.LinkedElementInfo = Convert.ToString(value),
			[Resourcemanagement.Sections.ResourceInfo.Service.Id] = (data, value) => data.LinkedServiceInfo = Convert.ToString(value),
			[Resourcemanagement.Sections.ResourceInternalProperties.Resource_Id.Id] = (data, value) => data.ResourceId = (Guid)value,
			[Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids.Id] = (data, value) => data.PoolIds = Convert.ToString(value),
			[Resourcemanagement.Sections.ResourceConnectionManagement.InputVsgs.Id] = (data, value) => data.VirtualSignalGroupInputIds = (List<Guid>)value,
			[Resourcemanagement.Sections.ResourceConnectionManagement.OutputVsgs.Id] = (data, value) => data.VirtualSignalGroupOutputIds = (List<Guid>)value,
			[Resourcemanagement.Sections.ResourceCost.Cost.Id] = (data, value) => data.Cost = Convert.ToDouble(value),
			[Resourcemanagement.Sections.ResourceCost.CostUnit.Id] = (data, value) => data.CostUnit = (Resourcemanagement.Enums.CostUnit)value,
			[Resourcemanagement.Sections.ResourceCost.Currency.Id] = (data, value) => data.Currency = (Resourcemanagement.Enums.Currency)value,
		};

		private readonly Dictionary<Guid, Action<ResourceProperty, object>> propertyMapper = new Dictionary<Guid, Action<ResourceProperty, object>>
		{
			[Resourcemanagement.Sections.ResourceProperties.Property.Id] = (data, value) => data.PropertyId = (Guid)value,
			[Resourcemanagement.Sections.ResourceProperties.PropertyValue.Id] = (data, value) => data.Value = Convert.ToString(value),
		};

		private readonly Dictionary<Guid, Action<ResourceCapacity, object>> capacityMapper = new Dictionary<Guid, Action<ResourceCapacity, object>>
		{
			[Resourcemanagement.Sections.ResourceCapacities.Capacity.Id] = (data, value) => data.CapacityId = (Guid)value,
			[Resourcemanagement.Sections.ResourceCapacities.CapacityValue.Id] = (data, value) => data.Value = (double)value,
		};
		#endregion

		internal ResourceDataLoader(DomHelper domHelper)
		{
			this.domHelper = domHelper ?? throw new ArgumentNullException(nameof(domHelper));
		}

		#region Methods
		internal Dictionary<Guid, ResourceData> Load()
		{
			var dic = new Dictionary<Guid, ResourceData>();

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Resource.Id));
			foreach (var instance in instances)
			{
				var data = ParseInstance(instance);

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private ResourceData ParseInstance(DomInstance instance)
		{
			var data = new ResourceData
			{
				Instance = instance,
			};

			foreach (var section in instance.Sections)
			{
				ParseSection(section, data);
			}

			return data;
		}

		private void ParseSection(Section section, ResourceData data)
		{
			if (section.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourceProperties.Id.Id)
			{
				ParsePropertiesSection(section, data);
			}
			else if (section.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourceCapacities.Id.Id)
			{
				ParseCapacitiesSection(section, data);
			}
			else
			{
				ParseGenericSection(section, data);
			}
		}

		private void ParseGenericSection(Section section, ResourceData data)
		{
			foreach (var fieldValue in section.FieldValues)
			{
				if (!genericMapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
				{
					continue;
				}

				action.Invoke(data, fieldValue.Value.Value);
			}
		}

		private void ParsePropertiesSection(Section section, ResourceData data)
		{
			if (!section.FieldValues.Any())
			{
				return;
			}

			var property = new ResourceProperty();

			foreach (var fieldValue in section.FieldValues)
			{
				if (!propertyMapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
				{
					continue;
				}

				action.Invoke(property, fieldValue.Value.Value);
			}

			data.Properties.Add(property);
		}

		private void ParseCapacitiesSection(Section section, ResourceData data)
		{
			if (!section.FieldValues.Any())
			{
				return;
			}

			var capacity = new ResourceCapacity();

			foreach (var fieldValue in section.FieldValues)
			{
				if (!capacityMapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
				{
					continue;
				}

				action.Invoke(capacity, fieldValue.Value.Value);
			}

			data.Capacities.Add(capacity);
		}
		#endregion
	}
}
