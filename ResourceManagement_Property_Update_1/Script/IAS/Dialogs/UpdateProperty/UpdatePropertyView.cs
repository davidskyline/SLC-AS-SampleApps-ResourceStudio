namespace Script.IAS.Dialogs.UpdateProperty
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UpdatePropertyView : ScriptDialog
	{
		public UpdatePropertyView(IEngine engine): base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label PropertyNameLabel { get; set; }

		public Button CancelButton { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "Update Property";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(PropertyNameLabel, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(DeleteButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			PropertyNameLabel = new Label();

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			DeleteButton = new Button("Delete") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
