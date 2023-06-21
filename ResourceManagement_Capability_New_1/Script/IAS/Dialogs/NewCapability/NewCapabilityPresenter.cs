namespace Script.IAS.Dialogs.NewCapability
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class NewCapabilityPresenter
	{
		#region Fields
		private readonly NewCapabilityView view;

		private readonly ScriptData model;
		#endregion

		public NewCapabilityPresenter(NewCapabilityView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Cancel;

		public event EventHandler<EventArgs> Add;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.CapabilityTypeDropDown.SetOptions(new List<string> { "Text", "Discrete" });
			view.CapabilityTypeDropDown.Selected = "Text";
		}

		public void BuildView()
		{
			foreach (var section in view.Discretes)
			{
				section.Build();
			}

			view.Build();
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.AddButton.Pressed += OnAddButtonPressed;

			view.CapabilityTypeDropDown.Changed += OnTypeChanged;
			view.AddDiscreteButton.Pressed += OnAddDiscreteButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Cancel?.Invoke(this, EventArgs.Empty);
		}

		private void OnAddButtonPressed(object sender, EventArgs e)
		{
			StoreToModel();
			model.AddCapability();

			Add?.Invoke(this, EventArgs.Empty);
		}

		private void OnTypeChanged(object sender, EventArgs e)
		{
			view.Discretes.Clear();

			BuildView();
		}

		private void OnAddDiscreteButtonPressed(object sender, EventArgs e)
		{
			var section = new DiscreteSection();
			section.DeleteButton.Pressed += OnDeleteDiscreteButtonPressed;

			view.Discretes.Add(section);

			BuildView();
		}

		private void OnDeleteDiscreteButtonPressed(object sender, EventArgs e)
		{
			var section = view.Discretes.Single(x => x.DeleteButton == sender);

			view.Discretes.Remove(section);

			BuildView();
		}

		private void StoreToModel()
		{
			model.Name = view.CapabilityNameTextBox.Text;
			model.SelectedType = view.CapabilityTypeDropDown.Selected;

			if (view.CapabilityTypeDropDown.Selected == "Discrete")
			{
				model.Discretes = view.Discretes.Where(x => !string.IsNullOrWhiteSpace(x.DiscreteTextBox.Text)).Select(x => x.DiscreteTextBox.Text).Distinct().ToList();
			}
		}
		#endregion
	}
}
