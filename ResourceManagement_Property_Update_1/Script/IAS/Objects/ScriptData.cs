namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
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

		private Lazy<List<ResourceData>> resourcesImplementingProperty;
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

		public List<ResourceData> ResourcesImplementingProperty
		{
			get
			{
				return resourcesImplementingProperty.Value;
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

			resourcesImplementingProperty = new Lazy<List<ResourceData>>(() => FindResourcesImplementingProperty());
		}

		private void DeleteDomInstances()
		{
			resourceManagerHandler.DomHelper.DomInstances.Delete(propertyData.Instance);
		}

		private List<ResourceData> FindResourcesImplementingProperty()
		{
			return resourceManagerHandler.Resources.Where(x => x.Properties.Any(y => y.PropertyId == propertyData.Instance.ID.Id)).ToList();
		}
		#endregion
	}
}
