using NextNet.UI.Theming.Presets;
using Xunit;

namespace NextNet.UI.Theming.Tests;

public class ThemeManagerSystemModeTests
{
    [Fact]
    public void Constructor_Should_DefaultToLightMode()
    {
        var manager = new ThemeManager();
        Assert.Equal(DarkMode.Light, manager.Mode);
    }

    [Fact]
    public void SetDarkMode_Should_SetModeToDark()
    {
        var manager = new ThemeManager();
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());
        manager.SetActiveTheme("light"); // Ensure light is active first

        manager.SetDarkMode(DarkMode.Dark);

        Assert.Equal(DarkMode.Dark, manager.Mode);
        Assert.Equal("dark", manager.ActiveTheme);
    }

    [Fact]
    public void SetDarkMode_Should_SetModeToLight()
    {
        var manager = new ThemeManager();
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());
        manager.SetActiveTheme("dark"); // Ensure dark is active first

        manager.SetDarkMode(DarkMode.Light);

        Assert.Equal(DarkMode.Light, manager.Mode);
        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void SetDarkMode_System_Should_UseResolverAndActivateLightByDefault()
    {
        var manager = new ThemeManager(new DefaultSystemPreferenceResolver());
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());
        manager.SetActiveTheme("dark"); // Ensure dark is active first

        manager.SetDarkMode(DarkMode.System);

        Assert.Equal(DarkMode.System, manager.Mode);
        // Default resolver returns false (light mode)
        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void SetDarkMode_System_Should_ActivateDark_When_ResolverPrefersDark()
    {
        var resolver = new MockDarkPreferenceResolver(prefersDark: true);
        var manager = new ThemeManager(resolver);
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());
        manager.SetActiveTheme("light"); // Ensure light is active first

        manager.SetDarkMode(DarkMode.System);

        Assert.Equal(DarkMode.System, manager.Mode);
        Assert.Equal("dark", manager.ActiveTheme);
    }

    [Fact]
    public void SetDarkMode_Should_ReturnFalse_When_ThemeNotRegistered()
    {
        var manager = new ThemeManager();
        // Don't register any themes

        var result = manager.SetDarkMode(DarkMode.Dark);

        Assert.False(result);
        Assert.Equal(string.Empty, manager.ActiveTheme);
    }

    [Fact]
    public void SetDarkMode_Should_ReturnTrue_When_ThemeRegistered()
    {
        var manager = new ThemeManager();
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());

        var result = manager.SetDarkMode(DarkMode.Dark);

        Assert.True(result);
    }

    [Fact]
    public void SetActiveTheme_WithSystem_Should_SetModeToSystem()
    {
        var resolver = new MockDarkPreferenceResolver(prefersDark: false);
        var manager = new ThemeManager(resolver);
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());

        var result = manager.SetActiveTheme("system");

        Assert.True(result);
        Assert.Equal(DarkMode.System, manager.Mode);
        Assert.Equal("light", manager.ActiveTheme);
    }

    [Fact]
    public void SetActiveTheme_WithExplicitName_Should_SetModeToLight()
    {
        var manager = new ThemeManager();
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());

        manager.SetDarkMode(DarkMode.System);
        manager.SetActiveTheme("dark");

        // Explicit theme name should override mode back to Light
        Assert.Equal(DarkMode.Light, manager.Mode);
        Assert.Equal("dark", manager.ActiveTheme);
    }

    [Fact]
    public void ThemeChanged_Should_Fire_When_SetDarkModeChangesTheme()
    {
        var manager = new ThemeManager();
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());
        manager.SetActiveTheme("light"); // Ensure a base is active

        var fired = false;
        string? oldTheme = null;
        string? newTheme = null;
        manager.ThemeChanged += (_, args) =>
        {
            fired = true;
            oldTheme = args.OldTheme;
            newTheme = args.NewTheme;
        };

        manager.SetDarkMode(DarkMode.Dark);

        Assert.True(fired);
        Assert.Equal("light", oldTheme);
        Assert.Equal("dark", newTheme);
    }

    [Fact]
    public void ThemeChanged_Should_NotFire_When_SetDarkModeWithSameMode()
    {
        var manager = new ThemeManager();
        manager.RegisterTheme(LightTheme.Create());
        manager.RegisterTheme(DarkTheme.Create());

        // First set to dark
        manager.SetDarkMode(DarkMode.Dark);

        var fired = false;
        manager.ThemeChanged += (_, _) => fired = true;

        // Set to dark again (same mode)
        manager.SetDarkMode(DarkMode.Dark);

        Assert.False(fired);
    }

    private sealed class MockDarkPreferenceResolver : ISystemPreferenceResolver
    {
        private readonly bool _prefersDark;
        public MockDarkPreferenceResolver(bool prefersDark) => _prefersDark = prefersDark;
        public bool IsDarkModePreferred() => _prefersDark;
    }
}
