namespace Script.IAS.Dialogs.ResultOverview
{
	using System;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ResultSection : ScriptSection
	{
		#region Fields
		private readonly string reference;
		#endregion

		public ResultSection(string reference)
		{
			if (string.IsNullOrEmpty(reference))
			{
				throw new ArgumentNullException(nameof(reference));
			}

			this.reference = reference;

			InitWidgets();
		}

		#region Properties
		public string Reference => reference;

		public Label Result { get; private set; }

		public Button DetailsButton { get; private set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(Result, Layout.RowPosition, 0, 1, 2);

			AddWidget(DetailsButton, ++Layout.RowPosition, 0);
		}

		private void InitWidgets()
		{
			Result = new Label();

			DetailsButton = new Button("Details...") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
