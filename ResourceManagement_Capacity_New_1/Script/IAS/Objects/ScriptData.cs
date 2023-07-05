namespace Script.IAS
{
	using System;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private Lazy<ResourceManagerHandler> resourceManagerHandler;

		private Lazy<SrmHelpers> srmHelpers;
		#endregion

		public ScriptData(IEngine engine)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));

			Init();
		}

		#region Properties
		public string Name { get; set; }

		public string Units { get; set; }

		public bool IsRangeMinEnabled { get; set; }

		public double RangeMin { get; set; }

		public bool IsRangeMaxEnabled { get; set; }

		public double RangeMax { get; set; }

		public bool IsStepSizeEnabled { get; set; }

		public double StepSize { get; set; }

		public bool IsDecimalsEnabled { get; set; }

		public int Decimals { get; set; }

		private ResourceManagerHandler ResourceManagerHandler => resourceManagerHandler.Value;

		private SrmHelpers SrmHelpers => srmHelpers.Value;
		#endregion

		#region Methods
		public void AddCapacity()
		{
			if (string.IsNullOrWhiteSpace(Name) || IsNameInUse())
			{
				return;
			}

			if (!TryCreateProfileParameter())
			{
				return;
			}

			CreateDomInstance();
		}

		public bool IsNameInUse()
		{
			if (ResourceManagerHandler.Capacities.Any(x => x.Name == Name))
			{
				return true;
			}

			var existingParameter = SrmHelpers.ProfileHelper.GetProfileParameterByName(Name);
			if (existingParameter != null)
			{
				return true;
			}

			return false;
		}

		private void Init()
		{
			resourceManagerHandler = new Lazy<ResourceManagerHandler>(() => new ResourceManagerHandler(engine));
			srmHelpers = new Lazy<SrmHelpers>(() => new SrmHelpers(engine));
		}

		private bool TryCreateProfileParameter()
		{
			var profileParameter = new Skyline.DataMiner.Net.Profiles.Parameter
			{
				Name = Name,
				Units = Units,
				Categories = Skyline.DataMiner.Net.Profiles.ProfileParameterCategory.Capacity,
				Type = Skyline.DataMiner.Net.Profiles.Parameter.ParameterType.Number,
			};

			if (IsRangeMinEnabled)
			{
				profileParameter.RangeMin = RangeMin;
			}

			if (IsRangeMaxEnabled)
			{
				profileParameter.RangeMax = RangeMax;
			}

			if (IsStepSizeEnabled)
			{
				profileParameter.Stepsize = StepSize;
			}

			if (IsDecimalsEnabled)
			{
				profileParameter.Decimals = Decimals;
			}

			SrmHelpers.ProfileHelper.ProfileParameters.Create(profileParameter);

			return true;
		}

		private void CreateDomInstance()
		{
			var capacityInfoSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Id);
			capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.CapacityName, new ValueWrapper<string>(Name)));

			if (!string.IsNullOrWhiteSpace(Units))
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Units, new ValueWrapper<string>(Units)));
			}

			if (IsRangeMinEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.MinRange, new ValueWrapper<double>(RangeMin)));
			}

			if (IsRangeMaxEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.MaxRange, new ValueWrapper<double>(RangeMax)));
			}

			if (IsStepSizeEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.StepSize, new ValueWrapper<double>(StepSize)));
			}

			if (IsDecimalsEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Decimals, new ValueWrapper<Int64>(Decimals)));
			}

			var capacityInstance = new DomInstance
			{
				DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Capacity,
			};
			capacityInstance.Sections.Add(capacityInfoSection);
			ResourceManagerHandler.DomHelper.DomInstances.Create(capacityInstance);
		}
		#endregion
	}
}
