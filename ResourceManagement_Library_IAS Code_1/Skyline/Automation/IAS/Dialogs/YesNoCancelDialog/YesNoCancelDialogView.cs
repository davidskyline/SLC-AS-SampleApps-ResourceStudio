namespace Skyline.Automation.IAS.Dialogs.YesNoCancelDialog
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class YesNoCancelDialogView : ScriptDialog
	{
		public YesNoCancelDialogView(IEngine engine)
			: base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label Message { get; set; }

		public Button CancelButton { get; set; }

		public Button YesButton { get; set; }

		public Button NoButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();
			Width = 600;
			AllowOverlappingWidgets = true;

			Title = "Confirmation required";

			AddWidget(Message, Layout.RowPosition, 0, 1, 2);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(YesButton, Layout.RowPosition, 1);
			AddWidget(NoButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			Message = new Label();

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			YesButton = new Button("Yes") { Width = 70, Height = 25 };
			NoButton = new Button("No") { Width = 70, Height = 25, Margin = new Margin(75, 5, 5, 5) };
		}
		#endregion
	}
}
