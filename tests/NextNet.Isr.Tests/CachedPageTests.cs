using NextNet.Isr.Cache;

namespace NextNet.Isr.Tests;

public class CachedPageTests
{
    [Fact]
    public void Constructor_Should_SetProperties_When_ValidParameters()
    {
        var metadata = new CacheEntry("/test", DateTime.UtcNow, 60);
        var page = new CachedPage("/test", "<html>hello</html>", metadata);

        Assert.Equal("/test", page.Route);
        Assert.Equal("<html>hello</html>", page.Content);
        Assert.Same(metadata, page.Metadata);
    }

    [Fact]
    public void Constructor_Should_Throw_When_RouteIsNull()
    {
        var metadata = new CacheEntry("/test", DateTime.UtcNow, 60);
        Assert.Throws<ArgumentNullException>(() =>
            new CachedPage(null!, "content", metadata));
    }

    [Fact]
    public void Constructor_Should_Throw_When_ContentIsNull()
    {
        var metadata = new CacheEntry("/test", DateTime.UtcNow, 60);
        Assert.Throws<ArgumentNullException>(() =>
            new CachedPage("/test", null!, metadata));
    }

    [Fact]
    public void Constructor_Should_Throw_When_MetadataIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CachedPage("/test", "content", null!));
    }
}
