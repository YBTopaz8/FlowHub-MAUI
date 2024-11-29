namespace FlowHub_MAUI;

public partial class AppShellMobile : Shell
{
	public AppShellMobile()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(HomeM), typeof(HomeM));
    }
    public HomePageVM Vm { get; }
}