namespace Skyline.Automation.SRM
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Profiles;

	public class SrmHelpers
	{
		private readonly IEngine engine;

		public SrmHelpers(IEngine engine)
		{
			this.engine = engine;

			Init();
		}

		public ServiceManagerHelper ServiceManagerHelper { get; private set; }

		public ResourceManagerHelper ResourceManagerHelper { get; private set; }

		public ProtocolFunctionHelper ProtocolFunctionHelper { get; private set; }

		public ProfileHelper ProfileHelper { get; private set; }

		private void Init()
		{
			ServiceManagerHelper = new ServiceManagerHelper();
			ServiceManagerHelper.RequestResponseEvent += (sender, e) => e.responseMessage = engine.SendSLNetSingleResponseMessage(e.requestMessage);

			ResourceManagerHelper = new ResourceManagerHelper();
			ResourceManagerHelper.RequestResponseEvent += (sender, e) => e.responseMessage = engine.SendSLNetSingleResponseMessage(e.requestMessage);

			ProtocolFunctionHelper = new ProtocolFunctionHelper();
			ProtocolFunctionHelper.RequestResponseEvent += (sender, e) => e.responseMessage = engine.SendSLNetSingleResponseMessage(e.requestMessage);
			
			ProfileHelper = new ProfileHelper(engine.SendSLNetMessages);
		}
	}
}