namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class CapabilityHandler
	{
		#region Fields
		private readonly ResourceManagerHandler resourceManagerHandler;

		private readonly SrmHelpers srmHelpers;

		private Dictionary<string, CapabilityData> capabilityDataByName;

		private Dictionary<Guid, List<CapabilityValueData>> capabilityValueDataByCapabilityId;

		private Dictionary<string, Skyline.DataMiner.Net.Profiles.Parameter> parametersByName;

		private List<CapabilityData> missing;

		private List<DataMapper> misconfigured;

		private bool validationOccured;
		#endregion

		public CapabilityHandler(ResourceManagerHandler resourceManagerHandler, SrmHelpers srmHelpers)
		{
			this.resourceManagerHandler = resourceManagerHandler ?? throw new ArgumentNullException(nameof(resourceManagerHandler));
			this.srmHelpers = srmHelpers ?? throw new ArgumentNullException(nameof(srmHelpers));
		}

		#region Properties
		public SynchronizationResult Result { get; private set; }
		#endregion

		#region Methods
		public void ValidateSynchronization()
		{
			var result = new SynchronizationResult("Capability");

			capabilityDataByName = resourceManagerHandler.Capabilities.ToDictionary(x => x.Name, x => x);
			capabilityValueDataByCapabilityId = resourceManagerHandler.CapabilityValues.GroupBy(x => x.CapabilityId).ToDictionary(x => x.Key, x => x.ToList());

			var profileParameters = GetProfileParameters();
			parametersByName = profileParameters.ToDictionary(x => x.Name, x => x);

			VerifyMissingParameters(profileParameters, result);

			misconfigured = new List<DataMapper>();
			foreach (var profileParameter in profileParameters)
			{
				VerifyParameterConfiguration(profileParameter, result);
			}

			validationOccured = true;
			Result = result;
		}

		public void EnsureSynchronization()
		{
			if (!validationOccured)
			{
				ValidateSynchronization();
			}

			CreateMissingParameters();
			UpdateMisconfiguredParameters();
		}

		private List<Skyline.DataMiner.Net.Profiles.Parameter> GetProfileParameters()
		{
			var filter = new ORFilterElement<Skyline.DataMiner.Net.Profiles.Parameter>(capabilityDataByName.Keys.Select(x => Skyline.DataMiner.Net.Profiles.ParameterExposers.Name.Equal(x)).ToArray());

			return srmHelpers.ProfileHelper.ProfileParameters.Read(filter);
		}

		private void VerifyMissingParameters(List<Skyline.DataMiner.Net.Profiles.Parameter> profileParameters, SynchronizationResult result)
		{
			missing = resourceManagerHandler.Capabilities.Where(x => !profileParameters.Select(y => y.Name).Contains(x.Name)).ToList();

			if (!missing.Any())
			{
				return;
			}

			foreach (var capabilityData in missing)
			{
				result.AddDeSyncDetail(capabilityData.Name, $"Profile Parameter does not exist");
			}
		}

		private void VerifyParameterConfiguration(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, SynchronizationResult result)
		{
			var isMisconfigured = false;

			var capabilityData = capabilityDataByName[profileParameter.Name];
			if (!capabilityValueDataByCapabilityId.TryGetValue(capabilityData.Instance.ID.Id, out var capabilityValueData))
			{
				return;
			}

			var clonedProfileParameter = new Skyline.DataMiner.Net.Profiles.Parameter(profileParameter);

			var discretes = clonedProfileParameter.GetDiscreets();
			var missingDiscretes = capabilityValueData.Select(x => x.Value).Except(discretes.Select(x => x.RawValue)).ToList();
			if (missingDiscretes.Any())
			{
				foreach (var discrete in missingDiscretes)
				{
					result.AddDeSyncDetail(capabilityData.Name, $"Discrete '{discrete}' is missing");

					discretes.Add(new Skyline.DataMiner.Net.ProfileManager.Objects.ProfileParameterDiscreet(discrete, discrete));
				}

				Skyline.DataMiner.Net.NaturalSortComparer comparer = new Skyline.DataMiner.Net.NaturalSortComparer();
				discretes.Sort((x, y) => comparer.Compare(x.RawValue, y.RawValue));

				clonedProfileParameter.SetDiscreets(discretes);

				isMisconfigured = true;
			}

			if (isMisconfigured)
			{
				misconfigured.Add(new DataMapper
				{
					CapabilityData = capabilityData,
					CapabilityValueData = capabilityValueData,
					OriginalParameter = profileParameter,
					CorrectedParameter = clonedProfileParameter,
				});
			}
		}

		private void CreateMissingParameters()
		{
			if (!missing.Any())
			{
				return;
			}

			foreach (var capabilityData in missing)
			{
				var profileParameter = new Skyline.DataMiner.Net.Profiles.Parameter
				{
					Name = capabilityData.Name,
					Categories = Skyline.DataMiner.Net.Profiles.ProfileParameterCategory.Capability,
				};

				if (!capabilityValueDataByCapabilityId.TryGetValue(capabilityData.Instance.ID.Id, out var capabilityValueData))
				{
					continue;
				}

				var discretes = capabilityValueData.Select(x => x.Value).ToList();
				Skyline.DataMiner.Net.NaturalSortComparer comparer = new Skyline.DataMiner.Net.NaturalSortComparer();
				discretes.Sort((x, y) => comparer.Compare(x, y));

				profileParameter.Type = Skyline.DataMiner.Net.Profiles.Parameter.ParameterType.Discrete;
				profileParameter.InterpreteType = new Skyline.DataMiner.Net.Profiles.InterpreteType
				{
					Type = Skyline.DataMiner.Net.Profiles.InterpreteType.TypeEnum.String,
					RawType = Skyline.DataMiner.Net.Profiles.InterpreteType.RawTypeEnum.Other,
				};
				profileParameter.Discretes = discretes;
				profileParameter.DiscreetDisplayValues = discretes;

				srmHelpers.ProfileHelper.ProfileParameters.Create(profileParameter);
			}
		}

		private void UpdateMisconfiguredParameters()
		{
			if (!misconfigured.Any())
			{
				return;
			}

			foreach (var mapping in misconfigured)
			{
				srmHelpers.ProfileHelper.ProfileParameters.Update(mapping.CorrectedParameter);
			}
		}
		#endregion

		#region Classes
		private sealed class DataMapper
		{
			public Skyline.DataMiner.Net.Profiles.Parameter OriginalParameter { get; set; }

			public Skyline.DataMiner.Net.Profiles.Parameter CorrectedParameter { get; set; }

			public CapabilityData CapabilityData { get; set; }

			public List<CapabilityValueData> CapabilityValueData { get; set; }
		}
		#endregion
	}
}
