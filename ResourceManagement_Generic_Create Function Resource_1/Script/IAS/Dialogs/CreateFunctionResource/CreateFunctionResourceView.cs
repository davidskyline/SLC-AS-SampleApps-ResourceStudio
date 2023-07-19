namespace Script.IAS.Dialogs.CreateFunctionResource
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class CreateFunctionResourceView : ScriptDialog
	{
		public CreateFunctionResourceView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label ResourceNameLabel { get; set; }

		public DropDown FunctionDropDown { get; set; }

		public DropDown ElementDropDown { get; set; }

		public CheckBox ShowAvailableElementsCheckBox { get; set; }

		public DropDown TableIndexDropDown { get; set; }

		public Button CancelButton { get; set; }

		public Button CreateButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Create Function Resource";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(ResourceNameLabel, Layout.RowPosition, 1);

			AddWidget(new Label("Function"), ++Layout.RowPosition, 0);
			AddWidget(FunctionDropDown, Layout.RowPosition, 1);

			AddWidget(new Label("Element"), ++Layout.RowPosition, 0);
			AddWidget(ElementDropDown, Layout.RowPosition, 1);
			AddWidget(ShowAvailableElementsCheckBox, Layout.RowPosition, 2);

			AddWidget(new Label("Table Index"), ++Layout.RowPosition, 0);
			AddWidget(TableIndexDropDown, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(CreateButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 260);
		}

		private void InitWidgets()
		{
			ResourceNameLabel = new Label();

			FunctionDropDown = new DropDown { Width = 250 };
			ElementDropDown = new DropDown { Width = 250 };
			TableIndexDropDown = new DropDown { Width = 250 };

			ShowAvailableElementsCheckBox = new CheckBox("Available only");

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			CreateButton = new Button("Create") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
