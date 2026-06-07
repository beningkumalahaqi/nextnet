using NextNet.Isr.Manifest;

namespace NextNet.Isr.Tests;

public class IsrRouteAttributeTests
{
    [Fact]
    public void DefaultValues()
    {
        var attr = new IsrRouteAttribute();

        Assert.Null(attr.RevalidateSeconds);
        Assert.Null(attr.Tags);
        Assert.Equal(1, attr.MaxConcurrentRegenerations);
        Assert.True(attr.ServeStaleWhileRevalidate);
    }

    [Fact]
    public void CanSetProperties()
    {
        var attr = new IsrRouteAttribute
        {
            RevalidateSeconds = 300,
            Tags = new[] { "blog", "content" },
            MaxConcurrentRegenerations = 2,
            ServeStaleWhileRevalidate = false
        };

        Assert.Equal(300, attr.RevalidateSeconds);
        Assert.Equal(new[] { "blog", "content" }, attr.Tags);
        Assert.Equal(2, attr.MaxConcurrentRegenerations);
        Assert.False(attr.ServeStaleWhileRevalidate);
    }
}
