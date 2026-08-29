namespace Plugin.Maui.KeyboardManager;

static class KeyboardPlatform
{
    public static IKeyboardPlatform Create() =>
#if ANDROID
        new AndroidKeyboardPlatform();
#elif IOS
        new IosKeyboardPlatform();
#else
        new NetKeyboardPlatform();
#endif
}
