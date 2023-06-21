namespace Script.IAS.Dialogs.ConfigureCapability
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ConfigureCapabilityView : ScriptDialog
	{
		public ConfigureCapabilityView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Label CapabilityNameLabel { get; set; }

		public Label CapabilityTypeLabel { get; set; }

		public TextBox TextValueTextBox { get; set; }

		public List<DiscreteSection> Discretes { get; set; }

		public Button AddDiscreteButton { get; set; }

		public Button ApplyButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Manage Capabilities";

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
			else
			{
				AddWidget(new Label("Value"), ++Layout.RowPosition, 0);
				AddWidget(TextValueTextBox, Layout.RowPosition, 1);
			}

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(ApplyButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}

		private void InitWidgets()
		{
			CapabilityNameLabel = new Label();

			CapabilityTypeLabel = new Label();

			TextValueTextBox = new TextBox { Width = 150 };

			Discretes = new List<DiscreteSection>();

			AddDiscreteButton = new Button("+") { Width = 25, Height = 25 };

			ApplyButton = new Button("Apply") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
