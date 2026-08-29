namespace Plugin.Maui.KeyboardManager;

sealed class KeyboardManagerImplementation : IKeyboardManager
{
    readonly IKeyboardPlatform platform;
    readonly HashSet<VisualElement> watchedInputs = [];

    Page? paddedPage;
    Thickness? storedPadding;
    WeakReference<Page>? appearingPage;
    bool started;
    bool dismissOnTapOutside;

    public KeyboardManagerImplementation(KeyboardManagerOptions options, IKeyboardPlatform platform)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        this.platform = platform ?? throw new ArgumentNullException(nameof(platform));
        AvoidanceMode = options.AvoidanceMode;
        dismissOnTapOutside = options.DismissOnTapOutside;
        this.platform.KeyboardChanged += OnPlatformKeyboardChanged;
    }

    public bool IsVisible { get; private set; }

    public double KeyboardHeight { get; private set; }

    public IView? FocusedView { get; private set; }

    public KeyboardAvoidanceMode AvoidanceMode { get; private set; }

    public bool IsDismissOnTapOutsideEnabled => dismissOnTapOutside;

    public Thickness SafeAreaInsets => platform.GetSafeAreaInsets();

    public Thickness KeyboardInsets =>
        IsVisible ? new Thickness(0, 0, 0, KeyboardHeight) : default;

    public KeyboardManagerOptions Options { get; }

    public event EventHandler<KeyboardChangedEventArgs>? VisibilityChanged;

    public event EventHandler<KeyboardChangedEventArgs>? HeightChanged;

    public event EventHandler<KeyboardFocusChangedEventArgs>? FocusChanged;

    public void Start()
    {
        if (started)
            return;

        started = true;
        platform.KeyboardChanged -= OnPlatformKeyboardChanged;
        platform.KeyboardChanged += OnPlatformKeyboardChanged;
        platform.Start();
        platform.SetAvoidanceMode(AvoidanceMode, Options.ExtraAvoidancePadding);
        platform.SetDismissOnTapOutside(dismissOnTapOutside);
        HookApplication();
        ApplyDismissOnTapOutside();
        ApplyPagePadding();
    }

    public void Stop()
    {
        if (!started)
            return;

        started = false;
        UnhookApplication();
        UnwatchInputs();
        platform.KeyboardChanged -= OnPlatformKeyboardChanged;
        platform.Stop();
    }

    public void Hide()
    {
        Start();

        if (FocusedView is VisualElement focused && focused.IsFocused)
            focused.Unfocus();

        SetFocused(null);
        platform.Hide();
    }

    public void Show(IView? view = null)
    {
        Start();

        view ??= FocusedView;
        if (view is VisualElement visual && !visual.IsFocused)
            visual.Focus();

        if (view is not null)
            SetFocused(view);

        platform.Show(view);
    }

    public void DismissOnTapOutside(bool enabled = true)
    {
        Start();
        dismissOnTapOutside = enabled;
        Options.DismissOnTapOutside = enabled;
        platform.SetDismissOnTapOutside(enabled);
        ApplyDismissOnTapOutside();
    }

    public void SetAvoidanceMode(KeyboardAvoidanceMode mode)
    {
        Start();
        AvoidanceMode = mode;
        Options.AvoidanceMode = mode;
        platform.SetAvoidanceMode(mode, Options.ExtraAvoidancePadding);
        ApplyPagePadding();
        platform.ApplyNativeAvoidance(mode, IsVisible, KeyboardHeight, Options.ExtraAvoidancePadding);
    }

    public void Configure(Action<KeyboardManagerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Options);
        SetAvoidanceMode(Options.AvoidanceMode);
        DismissOnTapOutside(Options.DismissOnTapOutside);
    }

    public KeyboardSnapshot GetSnapshot() =>
        new()
        {
            IsVisible = IsVisible,
            KeyboardHeight = KeyboardHeight,
            AvoidanceMode = AvoidanceMode,
            DismissOnTapOutside = dismissOnTapOutside,
            SafeAreaInsets = SafeAreaInsets,
            KeyboardInsets = KeyboardInsets,
            HasFocus = FocusedView is not null
        };

    internal void NotifyTapOutside()
    {
        if (dismissOnTapOutside)
            Hide();
    }

    internal IKeyboardPlatform Platform => platform;

    void OnPlatformKeyboardChanged(object? sender, NativeKeyboardEventArgs e)
    {
        var raise = () =>
        {
            var visibilityChanged = IsVisible != e.IsVisible;
            var heightChanged = Math.Abs(KeyboardHeight - e.KeyboardHeight) > 0.5;

            IsVisible = e.IsVisible;
            KeyboardHeight = e.IsVisible ? Math.Max(0, e.KeyboardHeight) : 0;

            if (!IsVisible)
                SetFocused(FocusedView is VisualElement ve && ve.IsFocused ? FocusedView : null);

            ApplyPagePadding();
            platform.ApplyNativeAvoidance(AvoidanceMode, IsVisible, KeyboardHeight, Options.ExtraAvoidancePadding);

            if (!visibilityChanged && !heightChanged)
                return;

            var args = new KeyboardChangedEventArgs(IsVisible, KeyboardHeight, SafeAreaInsets);
            if (visibilityChanged)
                VisibilityChanged?.Invoke(this, args);
            if (heightChanged)
                HeightChanged?.Invoke(this, args);
        };

#if ANDROID || IOS
        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(raise);
            return;
        }
#endif
        raise();
    }

    void SetFocused(IView? view)
    {
        if (ReferenceEquals(FocusedView, view))
            return;

        var previous = FocusedView;
        FocusedView = view;
        FocusChanged?.Invoke(this, new KeyboardFocusChangedEventArgs(previous, view));
    }

    void HookApplication()
    {
        var app = Application.Current;
        if (app is null)
            return;

        app.PageAppearing -= OnPageAppearing;
        app.PageAppearing += OnPageAppearing;
        app.PageDisappearing -= OnPageDisappearing;
        app.PageDisappearing += OnPageDisappearing;
    }

    void UnhookApplication()
    {
        var app = Application.Current;
        if (app is null)
            return;

        app.PageAppearing -= OnPageAppearing;
        app.PageDisappearing -= OnPageDisappearing;
    }

    void OnPageAppearing(object? sender, Page page)
    {
        appearingPage = new WeakReference<Page>(page);
        WatchInputs(page);
        ApplyDismissOnTapOutside(page);
        ApplyPagePadding();
    }

    void OnPageDisappearing(object? sender, Page page)
    {
        UnwatchInputs();
        if (ReferenceEquals(paddedPage, page))
            RestorePadding(page);
    }

    void WatchInputs(Page page)
    {
        UnwatchInputs();

        foreach (var input in EnumerateInputs(page))
        {
            input.Focused += OnInputFocused;
            input.Unfocused += OnInputUnfocused;
            watchedInputs.Add(input);
        }
    }

    void UnwatchInputs()
    {
        foreach (var input in watchedInputs)
        {
            input.Focused -= OnInputFocused;
            input.Unfocused -= OnInputUnfocused;
        }

        watchedInputs.Clear();
    }

    void OnInputFocused(object? sender, FocusEventArgs e)
    {
        if (sender is IView view)
            SetFocused(view);
    }

    void OnInputUnfocused(object? sender, FocusEventArgs e)
    {
        if (ReferenceEquals(sender, FocusedView))
            SetFocused(null);
    }

    void ApplyDismissOnTapOutside()
    {
        if (TryGetCurrentPage() is { } page)
            ApplyDismissOnTapOutside(page);
    }

    void ApplyDismissOnTapOutside(Page page)
    {
        if (page is ContentPage contentPage)
            contentPage.HideSoftInputOnTapped = dismissOnTapOutside;
    }

    void ApplyPagePadding()
    {
        var page = TryGetCurrentPage();
        if (page is null)
            return;

        var shouldPad = ShouldPadPage();
        if (!shouldPad || !IsVisible)
        {
            RestorePadding(paddedPage ?? page);
            return;
        }

        if (!ReferenceEquals(paddedPage, page))
            RestorePadding(paddedPage);

        storedPadding ??= page.Padding;
        var original = storedPadding.Value;
        page.Padding = new Thickness(
            original.Left,
            original.Top,
            original.Right,
            original.Bottom + KeyboardHeight + Options.ExtraAvoidancePadding);
        paddedPage = page;
    }

    bool ShouldPadPage() =>
        AvoidanceMode is KeyboardAvoidanceMode.SafeArea
        || (AvoidanceMode is KeyboardAvoidanceMode.Resize && !OperatingSystem.IsAndroid());

    void RestorePadding(Page? page)
    {
        if (page is not null && storedPadding is { } padding)
            page.Padding = padding;

        storedPadding = null;
        paddedPage = null;
    }

    Page? TryGetCurrentPage()
    {
        if (appearingPage is not null && appearingPage.TryGetTarget(out var appearing))
            return appearing;

        var window = Application.Current?.Windows.Count > 0
            ? Application.Current.Windows[0]
            : null;
        return Unwrap(window?.Page);
    }

    static Page? Unwrap(Page? page) =>
        page switch
        {
            Shell shell => shell.CurrentPage,
            NavigationPage navigation => navigation.CurrentPage,
            TabbedPage tabbed => Unwrap(tabbed.CurrentPage),
            FlyoutPage flyout => Unwrap(flyout.Detail),
            _ => page
        };

    static IEnumerable<VisualElement> EnumerateInputs(Element root)
    {
        foreach (var element in EnumerateVisualTree(root))
        {
            if (element is InputView)
                yield return element;
        }
    }

    static IEnumerable<VisualElement> EnumerateVisualTree(Element? root)
    {
        if (root is null)
            yield break;

        if (root is VisualElement visual)
            yield return visual;

        if (root is not IVisualTreeElement tree)
            yield break;

        foreach (var child in tree.GetVisualChildren())
        {
            if (child is Element element)
            {
                foreach (var nested in EnumerateVisualTree(element))
                    yield return nested;
            }
        }
    }
}
