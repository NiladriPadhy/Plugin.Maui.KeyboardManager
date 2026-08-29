using Microsoft.Extensions.Logging;
using Plugin.Maui.KeyboardManager;

namespace Plugin.Maui.KeyboardManager.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<MainPage>();

        builder
            .UseMauiApp<App>()
            .UseKeyboardManager(options =>
            {
                options.AvoidanceMode = KeyboardAvoidanceMode.Resize;
                options.DismissOnTapOutside = true;
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
