namespace Script.IAS
{
	using System;
	using System.Collections.Generic;

	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private bool isDiscreteType;

		private List<string> discretes;
		#endregion

		public ScriptData(IEngine engine)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
		}

		#region Properties
		public string Name { get; set; }

		public string SelectedType { get; set; }

		public List<string> Discretes { get; set; }
		#endregion

		#region Methods
		public void AddCapability()
		{
			if (string.IsNullOrWhiteSpace(Name))
			{
				return;
			}

			isDiscreteType = SelectedType == "Discrete";
			if (isDiscreteType && Discretes.Count == 0)
			{
				return;
			}

			if (!TryCreateProfileParameter())
			{
				return;
			}

			CreateDomInstances();
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
				Categories = Skyline.DataMiner.Net.Profiles.ProfileParameterCategory.Capability,
			};

			if (isDiscreteType)
			{
				discretes = new List<string>(Discretes);
				Skyline.DataMiner.Net.NaturalSortComparer comparer = new Skyline.DataMiner.Net.NaturalSortComparer();
				discretes.Sort((x, y) => comparer.Compare(x, y));

				profileParameter.Type = Skyline.DataMiner.Net.Profiles.Parameter.ParameterType.Discrete;
				profileParameter.InterpreteType = new Skyline.DataMiner.Net.Profiles.InterpreteType
				{
					Type = Skyline.DataMiner.Net.Profiles.InterpreteType.TypeEnum.String,
					RawType = Skyline.DataMiner.Net.Profiles.InterpreteType.RawTypeEnum.Other,
				};
				profileParameter.Discretes = discretes;
				profileParameter.DiscreetDisplayValues = discretes;
			}
			else
			{
				profileParameter.Type = Skyline.DataMiner.Net.Profiles.Parameter.ParameterType.Text;
			}

			srmHelpers.ProfileHelper.ProfileParameters.Create(profileParameter);

			return true;
		}

		private void CreateDomInstances()
		{
			var domHelper = new DomHelper(engine.SendSLNetMessages, Skyline.Automation.DOM.DomIds.Resourcemanagement.ModuleId);

			var capabilityInfoSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityInfo.Id);
			capabilityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityInfo.CapabilityName, new ValueWrapper<string>(Name)));
			capabilityInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityInfo.CapabilityType, new ValueWrapper<int>(isDiscreteType ? (int)Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.Enum : (int)Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String)));

			var capabilityInstance = new DomInstance
			{
				DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Capability,
			};
			capabilityInstance.Sections.Add(capabilityInfoSection);
			capabilityInstance = domHelper.DomInstances.Create(capabilityInstance);

			if (!isDiscreteType)
			{
				return;
			}

			foreach (var discrete in discretes)
			{
				var capabilityEnumValueDetailsSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityEnumValueDetails.Id);
				capabilityEnumValueDetailsSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityEnumValueDetails.Capability, new ValueWrapper<Guid>(capabilityInstance.ID.Id)));
				capabilityEnumValueDetailsSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.CapabilityEnumValueDetails.Value, new ValueWrapper<string>(discrete)));

				var capabilityValueInstance = new DomInstance
				{
					DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Capabilityenumvalue,
				};
				capabilityValueInstance.Sections.Add(capabilityEnumValueDetailsSection);
				domHelper.DomInstances.Create(capabilityValueInstance);
			}
		}
		#endregion
	}
}
