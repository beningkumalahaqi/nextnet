using NextNet.Isr.Manifest;

namespace NextNet.Isr.Tests;

public class IsrManifestTests
{
    [Fact]
    public void Empty_Should_ReturnEmptyManifest_When_Called()
    {
        var manifest = IsrManifest.Empty;

        Assert.Empty(manifest.Routes);
        Assert.False(manifest.HasIsrRoutes);
        Assert.NotNull(manifest.GlobalOptions);
    }

    [Fact]
    public void Constructor_Should_StoreRoutes_When_Provided()
    {
        var routes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/blog/{slug}"] = new() { RevalidateSeconds = 300 }
        };

        var manifest = new IsrManifest(routes, new IsrGlobalOptions());

        Assert.Single(manifest.Routes);
        Assert.True(manifest.HasIsrRoutes);
    }

    [Fact]
    public void TryGetMetadata_Should_ReturnTrue_When_ExactMatch()
    {
        var routes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/about"] = new() { RevalidateSeconds = 60 }
        };

        var manifest = new IsrManifest(routes, new IsrGlobalOptions());

        Assert.True(manifest.TryGetMetadata("/about", out var meta));
        Assert.NotNull(meta);
        Assert.Equal(60, meta.RevalidateSeconds);
    }

    [Fact]
    public void TryGetMetadata_Should_ReturnTrue_When_PatternMatch()
    {
        var routes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/blog/{slug}"] = new() { RevalidateSeconds = 300, Tags = new[] { "blog" } }
        };

        var manifest = new IsrManifest(routes, new IsrGlobalOptions());

        Assert.True(manifest.TryGetMetadata("/blog/hello-world", out var meta));
        Assert.NotNull(meta);
        Assert.Equal(300, meta.RevalidateSeconds);
        Assert.Contains("blog", meta.Tags!);
    }

    [Fact]
    public void TryGetMetadata_Should_ReturnFalse_When_NoMatch()
    {
        var manifest = IsrManifest.Empty;

        Assert.False(manifest.TryGetMetadata("/nonexistent", out var meta));
        Assert.Null(meta);
    }

    [Fact]
    public void GetMetadataOrDefault_Should_ReturnMetadata_When_MatchFound()
    {
        var routes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/about"] = new() { RevalidateSeconds = 120 }
        };

        var manifest = new IsrManifest(routes, new IsrGlobalOptions());

        var meta = manifest.GetMetadataOrDefault("/about");
        Assert.Equal(120, meta.RevalidateSeconds);
    }

    [Fact]
    public void GetMetadataOrDefault_Should_ReturnDefault_When_NoMatch()
    {
        var globalOptions = new IsrGlobalOptions { DefaultRevalidateSeconds = 60 };
        var manifest = new IsrManifest(new Dictionary<string, IsrRouteMetadata>(), globalOptions);

        var meta = manifest.GetMetadataOrDefault("/unknown");

        Assert.Equal("/unknown", meta.RoutePattern);
        Assert.Equal(60, meta.RevalidateSeconds);
    }

    [Fact]
    public void Constructor_Should_Throw_When_RoutesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IsrManifest(null!, new IsrGlobalOptions()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_GlobalOptionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new IsrManifest(new Dictionary<string, IsrRouteMetadata>(), null!));
    }
}
