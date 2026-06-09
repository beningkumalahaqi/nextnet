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
    public async Task FullBuild_Should_GenerateCorrectOutput_When_StaticRoutes()
    {
        using var tempDir = new TempDirectory();
        var appDir = System.IO.Path.Combine(tempDir.Path, NextNetConventions.AppDirectory);
        var publicDir = System.IO.Path.Combine(tempDir.Path, NextNetConventions.PublicDirectory);
        var outputDir = System.IO.Path.Combine(tempDir.Path, NextNetConventions.OutputDirectory);

        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(publicDir);

        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "styles.css"), "body { color: red; }");
        await File.WriteAllTextAsync(System.IO.Path.Combine(publicDir, "app.js"), "console.log('hello');");

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

        var services = new ServiceCollection();
        services.AddScoped<SamplePage>(_ => new SamplePage("<h1>Home</h1>"));
        services.AddScoped<IPage>(sp => sp.GetRequiredService<SamplePage>());
        var sp = services.BuildServiceProvider();

        var ssrRenderer = CreateTestRenderer(sp, manifest);

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

        var result = await engine.GenerateAsync();

        Assert.True(result.Success);
        Assert.NotEmpty(result.GeneratedFiles);
        Assert.Contains("index.html", result.GeneratedFiles);
        Assert.Contains("about/index.html", result.GeneratedFiles);
        Assert.Contains("index.html.gz", result.GeneratedFiles);
        Assert.True(result.AssetCount > 0);

        var homeHtml = await File.ReadAllTextAsync(System.IO.Path.Combine(outputDir, "index.html"));
        Assert.Contains("<h1>Home</h1>", homeHtml);

        Assert.True(File.Exists(manifestPath));
        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        var doc = JsonDocument.Parse(manifestJson);
        Assert.True(doc.RootElement.TryGetProperty("routes", out _));
        Assert.True(doc.RootElement.TryGetProperty("build", out _));
    }

    [Fact]
    public async Task FullBuild_Should_ResolveParams_When_DynamicRoutes()
    {
        using var tempDir = new TempDirectory();
        var outputDir = System.IO.Path.Combine(tempDir.Path, "dist");

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

        var result = await engine.GenerateAsync();

        Assert.True(result.Success, string.Join(", ", result.Errors.Select(e => e.Message)));

        var helloWorldFile = System.IO.Path.Combine(outputDir, "blog", "hello-world", "index.html");
        var gettingStartedFile = System.IO.Path.Combine(outputDir, "blog", "getting-started", "index.html");

        Assert.True(File.Exists(helloWorldFile), "hello-world file should exist");
        Assert.True(File.Exists(gettingStartedFile), "getting-started file should exist");

        Assert.True(File.Exists(manifestPath));
        var manifestJson = await File.ReadAllTextAsync(manifestPath);
        var doc = JsonDocument.Parse(manifestJson);
        var routes = doc.RootElement.GetProperty("routes");

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

    private static SsrRenderer CreateTestRenderer(IServiceProvider services, RouteManifest manifest)
    {
        var resolver = new TestRouteComponentResolver(services);
        return new SsrRenderer(
            services,
            manifest,
            componentResolver: resolver);
    }

    private sealed class TestRouteComponentResolver : IRouteComponentResolver
    {
        private readonly IServiceProvider _services;

        public TestRouteComponentResolver(IServiceProvider services)
        {
            _services = services;
        }

        public Type? GetPageType(RouteEntry entry)
        {
            if (entry.SegmentKind == RouteSegmentKind.Dynamic)
            {
                var pathProvider = _services.GetService<IStaticPathProvider>();
                if (pathProvider != null)
                    return pathProvider.GetType();
            }

            var page = _services.GetService<IPage>();
            return page?.GetType();
        }

        public Type? GetLayoutType(string layoutPath)
        {
            return null;
        }
    }
}
