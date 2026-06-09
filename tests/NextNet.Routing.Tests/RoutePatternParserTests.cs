using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RoutePatternParserTests
{
    private const string AppDir = "/project/app";

    [Fact]
    public void Parse_Should_ReturnStaticRoute_When_StaticPagePath()
    {
        var filePath = "/project/app/about/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnIndexRoute_When_IndexPagePath()
    {
        var filePath = "/project/app/index/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/index", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnDynamicRoute_When_DynamicSegmentInPath()
    {
        var filePath = "/project/app/blog/[slug]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog/{slug}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_Should_ReturnCatchAllRoute_When_CatchAllSegmentInPath()
    {
        var filePath = "/project/app/docs/[...path]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/docs/{*path}", pattern);
        Assert.Equal(RouteSegmentKind.CatchAll, kind);
    }

    [Fact]
    public void Parse_Should_ReturnOptionalCatchAllRoute_When_OptionalCatchAllSegment()
    {
        var filePath = "/project/app/docs/[[...path]]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/docs/{{*path}}", pattern);
        Assert.Equal(RouteSegmentKind.OptionalCatchAll, kind);
    }

    [Fact]
    public void Parse_Should_ReturnRootPattern_When_RootLayout()
    {
        var filePath = "/project/app/layout.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnLayoutPattern_When_NestedLayout()
    {
        var filePath = "/project/app/blog/layout.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnApiPattern_When_ApiRoutePath()
    {
        var filePath = "/project/app/api/users/route.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/api/users", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnRootPattern_When_ErrorPage()
    {
        var filePath = "/project/app/error.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnCorrectPath_When_NestedErrorPage()
    {
        var filePath = "/project/app/blog/error.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ReturnCorrectPattern_When_MultipleDynamicSegments()
    {
        var filePath = "/project/app/blog/[year]/[month]/[slug]/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/blog/{year}/{month}/{slug}", pattern);
        Assert.Equal(RouteSegmentKind.Dynamic, kind);
    }

    [Fact]
    public void Parse_Should_ReturnCorrectPattern_When_DeepNestedLayout()
    {
        var filePath = "/project/app/dashboard/settings/layout.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/dashboard/settings", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_NormalizeWindowsSeparators_When_WindowsPath()
    {
        var appDir = @"C:\project\app";
        var filePath = @"C:\project\app\about\page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, appDir);

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_HandleRelativeFilePath_When_AlreadyRelative()
    {
        // If filePath is already relative to appDir
        var filePath = "/project/app/about/page.cs";
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);

        Assert.Equal("/about", pattern);
        Assert.Equal(RouteSegmentKind.Static, kind);
    }

    [Fact]
    public void Parse_Should_ThrowArgumentNull_When_FilePathIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RoutePatternParser.Parse(null!, AppDir));
    }

    [Fact]
    public void Parse_Should_ThrowArgumentNull_When_AppDirIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            RoutePatternParser.Parse("/project/app/about/page.cs", null!));
    }

    [Theory]
    [InlineData("/project/app/products/[id]/page.cs", "/products/{id}", RouteSegmentKind.Dynamic)]
    [InlineData("/project/app/archive/[...date]/page.cs", "/archive/{*date}", RouteSegmentKind.CatchAll)]
    [InlineData("/project/app/settings/layout.cs", "/settings", RouteSegmentKind.Static)]
    [InlineData("/project/app/api/v2/users/route.cs", "/api/v2/users", RouteSegmentKind.Static)]
    public void Parse_Should_ReturnExpectedResults_When_VariousPatternsProvided(
        string filePath, string expectedPattern, RouteSegmentKind expectedKind)
    {
        var (pattern, kind) = RoutePatternParser.Parse(filePath, AppDir);
        Assert.Equal(expectedPattern, pattern);
        Assert.Equal(expectedKind, kind);
    }
}
