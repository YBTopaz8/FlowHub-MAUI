namespace FlowHub_MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseDevExpress(useLocalization: false)
            .UseDevExpressCollectionView()
            .UseDevExpressControls()
            .UseDevExpressDataGrid()
            .UseDevExpressEditors()
            .UseDevExpressGauges()

            .UseMauiCommunityToolkit(options =>
            {
                options.SetShouldSuppressExceptionsInAnimations(true);
                options.SetShouldSuppressExceptionsInBehaviors(true);
                options.SetShouldSuppressExceptionsInConverters(true);

            })

                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddMaterialSymbolsFonts();
                    fonts.AddFontAwesomeIconFonts();
                })
            .ConfigureSyncfusionToolkit();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton(FolderPicker.Default);
            //builder.Services.AddSingleton(FilePicker.Default);
            builder.Services.AddSingleton(FileSaver.Default);

            /* Registering the DataAccess Services */
            builder.Services.AddSingleton<IDataBaseService, DataBaseService>();


            /* Registering the ViewModels */
            builder.Services.AddSingleton(provider =>
            new Lazy<HomePageVM>(() => provider.GetRequiredService<HomePageVM>()));

            builder.Services.AddSingleton<HomePageVM>();



            /* Registering the Desktop Views */
            builder.Services.AddSingleton<HomeD>();



            /* Registering the Mobile Views */

            builder.Services.AddSingleton<HomeM>();
            builder.Services.AddSingleton<IDataBaseService,DataBaseService>();
            builder.Services.AddSingleton<IFlowsService, FlowService>();
            
            return builder.Build();
        }
    }
}
