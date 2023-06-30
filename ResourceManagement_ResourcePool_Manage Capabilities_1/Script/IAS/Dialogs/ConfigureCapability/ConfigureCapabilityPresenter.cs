namespace Script.IAS.Dialogs.ConfigureCapability
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.IAS;

	public class ConfigureCapabilityPresenter
	{
		#region Fields
		private readonly ConfigureCapabilityView view;

		private readonly ScriptData model;

		//private Dictionary<string, CapabilityValueData> capabilityValuesByName;

		private List<string> capabilityValues;
		#endregion

		public ConfigureCapabilityPresenter(ConfigureCapabilityView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			capabilityValues = model.GetCapabilityValuesByCapabilityId(model.CapabilityToConfigure.CapabilityData.Instance.ID.Id);

			InitWidgets();
		}

		public void BuildView()
		{
			if (view.CapabilityTypeLabel.Text == "Discrete")
			{
				foreach (var section in view.Discretes)
				{
					section.Build();
				}
			}

			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.ApplyButton.Pressed += OnApplyButtonPressed;

			view.AddDiscreteButton.Pressed += OnAddDiscreteButtonPressed;
		}

		private void InitWidgets()
		{
			view.TextValueTextBox.Text = string.Empty;
			view.Discretes.Clear();

			view.CapabilityNameLabel.Text = model.CapabilityToConfigure.CapabilityData.Name;

			switch (model.CapabilityToConfigure.CapabilityData.CapabilityType)
			{
				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String:
					view.CapabilityTypeLabel.Text = "Text";
					view.TextValueTextBox.Text = ((ConfiguredStringCapability)model.CapabilityToConfigure).Value;

					break;
				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.Enum:
					view.CapabilityTypeLabel.Text = "Discrete";

					foreach (var discrete in ((ConfiguredEnumCapability)model.CapabilityToConfigure).Discretes)
					{
						AddDiscreteSection(discrete);
					}

					break;

				default:
					// Do nothing
					break;
			}
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnApplyButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnAddDiscreteButtonPressed(object sender, EventArgs e)
		{
			AddDiscreteSection(AutomationData.InitialDropdownValue);

			BuildView();
		}

		private void OnDeleteDiscreteButtonPressed(object sender, EventArgs e)
		{
			var section = view.Discretes.Single(x => x.DeleteButton == sender);
			view.Discretes.Remove(section);

			UpdateDiscreteOptions();

			BuildView();
		}

		private void OnDiscreteDropDownChanged(object sender, EventArgs e)
		{
			UpdateDiscreteOptions();
		}

		private void AddDiscreteSection(string selected)
		{
			var section = new DiscreteSection();
			section.DiscreteDropDown.SetOptions(ComposeDiscreteOptions(selected));
			section.DiscreteDropDown.Selected = selected;
			section.DiscreteDropDown.Changed += OnDiscreteDropDownChanged;

			section.DeleteButton.Pressed += OnDeleteDiscreteButtonPressed;

			view.Discretes.Add(section);
		}

		private List<string> ComposeDiscreteOptions(string selected)
		{
			var options = new List<string>(capabilityValues);

			view.Discretes.ForEach(x =>
			{
				if (x.DiscreteDropDown.Selected != AutomationData.InitialDropdownValue && x.DiscreteDropDown.Selected != selected)
				{
					options.Remove(x.DiscreteDropDown.Selected);
				}
			});

			options.Sort();
			options.Insert(0, AutomationData.InitialDropdownValue);

			return options;
		}

		private void UpdateDiscreteOptions()
		{
			foreach (var section in view.Discretes)
			{
				section.DiscreteDropDown.SetOptions(ComposeDiscreteOptions(section.DiscreteDropDown.Selected));
			}
		}

		private void StoreToModel()
		{
			switch (model.CapabilityToConfigure.CapabilityData.CapabilityType)
			{
				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String:
					((ConfiguredStringCapability)model.CapabilityToConfigure).Value = view.TextValueTextBox.Text;

					break;

				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.Enum:
					((ConfiguredEnumCapability)model.CapabilityToConfigure).Discretes = view.Discretes.Where(x => x.DiscreteDropDown.Selected != AutomationData.InitialDropdownValue).Select(x => x.DiscreteDropDown.Selected).ToList();

					break;

				default:
					// Do nothing
					break;
			}
		}
		#endregion
	}
}
