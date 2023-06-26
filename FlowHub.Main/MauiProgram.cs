using CommunityToolkit.Maui;
using FlowHub.DataAccess;
using FlowHub.DataAccess.IRepositories;
using FlowHub.DataAccess.Repositories;
using FlowHub.Main.ViewModels;
using FlowHub.Main.ViewModels.Expenditures;
using FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;
using FlowHub.Main.ViewModels.Incomes;
using FlowHub.Main.ViewModels.Settings;
using FlowHub.Main.ViewModels.Statistics;
using FlowHub.Main.Views.Desktop;
using FlowHub.Main.Views.Desktop.Expenditures;
using FlowHub.Main.Views.Desktop.Incomes;
using FlowHub.Main.Views.Desktop.Settings;
using FlowHub.Main.Views.Mobile;
using FlowHub.Main.Views.Mobile.Expenditures;
using FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;
using FlowHub.Main.Views.Mobile.Incomes;
using FlowHub.Main.Views.Mobile.Settings;
using FlowHub.Main.Views.Mobile.Statistics;
using InputKit.Handlers;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace FlowHub.Main;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
            .UseSkiaSharp(true)
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
	        }).UseMauiCommunityToolkit();

        builder.ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
            handlers.AddHandler(typeof(Shell), typeof(MyShellRenderer));
#endif
            handlers.AddInputKitHandlers();
            handlers.AddUraniumUIHandlers();
        });

/*----------------------- REGISTERING Repositories ------------------------------------------------------------------------*/

        builder.Services.AddSingleton<IExpendituresRepository, ExpendituresRepository>();
        builder.Services.AddSingleton<IIncomeRepository, IncomeRepository>();
        builder.Services.AddSingleton<IDataAccessRepo, DataAccessRepo>();
        builder.Services.AddSingleton<ISettingsServiceRepository, SettingsServiceRepository>();
        builder.Services.AddSingleton<IUsersRepository, UserRepository>();
        builder.Services.AddSingleton<IOnlineCredentialsRepository, OnlineDataAccessRepository>();
        builder.Services.AddSingleton<IPlannedExpendituresRepository, PlannedExpendituresRepository>();

/*--------------------ADDING VIEWMODELS----------------------------------------------------------------------------------------*/

        /*-- Section for HomePage AND Login --*/
        builder.Services.AddSingleton<HomePageVM>();
        builder.Services.AddSingleton<LoginVM>();

        /*-- Section for Expenditures --*/
        builder.Services.AddSingleton<UpSertExpenditureVM>();
        builder.Services.AddSingleton<ManageExpendituresVM>();

        /* -- Section for Incomes --*/
        builder.Services.AddSingleton<UpSertIncomeVM>();
        builder.Services.AddSingleton<ManageIncomesVM>();
        builder.Services.AddSingleton<UserSettingsVM>();

        /*-- Section for Planned Expenditures --*/
        builder.Services.AddSingleton<ManageMonthlyMonthlyPlannedExpendituresVM>();
        builder.Services.AddSingleton<DetailsOfMonthlyPlannedExpVM>();
        builder.Services.AddSingleton<UpSertMonthlyPlannedExpVM>();

        /*-- Section for Statistics --*/
        builder.Services.AddTransient<StatisticsPageVM>();
        builder.Services.AddSingleton<SingleMonthStatsPageVM>();

/*------------------------REGISTERING DESKTOP VIEWS ----------------------------------------------------------------------------*/

        /*-- Section for HomePage AND Login --*/
        builder.Services.AddSingleton<HomePageD>();
        builder.Services.AddSingleton<LoginD>();

        /*-- Section for Expenditures --*/
        builder.Services.AddSingleton<UpSertExpenditurePageD>();
        builder.Services.AddSingleton<ManageExpendituresPageD>();

        /*-- Section for Incomes --*/
        builder.Services.AddSingleton<ManageIncomesD>();
        /*-- Section for Settings --*/
        builder.Services.AddSingleton<UserSettingsPageD>();
/*-------------------------------REGISTERING MOBILE VIEWS ---------------------------------------------------------------*/

        /*--  REGISTERING MOBILE VIEWS --*/
        builder.Services.AddSingleton<HomePageM>();
        builder.Services.AddTransient<LoginM>();

        /*-- Section for Expenditures --*/
        builder.Services.AddSingleton<ManageExpendituresM>();
        builder.Services.AddSingleton<UpSertExpenditurePageM>();

        /*-- Section for Incomes --*/
        builder.Services.AddSingleton<ManageIncomesM>();
        builder.Services.AddSingleton<UpSertIncomePageM>();

        /*-- Section for Settings --*/
        builder.Services.AddSingleton<UserSettingsPageM>();
        builder.Services.AddSingleton<ApplicationSettingsPageM>();
        builder.Services.AddTransient<EditUserSettingsPageM>();

        /*-- Section for Monthly Planned Expenditures --*/
        builder.Services.AddSingleton<ManageMonthlyPlannedExpendituresPageM>();
        builder.Services.AddSingleton<DetailsOfMonthlyPlannedExpPageM>();
        builder.Services.AddSingleton<UpSertMonthlyPlannedExpPageM>();

        /* -- Section For Statistics --*/
        builder.Services.AddTransient<StatisticsPageM>();
        builder.Services.AddSingleton<SingleMonthStatsPageM>();
/*--------------------------------------------------------------------------------------------------------------------------------*/
        return builder.Build();
	}
}
