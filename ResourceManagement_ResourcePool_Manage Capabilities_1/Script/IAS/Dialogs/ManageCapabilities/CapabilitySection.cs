namespace Script.IAS.Dialogs.ManageCapabilities
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class CapabilitySection : ScriptSection
	{
		public CapabilitySection()
		{
			InitWidgets();
		}

		#region Properties
		public DropDown CapabilityDropDown { get; set; }

		public Button ConfigureValuesButton { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(CapabilityDropDown, Layout.RowPosition, 0);
			AddWidget(ConfigureValuesButton, Layout.RowPosition, 1);
			AddWidget(DeleteButton, Layout.RowPosition, 2);
		}

		private void InitWidgets()
		{
			CapabilityDropDown = new DropDown { Width = 150 };

			ConfigureValuesButton = new Button("Values...") { Width = 150, Height = 25 };

			DeleteButton = new Button("X") { Width = 25, Height = 25 };
		}
		#endregion
	}
}
