using NextNet.Isr.Manifest;
using NextNet.Routing;
using NextNet.Routing.Models;

namespace NextNet.Isr.Tests;

public class IsrManifestGeneratorTests
{
    private readonly IsrGlobalOptions _globalOptions;
    private readonly RouteManifest _emptyRouteManifest;

    public IsrManifestGeneratorTests()
    {
        _globalOptions = new IsrGlobalOptions { DefaultRevalidateSeconds = 60 };
        // Create an empty manifest to avoid dependency on actual scanned routes
        _emptyRouteManifest = new RouteManifest(
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());
    }

    [Fact]
    public void Generate_Should_ReturnEmptyManifest_When_NoRoutes()
    {
        var generator = new IsrManifestGenerator(_emptyRouteManifest, _globalOptions);
        var manifest = generator.Generate();

        Assert.Empty(manifest.Routes);
        Assert.False(manifest.HasIsrRoutes);
    }

    [Fact]
    public void Generate_Should_AddRouteToManifest_When_PageExists()
    {
        var pages = new List<RouteEntry>
        {
            new("/about", "app/Pages/About.cs", RouteType.Page, RouteSegmentKind.Static)
        };

        var routeManifest = new RouteManifest(
            pages, pages, Array.Empty<RouteEntry>(), Array.Empty<RouteEntry>(), null, Array.Empty<RouteConflict>());

        var generator = new IsrManifestGenerator(routeManifest, _globalOptions);
        var manifest = generator.Generate();

        Assert.Single(manifest.Routes);
        Assert.True(manifest.HasIsrRoutes);
    }

    [Fact]
    public void CreateMetadata_Should_UseGlobalDefaults_When_DefaultRoute()
    {
        var entry = new RouteEntry("/test", "app/Pages/Test.cs", RouteType.Page, RouteSegmentKind.Static);

        var generator = new IsrManifestGenerator(_emptyRouteManifest, _globalOptions);
        var metadata = generator.CreateMetadata(entry);

        Assert.Equal("/test", metadata.RoutePattern);
        Assert.Equal(60, metadata.RevalidateSeconds);
        Assert.Equal(1, metadata.MaxConcurrentRegenerations);
        Assert.True(metadata.ServeStaleWhileRevalidate);
        Assert.Equal("app/Pages/Test.cs", metadata.FilePath);
    }

    [Fact]
    public void ConvertFilePathToTypeName_Should_ConvertCorrectly_When_ForwardSlashPath()
    {
        var result = IsrManifestGenerator.ConvertFilePathToTypeName("app/Pages/Blog/Index.cs");
        Assert.Equal("App.Pages.Blog.Index", result);
    }

    [Fact]
    public void ConvertFilePathToTypeName_Should_ConvertCorrectly_When_BackslashPath()
    {
        var result = IsrManifestGenerator.ConvertFilePathToTypeName("app\\Pages\\About.cshtml");
        Assert.Equal("App.Pages.About", result);
    }

    [Fact]
    public void ConvertFilePathToTypeName_Should_TrimBasedOnLastDot_When_NoExtension()
    {
        var result = IsrManifestGenerator.ConvertFilePathToTypeName("Home.Index");
        // The method removes the last extension part
        Assert.Equal("Home", result);
    }

    [Fact]
    public void ConvertFilePathToTypeName_Should_CapitalizeSegments_When_MixedCase()
    {
        var result = IsrManifestGenerator.ConvertFilePathToTypeName("app/pages/blog/post.cs");
        Assert.Equal("App.Pages.Blog.Post", result);
    }
}
