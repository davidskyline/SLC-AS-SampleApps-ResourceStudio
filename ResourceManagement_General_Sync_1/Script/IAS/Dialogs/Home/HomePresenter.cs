﻿namespace Script.IAS.Dialogs.Home
{
	using System;

	public class HomePresenter
	{
		#region Fields
		private readonly HomeView view;

		private readonly ScriptData model;
		#endregion

		public HomePresenter(HomeView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public EventHandler<EventArgs> Close;

		public EventHandler<EventArgs> Validate;
		#endregion

		#region Methods
		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.ValidateButton.Pressed += OnValidateButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnValidateButtonPressed(object sender, EventArgs e)
		{
			Validate?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
