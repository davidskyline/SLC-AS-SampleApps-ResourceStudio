namespace Script.IAS.Dialogs.ManageCapacities
{
	using System.Collections.Generic;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ManageCapacitiesView : ScriptDialog
	{
		public ManageCapacitiesView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label ResourceName { get; set; }

		public List<CapacitySection> Capacities { get; set; }

		public Button AddCapacityButton { get; set; }

		public Button UpdateButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "Manage Capacities";

			AddWidget(new Label("Resource Name"), Layout.RowPosition, 0);
			AddWidget(ResourceName, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 10 }, ++Layout.RowPosition, 0);

			AddWidget(new Label("Capabilities") { Style = TextStyle.Bold }, ++Layout.RowPosition, 0, 1, 2);

			foreach (var capacity in Capacities)
			{
				AddSection(capacity, ++Layout.RowPosition, 0);
			}

			AddWidget(AddCapacityButton, ++Layout.RowPosition, 0);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(UpdateButton, Layout.RowPosition, 1);
		}

		private void InitWidgets()
		{
			ResourceName = new Label();

			Capacities = new List<CapacitySection>();

			AddCapacityButton = new Button("+") { Width = 25, Height = 25 };

			UpdateButton = new Button("Update") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
