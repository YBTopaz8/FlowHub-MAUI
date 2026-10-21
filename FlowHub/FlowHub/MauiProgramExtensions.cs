
using Material.Components.Maui.Extensions;
using Microsoft.Extensions.Logging;

namespace FlowHub;

public static class MauiProgramExtensions
{
    public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
    {

        
        builder
            .UseMauiApp<App>()
            .UseMaterialComponents()
            //.UseMaterialComponents()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder;
    }
}
