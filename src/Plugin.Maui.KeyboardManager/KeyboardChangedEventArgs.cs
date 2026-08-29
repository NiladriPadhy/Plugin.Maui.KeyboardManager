namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Keyboard visibility, height, and inset snapshot.
/// </summary>
public sealed class KeyboardChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardChangedEventArgs"/> class.
    /// </summary>
    public KeyboardChangedEventArgs(
        bool isVisible,
        double keyboardHeight,
        Thickness safeAreaInsets)
    {
        IsVisible = isVisible;
        KeyboardHeight = keyboardHeight;
        SafeAreaInsets = safeAreaInsets;
        KeyboardInsets = isVisible
            ? new Thickness(0, 0, 0, keyboardHeight)
            : default;
    }

    /// <summary>
    /// Gets a value indicating whether the soft keyboard is on screen.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Gets the keyboard height in device-independent pixels.
    /// Zero when the keyboard is hidden.
    /// </summary>
    public double KeyboardHeight { get; }

    /// <summary>
    /// Gets the current system safe-area insets (notch, status bar, home indicator).
    /// This does not include the keyboard.
    /// </summary>
    public Thickness SafeAreaInsets { get; }

    /// <summary>
    /// Gets insets that match the keyboard: <c>(0, 0, 0, KeyboardHeight)</c> when visible.
    /// </summary>
    public Thickness KeyboardInsets { get; }
}
