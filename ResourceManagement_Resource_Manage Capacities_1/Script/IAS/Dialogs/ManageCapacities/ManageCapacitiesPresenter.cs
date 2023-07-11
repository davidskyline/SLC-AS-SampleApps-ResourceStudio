namespace Script.IAS.Dialogs.ManageCapacities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.IAS;

	public class ManageCapacitiesPresenter
	{
		#region Fields
		private readonly ManageCapacitiesView view;

		private readonly ScriptData model;

		private Dictionary<string, CapacityData> capacitiesByName;
		#endregion

		public ManageCapacitiesPresenter(ManageCapacitiesView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;

		public event EventHandler<EventArgs> Configure;

		public event EventHandler<EventArgs> UpdateNotPossible;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			capacitiesByName = model.Capacities.ToDictionary(x => x.Name, x => x);

			InitWidgets();
		}

		public void BuildView()
		{
			foreach (var section in view.Capacities)
			{
				section.Build();
			}

			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.UpdateButton.Pressed += OnUpdateButtonPressed;

			view.AddCapacityButton.Pressed += OnAddCapacityButtonPressed;
		}

		private void InitWidgets()
		{
			view.ResourceName.Text = model.ResourceName;

			foreach (var capacity in model.ConfiguredCapacities.Select(x => x.CapacityData))
			{
				AddCapacitySection(capacity.Name, true);
			}
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnUpdateButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();

			var result = model.TryUpdateResourceCapacities();
			if (result.Succeeded)
			{
				Close?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				UpdateNotPossible?.Invoke(this, EventArgs.Empty);
			}
		}

		private void OnAddCapacityButtonPressed(object sender, EventArgs e)
		{
			AddCapacitySection(AutomationData.InitialDropdownValue, false);

			BuildView();
		}

		private void OnDeleteCapacityButtonPressed(object sender, EventArgs e)
		{
			var section = view.Capacities.Single(x => x.DeleteButton == sender);
			view.Capacities.Remove(section);

			UpdateCapacityOptions();

			BuildView();
		}

		private void OnCapacityDropDownChanged(object sender, EventArgs e)
		{
			var section = view.Capacities.Single(x => x.CapacityDropDown == sender);
			if (section.CapacityDropDown.Selected == AutomationData.InitialDropdownValue)
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
			var section = view.Capacities.Single(x => x.ConfigureValuesButton == sender);
			var capacityData = capacitiesByName[section.CapacityDropDown.Selected];

			model.SetCapacityToConfigure(capacityData);

			Configure?.Invoke(this, EventArgs.Empty);
		}

		private void AddCapacitySection(string selected, bool isEnabled)
		{
			var section = new CapacitySection();
			section.CapacityDropDown.SetOptions(ComposeCapacityOptions(selected));
			section.CapacityDropDown.Selected = selected;
			section.CapacityDropDown.Changed += OnCapacityDropDownChanged;

			section.ConfigureValuesButton.IsEnabled = isEnabled;
			section.ConfigureValuesButton.Pressed += OnConfigureValuesbuttonPressed;

			section.DeleteButton.Pressed += OnDeleteCapacityButtonPressed;

			view.Capacities.Add(section);
		}

		private List<string> ComposeCapacityOptions(string selected)
		{
			var options = capacitiesByName.Keys.ToList();

			view.Capacities.ForEach(x =>
			{
				if (x.CapacityDropDown.Selected != AutomationData.InitialDropdownValue && x.CapacityDropDown.Selected != selected)
				{
					options.Remove(x.CapacityDropDown.Selected);
				}
			});

			options.Sort();
			options.Insert(0, AutomationData.InitialDropdownValue);

			return options;
		}

		private void UpdateCapacityOptions()
		{
			foreach (var section in view.Capacities)
			{
				section.CapacityDropDown.SetOptions(ComposeCapacityOptions(section.CapacityDropDown.Selected));
			}
		}

		private void StoreToModel()
		{
			var capacities = new List<CapacityData>();
			view.Capacities.ForEach(x =>
			{
				if (x.CapacityDropDown.Selected != AutomationData.InitialDropdownValue)
				{
					capacities.Add(capacitiesByName[x.CapacityDropDown.Selected]);
				}
			});

			model.SetConfiguredCapacities(capacities);
		}
		#endregion
	}
}
