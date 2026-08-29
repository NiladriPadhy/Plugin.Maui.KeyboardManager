using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// MAUI host registration for KeyboardManager.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IKeyboardManager"/> and starts keyboard visibility,
    /// avoidance, and optional tap-outside dismiss.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseKeyboardManager(options =>
    /// {
    ///     options.AvoidanceMode = KeyboardAvoidanceMode.Resize;
    ///     options.DismissOnTapOutside = true;
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseKeyboardManager(this MauiAppBuilder builder, Action<KeyboardManagerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddKeyboardManager(configure);
        builder.Services.AddTransient<IMauiInitializeService, KeyboardManagerInitializer>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnPostCreate((activity, _) => KeyboardManager.Current.Start());
                android.OnResume(_ => KeyboardManager.Current.Start());
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.OnActivated(_ => KeyboardManager.Current.Start());
            });
#endif
        });

        return builder;
    }
}
