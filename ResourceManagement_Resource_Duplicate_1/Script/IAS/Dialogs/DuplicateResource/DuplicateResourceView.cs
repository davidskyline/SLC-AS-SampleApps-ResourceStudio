namespace Script.IAS.Dialogs.DuplicateResource
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DuplicateResourceView : ScriptDialog
	{
		public DuplicateResourceView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label ResourceNameLabel { get; set; }

		public Numeric NumberOfDuplicatesNumeric { get; set; }

		public Button CancelButton { get; set; }

		public Button DuplicateButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Duplicate Resource";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(ResourceNameLabel, Layout.RowPosition, 1);

			AddWidget(new Label("# Duplicates"), ++Layout.RowPosition, 0);
			AddWidget(NumberOfDuplicatesNumeric, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(DuplicateButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 260);
		}

		private void InitWidgets()
		{
			ResourceNameLabel = new Label();

			NumberOfDuplicatesNumeric = new Numeric(1) { Width = 150, Minimum = 1, Maximum = 100 };

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			DuplicateButton = new Button("Duplicate") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
