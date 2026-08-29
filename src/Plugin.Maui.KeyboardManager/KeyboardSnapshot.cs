namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Point-in-time view of the keyboard.
/// </summary>
public sealed class KeyboardSnapshot
{
    /// <summary>
    /// Gets a value indicating whether the soft keyboard is on screen.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Gets the keyboard height in device-independent pixels.
    /// </summary>
    public double KeyboardHeight { get; init; }

    /// <summary>
    /// Gets the current avoidance mode.
    /// </summary>
    public KeyboardAvoidanceMode AvoidanceMode { get; init; }

    /// <summary>
    /// Gets a value indicating whether tapping outside a focused field hides the keyboard.
    /// </summary>
    public bool DismissOnTapOutside { get; init; }

    /// <summary>
    /// Gets the system safe-area insets.
    /// </summary>
    public Thickness SafeAreaInsets { get; init; }

    /// <summary>
    /// Gets keyboard insets (<c>0, 0, 0, height</c> when visible).
    /// </summary>
    public Thickness KeyboardInsets { get; init; }

    /// <summary>
    /// Gets a value indicating whether an input view is focused.
    /// </summary>
    public bool HasFocus { get; init; }
}
