namespace FlowHub.Main;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(HomePageD), typeof(HomePageD));
        Routing.RegisterRoute(nameof(LoginD), typeof(LoginD));

        Routing.RegisterRoute(nameof(ManageExpendituresPageD), typeof(ManageExpendituresPageD));

        Routing.RegisterRoute(nameof(UpSertExpenditurePageD), typeof(UpSertExpenditurePageD));

        Routing.RegisterRoute(nameof(ManageIncomesD), typeof(ManageIncomesD));

        Routing.RegisterRoute(nameof(StatisticsPageD), typeof(StatisticsPageD));

        Routing.RegisterRoute(nameof(UserSettingsPageD), typeof(UserSettingsPageD));
    }
}