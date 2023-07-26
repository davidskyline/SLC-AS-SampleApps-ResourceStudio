namespace Script.IAS.Dialogs.ResultDetails
{
	using System;
	using System.Text;

	public class ResultDetailsPresenter
	{
		#region Fields
		private readonly ResultDetailsView view;

		private readonly ScriptData model;
		#endregion

		public ResultDetailsPresenter(ResultDetailsView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentException(nameof(view));
			this.model = model?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public EventHandler<EventArgs> Previous;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.DialogLabel.Text = $"{model.Result.Reference}";

			view.Details.Clear();
			foreach (var item in model.Result.DeSyncedItems)
			{
				var sb = new StringBuilder();
				sb.AppendLine(item.Name);

				foreach (var detail in item.Details)
				{
					sb.AppendLine($"- {detail}");
				}

				var section = new DetailsSection();
				section.Details.Text = sb.ToString();

				view.Details.Add(section);
			}
		}

		public void BuildView()
		{
			foreach (var section in view.Details)
			{
				section.Build();
			}

			view.Build();
		}

		private void Init()
		{
			view.PreviousButton.Pressed += OnPreviousButtonPressed;
		}

		private void OnPreviousButtonPressed(object sender, EventArgs e)
		{
			Previous?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
