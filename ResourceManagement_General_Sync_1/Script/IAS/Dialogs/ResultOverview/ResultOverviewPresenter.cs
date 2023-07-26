namespace Script.IAS.Dialogs.ResultOverview
{
	using System;
	using System.Linq;

	public class ResultOverviewPresenter
	{
		#region Fields
		private readonly ResultOverviewView view;

		private readonly ScriptData model;
		#endregion

		public ResultOverviewPresenter(ResultOverviewView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public EventHandler<EventArgs> Close;

		public EventHandler<EventArgs> Details;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			foreach (var result in model.GetResults())
			{
				var section = new ResultSection(result.Reference);
				section.Result.Text = $"{result.DeSyncedItems.Count} {result.Reference} items are not synchronized.";

				section.DetailsButton.Pressed += OnSectionDetailsButtonPressed;

				view.Results.Add(section);
			}
		}

		public void BuildView()
		{
			if (view.Results.Any())
			{
				foreach (var result in view.Results)
				{
					result.Build();
				}
			}

			view.Build();
		}

		private void Init()
		{
			view.CloseButton.Pressed += OnCloseButtonPressed;
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.SynchronizeButton.Pressed += OnSynchronizeButtonPressed;
		}

		private void OnCloseButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnSynchronizeButtonPressed(object sender, EventArgs e)
		{
			model.Synchronize();

			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnSectionDetailsButtonPressed(object sender, EventArgs e)
		{
			var section = view.Results.Single(x => x.DetailsButton == sender);

			model.SetResultDetails(section.Reference);

			Details?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
