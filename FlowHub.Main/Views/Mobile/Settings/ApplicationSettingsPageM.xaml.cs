namespace FlowHub.Main.Views.Mobile.Settings;

public partial class ApplicationSettingsPageM : ContentPage
{
    UserSettingsVM viewModel;
    public ApplicationSettingsPageM(UserSettingsVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
        viewModel.SetThemeConfig();
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var uri = new Uri("https://github.com/YBTopaz8/FlowHub-MAUI");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        await Clipboard.SetTextAsync(uri.ToString());
    }
}
