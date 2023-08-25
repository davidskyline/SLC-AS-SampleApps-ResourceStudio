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

16/06/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using static Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums;

	[GQIMetaData(Name = "Resource Management - Get Resource Pool Links")]
	public class ResourceManagementDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{

		private DomHelper domHelper;

		public GQIColumn[] GetColumns()
		{
			return new[]
			{
				new GQIStringColumn("Resource Pool ID"),
				new GQIStringColumn("Resource Pool Name"),
				new GQIStringColumn("Linked Resource Pool ID"),
				new GQIStringColumn("Linked Resource Pool Name"),
				new GQIStringColumn("Resource Selection Type"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { };
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return new GQIPage(GetResourcePoolLinks())
			{
				HasNextPage = false
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			domHelper = new DomHelper(args.DMS.SendMessages, Skyline.Automation.DOM.DomIds.Resourcemanagement.ModuleId);

			return default;
		}

		private GQIRow[] GetResourcePoolLinks()
		{
			string debug1, debug2, debug3, debug4;
			debug1 = debug2 = debug3 = debug4 = "";
			var rows = new List<GQIRow>();

			var resourceManagerHandler = new ResourceManagerHandler(domHelper);
			
			var resourcePools = resourceManagerHandler.DomHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(Skyline.Automation.DOM.DomIds.Resourcemanagement.Definitions.Resourcepool.Id));

			if (resourcePools == null)
			{
				return rows.ToArray();
			}

			foreach (var resourcePool in resourcePools)
			{

				var poollinks = resourcePool.Sections.Where(x => x.SectionDefinitionID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolLinks.Id.Id);

				if (poollinks != null && poollinks.Any())
				{
					foreach (var poolLink in poollinks)
					{
						var linkedResourcePoolId = poolLink.FieldValues.Where(x => x.FieldDescriptorID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolLinks.LinkedResourcePool.Id).FirstOrDefault();
						
						if (linkedResourcePoolId != null) {

							var linkedResourcePool = resourceManagerHandler.DomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(Guid.Parse(linkedResourcePoolId.Value.Value.ToString()))).Single();
							var resourceSelectionType = poolLink.FieldValues.Where(x => x.FieldDescriptorID.Id == Skyline.Automation.DOM.DomIds.Resourcemanagement.Sections.ResourcePoolLinks.ResourceSelectionType.Id).FirstOrDefault();

							rows.Add(new GQIRow(new[]
							{
								new GQICell{Value = Convert.ToString(resourcePool.ID)},
								new GQICell{Value = resourcePool.Name},
								new GQICell{Value = Convert.ToString(linkedResourcePoolId.Value.Value)},
								new GQICell{Value = Convert.ToString(linkedResourcePool.Name)},
								new GQICell{Value = Convert.ToString((ResourceSelectionType) resourceSelectionType.Value.Value) },
							}));
						}
						
					}
				} 

			}

			/*rows.Add(new GQIRow(new[]
			{
					new GQICell{Value = debug1},
					new GQICell{Value = debug2},
					new GQICell{Value = debug3},
					new GQICell{Value = debug4 },
					new GQICell{Value = debug4 },
				}));*/

			return rows.ToArray();
		}
	}
}