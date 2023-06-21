namespace Skyline.Automation.DOM
{
	using System;
	using System.Collections.Generic;

	public class ResourcePoolCapability
	{
		public Guid CapabilityId { get; set; }

		public List<Guid> CapabilityEnumValueIds { get; set; } = new List<Guid>();

		public string CapabilityStringValue { get; set; }
	}
}
