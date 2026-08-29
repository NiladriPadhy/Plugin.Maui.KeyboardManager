namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Registers KeyboardManager services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IKeyboardManager"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddKeyboardManager(this IServiceCollection services, KeyboardManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IKeyboardManager>(sp =>
        {
            var resolved = sp.GetService<KeyboardManagerOptions>() ?? options;
            var instance = KeyboardManager.Create(resolved, KeyboardPlatform.Create());
            KeyboardManager.SetCurrent(instance);
            return instance;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IKeyboardManager"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddKeyboardManager(this IServiceCollection services, Action<KeyboardManagerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new KeyboardManagerOptions();
        configure?.Invoke(options);
        return services.AddKeyboardManager(options);
    }
}
