namespace Skyline.Automation.DOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM.DomIds;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	public class ResourceManagerHandler
	{
		#region Fields
		private readonly DomHelper domHelper;

		private Lazy<Dictionary<Guid, CapabilityData>> capabilitiesByInstanceId;

		private Lazy<Dictionary<Guid, CapabilityValueData>> capabilityValuesByInstanceId;

		private Lazy<Dictionary<Guid, ResourcePoolData>> resourcePoolsByInstanceId;

		private Lazy<Dictionary<Guid, ResourceData>> resourcesByInstanceId;

		private Lazy<Dictionary<Guid, CapacityData>> capacitiesByInstanceId;

		private Lazy<Dictionary<Guid, PropertyData>> propertiesByInstanceId;
		#endregion

		public ResourceManagerHandler(IEngine engine)
		{
			domHelper = new DomHelper(engine.SendSLNetMessages, Resourcemanagement.ModuleId);

			Init();
		}

		public ResourceManagerHandler(DomHelper domHelper)
		{
			if (domHelper == null)
			{
				throw new ArgumentNullException(nameof(domHelper));
			}

			if (domHelper.ModuleId != Resourcemanagement.ModuleId)
			{
				throw new InvalidOperationException($"DomHelper with module ID '{domHelper.ModuleId}' is provided while module ID '{Resourcemanagement.ModuleId}' is expected");
			}

			this.domHelper = domHelper;

			Init();
		}

		#region Properties
		public DomHelper DomHelper { get { return domHelper; } }

		public IReadOnlyCollection<CapabilityData> Capabilities
		{
			get
			{
				return capabilitiesByInstanceId.Value.Values;
			}
		}

		public IReadOnlyCollection<CapabilityValueData> CapabilityValues
		{
			get
			{
				return capabilityValuesByInstanceId.Value.Values;
			}
		}

		public IReadOnlyCollection<ResourcePoolData> ResourcePools
		{
			get
			{
				return resourcePoolsByInstanceId.Value.Values;
			}
		}

		public IReadOnlyCollection<ResourceData> Resources
		{
			get
			{
				return resourcesByInstanceId.Value.Values;
			}
		}

		public IReadOnlyCollection<CapacityData> Capacities
		{
			get
			{
				return capacitiesByInstanceId.Value.Values;
			}
		}

		public IReadOnlyCollection<PropertyData> Properties
		{
			get
			{
				return propertiesByInstanceId.Value.Values;
			}
		}
		#endregion

		#region Methods
		private void Init()
		{
			capabilitiesByInstanceId = new Lazy<Dictionary<Guid, CapabilityData>>(LoadCapabilityData);
			capabilityValuesByInstanceId = new Lazy<Dictionary<Guid, CapabilityValueData>>(LoadCapabityValueData);

			resourcePoolsByInstanceId = new Lazy<Dictionary<Guid, ResourcePoolData>>(LoadResourcePoolData);
			resourcesByInstanceId = new Lazy<Dictionary<Guid, ResourceData>>(LoadResourceData);

			capacitiesByInstanceId = new Lazy<Dictionary<Guid, CapacityData>>(LoadCapacityData);

			propertiesByInstanceId = new Lazy<Dictionary<Guid, PropertyData>>(LoadPropertyData);
		}

		private Dictionary<Guid, CapabilityData> LoadCapabilityData()
		{
			var dic = new Dictionary<Guid, CapabilityData>();

			var mapper = new Dictionary<Guid, Action<CapabilityData, object>>
			{
				[Resourcemanagement.Sections.CapabilityInfo.CapabilityName.Id] = (data, value) => data.Name = Convert.ToString(value),
				[Resourcemanagement.Sections.CapabilityInfo.CapabilityType.Id] = (data, value) => data.CapabilityType = (Resourcemanagement.Enums.CapabilityType)value,
			};

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Capability.Id));
			foreach (var instance in instances)
			{
				var data = new CapabilityData
				{
					Instance = instance,
				};

				foreach (var fieldValue in instance.Sections.SelectMany(x => x.FieldValues))
				{
					if (!mapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
					{
						continue;
					}

					action.Invoke(data, fieldValue.Value.Value);
				}

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private Dictionary<Guid, CapabilityValueData> LoadCapabityValueData()
		{
			var dic = new Dictionary<Guid, CapabilityValueData>();

			var mapper = new Dictionary<Guid, Action<CapabilityValueData, object>>
			{
				[Resourcemanagement.Sections.CapabilityEnumValueDetails.Capability.Id] = (data, value) => data.CapabilityId = (Guid)value,
				[Resourcemanagement.Sections.CapabilityEnumValueDetails.Value.Id] = (data, value) => data.Value = Convert.ToString(value),
			};

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Capabilityenumvalue.Id));
			foreach (var instance in instances)
			{
				var data = new CapabilityValueData
				{
					Instance = instance,
				};

				foreach (var fieldValue in instance.Sections.SelectMany(x => x.FieldValues))
				{
					if (!mapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
					{
						continue;
					}

					action.Invoke(data, fieldValue.Value.Value);
				}

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private Dictionary<Guid, ResourcePoolData> LoadResourcePoolData()
		{
			var dic = new Dictionary<Guid, ResourcePoolData>();

			var mapper = new Dictionary<Guid, Action<ResourcePoolData, object>>
			{
				[Resourcemanagement.Sections.ResourcePoolInfo.Name.Id] = (data, value) => data.Name = Convert.ToString(value),
				[Resourcemanagement.Sections.ResourcePoolInfo.Bookable.Id] = (data, value) => data.IsBookable = Convert.ToBoolean(value),
				[Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids.Id] = (data, value) => data.ResourceIds = Convert.ToString(value),
				[Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Pool_Id.Id] = (data, value) =>
				{
					var poolId = Convert.ToString(value);
					data.PoolId = string.IsNullOrEmpty(poolId) ? Guid.Empty : Guid.Parse(poolId);
				},
			};

			var capabilityMapper = new Dictionary<Guid, Action<ResourcePoolCapability, object>>
			{
				[Resourcemanagement.Sections.ResourcePoolCapabilities.Capability.Id] = (data, value) => data.CapabilityId = (Guid)value,
				[Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_Enum_Values.Id] = (data, value) => data.CapabilityEnumValueIds = (List<Guid>)value,
				[Resourcemanagement.Sections.ResourcePoolCapabilities.Capability_String_Value.Id] = (data, value) => data.CapabilityStringValue = Convert.ToString(value),
			};

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Resourcepool.Id));
			foreach (var instance in instances)
			{
				var data = new ResourcePoolData
				{
					Instance = instance,
				};

				foreach (var section in instance.Sections)
				{
					if (section.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourcePoolCapabilities.Id.Id)
					{
						if (!section.FieldValues.Any())
						{
							continue;
						}

						var capability = new ResourcePoolCapability();

						foreach (var fieldValue in section.FieldValues)
						{
							if (!capabilityMapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
							{
								continue;
							}

							action.Invoke(capability, fieldValue.Value.Value);
						}

						data.Capabilities.Add(capability);
					}
					else
					{
						foreach (var fieldValue in section.FieldValues)
						{
							if (!mapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
							{
								continue;
							}

							action.Invoke(data, fieldValue.Value.Value);
						}
					}
				}

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private Dictionary<Guid, ResourceData> LoadResourceData()
		{
			var dic = new Dictionary<Guid, ResourceData>();

			var mapper = new Dictionary<Guid, Action<ResourceData, object>>
			{
				[Resourcemanagement.Sections.ResourceInfo.Name.Id] = (data, value) => data.Name = Convert.ToString(value),
				[Resourcemanagement.Sections.ResourceInfo.Type.Id] = (data, value) => data.ResourceType = (Resourcemanagement.Enums.Type)value,
				[Resourcemanagement.Sections.ResourceInfo.Element.Id] = (data, value) => data.LinkedElementInfo = Convert.ToString(value),
				[Resourcemanagement.Sections.ResourceInternalProperties.Resource_Id.Id] = (data, value) => data.ResourceId = (Guid)value,
				[Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids.Id] = (data, value) => data.PoolIds = Convert.ToString(value),
				[Resourcemanagement.Sections.ResourceConnectionManagement.InputVsgs.Id] = (Data, value) => Data.VirtualSignalGroupInputIds = (List<Guid>)value,
				[Resourcemanagement.Sections.ResourceConnectionManagement.OutputVsgs.Id] = (Data, value) => Data.VirtualSignalGroupOutputIds = (List<Guid>)value,
			};

			var propertyMapper = new Dictionary<Guid, Action<ResourceProperty, object>>
			{
				[Resourcemanagement.Sections.ResourceProperties.Property.Id] = (data, value) => data.PropertyId = (Guid)value,
				[Resourcemanagement.Sections.ResourceProperties.PropertyValue.Id] = (data, value) => data.Value = Convert.ToString(value),
			};

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Resource.Id));
			foreach (var instance in instances)
			{
				var data = new ResourceData
				{
					Instance = instance,
				};

				foreach (var section in instance.Sections)
				{
					if (section.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourceProperties.Id.Id)
					{
						if (!section.FieldValues.Any())
						{
							continue;
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
					else
					{
						foreach (var fieldValue in section.FieldValues)
						{
							if (!mapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
							{
								continue;
							}

							action.Invoke(data, fieldValue.Value.Value);
						}
					}
				}

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private Dictionary<Guid, CapacityData> LoadCapacityData()
		{
			var dic = new Dictionary<Guid, CapacityData>();

			var mapper = new Dictionary<Guid, Action<CapacityData, object>>
			{
				[Resourcemanagement.Sections.CapacityInfo.CapacityName.Id] = (data, value) => data.Name = Convert.ToString(value),
			};

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Capacity.Id));
			foreach (var instance in instances)
			{
				var data = new CapacityData
				{
					Instance = instance,
				};

				foreach (var fieldValue in instance.Sections.SelectMany(x => x.FieldValues))
				{
					if (!mapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
					{
						continue;
					}

					action.Invoke(data, fieldValue.Value.Value);
				}

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}

		private Dictionary<Guid, PropertyData> LoadPropertyData()
		{
			var dic = new Dictionary<Guid, PropertyData>();

			var mapper = new Dictionary<Guid, Action<PropertyData, object>>
			{
				[Resourcemanagement.Sections.PropertyInfo.PropertyName.Id] = (data, value) => data.Name = Convert.ToString(value),
			};

			var instances = domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Resourcemanagement.Definitions.Resourceproperty.Id));
			foreach (var instance in instances)
			{
				var data = new PropertyData
				{
					Instance = instance,
				};

				foreach (var fieldValue in instance.Sections.SelectMany(x => x.FieldValues))
				{
					if (!mapper.TryGetValue(fieldValue.FieldDescriptorID.Id, out var action))
					{
						continue;
					}

					action.Invoke(data, fieldValue.Value.Value);
				}

				dic.Add(data.Instance.ID.Id, data);
			}

			return dic;
		}
		#endregion
	}
}
