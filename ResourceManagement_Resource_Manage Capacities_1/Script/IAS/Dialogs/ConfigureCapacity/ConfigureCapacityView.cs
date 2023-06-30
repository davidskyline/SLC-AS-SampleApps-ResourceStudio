namespace Script.IAS.Dialogs.ConfigureCapacity
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfigureCapacityView : ScriptDialog
	{
		public ConfigureCapacityView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label CapacityNameLabel { get; set; }

		public Label UnitsLabel { get; set; }

		public Numeric ValueNumeric { get; set; }

		public Button ApplyButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Manage Capacities";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(CapacityNameLabel, Layout.RowPosition, 1);

			AddWidget(new Label("Units"), ++Layout.RowPosition, 0);
			AddWidget(UnitsLabel, Layout.RowPosition, 1);

			AddWidget(new Label("Value"), ++Layout.RowPosition, 0);
			AddWidget(ValueNumeric, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(ApplyButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			CapacityNameLabel = new Label();

			UnitsLabel = new Label();

			ValueNumeric = new Numeric();

			ApplyButton = new Button("Apply") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
