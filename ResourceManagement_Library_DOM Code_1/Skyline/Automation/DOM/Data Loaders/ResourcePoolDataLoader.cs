namespace Skyline.Automation.DOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM.DomIds;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	public class ResourcePoolDataLoader
	{
		#region Fields
		private static readonly Dictionary<Guid, Action<ResourcePoolData, object>> GenericMapper = new Dictionary<Guid, Action<ResourcePoolData, object>>
		{
			[Resourcemanagement.Sections.ResourcePoolInfo.Name.Id] = (data, value) => data.Name = Convert.ToString(value),
			[Resourcemanagement.Sections.ResourcePoolInfo.AllowBookingsOnPoolLevel.Id] = (data, value) => data.IsBookable = Convert.ToBoolean(value),
			[Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids.Id] = (data, value) => data.ResourceIds = Convert.ToString(value),
			[Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Pool_Id.Id] = (data, value) =>
			{
				var poolId = Convert.ToString(value);
				data.PoolId = string.IsNullOrEmpty(poolId) ? Guid.Empty : Guid.Parse(poolId);
			},
			[Resourcemanagement.Sections.ResourcePoolInternalProperties.Pool_Resource_Id.Id] = (data, value) => data.ResourceId = (Guid)value,
		};

		private static readonly Dictionary<Guid, Action<ResourcePoolCapability, object>> CapabilityMapper = new Dictionary<Guid, Action<ResourcePoolCapability, object>>
		{
			[Resourcemanagement.Sections.ResourcePoolCapabilities.Capability.Id] = (data, value) => data.CapabilityId = (Guid)value,
			[Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_Enum_Values.Id] = (data, value) => data.CapabilityEnumValueIds = (List<Guid>)value,
			[Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_String_Value.Id] = (data, value) => data.CapabilityStringValue = Convert.ToString(value),
		};

		private readonly DomHelper domHelper;
		#endregion

		internal ResourcePoolDataLoader(DomHelper domHelper)
		{
			this.domHelper = domHelper ?? throw new ArgumentNullException(nameof(domHelper));
		}

		#region Methods
		public static ResourcePoolData ParseInstance(DomInstance instance)
		{
			var data = new ResourcePoolData
			{
				Instance = instance,
			};

			foreach (var section in instance.Sections)
			{
				ParseSection(section, data);
			}

			return data;
		}

		internal Dictionary<Guid, ResourcePoolData> Load()
		{
			var dic = new Dictionary<Guid, ResourcePoolData>();

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Resourcepool.Id));
			foreach (var instance in instances)
			{
				var data = ParseInstance(instance);

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private static void ParseSection(Section section, ResourcePoolData data)
		{
			if (section.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourcePoolCapabilities.Id.Id)
			{
				ParseCapabilitiesSection(section, data);
			}
			else
			{
				ParseGenericSection(section, data);
			}
		}

		private static void ParseGenericSection(Section section, ResourcePoolData data)
		{
			foreach (var fieldValue in section.FieldValues)
			{
				if (!GenericMapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
				{
					continue;
				}

				action.Invoke(data, fieldValue.Value.Value);
			}
		}

		private static void ParseCapabilitiesSection(Section section, ResourcePoolData data)
		{
			if (!section.FieldValues.Any())
			{
				return;
			}

			var capability = new ResourcePoolCapability();

			foreach (var fieldValue in section.FieldValues)
			{
				if (!CapabilityMapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
				{
					continue;
				}

				action.Invoke(capability, fieldValue.Value.Value);
			}

			data.Capabilities.Add(capability);
		}
		#endregion
	}
}
