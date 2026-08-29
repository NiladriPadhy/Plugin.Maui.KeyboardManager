#if !ANDROID && !IOS
namespace Plugin.Maui.KeyboardManager;

sealed class NetKeyboardPlatform : IKeyboardPlatform
{
    public const double DefaultSimulatedHeight = 336;

    public event EventHandler<NativeKeyboardEventArgs>? KeyboardChanged;

    public double SimulatedHeight { get; set; } = DefaultSimulatedHeight;

    public Thickness SimulatedSafeArea { get; set; } = new(0, 47, 0, 34);

    public bool DismissOnTapOutside { get; private set; }

    public KeyboardAvoidanceMode AvoidanceMode { get; private set; } = KeyboardAvoidanceMode.Resize;

    public Thickness GetSafeAreaInsets() => SimulatedSafeArea;

    public void Hide() => Raise(false, 0);

    public void Show(IView? view)
    {
        _ = view;
        Raise(true, SimulatedHeight);
    }

    public void SetAvoidanceMode(KeyboardAvoidanceMode mode, double extraPadding)
    {
        _ = extraPadding;
        AvoidanceMode = mode;
    }

    public void ApplyNativeAvoidance(KeyboardAvoidanceMode mode, bool visible, double keyboardHeight, double extraPadding)
    {
        _ = mode;
        _ = visible;
        _ = keyboardHeight;
        _ = extraPadding;
    }

    public void SetDismissOnTapOutside(bool enabled) => DismissOnTapOutside = enabled;

    public void Start()
    {
    }

    public void Stop()
    {
    }

    public void Simulate(bool isVisible, double keyboardHeight) =>
        Raise(isVisible, keyboardHeight);

    void Raise(bool isVisible, double keyboardHeight) =>
        KeyboardChanged?.Invoke(this, new NativeKeyboardEventArgs(isVisible, keyboardHeight));
}
#endif
