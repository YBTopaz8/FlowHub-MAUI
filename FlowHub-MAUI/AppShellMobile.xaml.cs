namespace FlowHub_MAUI;

public partial class AppShellMobile : Shell
{
	public AppShellMobile()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(HomeM), typeof(HomeM));
        Routing.RegisterRoute(nameof(SettingsM), typeof(SettingsM));
    }
    public HomePageVM? Vm { get; }
}