namespace Script.IAS.Dialogs.DeleteNotPossible
{
	using System.Collections.Generic;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DeleteNotPossibleView : ScriptDialog
	{
		public DeleteNotPossibleView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public List<string> ResourceNames { get; set; }

		public Button CloseButton { get; set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Delete Capacity";

			AddWidget(new Label("Not possible to delete capacity because it is configured on following resources:"), Layout.RowPosition, 0);

			foreach (var resourceName in ResourceNames)
			{
				AddWidget(new Label(resourceName), ++Layout.RowPosition, 0);

				AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

				AddWidget(CloseButton, ++Layout.RowPosition, 0);
			}
		}

		private void InitWidgets()
		{
			CloseButton = new Button("Close") { Width = 150, Height = 25 };
		}
		#endregion
	}
}
