using System.Collections.Generic;
using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Theme;
using NextNet.UI.Theming.Presets;
using Xunit;

namespace NextNet.UI.Theming.Tests;

public class ThemeManagerTests
{
    private readonly ThemeManager _manager;

    public ThemeManagerTests()
    {
        _manager = new ThemeManager();
        _manager.RegisterTheme(LightTheme.Create());
        _manager.RegisterTheme(DarkTheme.Create());
    }

    [Fact]
    public void RegisterTheme_Should_AddTheme_When_Valid()
    {
        var custom = CreateCustomTheme("custom");
        _manager.RegisterTheme(custom);

        Assert.Contains("custom", _manager.AvailableThemes);
    }

    [Fact]
    public void RegisterTheme_Should_ThrowArgumentNullException_When_ThemeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _manager.RegisterTheme(null!));
    }

    [Fact]
    public void RegisterTheme_Should_ThrowArgumentException_When_ThemeNameIsEmpty()
    {
        var theme = CreateCustomTheme("");
        var ex = Assert.Throws<ArgumentException>(() => _manager.RegisterTheme(theme));
        Assert.Contains("DS-204", ex.Message);
    }

    [Fact]
    public void RegisterTheme_Should_ReplaceExisting_When_DuplicateName()
    {
        var first = CreateCustomTheme("dup", tokens: new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#FF0000")
            }));
        var second = CreateCustomTheme("dup", tokens: new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#00FF00")
            }));

        _manager.RegisterTheme(first);
        _manager.RegisterTheme(second);

        var result = _manager.GetTheme("dup");
        Assert.Equal("#00FF00", result.Colors["primary-500"].Value);
    }

    [Fact]
    public void GetTheme_Should_ReturnTokens_When_ThemeExists()
    {
        var tokens = _manager.GetTheme("light");

        Assert.NotNull(tokens);
        Assert.NotEmpty(tokens.Colors);
        Assert.NotEmpty(tokens.Spacing);
    }

    [Fact]
    public void GetTheme_Should_ThrowKeyNotFoundException_When_ThemeNotFound()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => _manager.GetTheme("nonexistent"));
        Assert.Contains("DS-201", ex.Message);
    }

    [Fact]
    public void GetTheme_Should_ThrowArgumentException_When_NameIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() => _manager.GetTheme(""));
        Assert.Contains("DS-200", ex.Message);
    }

    [Fact]
    public void GetTheme_Should_ThrowArgumentException_When_NameIsNull()
    {
        var ex = Assert.Throws<ArgumentException>(() => _manager.GetTheme(null!));
        Assert.Contains("DS-200", ex.Message);
    }

    [Fact]
    public void GetThemeObject_Should_ReturnFullTheme_When_ThemeExists()
    {
        var theme = _manager.GetThemeObject("dark");

        Assert.NotNull(theme);
        Assert.Equal("dark", theme.Name);
        Assert.True(theme.Metadata.IsDark);
        Assert.Equal("Dark", theme.Metadata.DisplayName);
    }

    [Fact]
    public void GetThemeObject_Should_ThrowKeyNotFoundException_When_ThemeNotFound()
    {
        var ex = Assert.Throws<KeyNotFoundException>(() => _manager.GetThemeObject("nonexistent"));
        Assert.Contains("DS-203", ex.Message);
    }

    [Fact]
    public void ActiveTheme_Should_BeEmpty_Initially()
    {
        var fresh = new ThemeManager();
        Assert.Equal(string.Empty, fresh.ActiveTheme);
    }

    [Fact]
    public void ActiveTheme_Should_BeSet_After_SetActiveTheme()
    {
        _manager.SetActiveTheme("dark");
        Assert.Equal("dark", _manager.ActiveTheme);
    }

    [Fact]
    public void SetActiveTheme_Should_ReturnTrue_When_ThemeExists()
    {
        var result = _manager.SetActiveTheme("dark");
        Assert.True(result);
    }

    [Fact]
    public void SetActiveTheme_Should_ReturnFalse_When_ThemeNotFound()
    {
        var result = _manager.SetActiveTheme("nonexistent");
        Assert.False(result);
    }

    [Fact]
    public void SetActiveTheme_Should_ThrowArgumentException_When_NameIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() => _manager.SetActiveTheme(""));
        Assert.Contains("DS-205", ex.Message);
    }

    [Fact]
    public void AvailableThemes_Should_ContainRegisteredThemes()
    {
        Assert.Contains("light", _manager.AvailableThemes);
        Assert.Contains("dark", _manager.AvailableThemes);
    }

    [Fact]
    public void AvailableThemes_Should_BeReadOnly()
    {
        Assert.IsAssignableFrom<IReadOnlyList<string>>(_manager.AvailableThemes);
    }

    [Fact]
    public void ThemeChanged_Should_Fire_When_ActiveThemeChanges()
    {
        _manager.RegisterTheme(CreateCustomTheme("theme-a"));
        _manager.RegisterTheme(CreateCustomTheme("theme-b"));

        var events = new List<(string? Old, string New)>();
        _manager.ThemeChanged += (_, args) =>
        {
            events.Add((args.OldTheme, args.NewTheme));
        };

        _manager.SetActiveTheme("theme-a");
        _manager.SetActiveTheme("theme-b");

        Assert.Equal(2, events.Count);
        Assert.Null(events[0].Old);
        Assert.Equal("theme-a", events[0].New);
        Assert.Equal("theme-a", events[1].Old);
        Assert.Equal("theme-b", events[1].New);
    }

    [Fact]
    public void ThemeChanged_Should_NotFire_When_SettingSameTheme()
    {
        _manager.SetActiveTheme("light");

        var fired = false;
        _manager.ThemeChanged += (_, _) => fired = true;

        _manager.SetActiveTheme("light");

        Assert.False(fired);
    }

    [Fact]
    public void ThemeChanged_Should_ProvideOldAndNewNames()
    {
        _manager.RegisterTheme(CreateCustomTheme("theme-a"));
        _manager.RegisterTheme(CreateCustomTheme("theme-b"));
        _manager.SetActiveTheme("theme-a");

        string? capturedOld = null;
        string? capturedNew = null;
        _manager.ThemeChanged += (_, args) =>
        {
            capturedOld = args.OldTheme;
            capturedNew = args.NewTheme;
        };

        _manager.SetActiveTheme("theme-b");

        Assert.Equal("theme-a", capturedOld);
        Assert.Equal("theme-b", capturedNew);
    }

    [Fact]
    public void IThemeProvider_GetTheme_Should_ReturnTokens()
    {
        IThemeProvider provider = _manager;
        var tokens = provider.GetTheme("light");

        Assert.NotNull(tokens);
        Assert.NotEmpty(tokens.Colors);
    }

    [Fact]
    public void IThemeProvider_ActiveTheme_Should_Match()
    {
        _manager.SetActiveTheme("dark");
        IThemeProvider provider = _manager;
        Assert.Equal("dark", provider.ActiveTheme);
    }

    [Fact]
    public void IThemeProvider_AvailableThemes_Should_ListRegistered()
    {
        IThemeProvider provider = _manager;
        Assert.Contains("light", provider.AvailableThemes);
        Assert.Contains("dark", provider.AvailableThemes);
    }

    private static Theme CreateCustomTheme(string name, string? displayName = null, DesignTokenSet? tokens = null)
    {
        return new Theme(
            name,
            tokens ?? new DesignTokenSet(),
            new ThemeMetadata(
                IsDark: false,
                DisplayName: displayName ?? name,
                Description: null,
                IconUrl: null));
    }
}
