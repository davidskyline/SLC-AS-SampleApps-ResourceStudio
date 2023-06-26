namespace Skyline.Automation.DOM
{
	using System;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class CapabilityData
	{
		public DomInstance Instance { get; set; }

		public string Name { get; set; }

		public DomIds.Resourcemanagement.Enums.CapabilityType CapabilityType { get; set; }
	}
}
