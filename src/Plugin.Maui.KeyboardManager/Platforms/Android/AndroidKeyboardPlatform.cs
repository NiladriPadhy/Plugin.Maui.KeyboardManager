#if ANDROID
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.Core.View;
using View = Android.Views.View;

namespace Plugin.Maui.KeyboardManager;

sealed class AndroidKeyboardPlatform : IKeyboardPlatform
{
    ImeInsetsListener? insetsListener;
    ViewTreeObserver.IOnGlobalLayoutListener? layoutListener;
    View? attachedDecor;
    KeyboardAvoidanceMode avoidanceMode = KeyboardAvoidanceMode.Resize;
    double extraPadding;
    bool dismissOnTapOutside;
    bool started;
    double lastHeight;

    public event EventHandler<NativeKeyboardEventArgs>? KeyboardChanged;

    public Thickness GetSafeAreaInsets()
    {
        var activity = Platform.CurrentActivity;
        if (activity?.Window?.DecorView is not { } decor)
            return default;

        var insets = ViewCompat.GetRootWindowInsets(decor);
        if (insets is null)
            return default;

        var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
        if (bars is null)
            return default;

        var density = Density(decor);
        return new Thickness(bars.Left / density, bars.Top / density, bars.Right / density, bars.Bottom / density);
    }

    public void Hide()
    {
        var activity = Platform.CurrentActivity;
        if (activity is null)
            return;

        var imm = InputMethod(activity);
        var token = activity.CurrentFocus?.WindowToken ?? activity.Window?.DecorView.WindowToken;
        if (token is not null)
            imm?.HideSoftInputFromWindow(token, HideSoftInputFlags.None);

        activity.CurrentFocus?.ClearFocus();
        Raise(false, 0);
    }

    public void Show(IView? view)
    {
        var activity = Platform.CurrentActivity;
        if (activity is null)
            return;

        var native = view?.Handler?.PlatformView as View ?? activity.CurrentFocus;
        native?.RequestFocus();

        var imm = InputMethod(activity);
        if (native is not null)
            imm?.ShowSoftInput(native, ShowFlags.Implicit);
    }

    public void SetAvoidanceMode(KeyboardAvoidanceMode mode, double extraPadding)
    {
        avoidanceMode = mode;
        this.extraPadding = extraPadding;

        var window = Platform.CurrentActivity?.Window;
        if (window is null)
            return;

        window.SetSoftInputMode(mode switch
        {
            KeyboardAvoidanceMode.None => SoftInput.AdjustNothing,
            KeyboardAvoidanceMode.Pan => SoftInput.AdjustPan,
            KeyboardAvoidanceMode.Resize => SoftInput.AdjustResize,
            KeyboardAvoidanceMode.SafeArea => SoftInput.AdjustNothing,
            _ => SoftInput.AdjustUnspecified
        });
    }

    public void ApplyNativeAvoidance(KeyboardAvoidanceMode mode, bool visible, double keyboardHeight, double extraPadding)
    {
        _ = mode;
        _ = visible;
        _ = keyboardHeight;
        _ = extraPadding;
        SetAvoidanceMode(avoidanceMode, this.extraPadding);
    }

    public void SetDismissOnTapOutside(bool enabled) => dismissOnTapOutside = enabled;

    public void Start()
    {
        if (started)
        {
            Attach();
            return;
        }

        started = true;
        Attach();
    }

    public void Stop()
    {
        Detach();
        started = false;
    }

    void Attach()
    {
        var activity = Platform.CurrentActivity;
        var decor = activity?.Window?.DecorView;
        if (decor is null || ReferenceEquals(attachedDecor, decor))
            return;

        Detach();
        attachedDecor = decor;
        SetAvoidanceMode(avoidanceMode, extraPadding);

        insetsListener = new ImeInsetsListener(OnInsets);
        ViewCompat.SetOnApplyWindowInsetsListener(decor, insetsListener);

        layoutListener = new GlobalLayoutListener(OnGlobalLayout);
        decor.ViewTreeObserver?.AddOnGlobalLayoutListener(layoutListener);

        if (dismissOnTapOutside)
            decor.Touch += OnDecorTouch;
    }

    void Detach()
    {
        if (attachedDecor is null)
            return;

        ViewCompat.SetOnApplyWindowInsetsListener(attachedDecor, null);
        if (layoutListener is not null)
            attachedDecor.ViewTreeObserver?.RemoveOnGlobalLayoutListener(layoutListener);
        attachedDecor.Touch -= OnDecorTouch;
        attachedDecor = null;
        insetsListener = null;
        layoutListener = null;
    }

    void OnDecorTouch(object? sender, View.TouchEventArgs e)
    {
        if (!dismissOnTapOutside || e.Event is null || e.Event.Action != MotionEventActions.Down)
            return;

        var focused = Platform.CurrentActivity?.CurrentFocus;
        if (focused is null || !IsTextInput(focused))
            return;

        if (IsOutside(focused, e.Event))
            Hide();
    }

    void OnInsets(int imeBottomPx, View view)
    {
        var density = Density(view);
        var height = imeBottomPx / density;
        var visible = imeBottomPx > 0;
        Raise(visible, visible ? height : 0);
    }

    void OnGlobalLayout()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            return;

        var decor = attachedDecor ?? Platform.CurrentActivity?.Window?.DecorView;
        if (decor is null)
            return;

        var frame = new Android.Graphics.Rect();
        decor.GetWindowVisibleDisplayFrame(frame);
        var keyboardPx = Math.Max(0, (decor.RootView?.Height ?? decor.Height) - frame.Bottom);
        var density = Density(decor);
        var threshold = 80 * density;
        var visible = keyboardPx > threshold;
        Raise(visible, visible ? keyboardPx / density : 0);
    }

    void Raise(bool visible, double height)
    {
        if (visible == lastHeight > 0 && Math.Abs(lastHeight - height) < 0.5)
            return;

        lastHeight = height;
        KeyboardChanged?.Invoke(this, new NativeKeyboardEventArgs(visible, height));
    }

    static InputMethodManager? InputMethod(Android.App.Activity activity) =>
        activity.GetSystemService(Context.InputMethodService) as InputMethodManager;

    static float Density(View view)
    {
        var density = view.Resources?.DisplayMetrics?.Density ?? 1f;
        return density > 0 ? density : 1f;
    }

    static bool IsTextInput(View view) =>
        view is Android.Widget.EditText or Android.Widget.SearchView;

    static bool IsOutside(View view, MotionEvent ev)
    {
        var location = new int[2];
        view.GetLocationOnScreen(location);
        var x = ev.RawX;
        var y = ev.RawY;
        return x < location[0]
            || x > location[0] + view.Width
            || y < location[1]
            || y > location[1] + view.Height;
    }

    sealed class ImeInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        readonly Action<int, View> onInsets;

        public ImeInsetsListener(Action<int, View> onInsets) => this.onInsets = onInsets;

        public WindowInsetsCompat? OnApplyWindowInsets(View? v, WindowInsetsCompat? insets)
        {
            if (v is null || insets is null)
                return insets;

            var ime = insets.GetInsets(WindowInsetsCompat.Type.Ime());
            onInsets(ime?.Bottom ?? 0, v);
            return insets;
        }
    }

    sealed class GlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        readonly Action callback;

        public GlobalLayoutListener(Action callback) => this.callback = callback;

        public void OnGlobalLayout() => callback();
    }
}
#endif
