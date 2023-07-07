namespace Skyline.Automation.IAS.Dialogs.ConfirmDialog
{
	using System;

	internal class ConfirmDialogPresenter
	{
		#region Fields
		private readonly ConfirmDialogView view;

		private readonly ConfirmDialogModel model;
		#endregion

		public ConfirmDialogPresenter(ConfirmDialogView view, ConfirmDialogModel model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Confirm;

		public event EventHandler<EventArgs> Cancel;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.ConfirmationMessage.Text = WrapText.Wrap(model.ConfirmationMessage, 100);
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.ConfirmButton.Pressed += OnConfirmButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Cancel?.Invoke(this, EventArgs.Empty);
		}

		private void OnConfirmButtonPressed(object sender, EventArgs e)
		{
			Confirm?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
