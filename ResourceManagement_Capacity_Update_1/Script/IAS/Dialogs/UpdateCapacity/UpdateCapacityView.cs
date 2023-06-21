namespace Script.IAS.Dialogs.UpdateCapacity
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UpdateCapacityView : ScriptDialog
	{
		public UpdateCapacityView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label NameLabel { get; set; }

		public TextBox UnitsTextBox { get; set; }

		public CheckBox RangeMinCheckBox { get; set; }

		public Numeric RangeMinNumeric { get; set; }

		public CheckBox RangeMaxCheckBox { get; set; }

		public Numeric RangeMaxNumeric { get; set; }

		public CheckBox StepSizeCheckBox { get; set; }

		public Numeric StepSizeNumeric { get; set; }

		public CheckBox DecimalsCheckBox { get; set; }

		public Numeric DecimalsNumeric { get; set; }

		public Button UpdateButton { get; set; }

		public Button CancelButton { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "New Capacity";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(NameLabel, Layout.RowPosition, 1);

			AddWidget(new Label("Units"), ++Layout.RowPosition, 0);
			AddWidget(UnitsTextBox, Layout.RowPosition, 1);

			AddWidget(RangeMinCheckBox, ++Layout.RowPosition, 0);
			AddWidget(RangeMinNumeric, Layout.RowPosition, 1);

			AddWidget(RangeMaxCheckBox, ++Layout.RowPosition, 0);
			AddWidget(RangeMaxNumeric, Layout.RowPosition, 1);

			AddWidget(StepSizeCheckBox, ++Layout.RowPosition, 0);
			AddWidget(StepSizeNumeric, Layout.RowPosition, 1);

			AddWidget(DecimalsCheckBox, ++Layout.RowPosition, 0);
			AddWidget(DecimalsNumeric, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(UpdateButton, Layout.RowPosition, 1);

			AddWidget(DeleteButton, ++Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 260);
		}

		private void InitWidgets()
		{
			NameLabel = new Label();

			UnitsTextBox = new TextBox { Width = 250 };

			RangeMinCheckBox = new CheckBox("Min. Range");
			RangeMinNumeric = new Numeric { Width = 250 };

			RangeMaxCheckBox = new CheckBox("Max. Range");
			RangeMaxNumeric = new Numeric { Width = 250 };

			StepSizeCheckBox = new CheckBox("Step Size");
			StepSizeNumeric = new Numeric { Width = 250, Minimum = 0 };

			DecimalsCheckBox = new CheckBox("Decimals");
			DecimalsNumeric = new Numeric { Width = 250, Minimum = 0 };

			UpdateButton = new Button("Update") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			DeleteButton = new Button("Delete") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
