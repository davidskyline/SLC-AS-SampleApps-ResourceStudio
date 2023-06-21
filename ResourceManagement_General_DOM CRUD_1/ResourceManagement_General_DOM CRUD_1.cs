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

	using Skyline.Automation.DOM;
	using Skyline.Automation.DOM.DomIds;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.History;
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
			engine.ExitFail("This script should be executed using the 'OnDomInstanceCrud' entry point");
		}

		[AutomationEntryPoint(AutomationEntryPointType.Types.OnDomInstanceCrud)]
		public void OnDomInstanceCrud(Engine engine, Guid id, CrudType crudType)
		{
			this.engine = engine;

			try
			{
				RunSafe(id, crudType);
			}
			catch (Exception ex)
			{
				engine.Log(ex.ToString());
				engine.ExitFail(ex.Message);
			}
		}

		private void RunSafe(Guid id, CrudType crudType)
		{
			var domHelper = new DomHelper(engine.SendSLNetMessages, Resourcemanagement.ModuleId);
			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(id)).Single();

			var mapper = new Dictionary<Guid, Action<CrudType, DomHelper, DomInstance>>
			{
				[Resourcemanagement.Definitions.Resourcepool.Id] = HandleResourcePoolCRUD,
				[Resourcemanagement.Definitions.Resource.Id] = HandleResourceCRUD,
			};

			if (!mapper.TryGetValue(domInstance.DomDefinitionId.Id, out var action))
			{
				return;
			}

			action.Invoke(crudType, domHelper, domInstance);
		}

		private void HandleResourcePoolCRUD(CrudType crudType, DomHelper domHelper, DomInstance instance)
		{
			if (crudType != CrudType.Update || instance.StatusId != Resourcemanagement.Behaviors.Resourcepool_Behavior.Statuses.Complete)
			{
				return;
			}

			var resourcePoolHandler = new ResourcePoolHandler(engine, domHelper, instance);
			if (!resourcePoolHandler.ValidateNameChange())
			{
				return;
			}

			resourcePoolHandler.ValidateResources();
		}

		private void HandleResourceCRUD(CrudType crudType, DomHelper domHelper, DomInstance instance)
		{
			if (crudType != CrudType.Update || instance.StatusId != Resourcemanagement.Behaviors.Resource_Behavior.Statuses.Complete)
			{
				return;
			}

			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			var srmHelpers = new SrmHelpers(engine);

			var resourceData = resourceManagerHandler.Resources.Single(x => x.Instance.ID.Id == instance.ID.Id);
			var existingResource = srmHelpers.ResourceManagerHelper.GetResourceByName(resourceData.Name);
			if (existingResource != null)
			{
				if (existingResource.ID != resourceData.ResourceId)
				{
					SetErrorMessage($"Resource '{resourceData.Name}' already exists with ID '{existingResource.ID}'.");

					return;
				}

				if (existingResource.Name == resourceData.Name)
				{
					return;
				}
			}

			var resource = srmHelpers.ResourceManagerHelper.GetResource(resourceData.ResourceId);
			if (resource == null)
			{
				SetErrorMessage($"Resource '{resourceData.Name}' with ID '{resourceData.ResourceId}' does not exist.");

				return;
			}

			resource.Name = resourceData.Name;
			srmHelpers.ResourceManagerHelper.AddOrUpdateResources(resource);

			void SetErrorMessage(string errorMessage)
			{
				var resourceInfoSection = instance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourceInfo.Id.Id);
				resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Resourcemanagement.Sections.ResourceInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> {errorMessage}")));

				domHelper.DomInstances.Update(instance);
				domHelper.DomInstances.DoStatusTransition(instance.ID, Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Complete_To_Error);
			}
		}
	}
}