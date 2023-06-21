namespace Script.IAS.Dialogs.UpdateCapability
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DiscreteSection : ScriptSection
	{
		public DiscreteSection()
		{
			InitWidgets();
		}

		#region Properties
		public TextBox DiscreteTextBox { get; set; }

		public Button DeleteButton { get; set; }

		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(DiscreteTextBox, Layout.RowPosition, 0);
			AddWidget(DeleteButton, Layout.RowPosition, 1);
		}

		private void InitWidgets()
		{
			DiscreteTextBox = new TextBox { Width = 250 };

			DeleteButton = new Button("X") { Width = 25, Height = 25 };
		}
		#endregion
	}
}
