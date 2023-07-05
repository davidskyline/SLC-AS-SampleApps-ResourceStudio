namespace Skyline.Automation.IAS.Dialogs.ErrorDialog
{
	internal class ErrorDialogModel
	{
		private readonly string errorMessage;

		public ErrorDialogModel(string errorMessage)
		{
			this.errorMessage = errorMessage;
		}

		public string ErrorMessage
		{
			get { return errorMessage; }
		}
	}
}
