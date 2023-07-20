namespace FlowHub.Main.Views.Mobile.Settings;

public partial class UserSettingsPageM
{
    private readonly UserSettingsVM viewModel;
    public UserSettingsPageM(UserSettingsVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);

        var theme = AppThemesSettings.ThemeSettings.Theme;
        switch (theme)
        {
            case 0:
               // LightThemeToggler.IsEnabled = true;
            //    DarkThemeToggler.IsEnabled = false;
                break;
            case 1:
            //    LightThemeToggler.IsEnabled = false;
              //  DarkThemeToggler.IsEnabled = true;
                break;
        }
    }
}