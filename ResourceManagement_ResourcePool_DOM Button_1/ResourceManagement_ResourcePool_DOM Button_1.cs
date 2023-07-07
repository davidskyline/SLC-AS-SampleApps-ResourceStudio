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

25/05/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Skyline.Automation.DOM;
	using Skyline.Automation.IAS;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Actions;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
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
			engine.ExitFail("This script should be executed using the 'OnDomAction' entry point");
		}

		[AutomationEntryPoint(AutomationEntryPointType.Types.OnDomAction)]
		public void OnDomActionMethod(IEngine engine, ExecuteScriptDomActionContext context)
		{
			this.engine = engine;

			try
			{
				RunSafe(context);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception ex)
			{
				engine.Log(ex.ToString());
				engine.ExitFail(ex.Message);
			}
		}

		private static void UpdateErrorMessage(DomHelper domHelper, DomInstance instance, string errorMessage)
		{
			var resourcePoolInfoSection = instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInfo.Id.Id);
			resourcePoolInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> {errorMessage}")));

			domHelper.DomInstances.Update(instance);
		}

		private static List<ResourceData> GetResourcesDataToDeprecate(ResourceManagerHandler resourceManagerHandler, ResourcePoolData resourcePoolData)
		{
			var poolResourceIds = resourcePoolData.ResourceIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
			if (!poolResourceIds.Any())
			{
				return new List<ResourceData>();
			}

			var resourcesDataToDeprecate = resourceManagerHandler.Resources.Where(x => poolResourceIds.Contains(Convert.ToString(x.Instance.ID.Id)) && x.PoolIds == Convert.ToString(resourcePoolData.Instance.ID.Id) && x.Instance.StatusId != Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Deprecated).ToList();

			return resourcesDataToDeprecate;
		}

		private static List<ResourceData> GetPoolResourcesData(ResourceManagerHandler resourceManagerHandler, ResourcePoolData resourcePoolData)
		{
			var poolResourceIds = resourcePoolData.ResourceIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
			if (!poolResourceIds.Any())
			{
				return new List<ResourceData>();
			}

			var poolResourcesData = resourceManagerHandler.Resources.Where(x => poolResourceIds.Contains(Convert.ToString(x.Instance.ID.Id))).ToList();

			return poolResourcesData;
		}

		private static List<Resource> GetResourcesToDeprecate(SrmHelpers srmHelpers, List<ResourceData> resourcesDataToDeprecate)
		{
			var resourceFilter = new ORFilterElement<Resource>(resourcesDataToDeprecate.Where(x => x.ResourceId != Guid.Empty).Select(x => ResourceExposers.ID.Equal(x.ResourceId)).ToArray());

			var resourcesToDeprecate = new List<Resource>();
			foreach (var resource in srmHelpers.ResourceManagerHelper.GetResources(resourceFilter))
			{
				if (resource == null)
				{
					continue;
				}

				resourcesToDeprecate.Add(resource);
			}

			return resourcesToDeprecate;
		}

		private static List<Resource> GetResourcesWithReservations(SrmHelpers srmHelpers, List<Resource> resourcesToDeprecate)
		{
			var resourcesWithReservations = new List<Resource>();
			foreach (var resource in resourcesToDeprecate)
			{
				var filter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID).AND(ReservationInstanceExposers.End.GreaterThan(DateTime.UtcNow));
				var reservations = srmHelpers.ResourceManagerHelper.GetReservationInstances(filter).ToList();
				if (reservations.Any())
				{
					resourcesWithReservations.Add(resource);
				}
			}

			return resourcesWithReservations;
		}

		private static void SetResourcesAsUnavailable(SrmHelpers srmHelpers, List<Resource> resources)
		{
			resources.ForEach(x => x.Mode = ResourceMode.Unavailable);

			srmHelpers.ResourceManagerHelper.AddOrUpdateResources(resources.ToArray());
		}

		private static void UnassignPoolFromResources(ResourceManagerHandler resourceManagerHandler, string poolDomInstanceId, List<ResourceData> resourcesData)
		{
			foreach (var resourceData in resourcesData)
			{
				var resourcePools = resourceData.PoolIds?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
				if (resourcePools.Contains(poolDomInstanceId))
				{
					resourcePools.Remove(poolDomInstanceId);
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
		}

		private void RunSafe(ExecuteScriptDomActionContext context)
		{
			var instanceId = context.ContextId as DomInstanceId;
			var action = engine.GetScriptParam("Action")?.Value;

			var domHelper = new DomHelper(engine.SendSLNetMessages, instanceId.ModuleId);
			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(instanceId.Id)).Single();

			string transitionId = string.Empty;
			string errorMessage = string.Empty;

			switch (action)
			{
				case "Mark complete":
					if (TryHandleMarkCompleteAction(domHelper, domInstance, out errorMessage))
					{
						transitionId = (domInstance.StatusId == Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Statuses.Draft) ? Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Transitions.Draft_To_Complete : Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Transitions.Error_To_Complete;
					}
					else if (domInstance.StatusId != Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Statuses.Error)
					{
						transitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Transitions.Draft_To_Error;
					}
					else
					{
						// Do nothing
					}

					break;

				case "Deprecate":
					if (TryHandleDeprecateAction(domHelper, domInstance))
					{
						transitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resourcepool_Behavior.Transitions.Complete_To_Deprecated;
					}

					break;

				case "Delete":
					HandleDeleteAction(domHelper, domInstance);

					break;

				default:
					throw new NotSupportedException($"Action '{action}' is not supported");
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				UpdateErrorMessage(domHelper, domInstance, errorMessage);
			}

			if (!string.IsNullOrEmpty(transitionId))
			{
				domHelper.DomInstances.DoStatusTransition(instanceId, transitionId);
			}
		}

		private bool TryHandleMarkCompleteAction(DomHelper domHelper, DomInstance instance, out string errorMessage)
		{
			errorMessage = string.Empty;

			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			var srmHelpers = new SrmHelpers(engine);

			var resourcePoolData = resourceManagerHandler.ResourcePools.Single(x => x.Instance.ID.Id == instance.ID.Id);
			var isNew = resourcePoolData.PoolId == Guid.Empty;

			var existingResourcePool = srmHelpers.ResourceManagerHelper.GetResourcePoolByName(resourcePoolData.Name);
			if (existingResourcePool != null && (isNew || existingResourcePool.ID != resourcePoolData.PoolId))
			{
				errorMessage = $"Resource pool '{resourcePoolData.Name}' already exists with ID '{existingResourcePool.ID}'.";
				return false;
			}

			var resourcePool = isNew ? new ResourcePool() : srmHelpers.ResourceManagerHelper.GetResourcePool(resourcePoolData.PoolId) ?? new ResourcePool();
			resourcePool.Name = resourcePoolData.Name;
			resourcePool = srmHelpers.ResourceManagerHelper.AddOrUpdateResourcePools(resourcePool).First();

			var resourcePoolInternalPropertiesSection = instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Id.Id);
			resourcePoolInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolInternalProperties.Resource_Pool_Id, new ValueWrapper<string>(Convert.ToString(resourcePool.ID))));

			domHelper.DomInstances.Update(instance);

			return true;
		}

		private bool TryHandleDeprecateAction(DomHelper domHelper, DomInstance instance)
		{
			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			var srmHelpers = new SrmHelpers(engine);

			var resourcePoolData = resourceManagerHandler.ResourcePools.Single(x => x.Instance.ID.Id == instance.ID.Id);
			if (resourcePoolData.PoolId == Guid.Empty)
			{
				return true;
			}

			var resourcePool = srmHelpers.ResourceManagerHelper.GetResourcePool(resourcePoolData.PoolId);
			if (resourcePool == null)
			{
				return true;
			}

			if (!TryDeprecatePoolResource(srmHelpers, resourcePoolData.ResourceId))
			{
				return false;
			}

			var resourcesDataToDeprecate = GetResourcesDataToDeprecate(resourceManagerHandler, resourcePoolData);
			if (resourcesDataToDeprecate.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine("Do you want to deprecate all resources in the resource pool as well?");
				sb.AppendLine("Note: this action will not deprecate resources which are part of multiple pools.");

				var result = engine.ShowYesNoCancelDialog(sb.ToString());
				switch (result)
				{
					case YesNoCancelEnum.Yes:
						if (!TryDeprecateResourcesInPool(resourceManagerHandler, srmHelpers, resourcesDataToDeprecate))
						{
							return false;
						}

						break;

					case YesNoCancelEnum.No:
						break;

					case YesNoCancelEnum.Cancel:
					default:
						return false;
				}
			}

			return true;
		}

		private bool TryDeprecatePoolResource(SrmHelpers srmHelpers, Guid resourceId)
		{
			if (resourceId == Guid.Empty)
			{
				return true;
			}

			var poolResource = srmHelpers.ResourceManagerHelper.GetResource(resourceId);
			if (poolResource == null)
			{
				return true;
			}

			var filter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(poolResource.ID).AND(ReservationInstanceExposers.End.GreaterThan(DateTime.UtcNow));
			var reservations = srmHelpers.ResourceManagerHelper.GetReservationInstances(filter).ToList();
			if (!reservations.Any())
			{
				return true;
			}

			var sb = new StringBuilder();
			sb.AppendLine("The pool resource still has ongoing or future bookings on it. Are you sure you want to deprecate it?");
			sb.AppendLine("Note: deprecating it will not remove it from these bookings.");

			foreach (var reservation in reservations)
			{
				sb.AppendLine($"- {reservation.Name} | Start: {reservation.Start.ToLocalTime()} | End: {reservation.End.ToLocalTime()}");
			}

			var confirmed = engine.ShowConfirmDialog(sb.ToString());
			return confirmed;
		}

		private bool TryDeprecateResourcesInPool(ResourceManagerHandler resourceManagerHandler, SrmHelpers srmHelpers, List<ResourceData> resourcesDataToDeprecate)
		{
			var resourcesToDeprecate = GetResourcesToDeprecate(srmHelpers, resourcesDataToDeprecate);
			if (resourcesToDeprecate.Any())
			{
				var resourcesWithReservations = GetResourcesWithReservations(srmHelpers, resourcesToDeprecate);
				if (resourcesWithReservations.Any())
				{
					var sb = new StringBuilder();
					sb.AppendLine("The following resources still have ongoing or future bookings on them. Are you sure you want to deprecate them?");
					sb.AppendLine("Note: deprecating them will not remove them from these bookings.");

					foreach (var resource in resourcesWithReservations)
					{
						sb.AppendLine($"- {resource.Name}");
					}

					var confirmed = engine.ShowConfirmDialog(sb.ToString());
					if (!confirmed)
					{
						return false;
					}
				}

				SetResourcesAsUnavailable(srmHelpers, resourcesToDeprecate);
			}

			foreach (var resourceData in resourcesDataToDeprecate)
			{
				var transitionId = (resourceData.Instance.StatusId == Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Error) ? Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Error_To_Deprecated : Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Complete_To_Deprecated;

				resourceManagerHandler.DomHelper.DomInstances.DoStatusTransition(resourceData.Instance.ID, transitionId);
			}

			return true;
		}

		private void HandleDeleteAction(DomHelper domHelper, DomInstance instance)
		{
			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			var srmHelpers = new SrmHelpers(engine);

			var resourcePoolData = resourceManagerHandler.ResourcePools.Single(x => x.Instance.ID.Id == instance.ID.Id);
			if (resourcePoolData.PoolId != Guid.Empty)
			{
				var resourcePool = srmHelpers.ResourceManagerHelper.GetResourcePool(resourcePoolData.PoolId);
				if (resourcePool != null)
				{
					var poolResource = HandlePoolResourceDelete(srmHelpers, resourcePoolData.ResourceId);

					HandleResourceInPoolDelete(resourceManagerHandler, srmHelpers, resourcePoolData);

					if (poolResource != null)
					{
						DeleteResource(srmHelpers, poolResource);
					}

					srmHelpers.ResourceManagerHelper.RemoveResourcePools(resourcePool);
				}
			}

			resourceManagerHandler.DomHelper.DomInstances.Delete(resourcePoolData.Instance);
		}

		private Resource HandlePoolResourceDelete(SrmHelpers srmHelpers, Guid resourceId)
		{
			if (resourceId == Guid.Empty)
			{
				return null;
			}

			var resource = srmHelpers.ResourceManagerHelper.GetResource(resourceId);
			if (resource == null)
			{
				return null;
			}

			var filter = ReservationInstanceExposers.ResourceIDsInReservationInstance.Contains(resource.ID).AND(ReservationInstanceExposers.End.GreaterThan(DateTime.UtcNow));
			var reservations = srmHelpers.ResourceManagerHelper.GetReservationInstances(filter).ToList();
			if (reservations.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine($"Deleting resource '{resource.Name}' is not allowed because it is used in below bookings.");

				foreach (var reservation in reservations)
				{
					sb.AppendLine($"- {reservation.Name} | Start: {reservation.Start.ToLocalTime()} | End: {reservation.End.ToLocalTime()}");
				}

				engine.ShowErrorDialog(sb.ToString());
			}

			return resource;
		}

		private void HandleResourceInPoolDelete(ResourceManagerHandler resourceManagerHandler, SrmHelpers srmHelpers, ResourcePoolData resourcePoolData)
		{
			var poolResourcesData = GetPoolResourcesData(resourceManagerHandler, resourcePoolData);
			var deprecatedResourcesData = poolResourcesData.Where(x => x.Instance.StatusId == Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Deprecated).ToList();

			var areDeprecatedResourcesDeleted = false;
			if (deprecatedResourcesData.Any())
			{
				var sb = new StringBuilder();
				sb.AppendLine($"Do you want to delete all deprecated resources ({deprecatedResourcesData.Count}) in the resource pool as well?");

				var result = engine.ShowYesNoCancelDialog(sb.ToString());
				switch (result)
				{
					case YesNoCancelEnum.Yes:
						DeleteResourcesInPool(resourceManagerHandler, srmHelpers, deprecatedResourcesData);
						areDeprecatedResourcesDeleted = true;
						break;

					case YesNoCancelEnum.No:
						break;

					case YesNoCancelEnum.Cancel:
					default:
						return;
				}
			}

			var resourcesDataToUnassingPool = areDeprecatedResourcesDeleted ? poolResourcesData.Except(deprecatedResourcesData).ToList() : poolResourcesData;
			UnassignPoolFromResources(resourceManagerHandler, Convert.ToString(resourcePoolData.Instance.ID.Id), resourcesDataToUnassingPool);
		}

		private void DeleteResourcesInPool(ResourceManagerHandler resourceManagerHandler, SrmHelpers srmHelpers, List<ResourceData> resourcesData)
		{
			var resourceIds = resourcesData.Where(x => x.ResourceId != Guid.Empty).Select(x => x.ResourceId).ToList();
			if (resourceIds.Any())
			{
				var resourceFilter = new ORFilterElement<Resource>(resourceIds.Select(id => ResourceExposers.ID.Equal(id)).ToArray());
				var resourcesToDelete = new List<Resource>();
				foreach (var resource in srmHelpers.ResourceManagerHelper.GetResources(resourceFilter))
				{
					if (resource == null)
					{
						continue;
					}

					resourcesToDelete.Add(resource);
				}

				if (resourcesToDelete.Any())
				{
					var options = new ResourceDeleteOptions
					{
						Force = true,
						IgnoreCanceledReservations = true,
						IgnorePastReservation = true,
					};

					srmHelpers.ResourceManagerHelper.RemoveResources(resourcesToDelete.ToArray(), options);
				}
			}

			foreach (var resourceData in resourcesData)
			{
				resourceManagerHandler.DomHelper.DomInstances.Delete(resourceData.Instance);
			}
		}

		private void DeleteResource(SrmHelpers srmHelpers, Resource resource)
		{
			if (resource == null)
			{
				return;
			}

			var options = new ResourceDeleteOptions
			{
				Force = true,
				IgnoreCanceledReservations = true,
				IgnorePastReservation = true,
			};

			srmHelpers.ResourceManagerHelper.RemoveResources(new[] { resource }, options);
		}
	}
}