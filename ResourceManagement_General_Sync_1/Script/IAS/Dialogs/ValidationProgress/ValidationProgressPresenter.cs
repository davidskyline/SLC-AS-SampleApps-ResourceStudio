namespace Script.IAS.Dialogs.ValidationProgress
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class ValidationProgressPresenter
	{
		#region Fields
		private readonly ValidationProgressView view;

		private readonly ScriptData model;
		#endregion

		public ValidationProgressPresenter(ValidationProgressView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Continue;
		#endregion

		#region Methods
		public void BuildView()
		{
			view.Build();
		}

		public void StartProgress()
		{
			var capacityLabel = view.AddTextLine("Verify if capacities are synchronized...");
			view.Show(false);

			model.VerifyCapacities();

			capacityLabel.Text += "\r\nDONE";
			var capabilityLabel = view.AddTextLine("Verify if capabilities are synchronized...");
			view.Show(false);

			model.VerifyCapabilities();

			capabilityLabel.Text += "\r\nDONE";
			view.ShowButton();
			view.Show(true);
		}

		private void Init()
		{
			view.ResultButton.Pressed += OnResultButtonPressed;
		}

		private void OnResultButtonPressed(object sender, EventArgs e)
		{
			Continue?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
