namespace Skyline.Automation.DOM
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class ResourceData
	{
		public DomInstance Instance { get; set; }

		public string Name { get; set; }

		public DomIds.Resourcemanagement.Enums.Type? ResourceType { get; set; }

		public string LinkedElementInfo { get; set; }

		public Guid ResourceId { get; set; }

		public string PoolIds { get; set; }

		public List<ResourceProperty> Properties { get; set; } = new List<ResourceProperty>();

		public List<ResourceCapacity> Capacities { get; set; } = new List<ResourceCapacity>();

		public List<Guid> VirtualSignalGroupInputIds { get; set; }

		public List<Guid> VirtualSignalGroupOutputIds { get; set; }
	}
}
