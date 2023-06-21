namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;
		#endregion

		public ScriptData(IEngine engine)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
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
		#endregion

		#region Methods
		public void AddCapacity()
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				return;
			}

			if (!TryCreateProfileParameter())
			{
				return;
			}

			CreateDomInstance();
		}

		private bool TryCreateProfileParameter()
		{
			var srmHelpers = new SrmHelpers(engine);

			var existingParameter = srmHelpers.ProfileHelper.GetProfileParameterByName(Name);
			if (existingParameter != null)
			{
				return false;
			}

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

			srmHelpers.ProfileHelper.ProfileParameters.Create(profileParameter);

			return true;
		}

		private void CreateDomInstance()
		{
			var domHelper = new DomHelper(engine.SendSLNetMessages, Skyline.Automation.DOM.DomIds.Resourcemanagement.ModuleId);

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
			domHelper.DomInstances.Create(capacityInstance);
		}
		#endregion
	}
}
