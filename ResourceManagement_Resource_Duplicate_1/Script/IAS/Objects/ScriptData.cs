namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Newtonsoft.Json;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Sections.Fields;
	using Skyline.DataMiner.Net.LogHelpers;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private readonly Guid domInstanceId;

		private ResourceManagerHandler resourceManagerHandler;

		private ResourceData resourceData;
		#endregion

		public ScriptData(IEngine engine, Guid domInstanceId)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domInstanceId = domInstanceId;

			Init();
		}

		#region Properties
		public string ResourceName
		{
			get
			{
				return resourceData.Name;
			}
		}

		public int NumberOfDuplicates { get; set; }
		#endregion

		#region Methods
		public void DuplicateResource()
		{
			if (NumberOfDuplicates < 1)
			{
				return;
			}

			var newResourceNames = GetDuplicateNames();
			var newResourcesByName = new Dictionary<string, Resource>();

			var resourceExpected = resourceData.Instance.StatusId == Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Complete;
			if (resourceExpected)
			{
				var srmHelpers = new SrmHelpers(engine);

				var resource = srmHelpers.ResourceManagerHelper.GetResource(resourceData.ResourceId);
				if (resource == null)
				{
					UpdateErrorMessage($"No resource found with ID '{resourceData.ResourceId}'.");

					return;
				}

				newResourcesByName = CreateResources(srmHelpers, resource, newResourceNames);
			}

			CreateDomInstances(newResourceNames, newResourcesByName, resourceExpected);
		}

		private static Dictionary<string, Resource> CreateResources(SrmHelpers srmHelpers, Resource baseResource, List<string> namesToUse)
		{
			var createdResources = new Dictionary<string, Resource>();

			var resourceNameInUse = srmHelpers.ResourceManagerHelper.GetResources(ResourceExposers.Name.Contains(baseResource.Name)).Select(x => x.Name).ToList();

			var resourcesToCreate = new List<Resource>();
			foreach (string resourceName in namesToUse)
			{
				if (resourceNameInUse.Contains(resourceName))
				{
					continue;
				}

				var duplicateResource = baseResource.Clone() as Resource;
				duplicateResource.ID = Guid.Empty;
				duplicateResource.Name = resourceName;

				resourcesToCreate.Add(duplicateResource);
			}

			if (!resourcesToCreate.Any())
			{
				return createdResources;
			}

			foreach (var resource in srmHelpers.ResourceManagerHelper.AddOrUpdateResources(resourcesToCreate.ToArray()))
			{
				if (resource == null)
				{
					continue;
				}

				createdResources.Add(resource.Name, resource);
			}

			return createdResources;
		}

		private void Init()
		{
			resourceManagerHandler = new ResourceManagerHandler(engine);
			resourceData = resourceManagerHandler.Resources.Single(x => x.Instance.ID.Id == domInstanceId);

			if (!ValidateResourceState())
			{
				throw new NotSupportedException($"It is not allowed to duplicate a resource in state '{resourceData.Instance.StatusId}'.");
			}
		}

		private List<string> GetDuplicateNames()
		{
			var namesInUse = resourceManagerHandler.Resources.Select(x => x.Name).ToList();

			var namesToUse = new List<string>();
			var sequence = 1;
			do
			{
				var name = $"{resourceData.Name} - Copy ({sequence})";
				if (!namesInUse.Contains(name))
				{
					namesToUse.Add(name);
				}

				sequence++;

			} while (namesToUse.Count < NumberOfDuplicates && sequence < 10000);

			return namesToUse;
		}

		private void CreateDomInstances(List<string> newResourceNames, Dictionary<string, Resource> newResourcesByName, bool resourceExpected)
		{
			var newResourceDomInstanceIdsWithResourcePool = new List<string>();
			foreach (var newResourceName in newResourceNames)
			{
				var newResourceDomInstance = resourceData.Instance.Clone() as DomInstance;
				newResourceDomInstance.ID = new DomInstanceId(Guid.NewGuid());

				var resourceInfoSection = newResourceDomInstance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Id.Id);
				resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Name, new ValueWrapper<string>(newResourceName)));

				var resourceInternalPropertiesSection = newResourceDomInstance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Id.Id);
				if (newResourcesByName.TryGetValue(newResourceName, out var newResource))
				{
					resourceInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Resource_Id, new ValueWrapper<Guid>(newResource.ID)));

					if (newResource.PoolGUIDs.Any())
					{
						newResourceDomInstanceIdsWithResourcePool.Add(Convert.ToString(newResourceDomInstance.ID.Id));
					}
				}
				else if (resourceExpected)
				{
					resourceInternalPropertiesSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Resource_Id);
					resourceInternalPropertiesSection.RemoveFieldValueById(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids);

					newResourceDomInstance.StatusId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Error;

					resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> Resource '{newResourceName}' already exists.")));
				}
				else
				{
					// Do nothing
				}

				HandleVirtualSignalGroups(newResourceDomInstance);

				resourceManagerHandler.DomHelper.DomInstances.Create(newResourceDomInstance);
			}

			if (newResourceDomInstanceIdsWithResourcePool.Any())
			{
				var poolIds = resourceData.PoolIds.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
				var resourcePoolsData = resourceManagerHandler.ResourcePools.Where(x => poolIds.Contains(Convert.ToString(x.Instance.ID.Id))).ToList();

				foreach (var resourcePoolData in resourcePoolsData)
				{
					var poolResources = resourcePoolData.ResourceIds.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

					newResourceDomInstanceIdsWithResourcePool.ForEach(x =>
					{
						if (!poolResources.Contains(x))
						{
							poolResources.Add(x);
						}
					});

					var resourcePoolInternalPropertiesSection = resourcePoolData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Id.Id);
					resourcePoolInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids, new ValueWrapper<string>(string.Join(";", poolResources))));

					resourceManagerHandler.DomHelper.DomInstances.Update(resourcePoolData.Instance);
				}
			}
		}

		private void HandleVirtualSignalGroups(DomInstance newResourceDomInstance)
		{
			var resourceConnectionManagementSection = newResourceDomInstance.Sections.SingleOrDefault(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceConnectionManagement.Id.Id);

			if (resourceData.VirtualSignalGroupInputIds.Any())
			{
				var newInputIds = new List<Guid>();
				foreach (var inputId in resourceData.VirtualSignalGroupInputIds)
				{
					var newInputId = DuplicateVirtualSignalGroup(inputId);
					if (newInputId == Guid.Empty)
					{
						continue;
					}

					newInputIds.Add(newInputId);
				}

				resourceConnectionManagementSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceConnectionManagement.InputVsgs, new ListValueWrapper<Guid>(newInputIds)));
			}

			if (resourceData.VirtualSignalGroupOutputIds.Any())
			{
				var newOutputIds = new List<Guid>();
				foreach (var outputId in resourceData.VirtualSignalGroupOutputIds)
				{
					var newOutputId = DuplicateVirtualSignalGroup(outputId);
					if (newOutputId == Guid.Empty)
					{
						continue;
					}

					newOutputIds.Add(newOutputId);
				}

				resourceConnectionManagementSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceConnectionManagement.OutputVsgs, new ListValueWrapper<Guid>(newOutputIds)));
			}

			Guid DuplicateVirtualSignalGroup(Guid id)
			{
				var subScript = engine.PrepareSubScript("DuplicateVsg");

				subScript.SelectScriptParam("Virtual Signal Group ID", Convert.ToString(id));

				subScript.Synchronous = true;
				subScript.LockElements = false;

				subScript.StartScript();

				var subScriptResponse = subScript.GetScriptResult();
				if (subScriptResponse.TryGetValue("RESULT", out string duplicatedResultString))
				{
					var duplicatedResult = JsonConvert.DeserializeObject<DuplicatedResult>(duplicatedResultString);
					if (duplicatedResult != null && duplicatedResult.IsSucceeded)
					{
						return duplicatedResult.DuplicatedDomInstanceId;
					}
				}

				return Guid.Empty;
			}
		}

		private bool ValidateResourceState()
		{
			var allowedStates = new List<string>
			{
				Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Draft,
				Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Complete,
			};

			if (allowedStates.Contains(resourceData.Instance.StatusId))
			{
				return true;
			}

			return false;
		}

		private void UpdateErrorMessage(string errorMessage)
		{
			var resourceInfoSection = resourceData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Id.Id);
			resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> {errorMessage}")));

			resourceData.Instance = resourceManagerHandler.DomHelper.DomInstances.Update(resourceData.Instance);

			var transitionId = (resourceData.Instance.StatusId == Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Complete) ? Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Complete_To_Error : Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Draft_To_Error;

			resourceManagerHandler.DomHelper.DomInstances.DoStatusTransition(resourceData.Instance.ID, transitionId);
		}
		#endregion

		#region Classes
		public class DuplicatedResult
		{
			public bool IsSucceeded { get; set; }

			public Guid DuplicatedDomInstanceId { get; set; }
		}
		#endregion
	}
}
