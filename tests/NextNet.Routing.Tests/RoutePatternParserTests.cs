using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RoutePatternParserTests
{
    private const string AppDir = "/project/app";

    [Fact]
    public void Parse_StaticPage_ReturnsStaticRoute()
    {
        var filePath = "/project/app/about/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_IndexPage_ReturnsRootRoute()
    {
        var filePath = "/project/app/index/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/index", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_DynamicPage_ReturnsDynamicRoute()
    {
        var filePath = "/project/app/blog/[slug]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog/{slug}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_CatchAllPage_ReturnsCatchAllRoute()
    {
        var filePath = "/project/app/docs/[...path]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/docs/{*path}", pattern);
        Assert.Equal(RouteSegmentKind.CatchAll, kind);
    }

    [Fact]
    public void Parse_OptionalCatchAll_ReturnsOptionalCatchAllRoute()
    {
        var filePath = "/project/app/docs/[[...path]]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/docs/{{*path}}", pattern);
        Assert.Equal(RouteSegmentKind.OptionalCatchAll, kind);
    }

    [Fact]
    public void Parse_RootLayout_ReturnsRootPattern()
    {
        var filePath = "/project/app/layout.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_NestedLayout_ReturnsLayoutPattern()
    {
        var filePath = "/project/app/blog/layout.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_ApiRoute_ReturnsApiPattern()
    {
        var filePath = "/project/app/api/users/route.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/api/users", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_ErrorPage_ReturnsErrorPattern()
    {
        var filePath = "/project/app/error.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_NestedErrorPage_ReturnsErrorAtCorrectPath()
    {
        var filePath = "/project/app/blog/error.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_NestedDynamicWithMultipleSegments_ReturnsCorrectPattern()
    {
        var filePath = "/project/app/blog/[year]/[month]/[slug]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog/{year}/{month}/{slug}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_DeepNestedLayout_ReturnsCorrectPattern()
    {
        var filePath = "/project/app/dashboard/settings/layout.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/dashboard/settings", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_FilePathWithWindowsSeparators_NormalizesCorrectly()
    {
        var appDir = @"C:\project\app";
        var filePath = @"C:\project\app\about\page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, appDir);

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_RelativeFilePath_HandlesCorrectly()
    {
        // If filePath is already relative to appDir
        var filePath = "/project/app/about/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_NullFilePath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RoutePatternParser.Parse(null!, AppDir));
    }

    [Fact]
    public void Parse_NullAppDir_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RoutePatternParser.Parse("/project/app/about/page.cs", null!));
    }

    [Theory]
    [InlineData("/project/app/products/[id]/page.cs", "/products/{id}", RouteSegmentKind.Dynamic)]
    [InlineData("/project/app/archive/[...date]/page.cs", "/archive/{*date}", RouteSegmentKind.CatchAll)]
    [InlineData("/project/app/settings/layout.cs", "/settings", RouteSegmentKind.Static)]
    [InlineData("/project/app/api/v2/users/route.cs", "/api/v2/users", RouteSegmentKind.Static)]
    public void Parse_VariousPatterns_ReturnsExpectedResults(
        string filePath, string expectedPattern, RouteSegmentKind expectedKind)
    {
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);
        Assert.Equal(expectedPattern, pattern);
        Assert.Equal(expectedKind, kind);
    }
}
