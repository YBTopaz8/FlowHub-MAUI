namespace FlowHub.Main.Views.Mobile.Settings;

public partial class ApplicationSettingsPageM
{
    public ApplicationSettingsPageM()
    {
        InitializeComponent();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var uri = new Uri("https://github.com/YBTopaz8/FlowHub-MAUI");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        await Clipboard.SetTextAsync(uri.ToString());
    }
}