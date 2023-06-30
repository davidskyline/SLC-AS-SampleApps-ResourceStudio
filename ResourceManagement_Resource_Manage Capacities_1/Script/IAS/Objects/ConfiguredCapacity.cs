namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.DOM;

	public class ConfiguredCapacity
	{
		#region Fields
		private CapacityData capacityData;
		#endregion

		public ConfiguredCapacity(CapacityData capacityData)
		{
			this.capacityData = capacityData ?? throw new ArgumentNullException(nameof(capacityData));
		}

		public ConfiguredCapacity(CapacityData capacityData, double value) : this(capacityData)
		{
			Value = value;
		}

		#region Properties
		public CapacityData CapacityData
		{
			get { return capacityData; }
		}

		public double Value { get; set; }
		#endregion
	}
}
