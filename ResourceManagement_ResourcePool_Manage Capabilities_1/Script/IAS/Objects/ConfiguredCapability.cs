namespace Script.IAS
{
	using System;
	using System.Collections.Generic;

	using Skyline.Automation.DOM;

	public abstract class ConfiguredCapability
	{
		#region Fields
		protected CapabilityData capabilityData;
		#endregion

		protected ConfiguredCapability(CapabilityData capabilityData)
		{
			this.capabilityData = capabilityData ?? throw new ArgumentNullException(nameof(capabilityData));
		}

		#region Properties
		public CapabilityData CapabilityData { get { return capabilityData; } }
		#endregion
	}
}
