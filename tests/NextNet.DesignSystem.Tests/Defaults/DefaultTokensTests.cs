using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;
using Xunit;

namespace NextNet.DesignSystem.Tests.Defaults;

public class DefaultTokensTests
{
    private readonly DesignTokenSet _defaults;

    public DefaultTokensTests()
    {
        _defaults = DefaultTokens.Create();
    }

    [Fact]
    public void Create_Should_ReturnNonNullSet()
    {
        Assert.NotNull(_defaults);
    }

    [Fact]
    public void Create_Should_PopulateAllCategories()
    {
        Assert.NotEmpty(_defaults.Colors);
        Assert.NotEmpty(_defaults.Spacing);
        Assert.NotEmpty(_defaults.Typography);
        Assert.NotEmpty(_defaults.Borders);
        Assert.NotEmpty(_defaults.Shadows);
        Assert.NotEmpty(_defaults.Breakpoints);
    }

    [Fact]
    public void Colors_Should_HaveAllNamedScales()
    {
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("gray-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("primary-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("secondary-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("danger-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("success-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("warning-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("info-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("purple-"));
        Assert.Contains(_defaults.Colors, kvp => kvp.Key.StartsWith("teal-"));
    }

    [Fact]
    public void Colors_Should_HaveElevenShadesPerScale()
    {
        Assert.Equal(11, _defaults.Colors.Count(kvp => kvp.Key.StartsWith("gray-")));
        Assert.Equal(11, _defaults.Colors.Count(kvp => kvp.Key.StartsWith("primary-")));
        Assert.Equal(11, _defaults.Colors.Count(kvp => kvp.Key.StartsWith("danger-")));
    }

    [Fact]
    public void ColorValues_Should_BeNonNullOrEmpty()
    {
        foreach (var (key, token) in _defaults.Colors)
        {
            Assert.False(string.IsNullOrWhiteSpace(token.Name), $"Color {key} has null/empty Name");
            Assert.False(string.IsNullOrWhiteSpace(token.Value), $"Color {key} has null/empty Value");
        }
    }

    [Fact]
    public void SemanticColors_Should_HaveInteractiveStates()
    {
        Assert.NotNull(_defaults.Colors["primary-500"].Hover);
        Assert.NotNull(_defaults.Colors["primary-500"].Active);
        Assert.NotNull(_defaults.Colors["primary-500"].Foreground);
        Assert.NotNull(_defaults.Colors["secondary-500"].Hover);
        Assert.NotNull(_defaults.Colors["secondary-500"].Active);
        Assert.NotNull(_defaults.Colors["secondary-500"].Foreground);
        Assert.NotNull(_defaults.Colors["danger-500"].Hover);
        Assert.NotNull(_defaults.Colors["danger-500"].Active);
        Assert.NotNull(_defaults.Colors["danger-500"].Foreground);
        Assert.NotNull(_defaults.Colors["success-500"].Hover);
        Assert.NotNull(_defaults.Colors["success-500"].Active);
        Assert.NotNull(_defaults.Colors["success-500"].Foreground);
        Assert.NotNull(_defaults.Colors["warning-500"].Hover);
        Assert.NotNull(_defaults.Colors["warning-500"].Active);
        Assert.NotNull(_defaults.Colors["warning-500"].Foreground);
        Assert.NotNull(_defaults.Colors["info-500"].Hover);
        Assert.NotNull(_defaults.Colors["info-500"].Active);
        Assert.NotNull(_defaults.Colors["info-500"].Foreground);
    }

    [Fact]
    public void Spacing_Should_HaveCorrectCount()
    {
        Assert.Equal(35, _defaults.Spacing.Count);
    }

    [Fact]
    public void SpacingValues_Should_BeNonNullOrEmpty()
    {
        foreach (var (key, token) in _defaults.Spacing)
        {
            Assert.False(string.IsNullOrWhiteSpace(token.Name), $"Spacing {key} has null/empty Name");
            Assert.False(string.IsNullOrWhiteSpace(token.Value), $"Spacing {key} has null/empty Value");
        }
    }

    [Fact]
    public void Typography_Should_HaveKeyStyles()
    {
        Assert.Contains(_defaults.Typography, kvp => kvp.Key.StartsWith("heading-"));
        Assert.Contains(_defaults.Typography, kvp => kvp.Key.StartsWith("body-"));
        Assert.Contains("label", _defaults.Typography.Keys);
        Assert.Contains("caption", _defaults.Typography.Keys);
        Assert.Contains("overline", _defaults.Typography.Keys);
    }

    [Fact]
    public void TypographyProperties_Should_BeNonNullOrEmpty()
    {
        foreach (var (key, token) in _defaults.Typography)
        {
            Assert.False(string.IsNullOrWhiteSpace(token.Name), $"Typography {key} has null/empty Name");
            Assert.False(string.IsNullOrWhiteSpace(token.FontFamily), $"Typography {key} has null/empty FontFamily");
            Assert.False(string.IsNullOrWhiteSpace(token.FontSize), $"Typography {key} has null/empty FontSize");
            Assert.False(string.IsNullOrWhiteSpace(token.FontWeight), $"Typography {key} has null/empty FontWeight");
            Assert.False(string.IsNullOrWhiteSpace(token.LineHeight), $"Typography {key} has null/empty LineHeight");
            Assert.NotNull(token.LetterSpacing); // can be "normal"
        }
    }

    [Fact]
    public void Borders_Should_HaveKeyTokens()
    {
        Assert.Contains("default", _defaults.Borders.Keys);
        Assert.Contains("card", _defaults.Borders.Keys);
        Assert.Contains("input", _defaults.Borders.Keys);
        Assert.Contains("badge", _defaults.Borders.Keys);
        Assert.Contains("none", _defaults.Borders.Keys);
    }

    [Fact]
    public void BorderProperties_Should_BeNonNullOrEmpty()
    {
        foreach (var (key, token) in _defaults.Borders)
        {
            Assert.False(string.IsNullOrWhiteSpace(token.Name), $"Border {key} has null/empty Name");
            Assert.NotNull(token.Width);
            Assert.NotNull(token.Style);
            Assert.NotNull(token.Color);
            Assert.NotNull(token.Radius);
        }
    }

    [Fact]
    public void Shadows_Should_HaveAllElevations()
    {
        Assert.Contains("shadow-sm", _defaults.Shadows.Keys);
        Assert.Contains("shadow-md", _defaults.Shadows.Keys);
        Assert.Contains("shadow-lg", _defaults.Shadows.Keys);
        Assert.Contains("shadow-xl", _defaults.Shadows.Keys);
        Assert.Contains("shadow-2xl", _defaults.Shadows.Keys);
        Assert.Contains("shadow-inner", _defaults.Shadows.Keys);
    }

    [Fact]
    public void ShadowValues_Should_BeNonNullOrEmpty()
    {
        foreach (var (key, token) in _defaults.Shadows)
        {
            Assert.False(string.IsNullOrWhiteSpace(token.Name), $"Shadow {key} has null/empty Name");
            Assert.False(string.IsNullOrWhiteSpace(token.Value), $"Shadow {key} has null/empty Value");
        }
    }

    [Fact]
    public void Breakpoints_Should_HaveStandardValues()
    {
        Assert.Equal("640px", _defaults.Breakpoints["sm"].Value);
        Assert.Equal("768px", _defaults.Breakpoints["md"].Value);
        Assert.Equal("1024px", _defaults.Breakpoints["lg"].Value);
        Assert.Equal("1280px", _defaults.Breakpoints["xl"].Value);
        Assert.Equal("1536px", _defaults.Breakpoints["2xl"].Value);
    }

    [Fact]
    public void BreakpointValues_Should_BeNonNullOrEmpty()
    {
        foreach (var (key, token) in _defaults.Breakpoints)
        {
            Assert.False(string.IsNullOrWhiteSpace(token.Name), $"Breakpoint {key} has null/empty Name");
            Assert.False(string.IsNullOrWhiteSpace(token.Value), $"Breakpoint {key} has null/empty Value");
        }
    }

    [Fact]
    public void Create_Should_ReturnNewInstance_OnEachCall()
    {
        var set1 = DefaultTokens.Create();
        var set2 = DefaultTokens.Create();
        Assert.NotSame(set1, set2);
        Assert.Equal(set1, set2);
    }
}
