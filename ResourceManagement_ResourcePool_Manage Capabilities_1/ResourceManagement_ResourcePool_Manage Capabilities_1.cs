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

15/06/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.Automation.IAS;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private IEngine engine;

		private InteractiveController controller;

		private IAS.Dialogs.ManageCapabilities.ManageCapabilitiesView manageCapabilitiesView;
		private IAS.Dialogs.ManageCapabilities.ManageCapabilitiesPresenter manageCapabilitiesPresenter;

		private IAS.Dialogs.ConfigureCapability.ConfigureCapabilityView configureCapabilityView;
		private IAS.Dialogs.ConfigureCapability.ConfigureCapabilityPresenter configureCapabilityPresenter;

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
				engine.ShowErrorDialog(e.ToString());
			}
		}

		private void RunSafe()
		{
			// engine.ShowUI()
			var resourcePoolDomInstanceId = JsonConvert.DeserializeObject<List<Guid>>(engine.GetScriptParam("Resource Pool").Value).Single();

			controller = new InteractiveController(engine);

			scriptData = new IAS.ScriptData(engine, resourcePoolDomInstanceId);

			InitFields();
			InitEventHandlers();

			manageCapabilitiesPresenter.LoadFromModel();
			manageCapabilitiesPresenter.BuildView();

			controller.Run(manageCapabilitiesView);
		}

		private void InitFields()
		{
			manageCapabilitiesView = new IAS.Dialogs.ManageCapabilities.ManageCapabilitiesView(engine);
			manageCapabilitiesPresenter = new IAS.Dialogs.ManageCapabilities.ManageCapabilitiesPresenter(manageCapabilitiesView, scriptData);

			configureCapabilityView = new IAS.Dialogs.ConfigureCapability.ConfigureCapabilityView(engine);
			configureCapabilityPresenter = new IAS.Dialogs.ConfigureCapability.ConfigureCapabilityPresenter(configureCapabilityView, scriptData);
		}

		private void InitEventHandlers()
		{
			InitManageCapabilitiesEventHandlers();
			InitConfigureCapabilityEventHandlers();
		}

		private void InitManageCapabilitiesEventHandlers()
		{
			manageCapabilitiesPresenter.Close += (sender, args) =>
			{
				engine.ExitSuccess(string.Empty);
			};

			manageCapabilitiesPresenter.Configure += (sender, args) =>
			{
				configureCapabilityPresenter.LoadFromModel();
				configureCapabilityPresenter.BuildView();

				controller.ShowDialog(configureCapabilityView);
			};

			manageCapabilitiesPresenter.UpdateNotPossible += (sender, args) =>
			{
				engine.ShowErrorDialog($"Not allowed to update or delete capabilities because resource pool '{scriptData.PoolName}' contains resources which are having ongoing or future bookings on it.");
			};
		}

		private void InitConfigureCapabilityEventHandlers()
		{
			configureCapabilityPresenter.Close += (sender, args) =>
			{
				controller.ShowDialog(manageCapabilitiesView);
			};
		}
	}
}