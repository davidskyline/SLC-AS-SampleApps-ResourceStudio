namespace Script
{
	using System.Collections.Generic;

	using Skyline.Automation.DOM;
	using Skyline.DataMiner.Net.Messages;

	public class ResourceDataMapping
	{
		public Resource Resource { get; set; }

		public ResourceData ResourceData { get; set; }

		public List<ConfiguredCapability> ConfiguredCapabilities { get; set; } = new List<ConfiguredCapability>();
	}
}
