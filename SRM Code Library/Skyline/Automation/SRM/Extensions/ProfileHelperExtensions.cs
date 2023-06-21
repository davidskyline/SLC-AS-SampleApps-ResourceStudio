namespace Skyline.Automation.SRM
{
	using System;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Profiles;

	using Parameter = Skyline.DataMiner.Net.Profiles.Parameter;

	public static class ProfileHelperExtensions
	{
		public static Parameter GetProfileParameterByName(this ProfileHelper profileHelper, string name)
		{
			return profileHelper.ProfileParameters.Read(ParameterExposers.Name.Equal(name)).FirstOrDefault();
		}

		public static Parameter GetProfileParameterById(this ProfileHelper profileHelper, Guid id)
		{
			return profileHelper.ProfileParameters.Read(ParameterExposers.ID.Equal(id)).FirstOrDefault();
		}
	}
}
