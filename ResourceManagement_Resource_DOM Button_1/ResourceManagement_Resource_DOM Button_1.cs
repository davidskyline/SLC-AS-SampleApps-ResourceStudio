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

11/05/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Resources;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Actions;
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
			catch (Exception ex)
			{
				engine.Log(ex.ToString());
				engine.ExitFail(ex.Message);
			}
		}

		private static void UpdateErrorMessage(DomHelper domHelper, DomInstance instance, string errorMessage)
		{
			var resourceInfoSection = instance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.Id.Id);
			resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> {errorMessage}")));

			domHelper.DomInstances.Update(instance);
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
					if (TryHandleMarkCompleteAction(domHelper, instanceId, out errorMessage))
					{
						transitionId = (domInstance.StatusId == Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Draft) ? Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Draft_To_Complete : Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Error_To_Complete;
					}
					else if (domInstance.StatusId != Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Error)
					{
						transitionId = Skyline.Automation.DOM.DomIds.Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Draft_To_Error;
					}
					else
					{
						// Do nothing
					}

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

		private bool TryHandleMarkCompleteAction(DomHelper domHelper, DomInstanceId instanceId, out string errorMessage)
		{
			var supportedResourceTypes = new List<Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type>
			{
				Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.UnlinkedResource,
				Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.Element,
			};

			errorMessage = string.Empty;

			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			var srmHelpers = new SrmHelpers(engine);

			var resourceData = resourceManagerHandler.Resources.Single(x => x.Instance.ID.Id == instanceId.Id);
			if (resourceData.ResourceType == null || !supportedResourceTypes.Contains((Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type)resourceData.ResourceType))
			{
				errorMessage = $"Resource Type '{resourceData.ResourceType}' is not supported.";
				return false;
			}

			var isNew = resourceData.ResourceId == Guid.Empty;
			var existingResource = srmHelpers.ResourceManagerHelper.GetResourceByName(resourceData.Name);
			if (existingResource != null && (isNew || existingResource.ID != resourceData.ResourceId))
			{
				errorMessage = $"Resource '{resourceData.Name}' already exists with ID '{existingResource.ID}'.";
				return false;
			}

			var resource = srmHelpers.ResourceManagerHelper.GetResource(resourceData.ResourceId) ?? new Resource();
			resource.Name = resourceData.Name;

			if (resourceData.ResourceType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.Type.Element)
			{
				if (string.IsNullOrEmpty(resourceData.LinkedElementInfo))
				{
					errorMessage = "Element link is required.";
					return false;
				}

				var splittedElementInfo = resourceData.LinkedElementInfo.Split('/');
				resource.DmaID = Convert.ToInt32(splittedElementInfo[0]);
				resource.ElementID = Convert.ToInt32(splittedElementInfo[1]);
			}

			resource = srmHelpers.ResourceManagerHelper.AddOrUpdateResources(resource).Single();

			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(instanceId.Id)).Single();

			var resourceInternalPropertiesSection = domInstance.Sections.Single(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Id.Id);
			resourceInternalPropertiesSection.AddOrReplaceFieldValue(new FieldValue(Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourceInternalProperties.Resource_Id, new ValueWrapper<Guid>(resource.ID)));

			domHelper.DomInstances.Update(domInstance);

			return true;
		}
	}
}