namespace Skyline.Automation.IAS.Dialogs.ConfirmDialog
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class ConfirmDialogView : ScriptDialog
	{
		public ConfirmDialogView(IEngine engine)
			: base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label ConfirmationMessage { get; set; }

		public Button CancelButton { get; set; }

		public Button ConfirmButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Confirmation required";

			AddWidget(ConfirmationMessage, Layout.RowPosition, 0, 1, 2);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(ConfirmButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			ConfirmationMessage = new Label();

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			ConfirmButton = new Button("Confirm") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
