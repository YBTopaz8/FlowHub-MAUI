using FlowHub.Main.Views.Mobile;
using FlowHub.Main.Views.Mobile.Expenditures;
using FlowHub.Main.Views.Mobile.Incomes;
using FlowHub.Main.Views.Mobile.Settings;

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

        Routing.RegisterRoute(nameof(UserSettingsPageM), typeof(UserSettingsPageM));
    }
}