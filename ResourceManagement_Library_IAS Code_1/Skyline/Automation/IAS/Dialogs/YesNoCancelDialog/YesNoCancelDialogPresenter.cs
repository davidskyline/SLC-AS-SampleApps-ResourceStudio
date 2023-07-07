namespace Skyline.Automation.IAS.Dialogs.YesNoCancelDialog
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal class YesNoCancelDialogPresenter
	{
		#region Fields
		private readonly YesNoCancelDialogView view;

		private readonly YesNoCancelDialogModel model;
		#endregion

		public YesNoCancelDialogPresenter(YesNoCancelDialogView view, YesNoCancelDialogModel model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Cancel;

		public event EventHandler<EventArgs> Yes;

		public event EventHandler<EventArgs> No;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.Message.Text = WrapText.Wrap(model.Message, 100);
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.NoButton.Pressed += OnNoButtonPressed;
			view.YesButton.Pressed += OnYesButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Cancel?.Invoke(this, EventArgs.Empty);
		}

		private void OnNoButtonPressed(object sender, EventArgs e)
		{
			No?.Invoke(this, EventArgs.Empty);
		}

		private void OnYesButtonPressed(object sender, EventArgs e)
		{
			Yes?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
