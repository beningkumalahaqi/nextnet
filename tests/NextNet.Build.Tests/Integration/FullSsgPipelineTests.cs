using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.StaticGeneration;
using NextNet.Components;
using NextNet.Conventions;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Routing.Models;
using NextNet.Build.Tests.StaticGeneration;
using Xunit;

namespace NextNet.Build.Tests.Integration;

/// <summary>
/// Full SSG pipeline integration tests that exercise the end-to-end flow:
/// route discovery → param resolution → SSR rendering → minification →
/// gzip → output → asset copy → manifest generation.
/// </summary>
public class FullSsgPipelineTests
{
    /// <summary>
    /// A simple test page with no dependencies.
    /// </summary>
    private sealed class SamplePage : IPage
    {
        private readonly string _content;

        public SamplePage(string content = "<h1>Hello</h1>")
        {
            _content = content;
        }

        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render()
            => Task.FromResult(HtmlHelper.Raw(_content));
    }

    /// <summary>
    /// A page with static path parameters for dynamic routes.
    /// </summary>
    private sealed class BlogPage : IPage, IStaticPathProvider
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render()
            => Task.FromResult(HtmlHelper.Raw("<h1>Blog Post</h1>"));

        public async Task<IReadOnlyList<Dictionary<string, string>>> GetStaticPathsAsync()
        {
            await Task.CompletedTask;
            return new List<Dictionary<string, string>>
            {
                new() { ["slug"] = "hello-world" },
                new() { ["slug"] = "getting-started" },
            };
        }
    }

    [Fact]
    public async Task FullBuild_WithStaticRoutes_GeneratesCorrectOutput()
    {
        using var tempDir = new TempDirectory();
        var appDir = System.IO.Path.Combine(tempDir.Path, NextNetConventions.AppDirectory);
        var publicDir = System.IO.Path.Combine(tempDir.Path, NextNetConventions.PublicDirectory);
        var outputDir = System.IO.Path.Combine(tempDir.Path, NextNetConventions.OutputDirectory);

        // Create app directory structure
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(publicDir);

        // Create public assets
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "styles.css"), "body { color: red; }");
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "app.js"), "console.log('hello');");

        // We need to construct a RouteManifest and SsrRenderer manually since
        // the files don't actually exist on disk (the RouteScanner would need real files).
        // Instead, we manually create the manifest and test the engine directly.

        var manifest = new RouteManifest(
            new List<RouteEntry>
            {
                new("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static),
                new("/about", "app/about/page.cs", RouteType.Page, RouteSegmentKind.Static),
            },
            new List<RouteEntry>
            {
                new("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static),
                new("/about", "app/about/page.cs", RouteType.Page, RouteSegmentKind.Static),
            },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        // Set up DI with page registrations (concrete types must be registered for SSR)
        var services = new ServiceCollection();
        services.AddScoped<SamplePage>(_ => new SamplePage("<h1>Home</h1>"));
        services.AddScoped<IPage>(sp => sp.GetRequiredService<SamplePage>());
        var sp = services.BuildServiceProvider();

        // Create the SSR renderer with a test component resolver
        var ssrRenderer = CreateTestRenderer(sp, manifest);

        // Create engine components
        var options = new SsgOptions
        {
            OutputDirectory = outputDir,
            MinifyHtml = true,
            CompressGzip = true,
            GenerateBuildManifest = true,
            CopyAssets = true,
            CleanOutput = true,
        };

        var outputWriter = new OutputWriter(outputDir);
        var assetCopier = new PublicAssetCopier(publicDir, outputDir);
        var paramsResolver = new StaticParamsResolver(
            ssrRenderer.ComponentResolver, sp);
        var manifestPath = System.IO.Path.Combine(outputDir, "_buildManifest.json");
        var manifestGenerator = new BuildManifestGenerator(manifestPath);

        var engine = new StaticGenerationEngine(
            options,
            ssrRenderer,
            manifest,
            paramsResolver,
            outputWriter,
            assetCopier,
            manifestGenerator,
            appDir: appDir,
            publicDir: publicDir);

        // Act
        var result = await engine.GenerateAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.GeneratedFiles);

        // Check that HTML files were generated
        Assert.Contains("index.html", result.GeneratedFiles);
        Assert.Contains("about/index.html", result.GeneratedFiles);

        // Check that .gz files exist
        Assert.Contains("index.html.gz", result.GeneratedFiles);

        // Check that assets were copied
        Assert.True(result.AssetCount > 0);

        // Verify file contents
        var homeHtml = await File.ReadAllTextAsync(System.IO.Path.Combine(outputDir, "index.html"));
        Assert.Contains("<h1>Home</h1>", homeHtml);

        // Verify manifest exists with correct structure
        Assert.True(File.Exists(manifestPath));
        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        var doc = JsonDocument.Parse(manifestJson);
        Assert.True(doc.RootElement.TryGetProperty("routes", out _));
        Assert.True(doc.RootElement.TryGetProperty("build", out _));
    }

    [Fact]
    public async Task FullBuild_WithDynamicRoutes_ResolvesParams()
    {
        using var tempDir = new TempDirectory();
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");

        // Create manifest with a dynamic route
        var manifest = new RouteManifest(
            new List<RouteEntry>
            {
                new("/blog/{slug}",
                    "app/blog/[slug]/page.cs",
                    RouteType.Page,
                    RouteSegmentKind.Dynamic),
            },
            new List<RouteEntry>
            {
                new("/blog/{slug}",
                    "app/blog/[slug]/page.cs",
                    RouteType.Page,
                    RouteSegmentKind.Dynamic),
            },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        // Register both IPage and IStaticPathProvider (concrete types must be registered for SSR)
        var services = new ServiceCollection();
        services.AddScoped<BlogPage>(_ => new BlogPage());
        services.AddScoped<IPage>(sp => sp.GetRequiredService<BlogPage>());
        services.AddScoped<IStaticPathProvider>(sp => sp.GetRequiredService<BlogPage>());
        var sp = services.BuildServiceProvider();

        var ssrRenderer = CreateTestRenderer(sp, manifest);

        var options = new SsgOptions
        {
            OutputDirectory = outputDir,
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = true,
            CopyAssets = false,
            CleanOutput = true,
        };

        var outputWriter = new OutputWriter(outputDir);
        var paramsResolver = new StaticParamsResolver(
            ssrRenderer.ComponentResolver, sp);
        var manifestPath = System.IO.Path.Combine(outputDir, "_buildManifest.json");
        var manifestGenerator = new BuildManifestGenerator(manifestPath);
        var assetCopier = new PublicAssetCopier(
            System.IO.Path.Combine(tempDir.Path, "nonexistent"),
            outputDir);

        var engine = new StaticGenerationEngine(
            options, ssrRenderer, manifest, paramsResolver,
            outputWriter, assetCopier, manifestGenerator);

        // Act
        var result = await engine.GenerateAsync();

        // Assert
        Assert.True(result.Success, string.Join(", ", result.Errors.Select(e => e.Message)));

        // Should have generated both slug variants
        var helloWorldFile = System.IO.Path.Combine(outputDir, "blog", "hello-world", "index.html");
        var gettingStartedFile = System.IO.Path.Combine(outputDir, "blog", "getting-started", "index.html");

        Assert.True(File.Exists(helloWorldFile), "hello-world file should exist");
        Assert.True(File.Exists(gettingStartedFile), "getting-started file should exist");

        // Verify manifest includes the dynamic route
        Assert.True(File.Exists(manifestPath));
        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        var doc = JsonDocument.Parse(manifestJson);
        var routes = doc.RootElement.GetProperty("routes");

        // At least one route should have params
        var hasParamsRoute = false;
        foreach (var route in routes.EnumerateArray())
        {
            if (route.TryGetProperty("params", out _))
            {
                hasParamsRoute = true;
                Assert.False(route.GetProperty("static").GetBoolean());
                break;
            }
        }
        Assert.True(hasParamsRoute, "Manifest should contain a dynamic route with params");
    }

    /// <summary>
    /// Creates an SsrRenderer with a test component resolver that maps
    /// file paths to registered page types.
    /// </summary>
    private static SsrRenderer CreateTestRenderer(IServiceProvider services, RouteManifest manifest)
    {
        // Create a resolver that returns the first registered IPage for any route
        var resolver = new TestRouteComponentResolver(services);
        return new SsrRenderer(
            services,
            manifest,
            componentResolver: resolver);
    }

    /// <summary>
    /// A simple component resolver that always returns the first registered IPage type.
    /// </summary>
    private sealed class TestRouteComponentResolver : IRouteComponentResolver
    {
        private readonly IServiceProvider _services;

        public TestRouteComponentResolver(IServiceProvider services)
        {
            _services = services;
        }

        public Type? GetPageType(RouteEntry entry)
        {
            // If IStaticPathProvider is registered AND the route is dynamic, return that type first
            if (entry.SegmentKind == RouteSegmentKind.Dynamic)
            {
                var pathProvider = _services.GetService<IStaticPathProvider>();
                if (pathProvider != null)
                    return pathProvider.GetType();
            }

            // Fall back to IPage
            var page = _services.GetService<IPage>();
            return page?.GetType();
        }

        public Type? GetLayoutType(string layoutPath)
        {
            return null;
        }
    }
}
