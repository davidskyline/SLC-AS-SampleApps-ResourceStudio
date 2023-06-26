namespace Script.IAS.Dialogs.ManageProperties
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Net.ElementProtocol;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class PropertySection : ScriptSection
	{
		public PropertySection()
		{
			InitWidgets();
		}

		#region Properties
		public DropDown PropertyDropDown { get; set; }

		public TextBox PropertyValueTextBox { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			AddWidget(PropertyDropDown, Layout.RowPosition, 0);
			AddWidget(PropertyValueTextBox, Layout.RowPosition, 1);
			AddWidget(DeleteButton, Layout.RowPosition, 2);
		}

		private void InitWidgets()
		{
			PropertyDropDown = new DropDown { Width = 150 };

			PropertyValueTextBox = new TextBox { Width = 150 };

			DeleteButton = new Button("X") { Width = 25, Height = 25 };
		}
		#endregion
	}
}
