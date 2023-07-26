namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.Automation.DOM;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public class CapacityHandler
	{
		#region Fields
		private readonly ResourceManagerHandler resourceManagerHandler;

		private readonly SrmHelpers srmHelpers;

		private Dictionary<string, CapacityData> capacityDataByName;

		private List<CapacityData> missing;

		private List<DataMapper> misconfigured;

		private bool validationOccured;
		#endregion

		public CapacityHandler(ResourceManagerHandler resourceManagerHandler, SrmHelpers srmHelpers)
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
			var result = new SynchronizationResult("Capacity");

			capacityDataByName = resourceManagerHandler.Capacities.ToDictionary(x => x.Name, x => x);

			var profileParameters = GetProfileParameters();

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

		private static bool VerifyParameterConfiguration_Units(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, CapacityData capacityData, SynchronizationResult result)
		{
			if (profileParameter.Units.Equals(capacityData.Units))
			{
				return true;
			}
			else if (string.IsNullOrEmpty(profileParameter.Units))
			{
				result.AddDeSyncDetail(capacityData.Name, $"No units configured ['{capacityData.Units}']");
			}
			else if (string.IsNullOrEmpty(capacityData.Units))
			{
				result.AddDeSyncDetail(capacityData.Name, $"Units configured ['{profileParameter.Units}']");
			}
			else
			{
				result.AddDeSyncDetail(capacityData.Name, $"Wrong units configured ['{profileParameter.Units}' --> '{capacityData.Units}']");
			}

			profileParameter.Units = capacityData.Units;

			return false;
		}

		private static bool VerifyParameterConfiguration_RangeMin(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, CapacityData capacityData, SynchronizationResult result)
		{
			var shouldBeConfigured = capacityData.RangeMin.HasValue;
			var value = shouldBeConfigured ? capacityData.RangeMin.Value : double.NaN;

			if (profileParameter.RangeMin.Equals(value))
			{
				return true;
			}
			else if (double.IsNaN(profileParameter.RangeMin))
			{
				result.AddDeSyncDetail(capacityData.Name, $"No range min configured ['{value}']");
			}
			else if (double.IsNaN(value))
			{
				result.AddDeSyncDetail(capacityData.Name, $"Range min configured ['{profileParameter.RangeMin}']");
			}
			else
			{
				result.AddDeSyncDetail(capacityData.Name, $"Wrong range min configured ['{profileParameter.RangeMin}' --> '{value}']");
			}

			profileParameter.RangeMin = value;

			return false;
		}

		private static bool VerifyParameterConfiguration_RangeMax(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, CapacityData capacityData, SynchronizationResult result)
		{
			var shouldBeConfigured = capacityData.RangeMax.HasValue;
			var value = shouldBeConfigured ? capacityData.RangeMax.Value : double.NaN;

			if (profileParameter.RangeMax.Equals(value))
			{
				return true;
			}
			else if (double.IsNaN(profileParameter.RangeMax))
			{
				result.AddDeSyncDetail(capacityData.Name, $"No range max configured ['{value}']");
			}
			else if (double.IsNaN(value))
			{
				result.AddDeSyncDetail(capacityData.Name, $"Range max configured ['{profileParameter.RangeMax}']");
			}
			else
			{
				result.AddDeSyncDetail(capacityData.Name, $"Wrong range max configured ['{profileParameter.RangeMax}' --> '{value}']");
			}

			profileParameter.RangeMax = value;

			return false;
		}

		private static bool VerifyParameterConfiguration_Stepsize(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, CapacityData capacityData, SynchronizationResult result)
		{
			var shouldBeConfigured = capacityData.StepSize.HasValue;
			var value = shouldBeConfigured ? capacityData.StepSize.Value : double.NaN;

			if (profileParameter.Stepsize.Equals(value))
			{
				return true;
			}
			else if (double.IsNaN(profileParameter.Stepsize))
			{
				result.AddDeSyncDetail(capacityData.Name, $"No step size configured ['{value}']");
			}
			else if (double.IsNaN(value))
			{
				result.AddDeSyncDetail(capacityData.Name, $"Step size configured ['{profileParameter.Stepsize}']");
			}
			else
			{
				result.AddDeSyncDetail(capacityData.Name, $"Wrong step size configured ['{profileParameter.Stepsize}' --> '{value}']");
			}

			profileParameter.Stepsize = value;

			return false;
		}

		private static bool VerifyParameterConfiguration_Decimals(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, CapacityData capacityData, SynchronizationResult result)
		{
			var shouldBeConfigured = capacityData.Decimals.HasValue;
			var value = shouldBeConfigured ? (int)capacityData.Decimals.Value : int.MaxValue;

			if (profileParameter.Decimals.Equals(value))
			{
				return true;
			}
			else if (profileParameter.Decimals == int.MaxValue)
			{
				result.AddDeSyncDetail(capacityData.Name, $"No decimals configured ['{value}']");
			}
			else if (value == int.MaxValue)
			{
				result.AddDeSyncDetail(capacityData.Name, $"Decimals configured ['{profileParameter.Decimals}']");
			}
			else
			{
				result.AddDeSyncDetail(capacityData.Name, $"Wrong decimals configured ['{profileParameter.Decimals}' --> '{value}']");
			}

			profileParameter.Decimals = value;

			return false;
		}

		private List<Skyline.DataMiner.Net.Profiles.Parameter> GetProfileParameters()
		{
			var filter = new ORFilterElement<Skyline.DataMiner.Net.Profiles.Parameter>(capacityDataByName.Keys.Select(x => Skyline.DataMiner.Net.Profiles.ParameterExposers.Name.Equal(x)).ToArray());

			return srmHelpers.ProfileHelper.ProfileParameters.Read(filter);
		}

		private void VerifyMissingParameters(List<Skyline.DataMiner.Net.Profiles.Parameter> profileParameters, SynchronizationResult result)
		{
			missing = resourceManagerHandler.Capacities.Where(x => !profileParameters.Select(y => y.Name).Contains(x.Name)).ToList();

			if (!missing.Any())
			{
				return;
			}

			foreach (var capacityData in missing)
			{
				result.AddDeSyncDetail(capacityData.Name, $"Profile Parameter does not exist");
			}
		}

		private void VerifyParameterConfiguration(Skyline.DataMiner.Net.Profiles.Parameter profileParameter, SynchronizationResult result)
		{
			var isMisconfigured = false;

			var capacityData = capacityDataByName[profileParameter.Name];

			var clonedProfileParameter = new Skyline.DataMiner.Net.Profiles.Parameter(profileParameter);
			if (!VerifyParameterConfiguration_Units(clonedProfileParameter, capacityData, result) || !VerifyParameterConfiguration_RangeMin(clonedProfileParameter, capacityData, result) || !VerifyParameterConfiguration_RangeMax(clonedProfileParameter, capacityData, result))
			{
				isMisconfigured = true;
			}

			if (!VerifyParameterConfiguration_Stepsize(clonedProfileParameter, capacityData, result) || !VerifyParameterConfiguration_Decimals(clonedProfileParameter, capacityData, result))
			{
				isMisconfigured = true;
			}

			if (isMisconfigured)
			{
				misconfigured.Add(new DataMapper
				{
					CapacityData = capacityData,
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

			foreach (var capacityData in missing)
			{
				var profileParameter = new Skyline.DataMiner.Net.Profiles.Parameter
				{
					Name = capacityData.Name,
					Units = capacityData.Units,
					Categories = Skyline.DataMiner.Net.Profiles.ProfileParameterCategory.Capacity,
					Type = Skyline.DataMiner.Net.Profiles.Parameter.ParameterType.Number,
				};

				if (capacityData.RangeMin.HasValue)
				{
					profileParameter.RangeMin = capacityData.RangeMin.Value;
				}

				if (capacityData.RangeMax.HasValue)
				{
					profileParameter.RangeMax = capacityData.RangeMax.Value;
				}

				if (capacityData.StepSize.HasValue)
				{
					profileParameter.Stepsize = capacityData.StepSize.Value;
				}

				if (capacityData.Decimals.HasValue)
				{
					profileParameter.Decimals = (int)capacityData.Decimals.Value;
				}

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

			public CapacityData CapacityData { get; set; }
		}
		#endregion
	}
}
