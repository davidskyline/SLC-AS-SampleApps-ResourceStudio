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

		private readonly Guid domInstanceId;

		private ResourceManagerHandler resourceManagerHandler;

		private CapacityData capacity;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string Name { get; private set; }

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
		public void UpdateCapacity()
		{
			if (!HasChangedData())
			{
				return;
			}

			TryUpdateProfileParameter();
			UpdateDomInstances();
		}

		public void DeleteCapacity()
		{
			if (!TryDeleteProfileParameter())
			{
				return;
			}

			DeleteDomInstances();
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);

			capacity = resourceManagerHandler.Capacities.Single(x => x.Instance.ID.Id == domInstanceId);

			Name = capacity.Name;
			Units = capacity.Units;

			if (capacity.RangeMin != null)
			{
				IsRangeMinEnabled = true;
				RangeMin = (double)capacity.RangeMin;
			}

			if (capacity.RangeMax != null)
			{
				IsRangeMaxEnabled = true;
				RangeMax = (double)capacity.RangeMax;
			}

			if (capacity.StepSize != null)
			{
				IsStepSizeEnabled = true;
				StepSize = (double)capacity.StepSize;
			}

			if (capacity.Decimals != null)
			{
				IsDecimalsEnabled = true;
				Decimals = (int)capacity.Decimals;
			}
		}

		private void TryUpdateProfileParameter()
		{
			var srmHelpers = new SrmHelpers(engine);

			var existingParameter = srmHelpers.ProfileHelper.GetProfileParameterByName(Name);
			if (existingParameter == null)
			{
				return;
			}

			existingParameter.Units = Units;
			existingParameter.RangeMin = IsRangeMinEnabled ? RangeMin : double.NaN;
			existingParameter.RangeMax = IsRangeMaxEnabled ? RangeMax : double.NaN;
			existingParameter.Stepsize = IsStepSizeEnabled ? StepSize : double.NaN;
			existingParameter.Decimals = IsDecimalsEnabled ? Decimals : int.MaxValue;

			srmHelpers.ProfileHelper.ProfileParameters.Update(existingParameter);
		}

		private bool TryDeleteProfileParameter()
		{
			var srmHelpers = new SrmHelpers(engine);

			var existingParameter = srmHelpers.ProfileHelper.GetProfileParameterByName(Name);
			if (existingParameter == null)
			{
				return false;
			}

			srmHelpers.ProfileHelper.ProfileParameters.Delete(existingParameter);

			return true;
		}

		private void DeleteDomInstances()
		{
			resourceManagerHandler.DomHelper.DomInstances.Delete(capacity.Instance);
		}

		private void UpdateDomInstances()
		{
			var capacityInfoSection = capacity.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Id.Id);
			capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Units, new ValueWrapper<string>(Units)));

			if (IsRangeMinEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.MinRange, new ValueWrapper<double>(RangeMin)));
			}
			else if (capacity.RangeMin != null)
			{
				capacityInfoSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.MinRange);
			}

			if (IsRangeMaxEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.MaxRange, new ValueWrapper<double>(RangeMax)));
			}
			else if (capacity.RangeMax != null)
			{
				capacityInfoSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.MaxRange);
			}

			if (IsStepSizeEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.StepSize, new ValueWrapper<double>(StepSize)));
			}
			else if (capacity.StepSize != null)
			{
				capacityInfoSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.StepSize);
			}

			if (IsDecimalsEnabled)
			{
				capacityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Decimals, new ValueWrapper<Int64>(Decimals)));
			}
			else if (capacity.Decimals != null)
			{
				capacityInfoSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapacityInfo.Decimals);
			}

			resourceManagerHandler.DomHelper.DomInstances.Update(capacity.Instance);
		}

		private bool HasChangedData()
		{
			if (!Units.Equals(capacity.Units))
			{
				return true;
			}

			if ((capacity.RangeMin != null && (!IsRangeMinEnabled || !capacity.RangeMin.Equals(RangeMin))) ||
				(capacity.RangeMin == null && IsRangeMinEnabled))
			{
				return true;
			}

			if ((capacity.RangeMax != null && (!IsRangeMaxEnabled || !capacity.RangeMax.Equals(RangeMax))) ||
				(capacity.RangeMax == null && IsRangeMaxEnabled))
			{
				return true;
			}

			if ((capacity.StepSize != null && (!IsStepSizeEnabled || !capacity.StepSize.Equals(StepSize))) ||
				(capacity.StepSize == null && IsStepSizeEnabled))
			{
				return true;
			}

			if ((capacity.Decimals != null && (!IsDecimalsEnabled || !capacity.Decimals.Equals(Decimals))) ||
				(capacity.Decimals == null && IsDecimalsEnabled))
			{
				return true;
			}

			return false;
		}
		#endregion
	}
}