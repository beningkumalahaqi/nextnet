using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RoutePatternParserAdditionalTests
{
    [Fact]
    public void Parse_FilePathExactlyEqualsAppDir_ReturnsRoot()
    {
        // When filePath doesn't contain additional path beyond appDir
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/layout.cs",
            "/project/app");

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_EdgeCaseSingleCharSegments()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/a/b/[c]/page.cs",
            "/project/app");

        Assert.Equal("/a/b/{c}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_FileInAppRoot_ProducesSingleSegment()
    {
        var (pattern, kind) = RoutePatternParser.Parse(
            "/project/app/page.cs",
            "/project/app");

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_RelativePathStyle_Handled()
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
    public void KnownSuffixes_AllStripped(string suffix)
    {
        var filePath = $"/project/app/test/{suffix}";
        var (pattern, _) = RoutePatternParser.Parse(filePath, "/project/app");
        Assert.Equal("/test", pattern);
    }

    [Fact]
    public void DetermineSegmentKind_OriginalPathWithBrackets_DetectsCorrectly()
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
    public void ConvertBracketNotation_MultipleInSamePath_AllConverted()
    {
        var result = RoutePatternParser.ConvertBracketNotation(
            "[category]/[subcategory]/[slug]");
        Assert.Equal("{category}/{subcategory}/{slug}", result);
    }

    [Fact]
    public void ConvertBracketNotation_NoBrackets_Unchanged()
    {
        var result = RoutePatternParser.ConvertBracketNotation("plain/static/path");
        Assert.Equal("plain/static/path", result);
    }
}
