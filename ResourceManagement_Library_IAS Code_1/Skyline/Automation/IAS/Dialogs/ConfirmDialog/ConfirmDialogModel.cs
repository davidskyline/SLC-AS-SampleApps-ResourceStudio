namespace Skyline.Automation.IAS.Dialogs.ConfirmDialog
{
	internal class ConfirmDialogModel
	{
		private readonly string confirmationMessage;

		public ConfirmDialogModel(string confirmationMessage)
		{
			this.confirmationMessage = confirmationMessage;
		}

		public string ConfirmationMessage => confirmationMessage;
	}
}
