namespace Skyline.Automation.IAS
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public static class EngineExtensions
	{
		public static void ShowErrorDialog(this IEngine engine, string message)
		{
			var controller = new InteractiveController(engine);

			var model = new Dialogs.ErrorDialog.ErrorDialogModel(message);
			var view = new Dialogs.ErrorDialog.ErrorDialogView(engine);
			var presenter = new Dialogs.ErrorDialog.ErrorDialogPresenter(view, model);

			presenter.Close += (sender, arg) =>
			{
				engine.ExitSuccess(string.Empty);
			};

			presenter.LoadFromModel();
			presenter.BuildView();

			controller.Run(view);
		}
	}
}
