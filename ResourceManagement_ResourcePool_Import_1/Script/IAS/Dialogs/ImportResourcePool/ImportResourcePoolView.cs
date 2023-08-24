namespace Script.IAS.Dialogs.ImportResourcePool
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ImportResourcePoolView : ScriptDialog
	{
		public ImportResourcePoolView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public DropDown ResourcePoolDropDown { get; set; }

		public Button CancelButton { get; set; }

		public Button ImportButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Import Resource Pool";

			AddWidget(new Label("Resource Pool"), Layout.RowPosition, 0);
			AddWidget(ResourcePoolDropDown, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(ImportButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			ResourcePoolDropDown = new DropDown { Width = 250 };

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			ImportButton = new Button("Import") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
