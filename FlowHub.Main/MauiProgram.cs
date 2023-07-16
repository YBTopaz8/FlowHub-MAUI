
using CommunityToolkit.Maui;
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
	        })
            .UseMauiCommunityToolkit();

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
        builder.Services.AddSingleton<IDebtRepository, DebtRepository>();

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
        //builder.Services.AddSingleton<UpSertIncomeVM>();
        builder.Services.AddSingleton<ManageIncomesVM>();
        builder.Services.AddSingleton<UserSettingsVM>();

        /*-- Section for Planned Expenditures --*/
        builder.Services.AddSingleton<ManageMonthlyMonthlyPlannedExpendituresVM>();
        builder.Services.AddSingleton<DetailsOfMonthlyPlannedExpVM>();
        builder.Services.AddSingleton<UpSertMonthlyPlannedExpVM>();

        /*-- Section for Statistics --*/
        builder.Services.AddSingleton<StatisticsPageVM>();
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

        /* -- Section for Debts --*/
        builder.Services.AddSingleton<ManageDebtsPageD>();

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
        builder.Services.AddSingleton<StatisticsPageD>();

        builder.Services.AddSingleton<StatisticsPageM>();
        builder.Services.AddSingleton<SingleMonthStatsPageM>();

        /* -- Section for Debts --*/
        builder.Services.AddSingleton<ManageDebtsPageM>();
        /*--------------------------------------------------------------------------------------------------------------------------------*/
        return builder.Build();
	}
}
