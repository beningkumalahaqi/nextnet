using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.StaticGeneration;
using NextNet.Components;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class StaticGenerationEngineTests
{
    [Fact]
    public void ResolveRoutePattern_Should_ReplaceSingleParam_When_Provided()
    {
        var paramSet = new Dictionary<string, string>
        {
            ["slug"] = "hello-world",
        };

        var result = StaticGenerationEngine.ResolveRoutePattern("/blog/{slug}", paramSet);
        Assert.Equal("/blog/hello-world", result);
    }

    [Fact]
    public void ResolveRoutePattern_Should_ReplaceMultipleParams_When_Provided()
    {
        var paramSet = new Dictionary<string, string>
        {
            ["year"] = "2024",
            ["month"] = "01",
            ["slug"] = "post",
        };

        var result = StaticGenerationEngine.ResolveRoutePattern("/blog/{year}/{month}/{slug}", paramSet);
        Assert.Equal("/blog/2024/01/post", result);
    }

    [Fact]
    public void ResolveRoutePattern_Should_ReturnOriginal_When_NoParams()
    {
        var result = StaticGenerationEngine.ResolveRoutePattern("/about", new Dictionary<string, string>());
        Assert.Equal("/about", result);
    }

    [Fact]
    public void ResolveRoutePattern_Should_ThrowArgumentNullException_When_NullPattern()
    {
        Assert.Throws<ArgumentNullException>(
            () => StaticGenerationEngine.ResolveRoutePattern(null!, new Dictionary<string, string>()));
    }

    [Fact]
    public void ExtractParamNames_Should_ReturnCorrectNames_When_DynamicRoute()
    {
        var result = StaticGenerationEngine.ExtractParamNames("/blog/{slug}");
        Assert.Equal(new[] { "slug" }, result);
    }

    [Fact]
    public void ExtractParamNames_Should_ReturnMultipleNames_When_MultipleParams()
    {
        var result = StaticGenerationEngine.ExtractParamNames("/blog/{year}/{month}/{slug}");
        Assert.Equal(new[] { "year", "month", "slug" }, result);
    }

    [Fact]
    public void ExtractParamNames_Should_ReturnEmpty_When_StaticRoute()
    {
        var result = StaticGenerationEngine.ExtractParamNames("/about");
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractParamNames_Should_ReturnEmpty_When_EmptyString()
    {
        var result = StaticGenerationEngine.ExtractParamNames("");
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractParamNames_Should_ReturnEmpty_When_Null()
    {
        var result = StaticGenerationEngine.ExtractParamNames(null!);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GenerateForRouteAsync_Should_ThrowArgumentNullException_When_NullRoute()
    {
        var engine = CreateEmptyEngine();
        await Assert.ThrowsAsync<ArgumentNullException>(() => engine.GenerateForRouteAsync(null!));
    }

    [Fact]
    public void ResolveRoutePattern_Should_ThrowArgumentNullException_When_NullParamSet()
    {
        Assert.Throws<ArgumentNullException>(
            () => StaticGenerationEngine.ResolveRoutePattern("/test", null!));
    }

    /// <summary>
    /// Creates a StaticGenerationEngine with minimal valid dependencies for testing.
    /// </summary>
    private static StaticGenerationEngine CreateEmptyEngine()
    {
        var options = new SsgOptions
        {
            OutputDirectory = "/tmp/test-output",
            MinifyHtml = false,
            CompressGzip = false,
            GenerateBuildManifest = false,
            CopyAssets = false,
            CleanOutput = false,
        };

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var manifest = new RouteManifest(
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>()
        );

        var renderer = new SsrRenderer(sp, manifest);
        var resolver = new NextNet.Rendering.ConventionRouteComponentResolver(
            new Dictionary<string, Type>(),
            new Dictionary<string, Type>());
        var paramsResolver = new StaticParamsResolver(resolver, sp);
        var outputWriter = new OutputWriter("/tmp/test-output");
        var assetCopier = new PublicAssetCopier("/tmp/nonexistent-public", "/tmp/test-output");
        var manifestGen = new BuildManifestGenerator("/tmp/test-output/_buildManifest.json");

        return new StaticGenerationEngine(
            options,
            renderer,
            manifest,
            paramsResolver,
            outputWriter,
            assetCopier,
            manifestGen
        );
    }
}
