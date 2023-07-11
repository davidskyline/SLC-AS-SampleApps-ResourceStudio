namespace Skyline.Automation.DOM
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class ResourcePoolData
	{
		public DomInstance Instance { get; set; }

		public string Name { get; set; }

		public bool IsBookable { get; set; }

		public string ResourceIds { get; set; }

		public Guid PoolId { get; set; }

		public Guid ResourceId { get; set; }

		public List<ResourcePoolCapability> Capabilities { get; set; } = new List<ResourcePoolCapability>();
	}
}
