namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Static entry point for soft-keyboard control.
/// </summary>
public static class KeyboardManager
{
    static IKeyboardManager? current;

    /// <summary>
    /// Gets the shared instance. Created on first use when <c>UseKeyboardManager</c> was not called.
    /// </summary>
    public static IKeyboardManager Current
    {
        get
        {
            if (current is null)
                SetDefault(Create());
            return current!;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the soft keyboard is on screen.
    /// </summary>
    public static bool IsVisible => Current.IsVisible;

    /// <summary>
    /// Gets the keyboard height in device-independent pixels.
    /// </summary>
    public static double KeyboardHeight => Current.KeyboardHeight;

    /// <summary>
    /// Gets the currently focused input view, if any.
    /// </summary>
    public static IView? FocusedView => Current.FocusedView;

    /// <summary>
    /// Gets the current system safe-area insets.
    /// </summary>
    public static Thickness SafeAreaInsets => Current.SafeAreaInsets;

    /// <summary>
    /// Raised when the keyboard appears or disappears.
    /// </summary>
    public static event EventHandler<KeyboardChangedEventArgs>? VisibilityChanged
    {
        add => Current.VisibilityChanged += value;
        remove => Current.VisibilityChanged -= value;
    }

    /// <summary>
    /// Raised when the keyboard height changes.
    /// </summary>
    public static event EventHandler<KeyboardChangedEventArgs>? HeightChanged
    {
        add => Current.HeightChanged += value;
        remove => Current.HeightChanged -= value;
    }

    /// <summary>
    /// Raised when the focused input view changes.
    /// </summary>
    public static event EventHandler<KeyboardFocusChangedEventArgs>? FocusChanged
    {
        add => Current.FocusChanged += value;
        remove => Current.FocusChanged -= value;
    }

    /// <summary>
    /// Hides the soft keyboard.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyboardManager.Hide();
    /// </code>
    /// </example>
    public static void Hide() => Current.Hide();

    /// <summary>
    /// Shows the soft keyboard, focusing <paramref name="view"/> when supplied.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyboardManager.Show(nameEntry);
    /// </code>
    /// </example>
    public static void Show(IView? view = null) => Current.Show(view);

    /// <summary>
    /// Enables or disables hiding the keyboard when the user taps outside a focused field.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyboardManager.DismissOnTapOutside();
    /// </code>
    /// </example>
    public static void DismissOnTapOutside(bool enabled = true) => Current.DismissOnTapOutside(enabled);

    /// <summary>
    /// Sets how the window stays out of the keyboard.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyboardManager.SetAvoidanceMode(KeyboardAvoidanceMode.Resize);
    /// </code>
    /// </example>
    public static void SetAvoidanceMode(KeyboardAvoidanceMode mode) => Current.SetAvoidanceMode(mode);

    /// <summary>
    /// Updates options on the shared instance, creating one if needed.
    /// </summary>
    public static void Configure(Action<KeyboardManagerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (current is not null)
        {
            current.Configure(configure);
            return;
        }

        var options = new KeyboardManagerOptions();
        configure(options);
        SetDefault(Create(options));
    }

    /// <summary>
    /// Returns a point-in-time view of the keyboard.
    /// </summary>
    public static KeyboardSnapshot GetSnapshot() => Current.GetSnapshot();

    /// <summary>
    /// Creates a manager that uses the platform keyboard.
    /// </summary>
    public static IKeyboardManager Create(KeyboardManagerOptions? options = null)
    {
        var instance = Create(options ?? new KeyboardManagerOptions(), KeyboardPlatform.Create());
        SetDefault(instance);
        return instance;
    }

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IKeyboardManager implementation) =>
        current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static KeyboardManagerImplementation Create(
        KeyboardManagerOptions options,
        IKeyboardPlatform platform) =>
        new(options, platform);

    internal static void SetCurrent(IKeyboardManager? instance) => current = instance;

    internal static void Reset() => current = null;
}
