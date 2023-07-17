namespace FlowHub.Main.Views.Mobile.Settings;

public partial class UserSettingsPageM
{
	private readonly UserSettingsVM viewModel;
	public UserSettingsPageM(UserSettingsVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		BindingContext= viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		viewModel.PageLoadedCommand.Execute(null);
    }
}