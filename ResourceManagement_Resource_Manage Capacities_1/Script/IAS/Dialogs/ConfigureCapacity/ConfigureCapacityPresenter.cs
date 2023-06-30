namespace Script.IAS.Dialogs.ConfigureCapacity
{
	using System;

	public class ConfigureCapacityPresenter
	{
		#region Fields
		private readonly ConfigureCapacityView view;

		private readonly ScriptData model;
		#endregion

		public ConfigureCapacityPresenter(ConfigureCapacityView view, ScriptData model)
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

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.ApplyButton.Pressed += OnApplyButtonPressed;
		}

		private void InitWidgets()
		{
			view.CapacityNameLabel.Text = model.CapacityToConfigure.CapacityData.Name;
			view.UnitsLabel.Text = model.CapacityToConfigure.CapacityData.Units;

			view.ValueNumeric = new Skyline.DataMiner.Utils.InteractiveAutomationScript.Numeric();
			view.ValueNumeric.Value = model.CapacityToConfigure.Value;

			if (model.CapacityToConfigure.CapacityData.RangeMin != null)
			{
				view.ValueNumeric.Minimum = (double)model.CapacityToConfigure.CapacityData.RangeMin;
			}

			if (model.CapacityToConfigure.CapacityData.RangeMax != null)
			{
				view.ValueNumeric.Maximum = (double)model.CapacityToConfigure.CapacityData.RangeMax;
			}

			if (model.CapacityToConfigure.CapacityData.StepSize != null)
			{
				view.ValueNumeric.StepSize = (double)model.CapacityToConfigure.CapacityData.StepSize;
			}

			if (model.CapacityToConfigure.CapacityData.Decimals != null)
			{
				view.ValueNumeric.Decimals = (int)model.CapacityToConfigure.CapacityData.Decimals;
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

		private void StoreToModel()
		{
			model.CapacityToConfigure.Value = view.ValueNumeric.Value;
		}
		#endregion
	}
}
