namespace Plugin.Maui.KeyboardManager;

/// <summary>
/// How the window should stay out of the way of the soft keyboard.
/// </summary>
public enum KeyboardAvoidanceMode
{
    /// <summary>
    /// Leave the platform default alone (Android <c>adjustUnspecified</c>, iOS no extra shift).
    /// </summary>
    System = 0,

    /// <summary>
    /// Do not resize or pan. The keyboard may cover focused fields.
    /// </summary>
    None,

    /// <summary>
    /// Slide the window so the focused field stays visible.
    /// Android uses <c>SOFT_INPUT_ADJUST_PAN</c>. iOS translates the root view.
    /// </summary>
    Pan,

    /// <summary>
    /// Shrink the window above the keyboard.
    /// Android uses <c>SOFT_INPUT_ADJUST_RESIZE</c>. iOS pads the current page.
    /// </summary>
    Resize,

    /// <summary>
    /// Pad the current page by the keyboard height. Cross-platform and the most
    /// predictable option for MAUI layouts and <see cref="ScrollView"/>.
    /// </summary>
    SafeArea
}
