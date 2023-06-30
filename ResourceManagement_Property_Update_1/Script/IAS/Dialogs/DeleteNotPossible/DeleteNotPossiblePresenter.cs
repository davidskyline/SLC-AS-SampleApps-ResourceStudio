namespace Script.IAS.Dialogs.DeleteNotPossible
{
	using System;
	using System.Linq;

	public class DeleteNotPossiblePresenter
	{
		#region Fields
		private readonly DeleteNotPossibleView view;

		private readonly ScriptData model;
		#endregion

		public DeleteNotPossiblePresenter(DeleteNotPossibleView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.ResourceNames = model.ResourcesImplementingProperty.Select(x => x.Name).ToList();
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CloseButton.Pressed += OnCloseButtonPressed;
		}

		private void OnCloseButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
