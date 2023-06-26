namespace Script.IAS
{
	using System;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.DataMiner.Automation;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private readonly Guid domInstanceId;

		private ResourceManagerHandler resourceManagerHandler;

		private PropertyData propertyData;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string Name
		{
			get
			{
				return propertyData.Name;
			}
		}
		#endregion

		#region Methods
		public void DeleteProperty()
		{
			DeleteDomInstances();
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);

			propertyData = resourceManagerHandler.Properties.Single(x => x.Instance.ID.Id == domInstanceId);
		}

		private void DeleteDomInstances()
		{
			resourceManagerHandler.DomHelper.DomInstances.Delete(propertyData.Instance);
		}
		#endregion
	}
}
