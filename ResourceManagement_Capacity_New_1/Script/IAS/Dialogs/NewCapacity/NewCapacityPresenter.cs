namespace Script.IAS.Dialogs.NewCapacity
{
	using System;

	public class NewCapacityPresenter
	{
		#region Fields
		private readonly NewCapacityView view;

		private readonly ScriptData model;
		#endregion

		public NewCapacityPresenter(NewCapacityView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;

		public event EventHandler<EventArgs> NameInUse;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			// No data to load from model
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.AddButton.Pressed += OnAddButtonPressed;

			view.RangeMinCheckBox.IsChecked = false;
			view.RangeMinCheckBox.Changed += OnRangeMinCheckBoxChanged;
			view.RangeMinNumeric.IsEnabled = false;

			view.RangeMaxCheckBox.IsChecked = false;
			view.RangeMaxCheckBox.Changed += OnRangeMaxCheckBoxChanged;
			view.RangeMaxNumeric.IsEnabled = false;

			view.StepSizeCheckBox.IsChecked = false;
			view.StepSizeCheckBox.Changed += OnStepSizeCheckBoxChanged;
			view.StepSizeNumeric.IsEnabled = false;
			view.StepSizeNumeric.Changed += OnStepSizeNumericChanged;

			view.DecimalsCheckBox.IsChecked = false;
			view.DecimalsCheckBox.Changed += OnDecimalsCheckBoxChanged;
			view.DecimalsNumeric.IsEnabled = false;
			view.DecimalsNumeric.Changed += OnDecimalsNumericChanged;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnAddButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();

			if (model.IsNameInUse())
			{
				NameInUse?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				model.AddCapacity();

				Close?.Invoke(this, EventArgs.Empty);
			}
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
			model.Name = view.NameTextBox.Text;
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
