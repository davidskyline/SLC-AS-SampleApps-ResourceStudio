/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

31/05/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.Automation.DOM;
	using Skyline.Automation.DOM.DomIds;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Jobs;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private IEngine engine;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			this.engine = engine;

			try
			{
				RunSafe();
			}
			catch (Exception ex)
			{
				engine.Log(ex.ToString());
				engine.ExitFail(ex.Message);
			}
		}

		private static List<ResourceDataMapping> GetExistingResources(SrmHelpers srmHelpers, List<ResourceData> resourcesData)
		{
			var resourceMappings = new List<ResourceDataMapping>();

			var resourceDataByResourceId = resourcesData.Where(x => x.ResourceId != Guid.Empty).ToDictionary(x => x.ResourceId, x => x);
			var resourceFilter = new ORFilterElement<Resource>(resourceDataByResourceId.Keys.Select(id => ResourceExposers.ID.Equal(id)).ToArray());

			foreach (var resource in srmHelpers.ResourceManagerHelper.GetResources(resourceFilter))
			{
				if (resource == null)
				{
					continue;
				}

				resourceMappings.Add(new ResourceDataMapping
				{
					Resource = resource,
					ResourceData = resourceDataByResourceId[resource.ID],
				});
			}

			return resourceMappings;
		}

		private static List<ResourceDataMapping> AssignResourcesToPool(SrmHelpers srmHelpers, Guid poolId, List<ResourceDataMapping> resourceMappings)
		{
			var updatedResources = new List<ResourceDataMapping>();

			var resourcesToUpdate = new List<Resource>();
			foreach (var resourceMapping in resourceMappings)
			{
				if (resourceMapping.Resource.PoolGUIDs.Contains(poolId))
				{
					continue;
				}

				resourceMapping.Resource.PoolGUIDs.Add(poolId);
				resourcesToUpdate.Add(resourceMapping.Resource);
			}

			if (!resourcesToUpdate.Any())
			{
				return updatedResources;
			}

			foreach (var resource in srmHelpers.ResourceManagerHelper.AddOrUpdateResources(resourcesToUpdate.ToArray()))
			{
				if (resource == null)
				{
					continue;
				}

				var resourceMapping = resourceMappings.Single(x => x.Resource.ID == resource.ID);
				resourceMapping.Resource = resource;
				updatedResources.Add(resourceMapping);
			}

			return updatedResources;
		}

		private static void UpdateDomInstances(DomHelper domHelper, ResourcePoolData resourcePoolData, List<ResourceData> resourcesData)
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

				var resourceInternalPropertiesSection = resourceData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourceInternalProperties.Id.Id);
				if (resourcePools.Any())
				{
					resourceInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids, new ValueWrapper<string>(string.Join(";", resourcePools))));
				}
				else
				{
					resourceInternalPropertiesSection.RemoveFieldValueById(Resourcemanagement.Sections.ResourceInternalProperties.Pool_Ids);
				}

				domHelper.DomInstances.Update(resourceData.Instance);
			}

			var resourcePoolInternalPropertiesSection = resourcePoolData.Instance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourcePoolInternalProperties.Id.Id);
			if (poolResources.Any())
			{
				resourcePoolInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids, new ValueWrapper<string>(string.Join(";", poolResources))));
			}
			else
			{
				resourcePoolInternalPropertiesSection.RemoveFieldValueById(Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Ids);
			}

			domHelper.DomInstances.Update(resourcePoolData.Instance);
		}

		private void RunSafe()
		{
			var inputData = GetInputData();

			var domHelper = new DomHelper(engine.SendSLNetMessages, Resourcemanagement.ModuleId);
			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			var srmHelpers = new SrmHelpers(engine);

			var resourcePoolData = resourceManagerHandler.ResourcePools.Single(x => x.Instance.ID.Id == inputData.ResourcePoolDomInstanceId);
			var resourcesData = resourceManagerHandler.Resources.Where(x => inputData.ResourceDomInstanceIds.Contains(x.Instance.ID.Id)).ToList();
			engine.Log($"resourcesData: {JsonConvert.SerializeObject(resourcesData)}");
			var existingResources = GetExistingResources(srmHelpers, resourcesData);
			engine.Log($"existingResources: {JsonConvert.SerializeObject(existingResources)}");
			var updatedResources = AssignResourcesToPool(srmHelpers, resourcePoolData.PoolId, existingResources);
			engine.Log($"updatedResources: {JsonConvert.SerializeObject(updatedResources)}");
			var resourcesDataToUpdate = updatedResources.Select(x => x.ResourceData).ToList();
			UpdateDomInstances(domHelper, resourcePoolData, resourcesDataToUpdate);
		}

		private InputData GetInputData()
		{
			var data = new InputData
			{
				ResourcePoolDomInstanceId = JsonConvert.DeserializeObject<List<Guid>>(engine.GetScriptParam("Resource Pool").Value).Single(),
				ResourceDomInstanceIds = JsonConvert.DeserializeObject<List<Guid>>(engine.GetScriptParam("Resources").Value),
			};

			return data;
		}
	}
}