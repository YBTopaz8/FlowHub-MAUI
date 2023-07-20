namespace FlowHub.Main.Views.Mobile.Settings;

public partial class ApplicationSettingsPageM : ContentPage
{
    int selectedTheme;
    public int SelectedTheme
    {
        get => selectedTheme;
        set
        {
            selectedTheme = value;
            OnPropertyChanged(nameof(SelectedTheme));
        }
    }
    bool isLightTheme;
    public bool IsLightTheme
    {
        get => isLightTheme;
        set
        {
            isLightTheme = value;
            OnPropertyChanged(nameof(IsLightTheme));
        }
    }
    public ApplicationSettingsPageM()
    {
        InitializeComponent();
        SelectedTheme = AppThemesSettings.ThemeSettings.Theme;
        IsLightTheme = SelectedTheme == 0;
    }

    private void ThemeToggler_Clicked(object sender, EventArgs e)
    {
        SelectedTheme = AppThemesSettings.ThemeSettings.SwitchTheme();
        IsLightTheme = !IsLightTheme;
        darkBtns.IsVisible = !darkBtns.IsVisible;
    }

    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        var uri = new Uri("https://github.com/YBTopaz8/FlowHub-MAUI");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        await Clipboard.SetTextAsync(uri.ToString());
    }
}
