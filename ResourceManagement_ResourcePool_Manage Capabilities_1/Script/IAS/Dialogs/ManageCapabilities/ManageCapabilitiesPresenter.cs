namespace Script.IAS.Dialogs.ManageCapabilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.IAS;

	public class ManageCapabilitiesPresenter
	{
		#region Fields
		private readonly ManageCapabilitiesView view;

		private readonly ScriptData model;

		private Dictionary<string, CapabilityData> capabilitiesByName;
		#endregion

		public ManageCapabilitiesPresenter(ManageCapabilitiesView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;

		public event EventHandler<EventArgs> Configure;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			capabilitiesByName = model.Capabilities.ToDictionary(x => x.Name, x => x);

			InitWidgets();
		}

		public void BuildView()
		{
			foreach (var section in view.Capabilities)
			{
				section.Build();
			}

			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.UpdateButton.Pressed += OnUpdateButtonPressed;

			view.AddCapabilityButton.Pressed += OnAddCapabilityButtonPressed;
		}

		private void InitWidgets()
		{
			view.PoolName.Text = model.PoolName;

			foreach (var capability in model.ConfiguredCapabilities.Select(x => x.CapabilityData))
			{
				AddCapabilitySection(capability.Name, true);
			}
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnUpdateButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.UpdatePoolCapabilities();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnAddCapabilityButtonPressed(object sender, EventArgs e)
		{
			AddCapabilitySection(AutomationData.InitialDropdownValue, false);

			BuildView();
		}

		private void OnDeleteCapabilityButtonPressed(object sender, EventArgs e)
		{
			var section = view.Capabilities.Single(x => x.DeleteButton == sender);
			view.Capabilities.Remove(section);

			UpdateCapacityOptions();

			BuildView();
		}

		private void OnCapabilityDropDownChanged(object sender, EventArgs e)
		{
			var section = view.Capabilities.Single(x => x.CapabilityDropDown == sender);
			if (section.CapabilityDropDown.Selected == AutomationData.InitialDropdownValue)
			{
				section.ConfigureValuesButton.IsEnabled = false;
			}
			else
			{
				section.ConfigureValuesButton.IsEnabled = true;
			}

			UpdateCapacityOptions();
		}

		private void OnConfigureValuesbuttonPressed(object sender, EventArgs e)
		{
			var section = view.Capabilities.Single(x => x.ConfigureValuesButton == sender);
			var capabilityData = capabilitiesByName[section.CapabilityDropDown.Selected];

			model.SetCapabilityToConfigure(capabilityData);

			Configure?.Invoke(this, EventArgs.Empty);
		}

		private void AddCapabilitySection(string selected, bool isEnabled)
		{
			var section = new CapabilitySection();
			section.CapabilityDropDown.SetOptions(ComposeCapabilityOptions(selected));
			section.CapabilityDropDown.Selected = selected;
			section.CapabilityDropDown.Changed += OnCapabilityDropDownChanged;

			section.ConfigureValuesButton.IsEnabled = isEnabled;
			section.ConfigureValuesButton.Pressed += OnConfigureValuesbuttonPressed;

			section.DeleteButton.Pressed += OnDeleteCapabilityButtonPressed;

			view.Capabilities.Add(section);
		}

		private List<string> ComposeCapabilityOptions(string selected)
		{
			var options = capabilitiesByName.Keys.ToList();

			view.Capabilities.ForEach(x =>
			{
				if (x.CapabilityDropDown.Selected != AutomationData.InitialDropdownValue && x.CapabilityDropDown.Selected != selected)
				{
					options.Remove(x.CapabilityDropDown.Selected);
				}
			});

			options.Sort();
			options.Insert(0, AutomationData.InitialDropdownValue);

			return options;
		}

		private void UpdateCapacityOptions()
		{
			foreach (var section in view.Capabilities)
			{
				section.CapabilityDropDown.SetOptions(ComposeCapabilityOptions(section.CapabilityDropDown.Selected));
			}
		}

		private void StoreToModel()
		{
			var capabilities = new List<CapabilityData>();
			view.Capabilities.ForEach(x =>
			{
				if (x.CapabilityDropDown.Selected != AutomationData.InitialDropdownValue)
				{
					capabilities.Add(capabilitiesByName[x.CapabilityDropDown.Selected]);
				}
			});

			model.SetConfiguredCapabilities(capabilities);
		}
		#endregion
	}
}
