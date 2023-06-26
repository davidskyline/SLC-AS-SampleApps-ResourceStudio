namespace Skyline.Automation.IAS.Dialogs.ErrorDialog
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class ErrorDialogView : ScriptDialog
	{
		public ErrorDialogView(IEngine engine)
			: base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label ErrorMessage { get; set; }

		public Button CloseButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Script execution failed";

			AddWidget(ErrorMessage, Layout.RowPosition, 0);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CloseButton, ++Layout.RowPosition, 0);
		}

		private void InitWidgets()
		{
			ErrorMessage = new Label();

			CloseButton = new Button("Close") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
