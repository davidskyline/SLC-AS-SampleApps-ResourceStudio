namespace Script.IAS.Dialogs.ManageCapabilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ManageCapabilitiesView : ScriptDialog
	{
		public ManageCapabilitiesView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label PoolName { get; set; }

		public List<CapabilitySection> Capabilities { get; set; }

		public Button AddCapabilityButton { get; set; }

		public Button UpdateButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "Manage Capabilities";

			AddWidget(new Label("Pool Name"), Layout.RowPosition, 0);
			AddWidget(PoolName, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 10 }, ++Layout.RowPosition, 0);

			AddWidget(new Label("Capabilities") { Style = TextStyle.Bold }, ++Layout.RowPosition, 0, 1, 2);

			foreach (var capability in Capabilities)
			{
				AddSection(capability, ++Layout.RowPosition, 0);
			}

			AddWidget(AddCapabilityButton, ++Layout.RowPosition, 0);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(UpdateButton, Layout.RowPosition, 1);
		}

		private void InitWidgets()
		{
			PoolName = new Label();

			Capabilities= new List<CapabilitySection>();

			AddCapabilityButton = new Button("+") { Width = 25, Height = 25 };

			UpdateButton = new Button("Update") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
