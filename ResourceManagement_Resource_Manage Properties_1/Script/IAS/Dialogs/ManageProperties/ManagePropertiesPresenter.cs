namespace Script.IAS.Dialogs.ManageProperties
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.IAS;

	public class ManagePropertiesPresenter
	{
		#region Fields
		private readonly ManagePropertiesView view;

		private readonly ScriptData model;

		private Dictionary<string, PropertyData> propertiesByName;
		#endregion

		public ManagePropertiesPresenter(ManagePropertiesView view, ScriptData model)
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
			propertiesByName = model.Properties.ToDictionary(x => x.Name, x => x);

			InitWidgets();
		}

		public void BuildView()
		{
			foreach (var section in view.Properties)
			{
				section.Build();
			}

			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.UpdateButton.Pressed += OnUpdateButtonPressed;

			view.AddPropertyButton.Pressed += OnAddPropertyButtonPressed;
		}

		private void InitWidgets()
		{
			view.ResourceName.Text = model.ResourceName;

			foreach (var configuredProperty in model.ConfiguredProperties)
			{
				AddPropertySection(configuredProperty.PropertyData.Name, configuredProperty.Value);
			}
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnUpdateButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.UpdateResourceProperties();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnAddPropertyButtonPressed(object sender, EventArgs e)
		{
			AddPropertySection(AutomationData.InitialDropdownValue);

			BuildView();
		}

		private void OnDeletePropertyButtonPressed(object sender, EventArgs e)
		{
			var section = view.Properties.Single(x => x.DeleteButton == sender);
			view.Properties.Remove(section);

			UpdatePropertyOptions();

			BuildView();
		}

		private void OnPropertyDropDownChanged(object sender, EventArgs e)
		{
			var section = view.Properties.Single(x => x.PropertyDropDown == sender);
			if (section.PropertyDropDown.Selected == AutomationData.InitialDropdownValue)
			{
				section.PropertyValueTextBox.IsEnabled = false;
			}
			else
			{
				section.PropertyValueTextBox.IsEnabled = true;
			}

			UpdatePropertyOptions();
		}

		private void AddPropertySection(string selected, string value = null)
		{
			var section = new PropertySection();
			section.PropertyDropDown.SetOptions(ComposePropertyOptions(selected));
			section.PropertyDropDown.Selected = selected;
			section.PropertyDropDown.Changed += OnPropertyDropDownChanged;

			section.PropertyValueTextBox.IsEnabled = selected != AutomationData.InitialDropdownValue;
			if (value != null)
			{
				section.PropertyValueTextBox.Text = value;
			}

			section.DeleteButton.Pressed += OnDeletePropertyButtonPressed;

			view.Properties.Add(section);
		}

		private List<string> ComposePropertyOptions(string selected)
		{
			var options = propertiesByName.Keys.ToList();

			view.Properties.ForEach(x =>
			{
				if (x.PropertyDropDown.Selected != AutomationData.InitialDropdownValue && x.PropertyDropDown.Selected != selected)
				{
					options.Remove(x.PropertyDropDown.Selected);
				}
			});

			options.Sort();
			options.Insert(0, AutomationData.InitialDropdownValue);

			return options;
		}

		private void UpdatePropertyOptions()
		{
			foreach (var section in view.Properties)
			{
				section.PropertyDropDown.SetOptions(ComposePropertyOptions(section.PropertyDropDown.Selected));
			}
		}

		private void StoreToModel()
		{
			var configuredProperties = new List<ConfiguredProperty>();
			view.Properties.ForEach(x =>
			{
				if (x.PropertyDropDown.Selected != AutomationData.InitialDropdownValue && !string.IsNullOrWhiteSpace(x.PropertyValueTextBox.Text))
				{
					configuredProperties.Add(new ConfiguredProperty(propertiesByName[x.PropertyDropDown.Selected], x.PropertyValueTextBox.Text));
				}
			});

			model.SetConfiguredProperties(configuredProperties);
		}
		#endregion
	}
}
