namespace Skyline.Automation.DOM
{
	using System;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class CapabilityValueData
	{
		public DomInstance Instance { get; set; }

		public Guid CapabilityId { get; set; }

		public string Value { get; set; }
	}
}
