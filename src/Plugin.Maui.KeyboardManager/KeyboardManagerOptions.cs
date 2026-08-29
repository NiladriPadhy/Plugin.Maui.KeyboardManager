namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// Defaults applied by <c>UseKeyboardManager</c> or <see cref="KeyboardManager.Configure"/>.
/// </summary>
public sealed class KeyboardManagerOptions
{
    /// <summary>
    /// Gets or sets how the window stays out of the keyboard.
    /// Default is <see cref="KeyboardAvoidanceMode.Resize"/>.
    /// </summary>
    public KeyboardAvoidanceMode AvoidanceMode { get; set; } = KeyboardAvoidanceMode.Resize;

    /// <summary>
    /// Gets or sets whether tapping outside a focused <see cref="Entry"/>, <see cref="Editor"/>,
    /// or <see cref="SearchBar"/> hides the keyboard. Default is <c>true</c>.
    /// </summary>
    public bool DismissOnTapOutside { get; set; } = true;

    /// <summary>
    /// Gets or sets extra bottom padding (device-independent pixels) applied on top of the
    /// keyboard height when avoidance is <see cref="KeyboardAvoidanceMode.SafeArea"/>,
    /// <see cref="KeyboardAvoidanceMode.Resize"/> on iOS, or <see cref="KeyboardAvoidanceMode.Pan"/> on iOS.
    /// </summary>
    public double ExtraAvoidancePadding { get; set; }
}
