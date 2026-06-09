using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Theme;
using Xunit;

namespace NextNet.UI.Abstractions.Tests.Theme;

public class ThemeProviderTests
{
    [Fact]
    public void GetTheme_Should_ReturnTokens_When_ThemeExists()
    {
        var provider = new TestThemeProvider();

        var tokens = provider.GetTheme("light");

        Assert.NotNull(tokens);
        Assert.Empty(tokens.Colors);
    }

    [Fact]
    public void GetTheme_Should_Throw_When_ThemeNotFound()
    {
        var provider = new TestThemeProvider();

        Assert.Throws<KeyNotFoundException>(() => provider.GetTheme("nonexistent"));
    }

    [Fact]
    public void GetTheme_Should_Throw_When_ThemeNameIsNull()
    {
        var provider = new TestThemeProvider();

        Assert.Throws<ArgumentNullException>(() => provider.GetTheme(null!));
    }

    [Fact]
    public void ActiveTheme_Should_ReturnCurrentTheme()
    {
        var provider = new TestThemeProvider();

        Assert.Equal("light", provider.ActiveTheme);
    }

    [Fact]
    public void AvailableThemes_Should_ListAllThemes()
    {
        var provider = new TestThemeProvider();

        Assert.Contains("light", provider.AvailableThemes);
        Assert.Contains("dark", provider.AvailableThemes);
        Assert.Equal(2, provider.AvailableThemes.Count);
    }

    [Fact]
    public void ThemeChanged_Should_BeRaised_WhenThemeChanges()
    {
        var provider = new TestThemeProvider();
        ThemeChangedEventArgs? capturedArgs = null;

        provider.ThemeChanged += (sender, args) =>
        {
            capturedArgs = args;
        };

        provider.SetActiveTheme("dark");

        Assert.NotNull(capturedArgs);
        Assert.Equal("light", capturedArgs!.OldTheme);
        Assert.Equal("dark", capturedArgs.NewTheme);
    }

    [Fact]
    public void ThemeChangedEventArgs_Should_StoreOldAndNewTheme()
    {
        var args = new ThemeChangedEventArgs("light", "dark");

        Assert.Equal("light", args.OldTheme);
        Assert.Equal("dark", args.NewTheme);
    }

    [Fact]
    public void ThemeChangedEventArgs_Should_AllowNullOldTheme()
    {
        var args = new ThemeChangedEventArgs(null, "dark");

        Assert.Null(args.OldTheme);
        Assert.Equal("dark", args.NewTheme);
    }

    private class TestThemeProvider : IThemeProvider
    {
        private string _activeTheme = "light";
        private readonly Dictionary<string, DesignTokenSet> _themes = new()
        {
            ["light"] = new DesignTokenSet(),
            ["dark"] = new DesignTokenSet(),
        };

        public string ActiveTheme => _activeTheme;

        public IReadOnlyList<string> AvailableThemes => _themes.Keys.ToList().AsReadOnly();

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        public DesignTokenSet GetTheme(string themeName)
        {
            ArgumentException.ThrowIfNullOrEmpty(themeName);

            if (!_themes.TryGetValue(themeName, out var tokens))
            {
                throw new KeyNotFoundException($"Theme '{themeName}' not found.");
            }

            return tokens;
        }

        public void SetActiveTheme(string themeName)
        {
            var oldTheme = _activeTheme;
            _activeTheme = themeName;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, themeName));
        }
    }
}
