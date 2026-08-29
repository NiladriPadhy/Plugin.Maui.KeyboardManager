#if IOS
using CoreGraphics;
using Foundation;
using UIKit;

namespace Plugin.Maui.KeyboardManager;

sealed class IosKeyboardPlatform : IKeyboardPlatform
{
    NSObject? willShow;
    NSObject? willHide;
    NSObject? willChange;
    UITapGestureRecognizer? tap;
    KeyboardAvoidanceMode avoidanceMode = KeyboardAvoidanceMode.Resize;
    double extraPadding;
    bool dismissOnTapOutside;
    bool started;

    public event EventHandler<NativeKeyboardEventArgs>? KeyboardChanged;

    public Thickness GetSafeAreaInsets()
    {
        var insets = KeyWindow()?.SafeAreaInsets ?? UIEdgeInsets.Zero;
        return new Thickness(insets.Left, insets.Top, insets.Right, insets.Bottom);
    }

    public void Hide()
    {
        KeyWindow()?.EndEditing(true);
        Raise(false, 0);
    }

    public void Show(IView? view)
    {
        if (view?.Handler?.PlatformView is UIView native)
            native.BecomeFirstResponder();
        else
            FindFirstResponder(KeyWindow())?.BecomeFirstResponder();
    }

    public void SetAvoidanceMode(KeyboardAvoidanceMode mode, double extraPadding)
    {
        avoidanceMode = mode;
        this.extraPadding = extraPadding;
    }

    public void ApplyNativeAvoidance(KeyboardAvoidanceMode mode, bool visible, double keyboardHeight, double extraPadding)
    {
        avoidanceMode = mode;
        this.extraPadding = extraPadding;

        var root = RootView();
        if (root is null)
            return;

        if (mode is KeyboardAvoidanceMode.Pan && visible)
        {
            var overlap = Overlap(root, keyboardHeight + extraPadding);
            Animate(root, overlap > 0 ? CGAffineTransform.MakeTranslation(0, -overlap) : CGAffineTransform.MakeIdentity());
            return;
        }

        Animate(root, CGAffineTransform.MakeIdentity());
    }

    public void SetDismissOnTapOutside(bool enabled)
    {
        dismissOnTapOutside = enabled;
        if (started)
            SyncTapGesture();
    }

    public void Start()
    {
        if (started)
        {
            SyncTapGesture();
            return;
        }

        started = true;
        willShow = UIKeyboard.Notifications.ObserveWillShow((_, e) => Raise(true, Height(e.Notification)));
        willHide = UIKeyboard.Notifications.ObserveWillHide((_, _) => Raise(false, 0));
        willChange = UIKeyboard.Notifications.ObserveWillChangeFrame((_, e) =>
        {
            var height = Height(e.Notification);
            Raise(height > 0, height);
        });
        SyncTapGesture();
    }

    public void Stop()
    {
        willShow?.Dispose();
        willHide?.Dispose();
        willChange?.Dispose();
        willShow = willHide = willChange = null;
        RemoveTap();
        started = false;
    }

    void SyncTapGesture()
    {
        RemoveTap();
        if (!dismissOnTapOutside)
            return;

        var window = KeyWindow();
        if (window is null)
            return;

        tap = new UITapGestureRecognizer(Hide)
        {
            CancelsTouchesInView = false,
            RequiresExclusiveTouchType = false
        };
        tap.ShouldReceiveTouch = (_, touch) =>
            touch.View is not UITextField
            && touch.View is not UITextView
            && touch.View is not UISearchBar;
        window.AddGestureRecognizer(tap);
    }

    void RemoveTap()
    {
        if (tap is null)
            return;

        tap.View?.RemoveGestureRecognizer(tap);
        tap.Dispose();
        tap = null;
    }

    void Raise(bool visible, double height) =>
        KeyboardChanged?.Invoke(this, new NativeKeyboardEventArgs(visible, height));

    static double Height(NSNotification notification)
    {
        var frame = UIKeyboard.FrameEndFromNotification(notification);
        return frame.Height;
    }

    static nfloat Overlap(UIView root, double keyboardHeight)
    {
        var first = FindFirstResponder(root);
        if (first is null)
            return 0;

        var frame = first.ConvertRectToView(first.Bounds, root);
        var keyboardTop = root.Bounds.Height - (nfloat)keyboardHeight;
        return (nfloat)Math.Max(0, (double)(frame.Bottom - keyboardTop));
    }

    static void Animate(UIView root, CGAffineTransform transform) =>
        UIView.Animate(0.25, () => root.Transform = transform);

    static UIView? FindFirstResponder(UIView? root)
    {
        if (root is null)
            return null;
        if (root.IsFirstResponder)
            return root;

        foreach (var child in root.Subviews)
        {
            var found = FindFirstResponder(child);
            if (found is not null)
                return found;
        }

        return null;
    }

    static UIView? RootView() =>
        KeyWindow()?.RootViewController?.View;

    static UIWindow? KeyWindow()
    {
        foreach (var scene in UIApplication.SharedApplication.ConnectedScenes)
        {
            if (scene is UIWindowScene windowScene)
            {
                foreach (var window in windowScene.Windows)
                {
                    if (window.IsKeyWindow)
                        return window;
                }
            }
        }

        return null;
    }
}
#endif
