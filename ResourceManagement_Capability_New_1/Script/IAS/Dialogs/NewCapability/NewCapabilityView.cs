namespace Script.IAS.Dialogs.NewCapability
{
	using System.Collections.Generic;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class NewCapabilityView : ScriptDialog
	{
		public NewCapabilityView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public TextBox CapabilityNameTextBox { get; set; }

		public DropDown CapabilityTypeDropDown { get; set; }

		public List<DiscreteSection> Discretes { get; set; }

		public Button AddDiscreteButton { get; set; }

		public Button AddButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "New Capability";

			AddWidget(new Label("Name"), Layout.RowPosition, 0);
			AddWidget(CapabilityNameTextBox, Layout.RowPosition, 1);

			AddWidget(new Label("Type"), ++Layout.RowPosition, 0);
			AddWidget(CapabilityTypeDropDown, Layout.RowPosition, 1);

			if (CapabilityTypeDropDown.Selected == "Discrete")
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
			AddWidget(AddButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
			SetColumnWidth(1, 260);
		}

		private void InitWidgets()
		{
			CapabilityNameTextBox = new TextBox { Width = 250 };

			CapabilityTypeDropDown = new DropDown { Width = 250 };

			Discretes = new List<DiscreteSection>();

			AddDiscreteButton = new Button("+") { Width = 25, Height = 25 };

			AddButton = new Button("Add") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
