using CommunityToolkit.Maui;
using FlowHub.DataAccess;
using FlowHub.DataAccess.IRepositories;
using FlowHub.DataAccess.Repositories;
using FlowHub.Main.ViewModels;
using FlowHub.Main.ViewModels.Expenditures;
using FlowHub.Main.ViewModels.Incomes;
using FlowHub.Main.ViewModels.Settings;
using FlowHub.Main.Views.Desktop;
using FlowHub.Main.Views.Desktop.Expenditures;
using FlowHub.Main.Views.Mobile;
using FlowHub.Main.Views.Mobile.Expenditures;
using FlowHub.Main.Views.Mobile.Incomes;
using FlowHub.Main.Views.Mobile.Settings;
using InputKit.Handlers;
using Microsoft.Extensions.Logging;
using UraniumUI;

namespace FlowHub.Main;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
	        }).UseMauiCommunityToolkit();

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddInputKitHandlers();
            handlers.AddUraniumUIHandlers();
        });

/*----------------------- REGISTERING Repositories ------------------------------------------------------------------------*/

        builder.Services.AddSingleton<IExpendituresRepository, ExpendituresRepository>();
        builder.Services.AddSingleton<IIncomeRepository, IncomeRepository>();
        builder.Services.AddSingleton<IDataAccessRepo, DataAccessRepo>();
        builder.Services.AddSingleton<ISettingsServiceRepository, SettingsServiceRepository>();
        builder.Services.AddSingleton<IUsersRepository, UserRepository>();
        builder.Services.AddSingleton<IOnlineCredentialsRepository, OnlineCredentialsRepository>();

/*--------------------ADDING VIEWMODELS----------------------------------------------------------------------------------------*/

        /*-- Section for HomePage AND Login --*/
        builder.Services.AddSingleton<HomePageVM>();
        builder.Services.AddTransient<LoginVM>();

        /*-- Section for Expenditures --*/
        builder.Services.AddSingleton<UpSertExpenditureVM>();
        builder.Services.AddSingleton<ManageExpendituresVM>();

        /* -- Section for Incomes --*/
        builder.Services.AddSingleton<UpSertIncomeVM>();
        builder.Services.AddSingleton<ManageIncomesVM>();
        builder.Services.AddSingleton<UserSettingsVM>();

/*------------------------REGISTERING DESKTOP VIEWS ----------------------------------------------------------------------------*/

        /*-- Section for HomePage AND Login --*/
        builder.Services.AddSingleton<HomePageD>();
        builder.Services.AddTransient<LoginD>();

        /*-- Section for Expenditures --*/
        builder.Services.AddSingleton<UpSertExpenditurePageD>();
        builder.Services.AddSingleton<ManageExpendituresD>();


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

/*--------------------------------------------------------------------------------------------------------------------------------*/
        return builder.Build();
	}
}
