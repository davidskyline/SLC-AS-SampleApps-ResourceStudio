namespace Skyline.Automation.IAS
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class ScriptDialog : Dialog
	{
		#region Fields
		protected ScriptLayout Layout;
		#endregion

		protected ScriptDialog(IEngine engine)
			: base(engine)
		{
			Init();
		}

		#region Methods
		public abstract void Build();

		private void Init()
		{
			Layout = new ScriptLayout
			{
				RowPosition = 0,
			};
		}
		#endregion

		#region Structs
		protected struct ScriptLayout
		{
			public int RowPosition { get; set; }
		}
		#endregion
	}
}
