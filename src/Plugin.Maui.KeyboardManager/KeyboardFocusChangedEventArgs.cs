namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Raised when the focused input view changes.
/// </summary>
public sealed class KeyboardFocusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardFocusChangedEventArgs"/> class.
    /// </summary>
    public KeyboardFocusChangedEventArgs(IView? previous, IView? current)
    {
        Previous = previous;
        Current = current;
    }

    /// <summary>
    /// Gets the view that previously held focus, if any.
    /// </summary>
    public IView? Previous { get; }

    /// <summary>
    /// Gets the view that now holds focus, if any.
    /// </summary>
    public IView? Current { get; }

    /// <summary>
    /// Gets a value indicating whether an input view is focused.
    /// </summary>
    public bool IsFocused => Current is not null;
}
