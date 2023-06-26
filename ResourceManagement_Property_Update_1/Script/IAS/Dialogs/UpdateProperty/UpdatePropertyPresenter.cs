﻿namespace Script.IAS.Dialogs.UpdateProperty
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class UpdatePropertyPresenter
	{
		#region Fields
		private readonly UpdatePropertyView view;

		private readonly ScriptData model;
		#endregion

		public UpdatePropertyPresenter(UpdatePropertyView view, ScriptData model)
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
			view.PropertyNameLabel.Text = model.Name;
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.DeleteButton.Pressed += OnDeleteButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnDeleteButtonPressed(object sender, EventArgs e)
		{
			model.DeleteProperty();

			Close?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
