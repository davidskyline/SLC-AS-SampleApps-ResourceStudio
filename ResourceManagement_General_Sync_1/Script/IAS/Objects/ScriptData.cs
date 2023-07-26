namespace Script.IAS
{
	using System;
	using System.Collections.Generic;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private ResourceManagerHandler resourceManagerHandler;

		private SrmHelpers srmHelpers;

		private CapacityHandler capacityHandler;

		private CapabilityHandler capabilityHandler;
		#endregion

		public ScriptData(IEngine engine)
		{
			this.engine = engine ?? throw new ArgumentException(nameof(engine));

			Init();
		}

		#region Properties
		public SynchronizationResult Result { get; private set; }
		#endregion

		#region Methods
		public void VerifyCapacities()
		{
			capacityHandler = new CapacityHandler(resourceManagerHandler, srmHelpers);
			capacityHandler.ValidateSynchronization();
		}

		public void VerifyCapabilities()
		{
			capabilityHandler = new CapabilityHandler(resourceManagerHandler, srmHelpers);
			capabilityHandler.ValidateSynchronization();
		}

		public List<SynchronizationResult> GetResults()
		{
			var results = new List<SynchronizationResult>();

			if (!capacityHandler.Result.IsSynchronized)
			{
				results.Add(capacityHandler.Result);
			}

			if (!capabilityHandler.Result.IsSynchronized)
			{
				results.Add(capabilityHandler.Result);
			}

			return results;
		}

		public void SetResultDetails(string sectionReference)
		{
			switch (sectionReference)
			{
				case "Capacity":
					Result = capacityHandler.Result;
					break;

				case "Capability":
					Result = capabilityHandler.Result;
					break;

				default:
					break;
			}
		}

		public void Synchronize()
		{
			if (!capacityHandler.Result.IsSynchronized)
			{
				capacityHandler.EnsureSynchronization();
			}

			if (!capabilityHandler.Result.IsSynchronized)
			{
				capabilityHandler.EnsureSynchronization();
			}
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);
			srmHelpers = new SrmHelpers(engine);
		}
		#endregion
	}
}
