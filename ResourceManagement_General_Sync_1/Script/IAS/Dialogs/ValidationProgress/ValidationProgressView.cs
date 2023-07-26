namespace Script.IAS.Dialogs.ValidationProgress
{
	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ValidationProgressView : ScriptDialog
	{
		public ValidationProgressView(IEngine engine) : base(engine)
		{
			InitWidgets();
		}

		#region Properties
		public Button ResultButton { get; private set; }
		#endregion

		#region Methods
		public override void Build()
		{
			Clear();

			Width = 600;

			Title = "Synchronize Application";
		}

		public Label AddTextLine(string text)
		{
			var label = new Label(text);

			AddWidget(label, ++Layout.RowPosition, 0);

			return label;
		}

		public void ShowButton()
		{
			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);
			AddWidget(ResultButton, ++Layout.RowPosition, 0);

			ResultButton.IsEnabled = true;
		}

		private void InitWidgets()
		{
			ResultButton = new Button("Show Results...") { Width = 150, Height = 25, IsEnabled = false };
		}
		#endregion
	}
}
