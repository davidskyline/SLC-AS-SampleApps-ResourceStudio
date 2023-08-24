namespace Script.IAS.Dialogs.ImportResourcePool
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.IAS;

	public class ImportResourcePoolPresenter
	{
		#region Fields
		private readonly ImportResourcePoolView view;

		private readonly ScriptData model;
		#endregion

		public ImportResourcePoolPresenter(ImportResourcePoolView view, ScriptData model)
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
			InitWidgets();
		}

		public void BuildView()
		{
			view.Build();
		}

		private static List<string> ComposeDefaultOptions(List<string> options)
		{
			var defaultOptions = new List<string>(options);

			defaultOptions.Sort();
			defaultOptions.Insert(0, AutomationData.InitialDropdownValue);

			return defaultOptions;
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.ImportButton.Pressed += OnImportButtonPressed;

			view.ResourcePoolDropDown.Changed += OnResourcePoolDropDownChanged;
		}

		private void InitWidgets()
		{
			view.ResourcePoolDropDown.SetOptions(ComposeDefaultOptions(model.ResourcePools.ToList()));
			view.ResourcePoolDropDown.Selected = AutomationData.InitialDropdownValue;

			view.ImportButton.IsEnabled = false;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnImportButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.Import();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnResourcePoolDropDownChanged(object sender, EventArgs e)
		{
			if (view.ResourcePoolDropDown.Selected == AutomationData.InitialDropdownValue)
			{
				view.ImportButton.IsEnabled = false;

				return;
			}

			view.ImportButton.IsEnabled = true;
		}

		private void StoreToModel()
		{
			model.SelectedResourcePool = view.ResourcePoolDropDown.Selected;
		}
		#endregion
	}
}
