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

13/07/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private IEngine engine;

		private InputData inputData;

		private InteractiveController controller;

		private IAS.Dialogs.CreateFunctionResource.CreateFunctionResourceView createFunctionResourceView;
		private IAS.Dialogs.CreateFunctionResource.CreateFunctionResourcePresenter createFunctionResourcePresenter;

		private IAS.ScriptData scriptData;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			this.engine = engine;

			try
			{
				RunSafe();
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				if (IsOutputDataRequired())
				{
					SetOutputData(false, e.Message);
				}
				else
				{
					engine.ExitFail(e.Message);
				}

				engine.Log(e.ToString());
			}
		}

		private void RunSafe()
		{
			// engine.ShowUI()
			inputData = JsonConvert.DeserializeObject<InputData>(engine.GetScriptParam("Input Data").Value);
			controller = new InteractiveController(engine);

			scriptData = new IAS.ScriptData(engine, inputData.ResourceName);
			
			InitFields();
			InitEventHandlers();

			createFunctionResourcePresenter.LoadFromModel();
			createFunctionResourcePresenter.BuildView();

			controller.Run(createFunctionResourceView);
		}

		private void InitFields()
		{
			createFunctionResourceView = new IAS.Dialogs.CreateFunctionResource.CreateFunctionResourceView(engine);
			createFunctionResourcePresenter = new IAS.Dialogs.CreateFunctionResource.CreateFunctionResourcePresenter(createFunctionResourceView, scriptData);
		}

		private void InitEventHandlers()
		{
			InitCreateFunctionResourceEventHandlers();
		}

		private void InitCreateFunctionResourceEventHandlers()
		{
			createFunctionResourcePresenter.Close += (sender, args) =>
			{
				if (IsOutputDataRequired())
				{
					SetOutputData(false);
				}

				engine.ExitSuccess(string.Empty);
			};

			createFunctionResourcePresenter.Create += (sender, args) =>
			{
				if (IsOutputDataRequired())
				{
					SetOutputData(true);
				}

				engine.ExitSuccess(string.Empty);
			};
		}

		private bool IsOutputDataRequired()
		{
			if (inputData.OutputConfiguration == null || !inputData.OutputConfiguration.ReturnData || string.IsNullOrEmpty(inputData.OutputConfiguration.VariableName))
			{
				return false;
			}

			return true;
		}

		private void SetOutputData(bool isSuccess, string errorReason = "")
		{
			var outputData = new OutputData
			{
				IsSuccess = isSuccess,
			};

			if (isSuccess)
			{
				switch (inputData.OutputConfiguration.OutputData)
				{
					case OutputDataType.ResourceId:
						outputData.ResourceId = scriptData.CreatedResourceId;
						break;

					case OutputDataType.None:
					default:
						// Do nothing
						break;
				}
			}
			else
			{
				outputData.ErrorReason = errorReason;
			}

			engine.AddOrUpdateScriptOutput(inputData.OutputConfiguration.VariableName, JsonConvert.SerializeObject(outputData));
		}
	}
}