using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;
using NextNet.UI.Tailwind.Config;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Config;

public class TailwindConfigGeneratorTests
{
    [Fact]
    public void Generate_Should_ProduceConfig_When_GivenDefaultTokens()
    {
        var tokens = DefaultTokens.Create();
        var config = TailwindConfigGenerator.Generate(tokens);

        Assert.NotNull(config);
        Assert.NotEmpty(config.Colors);
        Assert.NotEmpty(config.Spacing);
    }

    [Fact]
    public void Generate_Should_UseDefaultTokens_When_TokenSetIsNull()
    {
        var config = TailwindConfigGenerator.Generate(null);

        Assert.NotNull(config);
        Assert.NotEmpty(config.Colors);
        Assert.NotEmpty(config.Spacing);
    }

    [Fact]
    public void Generate_Should_IncludeColorScale()
    {
        var tokens = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6"),
                ["primary-600"] = new ColorToken("primary-600", "#2563EB"),
                ["gray-100"] = new ColorToken("gray-100", "#F3F4F6")
            });

        var config = TailwindConfigGenerator.Generate(tokens);

        Assert.Contains("primary", config.Colors.Keys);
        Assert.Contains("gray", config.Colors.Keys);
    }

    [Fact]
    public void Generate_Should_IncludeSpacingScale()
    {
        var tokens = new DesignTokenSet(
            spacing: new Dictionary<string, SpacingToken>
            {
                ["spacing-4"] = new SpacingToken("spacing-4", "1rem"),
                ["spacing-8"] = new SpacingToken("spacing-8", "2rem")
            });

        var config = TailwindConfigGenerator.Generate(tokens);

        Assert.Contains("4", config.Spacing.Keys);
        Assert.Contains("8", config.Spacing.Keys);
    }

    [Fact]
    public void Generate_Should_IncludeTypography()
    {
        var tokens = new DesignTokenSet(
            typography: new Dictionary<string, TypographyToken>
            {
                ["body-base"] = new TypographyToken(
                    "body-base", "Inter, sans-serif", "1rem", "400", "1.5", "normal")
            });

        var config = TailwindConfigGenerator.Generate(tokens);

        Assert.NotEmpty(config.FontFamilies);
        Assert.NotEmpty(config.FontSizes);
        Assert.NotEmpty(config.FontWeights);
    }

    [Fact]
    public void Generate_Should_RespectContentPaths()
    {
        var contentPaths = new[] { "./Pages/**/*.cshtml" };
        var config = TailwindConfigGenerator.Generate(
            DefaultTokens.Create(),
            contentPaths: contentPaths);

        Assert.Equal(contentPaths, config.ContentPaths);
    }

    [Fact]
    public void Generate_Should_RespectSafelistPatterns()
    {
        var safelist = new[] { "btn-*", "badge-*" };
        var config = TailwindConfigGenerator.Generate(
            DefaultTokens.Create(),
            safelistPatterns: safelist);

        Assert.Equal(safelist, config.SafelistPatterns);
    }

    [Fact]
    public void Generate_Should_ProduceValidJsModuleString()
    {
        var tokens = DefaultTokens.Create();
        var config = TailwindConfigGenerator.Generate(tokens);

        var jsModule = config.ToJsModuleString();

        Assert.StartsWith("export default {", jsModule);
        Assert.Contains("content:", jsModule);
        Assert.Contains("theme:", jsModule);
        Assert.Contains("extend:", jsModule);
        Assert.EndsWith("};" + Environment.NewLine, jsModule);
    }

    [Fact]
    public void ToJsModuleString_Should_IncludeColors()
    {
        var tokens = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });

        var config = TailwindConfigGenerator.Generate(tokens);
        var jsModule = config.ToJsModuleString();

        Assert.Contains("primary", jsModule);
        Assert.Contains("#3B82F6", jsModule);
    }

    [Fact]
    public void ToJsModuleString_Should_IncludeSpacing()
    {
        var tokens = new DesignTokenSet(
            spacing: new Dictionary<string, SpacingToken>
            {
                ["spacing-4"] = new SpacingToken("spacing-4", "1rem")
            });

        var config = TailwindConfigGenerator.Generate(tokens);
        var jsModule = config.ToJsModuleString();

        Assert.Contains("spacing", jsModule);
        Assert.Contains("1rem", jsModule);
    }

    [Fact]
    public void ToJsModuleString_Should_IncludeSafelist_When_NotEmpty()
    {
        var config = TailwindConfigGenerator.Generate(
            DefaultTokens.Create(),
            safelistPatterns: new[] { "btn-*" });

        var jsModule = config.ToJsModuleString();

        Assert.Contains("safelist:", jsModule);
        Assert.Contains("btn-*", jsModule);
    }

    [Fact]
    public void ToJsModuleString_Should_NotIncludeSafelist_When_Empty()
    {
        var config = TailwindConfigGenerator.Generate(DefaultTokens.Create());
        var jsModule = config.ToJsModuleString();

        Assert.DoesNotContain("safelist:", jsModule);
    }

    [Fact]
    public void Generate_Should_HandleEmptyTokenSet()
    {
        var tokens = new DesignTokenSet();
        var config = TailwindConfigGenerator.Generate(tokens);

        Assert.NotNull(config);
        Assert.Empty(config.Colors);
        Assert.Empty(config.Spacing);
    }
}
