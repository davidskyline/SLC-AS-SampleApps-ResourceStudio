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

05/07/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;

	using Skyline.Automation.DOM;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	[GQIMetaData(Name = "Resource Management - Get Resource Capacities")]
	public class ResourceManagementDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		private readonly GQIStringArgument resourceIdArg = new GQIStringArgument("Resource ID") { IsRequired = true };

		private DomHelper domHelper;

		private Guid resourceId;

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Capacity ID"),
				new GQIStringColumn("Capacity Name"),
				new GQIDoubleColumn("Capacity Value"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { resourceIdArg };
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return new GQIPage(GetResourceCapacities())
			{
				HasNextPage = false,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			resourceId = Guid.Parse(args.GetArgumentValue(resourceIdArg));

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			domHelper = new DomHelper(args.DMS.SendMessages, Skyline.Automation.DOM.DomIds.Resourcemanagement.ModuleId);

			return default;
		}

		private GQIRow[] GetResourceCapacities()
		{
			var rows = new List<GQIRow>();

			var resourceManagerHandler = new ResourceManagerHandler(domHelper);

			var resource = resourceManagerHandler.Resources.SingleOrDefault(x => x.Instance.ID.Id.Equals(resourceId));
			if (resource == null)
			{
				return rows.ToArray();
			}

			var capacitiesById = resourceManagerHandler.Capacities.ToDictionary(x => x.Instance.ID.Id, x => x);

			foreach (var resourceCapacity in resource.Capacities)
			{
				if (!capacitiesById.TryGetValue(resourceCapacity.CapacityId, out var capacityData))
				{
					continue;
				}

				rows.Add(new GQIRow(new[]
				{
					new GQICell{Value = Convert.ToString(capacityData.Instance.ID.Id)},
					new GQICell{Value = capacityData.Name},
					new GQICell{Value = resourceCapacity.Value},
				}));
			}

			return rows.ToArray();
		}
	}
}