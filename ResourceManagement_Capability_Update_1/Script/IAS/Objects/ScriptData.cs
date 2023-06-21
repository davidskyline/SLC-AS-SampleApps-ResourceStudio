namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
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

		private CapabilityData capability;

		private List<CapabilityValueData> capabilityValues;

		private List<string> updatedDiscretes;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string Name
		{
			get
			{
				return capability.Name;
			}
		}

		public string Type
		{
			get
			{
				return (capability.CapabilityType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String) ? "Text" : "Discrete";
			}
		}

		public List<string> Discretes
		{
			get
			{
				return capabilityValues?.Select(x => x.Value).ToList() ?? new List<string>();
			}

			set
			{
				updatedDiscretes = value;
			}
		}
		#endregion

		#region Methods
		public void UpdateCapability()
		{
			if (capability.CapabilityType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String)
			{
				return;
			}

			var added = updatedDiscretes.Except(Discretes).ToList();
			var removed = capabilityValues.Where(x => !updatedDiscretes.Contains(x.Value)).ToList();

			if (!added.Any() && !removed.Any())
			{
				return;
			}

			TryUpdateProfileParameter(added, removed.Select(x => x.Value).ToList());
			UpdateDomInstances(added, removed);
		}

		public void DeleteCapability()
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

			capability = resourceManagerHandler.Capabilities.Single(x => x.Instance.ID.Id == domInstanceId);
			if (capability.CapabilityType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.Enum)
			{
				capabilityValues = resourceManagerHandler.CapabilityValues.Where(x => x.CapabilityId == domInstanceId).ToList();
			}
		}

		private void TryUpdateProfileParameter(List<string> added, List<string> removed)
		{
			var srmHelpers = new SrmHelpers(engine);

			var existingParameter = srmHelpers.ProfileHelper.GetProfileParameterByName(Name);
			if (existingParameter == null)
			{
				return;
			}

			var discretes = existingParameter.GetDiscreets();
			foreach (var discreteToAdd in added)
			{
				discretes.Add(new Skyline.DataMiner.Net.ProfileManager.Objects.ProfileParameterDiscreet(discreteToAdd, discreteToAdd));
			}

			foreach (var discretesToRemove in removed)
			{
				discretes.Remove(new Skyline.DataMiner.Net.ProfileManager.Objects.ProfileParameterDiscreet(discretesToRemove, discretesToRemove));
			}

			Skyline.DataMiner.Net.NaturalSortComparer comparer = new Skyline.DataMiner.Net.NaturalSortComparer();
			discretes.Sort((x, y) => comparer.Compare(x.RawValue, y.RawValue));

			existingParameter.SetDiscreets(discretes);
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

		private void UpdateDomInstances(List<string> added, List<CapabilityValueData> removed)
		{
			var domHelper = new DomHelper(engine.SendSLNetMessages, Skyline.Automation.DOM.DomIds.Resourcemanagement.ModuleId);

			foreach (var discrete in added)
			{
				var capabilityEnumValueDetailsSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityEnumValueDetails.Id);
				capabilityEnumValueDetailsSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityEnumValueDetails.Capability, new ValueWrapper<Guid>(capability.Instance.ID.Id)));
				capabilityEnumValueDetailsSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityEnumValueDetails.Value, new ValueWrapper<string>(discrete)));

				var capabilityValueInstance = new DomInstance
				{
					DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Capabilityenumvalue,
				};
				capabilityValueInstance.Sections.Add(capabilityEnumValueDetailsSection);
				domHelper.DomInstances.Create(capabilityValueInstance);
			}

			foreach (var capabilityValue in removed)
			{
				domHelper.DomInstances.Delete(capabilityValue.Instance);
			}
		}

		private void DeleteDomInstances()
		{
			var domHelper = new DomHelper(engine.SendSLNetMessages, Skyline.Automation.DOM.DomIds.Resourcemanagement.ModuleId);

			if (capabilityValues != null)
			{
				foreach (var capabilityValue in capabilityValues)
				{
					domHelper.DomInstances.Delete(capabilityValue.Instance);
				}
			}

			domHelper.DomInstances.Delete(capability.Instance);
		}
		#endregion
	}
}
