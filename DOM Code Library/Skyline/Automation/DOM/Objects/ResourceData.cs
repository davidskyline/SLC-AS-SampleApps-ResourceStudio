namespace Skyline.Automation.DOM
{
	using System;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class ResourceData
	{
		public DomInstance Instance { get; set; }

		public string Name { get; set; }

		public Guid ResourceId { get; set; }

		public string PoolIds { get; set; }
	}
}
