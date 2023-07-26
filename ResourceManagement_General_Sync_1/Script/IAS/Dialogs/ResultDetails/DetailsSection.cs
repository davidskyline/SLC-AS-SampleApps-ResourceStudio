namespace Script.IAS.Dialogs.ResultDetails
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DetailsSection : ScriptSection
	{
		public DetailsSection()
		{
			InitWidgets();
		}

		#region Properties
		public Label Details { get; private set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(Details, Layout.RowPosition, 0);
		}

		private void InitWidgets()
		{
			Details = new Label();
		}
		#endregion
	}
}
