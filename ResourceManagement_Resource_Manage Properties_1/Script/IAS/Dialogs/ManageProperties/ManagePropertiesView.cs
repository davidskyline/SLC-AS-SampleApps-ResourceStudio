namespace Script.IAS.Dialogs.ManageProperties
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ManagePropertiesView : ScriptDialog
	{
		public ManagePropertiesView(IEngine engine) : base(engine) {
			InitWidgets();
		}

		#region Properties
		public Label ResourceName { get; set; }

		public List<PropertySection> Properties { get; set; }

		public Button AddPropertyButton { get; set; }

		public Button UpdateButton { get; set; }

		public Button CancelButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;
			MaxHeight = 600;

			Title = "Manage Properties";

			AddWidget(new Label("Resource Name"), Layout.RowPosition, 0);
			AddWidget(ResourceName, Layout.RowPosition, 1);

			AddWidget(new WhiteSpace { Height = 10 }, ++Layout.RowPosition, 0);

			AddWidget(new Label("Properties") { Style = TextStyle.Bold }, ++Layout.RowPosition, 0, 1, 2);

			foreach (var property in Properties)
			{
				AddSection(property, ++Layout.RowPosition, 0);
			}

			AddWidget(AddPropertyButton, ++Layout.RowPosition, 0);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(CancelButton, ++Layout.RowPosition, 0);
			AddWidget(UpdateButton, Layout.RowPosition, 1);
		}

		private void InitWidgets()
		{
			ResourceName = new Label();

			Properties = new List<PropertySection>();

			AddPropertyButton = new Button("+") { Width = 25, Height = 25 };

			UpdateButton = new Button("Update") { Width = 150, Height = 25 };
			CancelButton = new Button("Cancel") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
