using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RoutePatternParserEdgeCaseTests
{
    [Fact]
    public void Parse_Should_ConvertAllSegments_When_MultipleDynamicSegments()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/category/[catId]/product/[prodId]/page.cs",
            "/project/app");

        Assert.Equal("/category/{catId}/product/{prodId}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_Should_DetectCorrectKind_When_MixedStaticAndDynamic()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/blog/2024/[slug]/page.cs",
            "/project/app");

        Assert.Equal("/blog/2024/{slug}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_Should_HandleStandardPage_When_FileHasCsExtension()
    {
        // This shouldn't normally happen since we only scan .cs files,
        // but the parser should handle it gracefully
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/about/page.cs",
            "/project/app");

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_HandleWindowsPaths_When_BackslashSeparators()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            @"C:\project\app\blog\[slug]\page.cs",
            @"C:\project\app");

        Assert.Equal("/blog/{slug}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_Should_ReturnRoot_When_FilePathDirectlyInAppDir()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/layout.cs",
            "/project/app");

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Theory]
    [InlineData("/project/app/about/page.cs", "/about", RouteSegmentKind.Static)]
    [InlineData("/project/app/blog/[slug]/page.cs", "/blog/{slug}", RouteSegmentKind.Dynamic)]
    [InlineData("/project/app/docs/[...path]/page.cs", "/docs/{*path}", RouteSegmentKind.CatchAll)]
    [InlineData("/project/app/docs/[[...path]]/page.cs", "/docs/{{*path}}", RouteSegmentKind.OptionalCatchAll)]
    [InlineData("/project/app/layout.cs", "/", RouteSegmentKind.Static)]
    [InlineData("/project/app/blog/layout.cs", "/blog", RouteSegmentKind.Static)]
    [InlineData("/project/app/api/users/route.cs", "/api/users", RouteSegmentKind.Static)]
    [InlineData("/project/app/error.cs", "/", RouteSegmentKind.Static)]
    public void Parse_Should_ReturnCorrectResults_When_ComprehensiveVariants(
        string filePath, string expectedPattern, RouteSegmentKind expectedKind)
    {
        var (pattern, kind) = RoutePatternParser.Parse(filePath, "/project/app");
        Assert.Equal(expectedPattern, pattern);
        Assert.Equal(expectedKind, kind);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ConvertOptionalCatchAll_When_InputHasDoubleBrackets()
    {
        var result = RoutePatternParser.ConvertBracketNotation("docs/[[...path]]");
        Assert.Equal("docs/{{*path}}", result);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ConvertCatchAll_When_InputHasTripleDot()
    {
        var result = RoutePatternParser.ConvertBracketNotation("docs/[...path]");
        Assert.Equal("docs/{*path}", result);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ConvertDynamic_When_InputHasSingleBracket()
    {
        var result = RoutePatternParser.ConvertBracketNotation("blog/[slug]");
        Assert.Equal("blog/{slug}", result);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ReturnUnchanged_When_Static()
    {
        var result = RoutePatternParser.ConvertBracketNotation("about");
        Assert.Equal("about", result);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ConvertAll_When_MixedNotations()
    {
        var result = RoutePatternParser.ConvertBracketNotation("blog/[year]/[month]/[slug]");
        Assert.Equal("blog/{year}/{month}/{slug}", result);
    }

    [Theory]
    [InlineData("/project/app/[[...path]]/page.cs", RouteSegmentKind.OptionalCatchAll)]
    [InlineData("/project/app/[...path]/page.cs", RouteSegmentKind.CatchAll)]
    [InlineData("/project/app/[slug]/page.cs", RouteSegmentKind.Dynamic)]
    [InlineData("/project/app/about/page.cs", RouteSegmentKind.Static)]
    public void Parse_Should_DetermineCorrectSegmentKind_When_VariousPatterns(
        string filePath, RouteSegmentKind expectedKind)
    {
        var (_, kind) = RoutePatternParser.Parse(filePath, "/project/app");
        Assert.Equal(expectedKind, kind);
    }
}
