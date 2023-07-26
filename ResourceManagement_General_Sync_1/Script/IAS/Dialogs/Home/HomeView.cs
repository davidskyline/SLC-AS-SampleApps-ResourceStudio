namespace Script.IAS.Dialogs.Home
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class HomeView : ScriptDialog
	{
		public HomeView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Button ValidateButton { get; private set; }

		public Button CancelButton { get; private set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Synchronize Application";

			AddWidget(new Label(WrapText.Wrap("This script will check if all data in the Resource Studio application is synchronized with the DMS and will provide an overview with all items that are not synchronized. The lookup can take some time depending of the number of items it needs to verify.", 100)), Layout.RowPosition, 0, 1, 3);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(ValidateButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 160);
		}

		private void InitWidgets()
		{
			ValidateButton = new Button("Validate...") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
