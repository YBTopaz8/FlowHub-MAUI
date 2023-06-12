using FlowHub.Main.ViewModels.Settings;

namespace FlowHub.Main.Views.Desktop.Settings;

public partial class UserSettingsPageD : ContentPage
{
	UserSettingsVM viewModel;
	public UserSettingsPageD(UserSettingsVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext= viewModel;
	}
	protected override void OnAppearing()
	{
            base.OnAppearing();
            viewModel.PageLoadedCommand.Execute(null);
    }
}