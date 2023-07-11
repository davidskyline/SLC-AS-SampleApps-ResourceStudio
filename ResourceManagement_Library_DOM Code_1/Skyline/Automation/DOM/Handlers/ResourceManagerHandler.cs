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
	using static System.Collections.Specialized.BitVector32;

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
			capabilityValuesByInstanceId = new Lazy<Dictionary<Guid, CapabilityValueData>>(LoadCapabilityValueData);

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

		private Dictionary<Guid, CapabilityValueData> LoadCapabilityValueData()
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
			var dataLoader = new ResourcePoolDataLoader(domHelper);

			return dataLoader.Load();
		}

		private Dictionary<Guid, ResourceData> LoadResourceData()
		{
			var dataLoader = new ResourceDataLoader(domHelper);

			return dataLoader.Load();
		}

		private Dictionary<Guid, CapacityData> LoadCapacityData()
		{
			var dic = new Dictionary<Guid, CapacityData>();

			var mapper = new Dictionary<Guid, Action<CapacityData, object>>
			{
				[Resourcemanagement.Sections.CapacityInfo.CapacityName.Id] = (data, value) => data.Name = Convert.ToString(value),
				[Resourcemanagement.Sections.CapacityInfo.Units.Id] = (data, value) => data.Units = Convert.ToString(value),
				[Resourcemanagement.Sections.CapacityInfo.MinRange.Id] = (data, value) => data.RangeMin = Convert.ToDouble(value),
				[Resourcemanagement.Sections.CapacityInfo.MaxRange.Id] = (data, value) => data.RangeMax = Convert.ToDouble(value),
				[Resourcemanagement.Sections.CapacityInfo.StepSize.Id] = (data, value) => data.StepSize = Convert.ToDouble(value),
				[Resourcemanagement.Sections.CapacityInfo.Decimals.Id] = (data, value) => data.Decimals = Convert.ToInt64(value),
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
