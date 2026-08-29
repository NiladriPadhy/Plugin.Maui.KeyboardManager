namespace Plugin.Maui.KeyboardManager.Tests;

public sealed class KeyboardManagerTests
{
    [Fact]
    public void Hide_clears_visibility_and_height()
    {
        var (manager, _) = Harness.Create();
        manager.Show();

        manager.Hide();

        Assert.False(manager.IsVisible);
        Assert.Equal(0, manager.KeyboardHeight);
        Assert.Equal(default, manager.KeyboardInsets);
    }

    [Fact]
    public void Show_sets_visibility_and_simulated_height()
    {
        var (manager, platform) = Harness.Create();

        manager.Show();

        Assert.True(manager.IsVisible);
        Assert.Equal(platform.SimulatedHeight, manager.KeyboardHeight);
        Assert.Equal(new Thickness(0, 0, 0, platform.SimulatedHeight), manager.KeyboardInsets);
    }

    [Fact]
    public void Show_tracks_focused_view()
    {
        var (manager, _) = Harness.Create();
        var entry = new Entry();

        manager.Show(entry);

        Assert.Same(entry, manager.FocusedView);
        Assert.True(manager.GetSnapshot().HasFocus);
    }

    [Fact]
    public void Hide_clears_focus()
    {
        var (manager, _) = Harness.Create();
        manager.Show(new Entry());

        manager.Hide();

        Assert.Null(manager.FocusedView);
        Assert.False(manager.GetSnapshot().HasFocus);
    }

    [Fact]
    public void VisibilityChanged_fires_on_show_and_hide()
    {
        var (manager, _) = Harness.Create();
        var visible = new List<bool>();
        manager.VisibilityChanged += (_, e) => visible.Add(e.IsVisible);

        manager.Show();
        manager.Hide();

        Assert.Equal([true, false], visible);
    }

    [Fact]
    public void HeightChanged_fires_with_keyboard_height()
    {
        var (manager, platform) = Harness.Create();
        double? height = null;
        manager.HeightChanged += (_, e) => height = e.KeyboardHeight;

        manager.Show();

        Assert.Equal(platform.SimulatedHeight, height);
    }

    [Fact]
    public void FocusChanged_reports_previous_and_current()
    {
        var (manager, _) = Harness.Create();
        var first = new Entry();
        var second = new Entry();
        KeyboardFocusChangedEventArgs? args = null;
        manager.FocusChanged += (_, e) => args = e;

        manager.Show(first);
        manager.Show(second);

        Assert.NotNull(args);
        Assert.Same(first, args.Previous);
        Assert.Same(second, args.Current);
        Assert.True(args.IsFocused);
    }

    [Fact]
    public void DismissOnTapOutside_hides_when_notified()
    {
        var (manager, _) = Harness.Create();
        manager.Show();
        manager.DismissOnTapOutside();

        manager.NotifyTapOutside();

        Assert.False(manager.IsVisible);
    }

    [Fact]
    public void DismissOnTapOutside_false_does_not_hide()
    {
        var (manager, _) = Harness.Create();
        manager.Show();
        manager.DismissOnTapOutside(false);

        manager.NotifyTapOutside();

        Assert.True(manager.IsVisible);
    }

    [Fact]
    public void SafeAreaInsets_come_from_the_platform()
    {
        var (manager, platform) = Harness.Create();
        platform.SimulatedSafeArea = new Thickness(10, 47, 10, 34);

        Assert.Equal(platform.SimulatedSafeArea, manager.SafeAreaInsets);
    }

    [Fact]
    public void Snapshot_includes_insets_and_mode()
    {
        var (manager, platform) = Harness.Create(options =>
        {
            options.AvoidanceMode = KeyboardAvoidanceMode.SafeArea;
            options.DismissOnTapOutside = false;
        });
        manager.Show();

        var snapshot = manager.GetSnapshot();

        Assert.True(snapshot.IsVisible);
        Assert.Equal(platform.SimulatedHeight, snapshot.KeyboardHeight);
        Assert.Equal(KeyboardAvoidanceMode.SafeArea, snapshot.AvoidanceMode);
        Assert.False(snapshot.DismissOnTapOutside);
        Assert.Equal(new Thickness(0, 0, 0, platform.SimulatedHeight), snapshot.KeyboardInsets);
        Assert.Equal(platform.SimulatedSafeArea, snapshot.SafeAreaInsets);
    }

    [Fact]
    public void Static_api_hide_show_and_snapshot()
    {
        Harness.Create();

        KeyboardManager.Show();
        Assert.True(KeyboardManager.IsVisible);
        Assert.True(KeyboardManager.GetSnapshot().IsVisible);

        KeyboardManager.Hide();
        Assert.False(KeyboardManager.IsVisible);
        Assert.Equal(0, KeyboardManager.KeyboardHeight);
    }

    [Fact]
    public void Platform_height_change_updates_manager()
    {
        var (manager, platform) = Harness.Create();
        var heights = new List<double>();
        manager.HeightChanged += (_, e) => heights.Add(e.KeyboardHeight);

        platform.Simulate(true, 220);
        platform.Simulate(true, 280);
        platform.Simulate(false, 0);

        Assert.Equal([220, 280, 0], heights);
        Assert.False(manager.IsVisible);
    }
}
