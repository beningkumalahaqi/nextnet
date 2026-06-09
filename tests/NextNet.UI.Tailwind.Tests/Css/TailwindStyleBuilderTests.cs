using NextNet.UI.Tailwind.Css;
using Xunit;

namespace NextNet.UI.Tailwind.Tests.Css;

public class TailwindStyleBuilderTests
{
    private readonly TailwindStyleBuilder _builder;

    public TailwindStyleBuilderTests()
    {
        _builder = new TailwindStyleBuilder();
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeStyleTags()
    {
        var html = _builder.BuildStyleTag();

        Assert.StartsWith("<style>", html);
        Assert.EndsWith("</style>" + Environment.NewLine, html);
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeTailwindBaseByDefault()
    {
        var html = _builder.BuildStyleTag();

        Assert.Contains("@tailwind base;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeTailwindComponentsByDefault()
    {
        var html = _builder.BuildStyleTag();

        Assert.Contains("@tailwind components;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeTailwindUtilitiesByDefault()
    {
        var html = _builder.BuildStyleTag();

        Assert.Contains("@tailwind utilities;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_ExcludeBase_When_IncludeBaseIsFalse()
    {
        var html = _builder.BuildStyleTag(includeBase: false);

        Assert.DoesNotContain("@tailwind base;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_ExcludeComponents_When_IncludeComponentsIsFalse()
    {
        var html = _builder.BuildStyleTag(includeComponents: false);

        Assert.DoesNotContain("@tailwind components;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_ExcludeUtilities_When_IncludeUtilitiesIsFalse()
    {
        var html = _builder.BuildStyleTag(includeUtilities: false);

        Assert.DoesNotContain("@tailwind utilities;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeCustomCss_When_Provided()
    {
        var customCss = ".my-custom { color: red; }";
        var html = _builder.BuildStyleTag(customCss: customCss);

        Assert.Contains(customCss, html);
    }

    [Fact]
    public void BuildStyleTag_Should_NotIncludeCustomCss_When_Null()
    {
        var html = _builder.BuildStyleTag(customCss: null);

        Assert.DoesNotContain(".my-custom", html);
    }

    [Fact]
    public void BuildStyleTag_Should_NotIncludeCustomCss_When_Empty()
    {
        var html = _builder.BuildStyleTag(customCss: "");

        Assert.DoesNotContain(".my-custom", html);
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeAllThreeDirectives_When_AllTrue()
    {
        var html = _builder.BuildStyleTag(true, true, true);

        Assert.Contains("@tailwind base;", html);
        Assert.Contains("@tailwind components;", html);
        Assert.Contains("@tailwind utilities;", html);
    }

    [Fact]
    public void BuildStyleTag_Should_ProduceEmptyStyleTag_When_AllFalse()
    {
        var html = _builder.BuildStyleTag(false, false, false);

        Assert.DoesNotContain("@tailwind", html);
        Assert.StartsWith("<style>", html);
        Assert.EndsWith("</style>" + Environment.NewLine, html);
    }

    [Fact]
    public void BuildStyleTag_Should_IncludeCustomCssAfterDirectives()
    {
        var customCss = "/* Custom styles */";
        var html = _builder.BuildStyleTag(customCss: customCss);

        var utilitiesIndex = html.IndexOf("@tailwind utilities;");
        var customCssIndex = html.IndexOf(customCss);

        Assert.True(customCssIndex > utilitiesIndex,
            "Custom CSS should appear after Tailwind directives");
    }
}
