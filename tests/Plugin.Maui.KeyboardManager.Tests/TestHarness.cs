namespace Plugin.Maui.KeyboardManager.Tests;

static class Harness
{
    public static (KeyboardManagerImplementation Manager, NetKeyboardPlatform Platform) Create(
        Action<KeyboardManagerOptions>? configure = null)
    {
        KeyboardManager.Reset();

        var options = new KeyboardManagerOptions
        {
            AvoidanceMode = KeyboardAvoidanceMode.Resize,
            DismissOnTapOutside = true
        };
        configure?.Invoke(options);

        var platform = new NetKeyboardPlatform();
        var manager = KeyboardManager.Create(options, platform);
        KeyboardManager.SetDefault(manager);
        manager.Start();
        return (manager, platform);
    }
}
