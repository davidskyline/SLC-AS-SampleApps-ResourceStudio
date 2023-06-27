namespace Skyline.Automation.DOM
{
	using System;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public class CapacityData
	{
		public DomInstance Instance { get; set; }

		public string Name { get; set; }

		public string Units { get; set; }

		public double? RangeMin { get; set; }

		public double? RangeMax { get; set;}

		public double? StepSize { get; set; }

		public Int64? Decimals { get; set; }
	}
}
