using FlowHub.Main.Views.Desktop;
using FlowHub.Main.Views.Desktop.Expenditures;

namespace FlowHub.Main;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(HomePageD), typeof(HomePageD));
		Routing.RegisterRoute(nameof(LoginD), typeof(LoginD));

		Routing.RegisterRoute(nameof(ManageExpendituresD), typeof(ManageExpendituresD));
		Routing.RegisterRoute(nameof(UpSertExpenditurePageD), typeof(UpSertExpenditurePageD));

	}

}