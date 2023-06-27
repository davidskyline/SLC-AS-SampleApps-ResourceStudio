namespace Script.IAS.Dialogs.UpdateCapacity
{
	using System;

	public class UpdateCapacityPresenter
	{
		#region Fields
		private readonly UpdateCapacityView view;

		private readonly ScriptData model;
		#endregion

		public UpdateCapacityPresenter(UpdateCapacityView view, ScriptData model)
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
			view.NameLabel.Text = model.Name;
			view.UnitsTextBox.Text = model.Units;

			view.RangeMinCheckBox.IsChecked = model.IsRangeMinEnabled;
			view.RangeMinNumeric.IsEnabled = model.IsRangeMinEnabled;
			if (model.IsRangeMinEnabled)
			{
				view.RangeMinNumeric.Value = model.RangeMin;
			}

			view.RangeMaxCheckBox.IsChecked = model.IsRangeMaxEnabled;
			view.RangeMaxNumeric.IsEnabled = model.IsRangeMaxEnabled;
			if (model.IsRangeMaxEnabled)
			{
				view.RangeMaxNumeric.Value = model.RangeMax;
			}

			view.StepSizeCheckBox.IsChecked = model.IsStepSizeEnabled;
			view.StepSizeNumeric.IsEnabled = model.IsStepSizeEnabled;
			if (model.IsStepSizeEnabled)
			{
				view.StepSizeNumeric.Value = model.StepSize;
			}

			view.DecimalsCheckBox.IsChecked = model.IsDecimalsEnabled;
			view.DecimalsNumeric.IsEnabled = model.IsDecimalsEnabled;
			if (model.IsDecimalsEnabled)
			{
				view.DecimalsNumeric.Value = model.Decimals;
			}
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.UpdateButton.Pressed += OnUpdateButtonPressed;
			view.DeleteButton.Pressed += OnDeleteButtonPressed;

			view.RangeMinCheckBox.Changed += OnRangeMinCheckBoxChanged;
			view.RangeMaxCheckBox.Changed += OnRangeMaxCheckBoxChanged;

			view.StepSizeCheckBox.Changed += OnStepSizeCheckBoxChanged;
			view.StepSizeNumeric.Changed += OnStepSizeNumericChanged;

			view.DecimalsCheckBox.Changed += OnDecimalsCheckBoxChanged;
			view.DecimalsNumeric.Changed += OnDecimalsNumericChanged;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnUpdateButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.UpdateCapacity();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnDeleteButtonPressed(object sender, EventArgs e)
		{
			model.DeleteCapacity();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnRangeMinCheckBoxChanged(object sender, EventArgs e)
		{
			if (view.RangeMinCheckBox.IsChecked)
			{
				view.RangeMinNumeric.IsEnabled = true;
			}
			else
			{
				view.RangeMinNumeric.IsEnabled = false;
			}
		}

		private void OnRangeMaxCheckBoxChanged(object sender, EventArgs e)
		{
			if (view.RangeMaxCheckBox.IsChecked)
			{
				view.RangeMaxNumeric.IsEnabled = true;
			}
			else
			{
				view.RangeMaxNumeric.IsEnabled = false;
			}
		}

		private void OnStepSizeCheckBoxChanged(object sender, EventArgs e)
		{
			if (view.StepSizeCheckBox.IsChecked)
			{
				view.StepSizeNumeric.IsEnabled = true;
				ApplyStepSize(view.StepSizeNumeric.Value);
			}
			else
			{
				view.StepSizeNumeric.IsEnabled = false;
				ApplyStepSize(0.0);
			}
		}

		private void OnStepSizeNumericChanged(object sender, EventArgs e)
		{
			ApplyStepSize(view.StepSizeNumeric.Value);
		}

		private void OnDecimalsCheckBoxChanged(object sender, EventArgs e)
		{
			if (view.DecimalsCheckBox.IsChecked)
			{
				view.DecimalsNumeric.IsEnabled = true;
				ApplyDecimals(Convert.ToInt32(view.DecimalsNumeric.Value));
			}
			else
			{
				view.DecimalsNumeric.IsEnabled = false;
				ApplyDecimals(0);
			}
		}

		private void OnDecimalsNumericChanged(object sender, EventArgs e)
		{
			ApplyDecimals(Convert.ToInt32(view.DecimalsNumeric.Value));
		}

		private void ApplyStepSize(double value)
		{
			view.RangeMinNumeric.StepSize = value;
			view.RangeMaxNumeric.StepSize = value;
		}

		private void ApplyDecimals(int value)
		{
			view.RangeMinNumeric.Decimals = value;
			view.RangeMaxNumeric.Decimals = value;
			view.StepSizeNumeric.Decimals = value;
		}

		private void StoreToModel()
		{
			model.Units = view.UnitsTextBox.Text;

			model.IsRangeMinEnabled = view.RangeMinCheckBox.IsChecked;
			model.RangeMin = view.RangeMinNumeric.Value;

			model.IsRangeMaxEnabled = view.RangeMaxCheckBox.IsChecked;
			model.RangeMax = view.RangeMaxNumeric.Value;

			model.IsStepSizeEnabled = view.StepSizeCheckBox.IsChecked;
			model.StepSize = view.StepSizeNumeric.Value;

			model.IsDecimalsEnabled = view.DecimalsCheckBox.IsChecked;
			model.Decimals = Convert.ToInt32(view.DecimalsNumeric.Value);
		}
		#endregion
	}
}
