namespace Script.IAS
{
	using System.Collections.Generic;

	using Skyline.Automation.DOM;

	public class ConfiguredEnumCapability : ConfiguredCapability
	{
		public ConfiguredEnumCapability(CapabilityData capabilityData) : base(capabilityData)
		{
			Discretes = new List<string>();
		}

		public ConfiguredEnumCapability(CapabilityData capabilityData, List<string> discretes) : this(capabilityData)
		{
			Discretes = discretes;
		}

		#region Properties
		public List<string> Discretes { get; set; }
		#endregion
	}
}
