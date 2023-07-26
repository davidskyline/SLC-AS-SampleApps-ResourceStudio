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

10/07/2023	1.0.0.1		JVW, Skyline	Initial version
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

	[GQIMetaData(Name = "Resource Management - Get Resource Capabilities")]
	public class ResourceManagementDataSource : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		private readonly GQIStringArgument resourceIdArg = new GQIStringArgument("Resource ID") { IsRequired = true };

		private DomHelper domHelper;

		private Guid resourceId;

		private Dictionary<Guid, List<ResourcePoolCapability>> poolCapabilitiesByPoolDomInstanceId;

		private Dictionary<Guid, CapabilityData> capabilitiesById;

		private Dictionary<Guid, CapabilityValueData> capabilityValuesById;

		public GQIColumn[] GetColumns()
		{
			return new[]
			{
				new GQIStringColumn("Capability ID"),
				new GQIStringColumn("Capability Name"),
				new GQIStringColumn("Capability Value"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { resourceIdArg };
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return new GQIPage(GetResourceCapabilities())
			{
				HasNextPage = false
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

		private static List<ConfiguredCapability> MergeCapabilities(List<ConfiguredCapability> configuredCapabilities)
		{
			var mergedCapabilitiesById = new Dictionary<Guid, ConfiguredCapability>();

			foreach (var configuredCapability in configuredCapabilities)
			{
				if (configuredCapability is ConfiguredStringCapability)
				{
					continue;
				}

				if (configuredCapability is ConfiguredEnumCapability configuredEnumCapability)
				{
					if (!mergedCapabilitiesById.TryGetValue(configuredEnumCapability.CapabilityData.Instance.ID.Id, out var mergedCapability))
					{
						mergedCapability = new ConfiguredEnumCapability(configuredCapability.CapabilityData);

						mergedCapabilitiesById.Add(mergedCapability.CapabilityData.Instance.ID.Id, mergedCapability);
					}

					if (!(mergedCapability is ConfiguredEnumCapability mergedEnumCapability))
					{
						continue;
					}

					configuredEnumCapability.Discretes.ForEach(x =>
					{
						if (!mergedEnumCapability.Discretes.Contains(x))
						{
							mergedEnumCapability.Discretes.Add(x);
						}
					});
				}
			}

			return mergedCapabilitiesById.Values.ToList();
		}

		private GQIRow[] GetResourceCapabilities()
		{
			var rows = new List<GQIRow>();

			var resourceManagerHandler = new ResourceManagerHandler(domHelper);

			var resource = resourceManagerHandler.Resources.SingleOrDefault(x => x.Instance.ID.Id.Equals(resourceId));
			if (resource == null || string.IsNullOrEmpty(resource.PoolIds))
			{
				return rows.ToArray();
			}

			poolCapabilitiesByPoolDomInstanceId = resourceManagerHandler.ResourcePools.ToDictionary(x => x.Instance.ID.Id, x => x.Capabilities);
			capabilitiesById = resourceManagerHandler.Capabilities.ToDictionary(x => x.Instance.ID.Id, x => x);
			capabilityValuesById = resourceManagerHandler.CapabilityValues.ToDictionary(x => x.Instance.ID.Id, x => x);

			var configuredResourceCapabilities = GetConfiguredResourceCapabilities(resource);
			var mergedCapabilities = MergeCapabilities(configuredResourceCapabilities);

			foreach (var mergedCapability in mergedCapabilities)
			{
				if (!(mergedCapability is ConfiguredEnumCapability mergedEnumCapability))
				{
					continue;
				}

				mergedEnumCapability.Discretes.ForEach(x =>
				{
					rows.Add(new GQIRow(new[]
					{
						new GQICell{Value = Convert.ToString(mergedEnumCapability.CapabilityData.Instance.ID.Id)},
						new GQICell{Value = mergedEnumCapability.CapabilityData.Name},
						new GQICell{Value = x},
					}));
				});
			}

			return rows.ToArray();
		}

		private bool TryGetConfiguredCapabilities(Guid poolDomInstanceId, out List<ConfiguredCapability> configuredCapabilities)
		{
			configuredCapabilities = new List<ConfiguredCapability>();

			if (!poolCapabilitiesByPoolDomInstanceId.TryGetValue(poolDomInstanceId, out var capabilities))
			{
				return false;
			}

			configuredCapabilities = new List<ConfiguredCapability>();
			foreach (var poolCapability in capabilities)
			{
				if (!capabilitiesById.TryGetValue(poolCapability.CapabilityId, out var capabilityData))
				{
					continue;
				}

				ConfiguredCapability configuredCapability;
				if (capabilityData.CapabilityType == Skyline.Automation.DOM.DomIds.Resourcemanagement.Enums.CapabilityType.String)
				{
					configuredCapability = new ConfiguredStringCapability(capabilityData, poolCapability.CapabilityStringValue);
				}
				else
				{
					var discretes = new List<string>();

					poolCapability.CapabilityEnumValueIds.ForEach(x =>
					{
						if (capabilityValuesById.TryGetValue(x, out var capabilityValueData) && capabilityValueData.CapabilityId.Equals(capabilityData.Instance.ID.Id))
						{
							discretes.Add(capabilityValueData.Value);
						}
					});

					configuredCapability = new ConfiguredEnumCapability(capabilityData, discretes);
				}

				configuredCapabilities.Add(configuredCapability);
			}

			return true;
		}

		private List<ConfiguredCapability> GetConfiguredResourceCapabilities(ResourceData resource)
		{
			var configuredResourceCapabilities = new List<ConfiguredCapability>();
			foreach (var poolDomInstanceId in resource.PoolIds.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => Guid.Parse(x)))
			{
				if (!TryGetConfiguredCapabilities(poolDomInstanceId, out var configuredPoolCapabilities))
				{
					continue;
				}

				configuredResourceCapabilities.AddRange(configuredPoolCapabilities);
			}

			return configuredResourceCapabilities;
		}
	}
}