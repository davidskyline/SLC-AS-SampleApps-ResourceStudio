namespace Script.IAS.Dialogs.ManageCapacities
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class CapacitySection : ScriptSection
	{
		public CapacitySection()
		{
			InitWidgets();
		}

		#region Properties
		public DropDown CapacityDropDown { get; set; }

		public Button ConfigureValuesButton { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(CapacityDropDown, Layout.RowPosition, 0);
			AddWidget(ConfigureValuesButton, Layout.RowPosition, 1);
			AddWidget(DeleteButton, Layout.RowPosition, 2);
		}

		private void InitWidgets()
		{
			CapacityDropDown = new DropDown { Width = 150 };

			ConfigureValuesButton = new Button("Values...") { Width = 150, Height = 25 };

			DeleteButton = new Button("X") { Width = 25, Height = 25 };
		}
		#endregion
	}
}
