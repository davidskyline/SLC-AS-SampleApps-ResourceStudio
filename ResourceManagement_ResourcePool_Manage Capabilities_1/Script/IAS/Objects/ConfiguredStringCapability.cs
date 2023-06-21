namespace Script.IAS
{
	using System;

	using Skyline.Automation.DOM;

	public class ConfiguredStringCapability : ConfiguredCapability
	{
		public ConfiguredStringCapability(CapabilityData capabilityData) : base(capabilityData)
		{

		}

		public ConfiguredStringCapability(CapabilityData capabilityData, string value) : this(capabilityData)
		{
			Value = value;
		}

		#region Properties
		public String Value { get; set; }
		#endregion
	}
}
