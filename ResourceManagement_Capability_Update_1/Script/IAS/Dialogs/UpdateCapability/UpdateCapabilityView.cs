namespace Script.IAS.Dialogs.UpdateCapability
{
	using System.Collections.Generic;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UpdateCapabilityView : ScriptDialog
	{
		public UpdateCapabilityView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label CapabilityNameLabel { get; set; }

		public Label CapabilityTypeLabel { get; set; }

		public List<DiscreteSection> Discretes { get; set; }

		public Button AddDiscreteButton { get; set; }

		public Button UpdateButton { get; set; }

		public Button CancelButton { get; set; }

		public Button DeleteButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "Update Capability";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(CapabilityNameLabel, Layout.RowPosition, 1);

			AddWidget(new Label("Type"), ++Layout.RowPosition, 0);
			AddWidget(CapabilityTypeLabel, Layout.RowPosition, 1);

			if (CapabilityTypeLabel.Text == "Discrete")
			{
				AddWidget(new WhiteSpace { Height = 10 }, ++Layout.RowPosition, 0);

				AddWidget(new Label("Discretes") { Style = TextStyle.Bold }, ++Layout.RowPosition, 1);

				foreach (var discrete in Discretes)
				{
					AddSection(discrete, ++Layout.RowPosition, 1);
				}

				AddWidget(AddDiscreteButton, ++Layout.RowPosition, 1);
			}

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(UpdateButton, Layout.RowPosition, 1);

			AddWidget(DeleteButton, ++Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 260);
		}

		private void InitWidgets()
		{
			CapabilityNameLabel = new Label();

			CapabilityTypeLabel = new Label();

			Discretes = new List<DiscreteSection>();

			AddDiscreteButton = new Button("+") { Width = 25, Height = 25 };

			UpdateButton = new Button("Update") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
			DeleteButton = new Button("Delete") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
