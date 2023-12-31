﻿namespace Script.IAS.Dialogs.NewProperty
{
	using System;

	public class NewPropertyPresenter
	{
		#region Fields
		private readonly NewPropertyView view;

		private readonly ScriptData model;
		#endregion

		public NewPropertyPresenter(NewPropertyView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;

		public event EventHandler<EventArgs> NameInUse;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			// Do nothing
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.AddButton.Pressed += OnAddButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnAddButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();

			if (model.IsNameInUse())
			{
				NameInUse?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				model.AddProperty();

				Close?.Invoke(this, EventArgs.Empty);
			}
		}

		private void StoreToModel()
		{
			model.Name = view.PropertyNameTextBox.Text;
		}
		#endregion
	}
}
