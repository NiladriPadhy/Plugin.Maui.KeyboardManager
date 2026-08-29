namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Soft-keyboard control: hide, show, dismiss, avoidance, visibility, and focus.
/// </summary>
public interface IKeyboardManager
{
    /// <summary>
    /// Gets a value indicating whether the soft keyboard is on screen.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets the keyboard height in device-independent pixels. Zero when hidden.
    /// </summary>
    double KeyboardHeight { get; }

    /// <summary>
    /// Gets the currently focused input view, if any.
    /// </summary>
    IView? FocusedView { get; }

    /// <summary>
    /// Gets the current avoidance mode.
    /// </summary>
    KeyboardAvoidanceMode AvoidanceMode { get; }

    /// <summary>
    /// Gets a value indicating whether tapping outside a focused field hides the keyboard.
    /// </summary>
    bool IsDismissOnTapOutsideEnabled { get; }

    /// <summary>
    /// Gets the current system safe-area insets (notch, status bar, home indicator).
    /// </summary>
    Thickness SafeAreaInsets { get; }

    /// <summary>
    /// Gets keyboard insets (<c>0, 0, 0, height</c> when visible).
    /// </summary>
    Thickness KeyboardInsets { get; }

    /// <summary>
    /// Gets the live options. Mutate through <see cref="Configure"/>.
    /// </summary>
    KeyboardManagerOptions Options { get; }

    /// <summary>
    /// Raised when the keyboard appears or disappears.
    /// </summary>
    event EventHandler<KeyboardChangedEventArgs>? VisibilityChanged;

    /// <summary>
    /// Raised when the keyboard height changes (including show and hide).
    /// </summary>
    event EventHandler<KeyboardChangedEventArgs>? HeightChanged;

    /// <summary>
    /// Raised when the focused input view changes.
    /// </summary>
    event EventHandler<KeyboardFocusChangedEventArgs>? FocusChanged;

    /// <summary>
    /// Hides the soft keyboard and unfocuses the current input.
    /// </summary>
    void Hide();

    /// <summary>
    /// Shows the soft keyboard. When <paramref name="view"/> is set, that view is focused first.
    /// </summary>
    /// <param name="view">An <see cref="Entry"/>, <see cref="Editor"/>, <see cref="SearchBar"/>, or other focusable view.</param>
    void Show(IView? view = null);

    /// <summary>
    /// Enables or disables hiding the keyboard when the user taps outside a focused field.
    /// </summary>
    void DismissOnTapOutside(bool enabled = true);

    /// <summary>
    /// Sets how the window stays out of the keyboard (resize, pan, safe-area padding, or none).
    /// </summary>
    void SetAvoidanceMode(KeyboardAvoidanceMode mode);

    /// <summary>
    /// Updates options and applies avoidance / tap-outside defaults.
    /// </summary>
    void Configure(Action<KeyboardManagerOptions> configure);

    /// <summary>
    /// Returns a point-in-time view of the keyboard.
    /// </summary>
    KeyboardSnapshot GetSnapshot();

    /// <summary>
    /// Starts platform listeners. Called by <c>UseKeyboardManager</c>; safe to call more than once.
    /// </summary>
    void Start();
}
