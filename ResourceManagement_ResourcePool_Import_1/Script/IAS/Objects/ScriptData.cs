namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private SrmHelpers srmHelpers;

		private ResourceManagerHandler resourceManagerHandler;

		private Dictionary<string, ResourcePool> resourcePoolsbyName;

		private Dictionary<Guid, ResourceData> resourceDataByResourceId;
		#endregion

		public ScriptData(IEngine engine)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));

			Init();
		}

		#region Properties
		public IEnumerable<string> ResourcePools => resourcePoolsbyName.Keys;

		public string SelectedResourcePool { get; set; }
		#endregion

		#region Methods
		public void Import()
		{
			LoadResourceDataWithResourceId();

			var resourcePool = resourcePoolsbyName[SelectedResourcePool];
			if (!TryGetResourcePoolDataByResourcePoolId(resourcePool.ID, out var resourcePoolData))
			{
				resourcePoolData = CreateResourcePoolDomInstance(resourcePool);
			}

			var resourcesData = new List<ResourceData>();
			var poolResources = GetResourcesByResourcePoolId(resourcePool.ID);
			foreach (var resource in poolResources)
			{
				if (!resourceDataByResourceId.TryGetValue(resource.ID, out ResourceData resourceData))
				{
					resourceData = CreateResourceDomInstance(resource);
				}

				resourcesData.Add(resourceData);
			}

			UpdateDomInstancesLinks(resourcePoolData, resourcesData);
		}

		private static Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type ParseResourceType(Resource resource)
		{
			if (resource is FunctionResource)
			{
				return Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.VirtualFunction;
			}

			if (resource.DmaID > 0 && resource.ElementID > 0)
			{
				return Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.Element;
			}

			return Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.UnlinkedResource;
		}

		private void Init()
		{
			srmHelpers = new SrmHelpers(engine);
			resourceManagerHandler = new ResourceManagerHandler(engine);

			LoadResourcePools();
		}

		private void LoadResourcePools()
		{
			resourcePoolsbyName = new Dictionary<string, ResourcePool>();

			srmHelpers.ResourceManagerHelper.GetResourcePools().ForEach(x => resourcePoolsbyName.Add(x.Name, x));
		}

		private void LoadResourceDataWithResourceId()
		{
			resourceDataByResourceId = new Dictionary<Guid, ResourceData>();

			resourceManagerHandler.Resources.Where(x => x.ResourceId != Guid.Empty).ForEach(x => resourceDataByResourceId.Add(x.ResourceId, x));
		}

		private List<Resource> GetResourcesByResourcePoolId(Guid resourcePoolId)
		{
			return srmHelpers.ResourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(resourcePoolId)).ToList();
		}

		private bool TryGetResourcePoolDataByResourcePoolId(Guid id, out ResourcePoolData resourcePoolData)
		{
			resourcePoolData = resourceManagerHandler.ResourcePools.SingleOrDefault(x => x.PoolId == id);

			return resourcePoolData != null;
		}

		private ResourcePoolData CreateResourcePoolDomInstance(ResourcePool resourcePool)
		{
			var resourcePoolInfoSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInfo.Id);
			resourcePoolInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInfo.Name, new ValueWrapper<string>(resourcePool.Name)));

			var resourcePoolInternalPropertiesSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Id);
			resourcePoolInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Pool_Id, new ValueWrapper<string>(Convert.ToString(resourcePool.ID))));

			var resourcePoolInstance = new DomInstance
			{
				DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Resourcepool,
				StatusId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Statuses.Complete,
			};
			resourcePoolInstance.Sections.Add(resourcePoolInfoSection);
			resourcePoolInstance.Sections.Add(resourcePoolInternalPropertiesSection);

			resourcePoolInstance = resourceManagerHandler.DomHelper.DomInstances.Create(resourcePoolInstance);

			return ResourcePoolDataLoader.ParseInstance(resourcePoolInstance);
		}

		private ResourceData CreateResourceDomInstance(Resource resource)
		{
			var resourceInfoSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Id);
			resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Name, new ValueWrapper<string>(resource.Name)));
			resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Concurrency, new ValueWrapper<Int64>(resource.MaxConcurrency)));

			var resourceType = ParseResourceType(resource);
			resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Type, new ValueWrapper<int>((int)resourceType)));

			switch (resourceType)
			{
				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.Element:
					resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Element, new ValueWrapper<string>($"{resource.DmaID}/{resource.ElementID}")));

					break;

				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.VirtualFunction:
				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.Service:
				case Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.UnlinkedResource:
				default:
					break;
			}

			var resourceInternalPropertiesSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Id);
			resourceInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Resource_Id, new ValueWrapper<Guid>(resource.ID)));

			var resourceConnectionManagementSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceConnectionManagement.Id);

			var resourceCostSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceCost.Id);

			var resourceFlowEngineeringSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceFlowEngineering.Id);

			var resourceOtherSection = new Section(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceOther.Id);

			var resourceInstance = new DomInstance
			{
				DomDefinitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Resource,
				StatusId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Complete,
			};
			resourceInstance.Sections.Add(resourceInfoSection);
			resourceInstance.Sections.Add(resourceInternalPropertiesSection);
			resourceInstance.Sections.Add(resourceConnectionManagementSection);
			resourceInstance.Sections.Add(resourceCostSection);
			resourceInstance.Sections.Add(resourceFlowEngineeringSection);
			resourceInstance.Sections.Add(resourceOtherSection);

			resourceInstance = resourceManagerHandler.DomHelper.DomInstances.Create(resourceInstance);

			return ResourceDataLoader.ParseInstance(resourceInstance);
		}

		private void UpdateDomInstancesLinks(ResourcePoolData resourcePoolData, List<ResourceData> resourcesData)
		{
			var poolDomInstanceId = Convert.ToString(resourcePoolData.Instance.ID.Id);
			var poolResources = resourcePoolData.ResourceIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

			foreach (var resourceData in resourcesData)
			{
				var resourceDomInstanceId = Convert.ToString(resourceData.Instance.ID.Id);
				if (!poolResources.Contains(resourceDomInstanceId))
				{
					poolResources.Add(resourceDomInstanceId);
				}

				var resourcePools = resourceData.PoolIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
				if (!resourcePools.Contains(poolDomInstanceId))
				{
					resourcePools.Add(poolDomInstanceId);
				}

				var resourceInternalPropertiesSection = resourceData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Id.Id);
				if (resourcePools.Any())
				{
					resourceInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids, new ValueWrapper<string>(string.Join(";", resourcePools))));
				}
				else
				{
					resourceInternalPropertiesSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids);
				}

				resourceManagerHandler.DomHelper.DomInstances.Update(resourceData.Instance);
			}

			var resourcePoolInternalPropertiesSection = resourcePoolData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Id.Id);
			if (poolResources.Any())
			{
				resourcePoolInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids, new ValueWrapper<string>(string.Join(";", poolResources))));
			}
			else
			{
				resourcePoolInternalPropertiesSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids);
			}

			resourceManagerHandler.DomHelper.DomInstances.Update(resourcePoolData.Instance);
		}
		#endregion
	}
}
