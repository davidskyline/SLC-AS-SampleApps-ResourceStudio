namespace Script.IAS
{
	using System;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private ResourceManagerHandler resourceManagerHandler;
		#endregion

		public ScriptData(IEngine engine)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));

			Init();
		}

		#region Properties
		public string Name { get; set; }
		#endregion

		#region Methods
		public void AddProperty()
		{
			if (string.IsNullOrWhiteSpace(Name) || IsNameInUse())
			{
				return;
			}

			CreateDomInstances();
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);
		}

		private bool IsNameInUse()
		{
			if (resourceManagerHandler.Properties.Any(x => x.Name == Name))
			{
				return true;
			}

			return false;
		}

		private void CreateDomInstances()
		{
			var propertyInfoSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.PropertyInfo.Id);
			propertyInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.PropertyInfo.PropertyName, new ValueWrapper<string>(Name)));

			var propertyInstance = new DomInstance
			{
				DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Resourceproperty,
			};
			propertyInstance.Sections.Add(propertyInfoSection);
			resourceManagerHandler.DomHelper.DomInstances.Create(propertyInstance);
		}
		#endregion
	}
}
