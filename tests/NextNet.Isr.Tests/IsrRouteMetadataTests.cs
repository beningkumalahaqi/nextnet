using NextNet.Isr.Manifest;

namespace NextNet.Isr.Tests;

public class IsrRouteMetadataTests
{
    [Fact]
    public void DefaultValues()
    {
        var meta = new IsrRouteMetadata();

        Assert.Equal(string.Empty, meta.RoutePattern);
        Assert.Null(meta.RevalidateSeconds);
        Assert.Null(meta.Tags);
        Assert.Equal(1, meta.MaxConcurrentRegenerations);
        Assert.True(meta.ServeStaleWhileRevalidate);
        Assert.Null(meta.FilePath);
    }

    [Fact]
    public void ToOptions_ConvertsCorrectly()
    {
        var meta = new IsrRouteMetadata
        {
            RoutePattern = "/blog/{slug}",
            RevalidateSeconds = 300,
            Tags = new[] { "blog" },
            MaxConcurrentRegenerations = 2,
            ServeStaleWhileRevalidate = false
        };

        var options = meta.ToOptions();

        Assert.Equal(300, options.Revalidate);
        Assert.Equal(new[] { "blog" }, options.RevalidateTags);
        Assert.Equal(2, options.MaxConcurrentRegenerations);
        Assert.False(options.ServeStaleWhileRevalidate);
    }

    [Fact]
    public void ToOptions_WithNullValues_MapsCorrectly()
    {
        var meta = new IsrRouteMetadata();
        var options = meta.ToOptions();

        Assert.Null(options.Revalidate);
        Assert.Null(options.RevalidateTags);
        Assert.Equal(1, options.MaxConcurrentRegenerations);
        Assert.True(options.ServeStaleWhileRevalidate);
    }
}
