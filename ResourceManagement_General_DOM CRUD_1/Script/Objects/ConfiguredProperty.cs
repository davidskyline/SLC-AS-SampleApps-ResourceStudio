namespace Script
{
	using System;

	using Skyline.Automation.DOM;

	public class ConfiguredProperty
	{
		#region Fields
		private readonly PropertyData propertyData;
		#endregion

		public ConfiguredProperty(PropertyData propertyData)
		{
			this.propertyData = propertyData ?? throw new ArgumentException(nameof(propertyData));
		}

		public ConfiguredProperty(PropertyData propertyData, string value) : this(propertyData)
		{
			Value = value;
		}

		#region Properties
		public PropertyData PropertyData { get { return propertyData; } }

		public string Value { get; set; }
		#endregion
	}
}
