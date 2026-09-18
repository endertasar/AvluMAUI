using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Avlu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Inter fontunu Resources/Fonts klasörüne ekledikten sonra aşağıdaki isim register edilir.
                fonts.AddFont("Inter-Regular.ttf", "Inter");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
