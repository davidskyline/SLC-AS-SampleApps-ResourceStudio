namespace Script.IAS.Dialogs.ResultDetails
{
	using System.Collections.Generic;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ResultDetailsView : ScriptDialog
	{
		public ResultDetailsView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label DialogLabel { get; private set; }

		public List<DetailsSection> Details { get; private set; }

		public Button PreviousButton { get; private set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Synchronize Application";

			AddWidget(DialogLabel, Layout.RowPosition, 0, 1, 2);

			foreach (var detail in Details)
			{
				AddSection(detail, ++Layout.RowPosition, 0);
			}

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(PreviousButton, ++Layout.RowPosition, 0);
		}

		private void InitWidgets()
		{
			DialogLabel = new Label { Style = TextStyle.Bold };

			Details = new List<DetailsSection>();

			PreviousButton = new Button("Previous") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
