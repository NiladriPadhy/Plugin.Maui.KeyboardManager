namespace Plugin.Maui.KeyboardManager.Tests;

public sealed class AvoidanceModeTests
{
    [Fact]
    public void SetAvoidanceMode_updates_mode_and_snapshot()
    {
        var (manager, platform) = Harness.Create();

        manager.SetAvoidanceMode(KeyboardAvoidanceMode.Pan);

        Assert.Equal(KeyboardAvoidanceMode.Pan, manager.AvoidanceMode);
        Assert.Equal(KeyboardAvoidanceMode.Pan, manager.GetSnapshot().AvoidanceMode);
        Assert.Equal(KeyboardAvoidanceMode.Pan, platform.AvoidanceMode);
    }

    [Theory]
    [InlineData(KeyboardAvoidanceMode.System)]
    [InlineData(KeyboardAvoidanceMode.None)]
    [InlineData(KeyboardAvoidanceMode.Pan)]
    [InlineData(KeyboardAvoidanceMode.Resize)]
    [InlineData(KeyboardAvoidanceMode.SafeArea)]
    public void Every_avoidance_mode_can_be_set(KeyboardAvoidanceMode mode)
    {
        var (manager, _) = Harness.Create();

        manager.SetAvoidanceMode(mode);

        Assert.Equal(mode, manager.AvoidanceMode);
        Assert.Equal(mode, manager.Options.AvoidanceMode);
    }

    [Fact]
    public void Configure_applies_avoidance_and_tap_outside()
    {
        var (manager, platform) = Harness.Create();

        manager.Configure(options =>
        {
            options.AvoidanceMode = KeyboardAvoidanceMode.SafeArea;
            options.DismissOnTapOutside = false;
            options.ExtraAvoidancePadding = 12;
        });

        Assert.Equal(KeyboardAvoidanceMode.SafeArea, manager.AvoidanceMode);
        Assert.False(manager.IsDismissOnTapOutsideEnabled);
        Assert.Equal(12, manager.Options.ExtraAvoidancePadding);
        Assert.False(platform.DismissOnTapOutside);
    }

    [Fact]
    public void Static_configure_creates_shared_instance()
    {
        KeyboardManager.Reset();

        KeyboardManager.Configure(options =>
        {
            options.AvoidanceMode = KeyboardAvoidanceMode.Pan;
            options.DismissOnTapOutside = true;
        });

        KeyboardManager.SetAvoidanceMode(KeyboardAvoidanceMode.None);
        KeyboardManager.DismissOnTapOutside();

        Assert.Equal(KeyboardAvoidanceMode.None, KeyboardManager.Current.AvoidanceMode);
        Assert.True(KeyboardManager.Current.IsDismissOnTapOutsideEnabled);
    }

    [Fact]
    public void Options_defaults_are_resize_and_dismiss_on_tap()
    {
        var options = new KeyboardManagerOptions();

        Assert.Equal(KeyboardAvoidanceMode.Resize, options.AvoidanceMode);
        Assert.True(options.DismissOnTapOutside);
        Assert.Equal(0, options.ExtraAvoidancePadding);
    }
}
