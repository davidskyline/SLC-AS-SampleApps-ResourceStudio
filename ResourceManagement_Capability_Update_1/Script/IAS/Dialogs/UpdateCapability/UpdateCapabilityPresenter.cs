namespace Script.IAS.Dialogs.UpdateCapability
{
	using System;
	using System.Linq;

	public class UpdateCapabilityPresenter
	{
		#region Fields
		private readonly UpdateCapabilityView view;

		private readonly ScriptData model;
		#endregion

		public UpdateCapabilityPresenter(UpdateCapabilityView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;

		public event EventHandler<EventArgs> DeleteNotPossible;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.CapabilityNameLabel.Text = model.Name;
			view.CapabilityTypeLabel.Text = model.Type;

			if (model.Type == "Text")
			{
				view.UpdateButton.IsEnabled = false;
			}

			foreach (var discrete in model.Discretes)
			{
				var section = new DiscreteSection();
				section.DiscreteTextBox.Text = discrete;
				section.DeleteButton.Pressed += OnDeleteDiscreteButtonPressed;

				view.Discretes.Add(section);
			}
		}

		public void BuildView()
		{
			foreach (var section in view.Discretes)
			{
				section.Build();
			}

			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.UpdateButton.Pressed += OnUpdateButtonPressed;
			view.DeleteButton.Pressed += OnDeleteButtonPressed;

			view.AddDiscreteButton.Pressed += OnAddDiscreteButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnUpdateButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.UpdateCapability();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnDeleteButtonPressed(object sender, EventArgs e)
		{
			if (model.ResourcePoolsImplementingCapability.Any())
			{
				DeleteNotPossible?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				model.DeleteCapability();

				Close?.Invoke(this, EventArgs.Empty);
			}
		}

		private void OnAddDiscreteButtonPressed(object sender, EventArgs e)
		{
			var section = new DiscreteSection();
			section.DeleteButton.Pressed += OnDeleteDiscreteButtonPressed;

			view.Discretes.Add(section);

			BuildView();
		}

		private void OnDeleteDiscreteButtonPressed(object sender, EventArgs e)
		{
			var section = view.Discretes.Single(x => x.DeleteButton == sender);

			view.Discretes.Remove(section);

			BuildView();
		}

		private void StoreToModel()
		{
			if (view.CapabilityTypeLabel.Text == "Discrete")
			{
				model.Discretes = view.Discretes.Where(x => !string.IsNullOrWhiteSpace(x.DiscreteTextBox.Text)).Select(x => x.DiscreteTextBox.Text).Distinct().ToList();
			}
		}
		#endregion
	}
}
