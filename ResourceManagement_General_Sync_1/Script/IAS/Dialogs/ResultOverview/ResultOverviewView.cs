namespace Script.IAS.Dialogs.ResultOverview
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ResultOverviewView : ScriptDialog
	{
		public ResultOverviewView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public List<ResultSection> Results { get; private set; }

		public Button CancelButton { get; private set; }

		public Button CloseButton { get; private set; }

		public Button SynchronizeButton { get; private set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Synchronize Application";

			AddWidget(new Label("Result Overview") { Style = TextStyle.Bold }, Layout.RowPosition, 0, 1, 2);

			if (Results.Any())
			{
				foreach (var result in Results)
				{
					AddSection(result, ++Layout.RowPosition, 0);
					Layout.RowPosition += result.RowCount;
				}

				AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

				AddWidget(CancelButton, ++Layout.RowPosition, 0);
				AddWidget(SynchronizeButton, Layout.RowPosition, 1);
			}
			else
			{
				AddWidget(new Label("No items found which are not synchronized."), ++Layout.RowPosition, 0, 1, 2);

				AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

				AddWidget(CloseButton, ++Layout.RowPosition, 0);
			}

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 160);
		}

		private void InitWidgets()
		{
			Results = new List<ResultSection>();

			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			CloseButton = new Button("Close") { Width = 150, Height = 25 };
			SynchronizeButton = new Button("Synchronize") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
