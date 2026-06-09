using NextNet.DesignSystem.Defaults;
using NextNet.UI.Theming.Presets;
using Xunit;

namespace NextNet.UI.Theming.Tests.Presets;

public class PresetThemeTests
{
    [Fact]
    public void LightTheme_Create_Should_ReturnNonNullTheme()
    {
        var theme = LightTheme.Create();
        Assert.NotNull(theme);
    }

    [Fact]
    public void LightTheme_Create_Should_HaveCorrectName()
    {
        var theme = LightTheme.Create();
        Assert.Equal("light", theme.Name);
    }

    [Fact]
    public void LightTheme_Create_Should_HaveIsDarkFalse()
    {
        var theme = LightTheme.Create();
        Assert.False(theme.Metadata.IsDark);
    }

    [Fact]
    public void LightTheme_Create_Should_HaveDisplayName()
    {
        var theme = LightTheme.Create();
        Assert.Equal("Light", theme.Metadata.DisplayName);
    }

    [Fact]
    public void LightTheme_Create_Should_HaveAllTokenCategories()
    {
        var theme = LightTheme.Create();
        Assert.NotEmpty(theme.Tokens.Colors);
        Assert.NotEmpty(theme.Tokens.Spacing);
        Assert.NotEmpty(theme.Tokens.Typography);
        Assert.NotEmpty(theme.Tokens.Borders);
        Assert.NotEmpty(theme.Tokens.Shadows);
        Assert.NotEmpty(theme.Tokens.Breakpoints);
    }

    [Fact]
    public void LightTheme_Create_Should_BeLightColored()
    {
        var theme = LightTheme.Create();
        // Light backgrounds should have bright gray-50
        Assert.Equal("#F9FAFB", theme.Tokens.Colors["gray-50"].Value);
        // Dark text uses dark gray-900
        Assert.Equal("#111827", theme.Tokens.Colors["gray-900"].Value);
    }

    [Fact]
    public void LightTheme_Colors_Should_MatchDefaults()
    {
        var defaults = DefaultTokens.Create();
        var theme = LightTheme.Create();

        Assert.Equal(defaults.Colors["primary-500"].Value, theme.Tokens.Colors["primary-500"].Value);
        Assert.Equal(defaults.Colors["danger-500"].Hover, theme.Tokens.Colors["danger-500"].Hover);
    }

    [Fact]
    public void DarkTheme_Create_Should_ReturnNonNullTheme()
    {
        var theme = DarkTheme.Create();
        Assert.NotNull(theme);
    }

    [Fact]
    public void DarkTheme_Create_Should_HaveCorrectName()
    {
        var theme = DarkTheme.Create();
        Assert.Equal("dark", theme.Name);
    }

    [Fact]
    public void DarkTheme_Create_Should_HaveIsDarkTrue()
    {
        var theme = DarkTheme.Create();
        Assert.True(theme.Metadata.IsDark);
    }

    [Fact]
    public void DarkTheme_Create_Should_HaveDisplayName()
    {
        var theme = DarkTheme.Create();
        Assert.Equal("Dark", theme.Metadata.DisplayName);
    }

    [Fact]
    public void DarkTheme_Create_Should_HaveAllTokenCategories()
    {
        var theme = DarkTheme.Create();
        Assert.NotEmpty(theme.Tokens.Colors);
        Assert.NotEmpty(theme.Tokens.Spacing);
        Assert.NotEmpty(theme.Tokens.Typography);
        Assert.NotEmpty(theme.Tokens.Borders);
        Assert.NotEmpty(theme.Tokens.Shadows);
        Assert.NotEmpty(theme.Tokens.Breakpoints);
    }

    [Fact]
    public void DarkTheme_Create_Should_BeDarkColored()
    {
        var theme = DarkTheme.Create();
        // Dark backgrounds: gray-50 should be the darkest shade
        Assert.Equal("#030712", theme.Tokens.Colors["gray-50"].Value);
        // Light text: gray-900 should be a light shade
        Assert.Equal("#F3F4F6", theme.Tokens.Colors["gray-900"].Value);
    }

    [Fact]
    public void DarkTheme_Create_Should_InvertGrayScale()
    {
        var light = LightTheme.Create();
        var dark = DarkTheme.Create();

        // The lightest light gray should be the darkest dark gray
        Assert.Equal(light.Tokens.Colors["gray-950"].Value, dark.Tokens.Colors["gray-50"].Value);
        // The darkest light gray should be the lightest dark gray
        Assert.Equal(light.Tokens.Colors["gray-50"].Value, dark.Tokens.Colors["gray-950"].Value);
    }

    [Fact]
    public void DarkTheme_Create_Should_UseCorrectPrimaryHover()
    {
        var dark = DarkTheme.Create();
        var primary500 = dark.Tokens.Colors["primary-500"];

        Assert.NotNull(primary500.Hover);
        Assert.NotNull(primary500.Active);
        Assert.NotNull(primary500.Foreground);
    }

    [Fact]
    public void DarkTheme_Create_Should_UseCorrectDangerHover()
    {
        var dark = DarkTheme.Create();
        var danger500 = dark.Tokens.Colors["danger-500"];

        Assert.NotNull(danger500.Hover);
        Assert.NotNull(danger500.Active);
        Assert.NotNull(danger500.Foreground);
    }

    [Fact]
    public void DarkTheme_Create_Should_UseCorrectSuccessHover()
    {
        var dark = DarkTheme.Create();
        var success500 = dark.Tokens.Colors["success-500"];

        Assert.NotNull(success500.Hover);
        Assert.NotNull(success500.Active);
        Assert.NotNull(success500.Foreground);
    }

    [Fact]
    public void DarkTheme_Create_Should_UseCorrectWarningHover()
    {
        var dark = DarkTheme.Create();
        var warning500 = dark.Tokens.Colors["warning-500"];

        Assert.NotNull(warning500.Hover);
        Assert.NotNull(warning500.Active);
        Assert.NotNull(warning500.Foreground);
    }

    [Fact]
    public void DarkTheme_Create_Should_UseCorrectSecondaryHover()
    {
        var dark = DarkTheme.Create();
        var secondary500 = dark.Tokens.Colors["secondary-500"];

        Assert.NotNull(secondary500.Hover);
        Assert.NotNull(secondary500.Active);
        Assert.NotNull(secondary500.Foreground);
    }

    [Fact]
    public void DarkTheme_Create_Should_UseCorrectInfoHover()
    {
        var dark = DarkTheme.Create();
        var info500 = dark.Tokens.Colors["info-500"];

        Assert.NotNull(info500.Hover);
        Assert.NotNull(info500.Active);
        Assert.NotNull(info500.Foreground);
    }

    [Fact]
    public void DarkTheme_Create_Should_HaveDarkBorders()
    {
        var dark = DarkTheme.Create();
        Assert.Equal("#4B5563", dark.Tokens.Borders["default"].Color);
        Assert.Equal("#4B5563", dark.Tokens.Borders["card"].Color);
    }

    [Fact]
    public void DarkTheme_Breakpoints_Should_MatchDefaults()
    {
        var defaults = DefaultTokens.Create();
        var dark = DarkTheme.Create();

        foreach (var (key, breakpoint) in defaults.Breakpoints)
        {
            Assert.True(dark.Tokens.Breakpoints.ContainsKey(key));
            Assert.Equal(breakpoint.Value, dark.Tokens.Breakpoints[key].Value);
        }
    }

    [Fact]
    public void BothPresets_Should_HaveSameNumberOfTokenTypes()
    {
        var light = LightTheme.Create();
        var dark = DarkTheme.Create();

        Assert.Equal(light.Tokens.Colors.Count, dark.Tokens.Colors.Count);
        Assert.Equal(light.Tokens.Spacing.Count, dark.Tokens.Spacing.Count);
        Assert.Equal(light.Tokens.Typography.Count, dark.Tokens.Typography.Count);
        Assert.Equal(light.Tokens.Borders.Count, dark.Tokens.Borders.Count);
        Assert.Equal(light.Tokens.Shadows.Count, dark.Tokens.Shadows.Count);
        Assert.Equal(light.Tokens.Breakpoints.Count, dark.Tokens.Breakpoints.Count);
    }

    [Fact]
    public void BothPresets_Should_HaveNoGapsInColorScales()
    {
        var light = LightTheme.Create();
        var dark = DarkTheme.Create();
        var expectedShades = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var scales = new[] { "gray", "primary", "secondary", "danger", "success", "warning", "info", "purple", "teal" };

        foreach (var scale in scales)
        {
            foreach (var shade in expectedShades)
            {
                var key = $"{scale}-{shade}";
                Assert.True(light.Tokens.Colors.ContainsKey(key),
                    $"Light theme is missing color: {key}");
                Assert.True(dark.Tokens.Colors.ContainsKey(key),
                    $"Dark theme is missing color: {key}");
            }
        }
    }
}
