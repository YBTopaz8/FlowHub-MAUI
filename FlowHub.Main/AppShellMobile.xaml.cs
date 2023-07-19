namespace FlowHub.Main;

public partial class AppShellMobile : Shell
{
    public AppShellMobile()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(HomePageM), typeof(HomePageM));
        Routing.RegisterRoute(nameof(LoginM), typeof(LoginM));

        Routing.RegisterRoute(nameof(ManageExpendituresM), typeof(ManageExpendituresM));
        Routing.RegisterRoute(nameof(UpSertExpenditurePageM), typeof(UpSertExpenditurePageM));

        Routing.RegisterRoute(nameof(ManageIncomesM), typeof(ManageIncomesM));
        Routing.RegisterRoute(nameof(UpSertIncomePageM), typeof(UpSertIncomePageM));

        Routing.RegisterRoute(nameof(StatisticsPageM), typeof(StatisticsPageM));
        Routing.RegisterRoute(nameof(SingleMonthStatsPageM), typeof(SingleMonthStatsPageM));

        Routing.RegisterRoute(nameof(UserSettingsPageM), typeof(UserSettingsPageM));
        Routing.RegisterRoute(nameof(EditUserSettingsPageM), typeof(EditUserSettingsPageM));

        Routing.RegisterRoute(nameof(ManageMonthlyPlannedExpendituresPageM), typeof(ManageMonthlyPlannedExpendituresPageM));
        Routing.RegisterRoute(nameof(DetailsOfMonthlyPlannedExpPageM), typeof(DetailsOfMonthlyPlannedExpPageM));
        Routing.RegisterRoute(nameof(UpSertMonthlyPlannedExpPageM), typeof(UpSertMonthlyPlannedExpPageM));

        Routing.RegisterRoute(nameof(ManageDebtsPageM), typeof(ManageDebtsPageM));
        Routing.RegisterRoute(nameof(UpSertDebtPageM), typeof(UpSertDebtPageM));
    }
}