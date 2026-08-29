namespace Plugin.Maui.KeyboardManager;

sealed class NativeKeyboardEventArgs : EventArgs
{
    public NativeKeyboardEventArgs(bool isVisible, double keyboardHeight)
    {
        IsVisible = isVisible;
        KeyboardHeight = keyboardHeight;
    }

    public bool IsVisible { get; }

    public double KeyboardHeight { get; }
}

interface IKeyboardPlatform
{
    event EventHandler<NativeKeyboardEventArgs>? KeyboardChanged;

    Thickness GetSafeAreaInsets();

    void Hide();

    void Show(IView? view);

    void SetAvoidanceMode(KeyboardAvoidanceMode mode, double extraPadding);

    void ApplyNativeAvoidance(KeyboardAvoidanceMode mode, bool visible, double keyboardHeight, double extraPadding);

    void SetDismissOnTapOutside(bool enabled);

    void Start();

    void Stop();
}
