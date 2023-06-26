namespace Skyline.Automation.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class ScriptSection : Section
	{
		#region Fields
		protected ScriptLayout Layout;
		#endregion

		protected ScriptSection()
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
