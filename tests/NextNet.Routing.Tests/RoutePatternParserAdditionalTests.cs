using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RoutePatternParserAdditionalTests
{
    [Fact]
    public void Parse_Should_ReturnRoot_When_FilePathEqualsAppDir()
    {
        // When filePath doesn't contain additional path beyond appDir
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/layout.cs",
            "/project/app");

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_HandleSingleCharSegments_When_ShortPath()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/a/b/[c]/page.cs",
            "/project/app");

        Assert.Equal("/a/b/{c}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_Should_ProduceRootSegment_When_FileInAppRoot()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/page.cs",
            "/project/app");

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_HandleRelativePathStyle_When_NoAppDirPrefix()
    {
        // Simulate a file path that doesn't contain the appDir prefix
        // This handles cases where the file path is already relative
        var (pattern, kind) = RoutePatternParser.Parse(
            "about/page.cs",
            "/project/app");

        // Since the file path doesn't start with appDir, it uses it as-is
        Assert.NotNull(pattern);
        Assert.StartsWith("/", pattern);
    }

    [Theory]
    [InlineData("page.cs")]
    [InlineData("layout.cs")]
    [InlineData("route.cs")]
    [InlineData("error.cs")]
    public void Parse_Should_StripAllKnownSuffixes_When_Present(string suffix)
    {
        var filePath = $"/project/app/test/{suffix}";
        var (pattern, _) = RoutePatternParser.Parse(filePath, "/project/app");
        Assert.Equal("/test", pattern);
    }

    [Fact]
    public void DetermineSegmentKind_Should_DetectCorrectKind_When_OriginalPathHasBrackets()
    {
        // Test the internal method directly
        var kind1 = RoutePatternParser.DetermineSegmentKind(
            "/docs/{*path}", "/project/app/docs/[...path]/page.cs");
        Assert.Equal(RouteSegmentKind.CatchAll, kind1);

        var kind2 = RoutePatternParser.DetermineSegmentKind(
            "/docs/{{*path}}", "/project/app/docs/[[...path]]/page.cs");
        Assert.Equal(RouteSegmentKind.OptionalCatchAll, kind2);

        var kind3 = RoutePatternParser.DetermineSegmentKind(
            "/about", "/project/app/about/page.cs");
        Assert.Equal(RouteSegmentKind.Static, kind3);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ConvertAllSegments_When_MultipleInSamePath()
    {
        var result = RoutePatternParser.ConvertBracketNotation(
            "[category]/[subcategory]/[slug]");
        Assert.Equal("{category}/{subcategory}/{slug}", result);
    }

    [Fact]
    public void ConvertBracketNotation_Should_ReturnUnchanged_When_NoBrackets()
    {
        var result = RoutePatternParser.ConvertBracketNotation("plain/static/path");
        Assert.Equal("plain/static/path", result);
    }
}
