namespace Script.IAS.Dialogs.NewProperty
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NewPropertyView : ScriptDialog
	{
		public NewPropertyView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public TextBox PropertyNameTextBox { get; set; }

		public Button AddButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "New Property";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(PropertyNameTextBox, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(AddButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			PropertyNameTextBox = new TextBox { Width = 250 };

			AddButton = new Button("Add") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
