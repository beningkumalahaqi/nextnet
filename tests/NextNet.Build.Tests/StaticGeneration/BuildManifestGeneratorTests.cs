using System.Text.Json;
using NextNet.Build.StaticGeneration;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class BuildManifestGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_Should_CreateManifestFile_When_Invoked()
    {
        using var tempDir = new TempDirectory();
        var manifestPath = System.IO.Path.Combine(tempDir.Path, "_buildManifest.json");
        var generator = new BuildManifestGenerator(manifestPath, new Version(1, 0, 0));

        var routeManifest = CreateSampleRouteManifest();
        var generatedRoutes = new List<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)>
        {
            ("/", "index.html", true, null, 1234),
            ("/about", "about/index.html", true, null, 890),
            ("/blog/{slug}", "blog/hello-world/index.html", false, new[] { "slug" }, 3400),
        };
        var assets = new List<string> { "images/logo.png", "css/styles.css" };

        await generator.GenerateAsync(routeManifest, generatedRoutes, assets, TimeSpan.FromMilliseconds(1250), 5);

        Assert.True(File.Exists(manifestPath));

        var json = await File.ReadAllTextAsync(manifestPath);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("generatedAt", out _));
        Assert.Equal("1.0.0", root.GetProperty("nextnetVersion").GetString());

        var routes = root.GetProperty("routes");
        Assert.Equal(3, routes.GetArrayLength());

        var firstRoute = routes[0];
        Assert.Equal("/", firstRoute.GetProperty("route").GetString());
        Assert.Equal("index.html", firstRoute.GetProperty("file").GetString());
        Assert.True(firstRoute.GetProperty("static").GetBoolean());

        var lastRoute = routes[2];
        Assert.Equal("/blog/{slug}", lastRoute.GetProperty("route").GetString());
        Assert.False(lastRoute.GetProperty("static").GetBoolean());

        var assetsArray = root.GetProperty("assets");
        Assert.Equal(2, assetsArray.GetArrayLength());

        var build = root.GetProperty("build");
        Assert.Equal(5, build.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task GenerateAsync_Should_OmitsParams_When_StaticRoute()
    {
        using var tempDir = new TempDirectory();
        var manifestPath = System.IO.Path.Combine(tempDir.Path, "manifest.json");
        var generator = new BuildManifestGenerator(manifestPath);

        var routeManifest = CreateSampleRouteManifest();
        var generatedRoutes = new List<(string route, string file, bool isStatic, IReadOnlyList<string>? paramNames, long size)>
        {
            ("/", "index.html", true, null, 100),
        };

        await generator.GenerateAsync(routeManifest, generatedRoutes, Array.Empty<string>(), TimeSpan.Zero, 1);

        var json = await File.ReadAllTextAsync(manifestPath);
        var doc = JsonDocument.Parse(json);
        var routes = doc.RootElement.GetProperty("routes");

        Assert.Equal(1, routes.GetArrayLength());
        Assert.False(routes[0].TryGetProperty("params", out _));
    }

    private static RouteManifest CreateSampleRouteManifest()
    {
        var pages = new List<RouteEntry>
        {
            new("/", "app/index/page.cs", RouteType.Page, RouteSegmentKind.Static),
            new("/about", "app/about/page.cs", RouteType.Page, RouteSegmentKind.Static),
            new("/blog", "app/blog/page.cs", RouteType.Page, RouteSegmentKind.Static),
            new("/blog/{slug}", "app/blog/[slug]/page.cs", RouteType.Page, RouteSegmentKind.Dynamic),
            new("/api/users", "app/api/users/route.cs", RouteType.Api, RouteSegmentKind.Static),
        };

        return new RouteManifest(
            pages,
            pages.Where(p => p.Type == RouteType.Page).ToList(),
            Array.Empty<RouteEntry>(),
            pages.Where(p => p.Type == RouteType.Api).ToList(),
            null,
            Array.Empty<RouteConflict>());
    }
}
