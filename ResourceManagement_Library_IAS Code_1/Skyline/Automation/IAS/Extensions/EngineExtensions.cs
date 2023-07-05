namespace Skyline.Automation.IAS
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public static class EngineExtensions
	{
		public static void ShowErrorDialog(this IEngine engine, string message)
		{
			var model = new Dialogs.ErrorDialog.ErrorDialogModel(message);
			var view = new Dialogs.ErrorDialog.ErrorDialogView(engine);
			var presenter = new Dialogs.ErrorDialog.ErrorDialogPresenter(view, model);

			presenter.Close += (sender, arg) =>
			{
				engine.ExitSuccess(string.Empty);
			};

			presenter.LoadFromModel();
			presenter.BuildView();

			view.Show();
		}

		public static bool ShowConfirmDialog(this IEngine engine, string message)
		{
			var model = new Dialogs.ConfirmDialog.ConfirmDialogModel(message);
			var view = new Dialogs.ConfirmDialog.ConfirmDialogView(engine);
			var presenter = new Dialogs.ConfirmDialog.ConfirmDialogPresenter(view, model);

			var confirmed = false;
			presenter.Cancel += (sender, arg) =>
			{
				confirmed = false;
			};
			presenter.Confirm += (sender, arg) =>
			{
				confirmed = true;
			};

			presenter.LoadFromModel();
			presenter.BuildView();

			view.Show();

			return confirmed;
		}
	}
}
