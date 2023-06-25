using FlowHub.Main.Views.Desktop;
using FlowHub.Main.Views.Desktop.Expenditures;
using FlowHub.Main.Views.Desktop.Incomes;
using FlowHub.Main.Views.Desktop.Settings;

namespace FlowHub.Main;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(HomePageD), typeof(HomePageD));
		Routing.RegisterRoute(nameof(LoginD), typeof(LoginD));

		Routing.RegisterRoute(nameof(ManageExpendituresPageD), typeof(ManageExpendituresPageD));
		
		Routing.RegisterRoute(nameof(ManageExpendituresD), typeof(ManageExpendituresD)); //deprecated
		Routing.RegisterRoute(nameof(UpSertExpenditurePageD), typeof(UpSertExpenditurePageD));

		Routing.RegisterRoute(nameof(ManageIncomesD), typeof(ManageIncomesD));

		Routing.RegisterRoute(nameof(UserSettingsPageD), typeof(UserSettingsPageD));
	}
}