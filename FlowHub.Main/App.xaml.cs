namespace FlowHub.Main;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
#if ANDROID
        MainPage = new AppShellMobile();
#elif WINDOWS

        MainPage = new AppShell();
#endif

        AppThemesSettings.ThemeSettings.SetTheme();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        window.MinimumHeight = 730;
        window.MinimumWidth = 1280;
        window.Title = "FlowHub";
        return window;
    }
}
