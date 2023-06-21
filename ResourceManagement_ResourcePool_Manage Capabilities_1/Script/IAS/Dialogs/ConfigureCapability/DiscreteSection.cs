namespace Script.IAS.Dialogs.ConfigureCapability
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DiscreteSection : ScriptSection
	{
		public DiscreteSection()
		{
			InitWidgets();
		}

		#region Properties
		public DropDown DiscreteDropDown { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(DiscreteDropDown, Layout.RowPosition, 0);
			AddWidget(DeleteButton, Layout.RowPosition, 2);
		}

		private void InitWidgets()
		{
			DiscreteDropDown = new DropDown { Width = 150 };

			DeleteButton = new Button("X") { Width = 25, Height = 25 };
		}
		#endregion
	}
}
