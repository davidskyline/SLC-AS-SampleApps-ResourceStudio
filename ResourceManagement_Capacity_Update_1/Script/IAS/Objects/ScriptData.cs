namespace Script.IAS
{
	using System;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.DataMiner.Automation;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private readonly Guid domInstanceId;

		private ResourceManagerHandler resourceManagerHandler;

		private CapacityData capacity;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string Name { get; private set; }

		public string Units { get; set; }

		public bool IsRangeMinEnabled { get; set; }

		public double RangeMin { get; set; }

		public bool IsRangeMaxEnabled { get; set; }

		public double RangeMax { get; set; }

		public bool IsStepSizeEnabled { get; set; }

		public double StepSize { get; set; }

		public bool IsDecimalsEnabled { get; set; }

		public int Decimals { get; set; }
		#endregion

		#region Methods
		public void UpdateCapacity()
		{

		}

		public void DeleteCapacity()
		{
			
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);

			capacity = resourceManagerHandler.Capacities.Single(x => x.Instance.ID.Id == domInstanceId);
		}
		#endregion
	}
}