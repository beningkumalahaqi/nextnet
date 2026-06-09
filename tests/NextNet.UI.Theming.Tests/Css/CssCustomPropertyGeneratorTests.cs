using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;
using NextNet.UI.Theming.Css;
using Xunit;

namespace NextNet.UI.Theming.Tests.Css;

public class CssCustomPropertyGeneratorTests
{
    [Fact]
    public void Generate_Should_ThrowArgumentNullException_When_TokensIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => CssCustomPropertyGenerator.Generate(null!));
    }

    [Fact]
    public void Generate_Should_ReturnNonEmptyString_ForDefaultTokens()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void Generate_Should_IncludeColorProperties()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--color-gray-50", result);
        Assert.Contains("--color-primary-500", result);
        Assert.Contains("--color-danger-500", result);
    }

    [Fact]
    public void Generate_Should_IncludeColorInteractiveStates()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        // primary-500 has Hover, Active, Foreground
        Assert.Contains("--color-primary-500-hover", result);
        Assert.Contains("--color-primary-500-active", result);
        Assert.Contains("--color-primary-500-foreground", result);
    }

    [Fact]
    public void Generate_Should_IncludeSpacingProperties()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--spacing-0", result);
        Assert.Contains("--spacing-4", result);
        Assert.Contains("--spacing-96", result);
    }

    [Fact]
    public void Generate_Should_IncludeTypographyProperties()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--typography-heading-xl-font-family", result);
        Assert.Contains("--typography-heading-xl-font-size", result);
        Assert.Contains("--typography-heading-xl-font-weight", result);
        Assert.Contains("--typography-body-base-line-height", result);
        Assert.Contains("--typography-body-base-letter-spacing", result);
    }

    [Fact]
    public void Generate_Should_IncludeBorderProperties()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--border-default-width", result);
        Assert.Contains("--border-default-style", result);
        Assert.Contains("--border-default-color", result);
        Assert.Contains("--border-default-radius", result);
        Assert.Contains("--border-card-radius", result);
    }

    [Fact]
    public void Generate_Should_IncludeShadowProperties()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--shadow-sm", result);
        Assert.Contains("--shadow-md", result);
        Assert.Contains("--shadow-lg", result);
        Assert.Contains("--shadow-xl", result);
    }

    [Fact]
    public void Generate_Should_IncludeBreakpointProperties()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--breakpoint-sm", result);
        Assert.Contains("--breakpoint-md", result);
        Assert.Contains("--breakpoint-lg", result);
        Assert.Contains("--breakpoint-xl", result);
        Assert.Contains("--breakpoint-2xl", result);
    }

    [Fact]
    public void Generate_Should_UseCorrectColorValues()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--color-primary-500: #3B82F6", result);
        Assert.Contains("--color-primary-500-hover: #2563EB", result);
    }

    [Fact]
    public void Generate_Should_UseCorrectSpacingValues()
    {
        var tokens = DefaultTokens.Create();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--spacing-4: 1rem", result);
        Assert.Contains("--spacing-0: 0px", result);
    }

    [Fact]
    public void Generate_Should_ReturnEmptyString_ForEmptyTokenSet()
    {
        var tokens = new DesignTokenSet();
        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Generate_Should_FormatEachPropertyOnSeparateLine()
    {
        var tokens = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["test"] = new ColorToken("test", "#FF0000")
                {
                    Hover = "#00FF00",
                    Foreground = "#FFFFFF"
                }
            });

        var result = CssCustomPropertyGenerator.Generate(tokens);
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.Contains(lines, l => l.Contains("--color-test: #FF0000"));
        Assert.Contains(lines, l => l.Contains("--color-test-hover: #00FF00"));
        Assert.Contains(lines, l => l.Contains("--color-test-foreground: #FFFFFF"));
    }

    [Fact]
    public void Generate_Should_NotIncludeColorWithoutInteractiveStates_WhenNotSet()
    {
        var tokens = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["simple"] = new ColorToken("simple", "#CCCCCC")
            });

        var result = CssCustomPropertyGenerator.Generate(tokens);

        Assert.Contains("--color-simple: #CCCCCC", result);
        Assert.DoesNotContain("--color-simple-hover", result);
        Assert.DoesNotContain("--color-simple-active", result);
        Assert.DoesNotContain("--color-simple-foreground", result);
    }
}
