namespace Plugin.Maui.KeyboardManager;

sealed class KeyboardManagerInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var manager = services.GetService<IKeyboardManager>();
        if (manager is null)
            return;

        KeyboardManager.SetCurrent(manager);
        manager.Start();
    }
}
