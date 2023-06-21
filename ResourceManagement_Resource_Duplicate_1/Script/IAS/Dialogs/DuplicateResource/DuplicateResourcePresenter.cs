namespace Script.IAS.Dialogs.DuplicateResource
{
	using System;

	public class DuplicateResourcePresenter
	{
		#region Fields
		private readonly DuplicateResourceView view;

		private readonly ScriptData model;
		#endregion

		public DuplicateResourcePresenter(DuplicateResourceView view, ScriptData model)
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
			view.ResourceNameLabel.Text = model.ResourceName;
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.DuplicateButton.Pressed += OnDuplicateButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnDuplicateButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.DuplicateResource();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void StoreToModel()
		{
			model.NumberOfDuplicates = Convert.ToInt32(view.NumberOfDuplicatesNumeric.Value);
		}
		#endregion
	}
}
