using Plugin.Maui.KeyboardManager;

namespace Plugin.Maui.KeyboardManager.Sample;

public partial class MainPage : ContentPage
{
    readonly IKeyboardManager keyboard;
    bool suppress;

    public MainPage(IKeyboardManager keyboard)
    {
        InitializeComponent();
        this.keyboard = keyboard;
        this.keyboard.VisibilityChanged += (_, _) => MainThread.BeginInvokeOnMainThread(Refresh);
        this.keyboard.HeightChanged += (_, _) => MainThread.BeginInvokeOnMainThread(Refresh);
        this.keyboard.FocusChanged += (_, _) => MainThread.BeginInvokeOnMainThread(Refresh);

        AvoidancePicker.ItemsSource = Enum.GetNames<KeyboardAvoidanceMode>();
        AvoidancePicker.SelectedIndex = (int)keyboard.AvoidanceMode;
        DismissSwitch.IsToggled = keyboard.IsDismissOnTapOutsideEnabled;
        Refresh();
    }

    void OnHideClicked(object? sender, EventArgs e) => KeyboardManager.Hide();

    void OnShowClicked(object? sender, EventArgs e) => KeyboardManager.Show(NameEntry);

    void OnAvoidanceChanged(object? sender, EventArgs e)
    {
        if (AvoidancePicker.SelectedIndex < 0)
            return;

        KeyboardManager.SetAvoidanceMode((KeyboardAvoidanceMode)AvoidancePicker.SelectedIndex);
        Refresh();
    }

    void OnDismissToggled(object? sender, ToggledEventArgs e)
    {
        if (suppress)
            return;

        KeyboardManager.DismissOnTapOutside(e.Value);
        Refresh();
    }

    void Refresh()
    {
        var snapshot = keyboard.GetSnapshot();
        var focused = keyboard.FocusedView switch
        {
            Entry entry => string.IsNullOrWhiteSpace(entry.Placeholder) ? "Entry" : entry.Placeholder,
            Editor => "Notes",
            _ => "—"
        };

        StatusLabel.Text =
            $"visible={snapshot.IsVisible}  height={snapshot.KeyboardHeight:0}  mode={snapshot.AvoidanceMode}  focus={focused}";
        InsetsLabel.Text =
            $"safe={Format(snapshot.SafeAreaInsets)}  keyboard={Format(snapshot.KeyboardInsets)}  tapOutside={snapshot.DismissOnTapOutside}";

        suppress = true;
        DismissSwitch.IsToggled = snapshot.DismissOnTapOutside;
        suppress = false;
    }

    static string Format(Thickness insets) =>
        $"{insets.Left:0},{insets.Top:0},{insets.Right:0},{insets.Bottom:0}";
}
