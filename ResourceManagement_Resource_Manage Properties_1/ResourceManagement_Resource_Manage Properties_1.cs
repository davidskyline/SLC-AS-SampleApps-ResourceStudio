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

23/06/2023	1.0.0.1		JVW, Skyline	Initial version
****************************************************************************
*/

namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
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

		private IAS.Dialogs.ManageProperties.ManagePropertiesView managePropertiesView;
		private IAS.Dialogs.ManageProperties.ManagePropertiesPresenter managePropertiesPresenter;

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
			var resourceDomInstanceId = JsonConvert.DeserializeObject<List<Guid>>(engine.GetScriptParam("Resource").Value).Single();

			controller = new InteractiveController(engine);

			scriptData = new IAS.ScriptData(engine, resourceDomInstanceId);

			InitFields();
			InitEventHandlers();

			managePropertiesPresenter.LoadFromModel();
			managePropertiesPresenter.BuildView();

			controller.Run(managePropertiesView);
		}

		private void InitFields()
		{
			managePropertiesView = new IAS.Dialogs.ManageProperties.ManagePropertiesView(engine);
			managePropertiesPresenter = new IAS.Dialogs.ManageProperties.ManagePropertiesPresenter(managePropertiesView, scriptData);
		}

		private void InitEventHandlers()
		{
			InitManagePropertiesEventHandlers();
		}

		private void InitManagePropertiesEventHandlers()
		{
			managePropertiesPresenter.Close += (sender, args) =>
			{
				engine.ExitSuccess(string.Empty);
			};
		}
	}
}