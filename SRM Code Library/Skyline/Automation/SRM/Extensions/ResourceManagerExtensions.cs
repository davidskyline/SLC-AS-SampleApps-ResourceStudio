namespace Skyline.Automation.SRM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public static class ResourceManagerExtensions
	{
		public static ResourcePool GetResourcePoolByName(this ResourceManagerHelper resourceManagerHelper, string name)
		{
			return resourceManagerHelper.GetResourcePools(new ResourcePool { Name = name }).FirstOrDefault();
		}

		public static Resource GetResourceByName(this ResourceManagerHelper resourceManagerHelper, string name)
		{
			return resourceManagerHelper.GetResources(ResourceExposers.Name.Equal(name)).FirstOrDefault();
		}

		public static List<Resource> GetResourcesByNames(this ResourceManagerHelper resourceManagerHelper, List<string> resourceNames)
		{
			var filter = new ORFilterElement<Resource>(resourceNames.Select(name => ResourceExposers.Name.Equal(name)).ToArray());

			return resourceManagerHelper.GetResources(filter).ToList();
		}

		public static List<Resource> GetResourcesFromPool(this ResourceManagerHelper resourceManagerHelper, Guid poolId)
		{
			return resourceManagerHelper.GetResources(ResourceExposers.PoolGUIDs.Contains(poolId)).ToList();
		}
	}
}