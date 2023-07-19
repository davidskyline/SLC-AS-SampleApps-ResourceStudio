namespace Script.IAS.Dialogs.CreateFunctionResource
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.IAS;

	public class CreateFunctionResourcePresenter
	{
		#region Fields
		private readonly CreateFunctionResourceView view;

		private readonly ScriptData model;
		#endregion

		public CreateFunctionResourcePresenter(CreateFunctionResourceView view, ScriptData model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;

		public event EventHandler<EventArgs> Create;
		#endregion

		#region Properties
		private bool ShowAvailableElements => view.ShowAvailableElementsCheckBox.IsChecked;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			InitWidgets();
		}

		public void BuildView()
		{
			view.Build();
		}

		private static List<string> ComposeDefaultOptions(List<string> options)
		{
			var defaultOptions = new List<string>(options);

			defaultOptions.Sort();
			defaultOptions.Insert(0, AutomationData.InitialDropdownValue);

			return defaultOptions;
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;

			view.FunctionDropDown.Changed += OnFunctionDropDownChanged;

			view.ShowAvailableElementsCheckBox.Checked += OnShowAvailableElementsCheckBoxChecked;
			view.ShowAvailableElementsCheckBox.UnChecked += OnShowAvailableElementsCheckBoxUnChecked;

			view.ElementDropDown.Changed += OnElementDropDownChanged;

			view.TableIndexDropDown.Changed += OnTableIndexDropDownChanged;
		}

		private void InitWidgets()
		{
			view.ResourceNameLabel.Text = model.ResourceName;

			view.FunctionDropDown.SetOptions(ComposeDefaultOptions(model.Functions.ToList()));
			view.FunctionDropDown.Selected = AutomationData.InitialDropdownValue;

			view.ElementDropDown.Selected = AutomationData.InitialDropdownValue;
			view.ElementDropDown.IsEnabled = false;

			view.ShowAvailableElementsCheckBox.IsEnabled = false;

			view.TableIndexDropDown.Selected = AutomationData.InitialDropdownValue;
			view.TableIndexDropDown.IsEnabled = false;

			view.CreateButton.IsEnabled = false;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}

		private void OnCreateButtonPressed(object sender, EventArgs e)
		{
			model.CreateFunctionResource();

			Create?.Invoke(this, EventArgs.Empty);
		}

		private void OnShowAvailableElementsCheckBoxChecked(object sender, EventArgs e)
		{
			view.ElementDropDown.SetOptions(ComposeDefaultOptions(model.GetFilteredElements()));
			view.ElementDropDown.Selected = AutomationData.InitialDropdownValue;

			view.TableIndexDropDown.Selected = string.Empty;
			view.TableIndexDropDown.IsEnabled = false;

			view.CreateButton.IsEnabled = false;
		}

		private void OnShowAvailableElementsCheckBoxUnChecked(object sender, EventArgs e)
		{
			view.ElementDropDown.SetOptions(ComposeDefaultOptions(model.Elements.ToList()));
			view.ElementDropDown.Selected = AutomationData.InitialDropdownValue;

			view.TableIndexDropDown.Selected = string.Empty;
			view.TableIndexDropDown.IsEnabled = false;

			view.CreateButton.IsEnabled = false;
		}

		private void OnFunctionDropDownChanged(object sender, EventArgs e)
		{
			view.ElementDropDown.SetOptions(new List<string> { AutomationData.InitialDropdownValue });
			view.ElementDropDown.Selected = string.Empty;
			view.ElementDropDown.IsEnabled = false;

			view.ShowAvailableElementsCheckBox.IsEnabled = false;

			view.TableIndexDropDown.Selected = string.Empty;
			view.TableIndexDropDown.IsEnabled = false;

			view.CreateButton.IsEnabled = false;

			if (view.FunctionDropDown.Selected == AutomationData.InitialDropdownValue)
			{
				return;
			}

			model.SelectedFunction = view.FunctionDropDown.Selected;

			var elementOptions = ShowAvailableElements ? model.GetFilteredElements() : model.Elements.ToList();
			view.ElementDropDown.SetOptions(ComposeDefaultOptions(elementOptions));
			view.ElementDropDown.Selected = AutomationData.InitialDropdownValue;
			view.ElementDropDown.IsEnabled = true;

			view.ShowAvailableElementsCheckBox.IsEnabled = true;
		}

		private void OnElementDropDownChanged(object sender, EventArgs e)
		{
			view.TableIndexDropDown.SetOptions(new List<string> { AutomationData.InitialDropdownValue });
			view.TableIndexDropDown.Selected = string.Empty;
			view.TableIndexDropDown.IsEnabled = false;

			view.CreateButton.IsEnabled = false;

			if (view.ElementDropDown.Selected == AutomationData.InitialDropdownValue)
			{
				return;
			}

			model.SelectedElement = view.ElementDropDown.Selected;

			if (!model.FunctionHasEntryPoints)
			{
				if (model.IsSelectedElementAvailable())
				{
					view.CreateButton.IsEnabled = true;
				}
			}
			else
			{
				view.TableIndexDropDown.SetOptions(ComposeDefaultOptions(model.TableIndexes.ToList()));
				view.TableIndexDropDown.Selected = AutomationData.InitialDropdownValue;
				view.TableIndexDropDown.IsEnabled = true;

				view.CreateButton.IsEnabled = false;
			}
		}

		private void OnTableIndexDropDownChanged(object sender, EventArgs e)
		{
			if (view.TableIndexDropDown.Selected == AutomationData.InitialDropdownValue)
			{
				view.CreateButton.IsEnabled = false;

				return;
			}

			model.SelectedTableIndex = view.TableIndexDropDown.Selected;

			view.CreateButton.IsEnabled = true;
		}
		#endregion
	}
}
