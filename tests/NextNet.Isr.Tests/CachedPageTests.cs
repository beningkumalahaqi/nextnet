using NextNet.Isr.Cache;

namespace NextNet.Isr.Tests;

public class CachedPageTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var metadata = new CacheEntry("/test", DateTime.UtcNow, 60);
        var page = new CachedPage("/test", "<html>hello</html>", metadata);

        Assert.Equal("/test", page.Route);
        Assert.Equal("<html>hello</html>", page.Content);
        Assert.Same(metadata, page.Metadata);
    }

    [Fact]
    public void Constructor_NullRoute_Throws()
    {
        var metadata = new CacheEntry("/test", DateTime.UtcNow, 60);
        Assert.Throws<ArgumentNullException>(() =>
            new CachedPage(null!, "content", metadata));
    }

    [Fact]
    public void Constructor_NullContent_Throws()
    {
        var metadata = new CacheEntry("/test", DateTime.UtcNow, 60);
        Assert.Throws<ArgumentNullException>(() =>
            new CachedPage("/test", null!, metadata));
    }

    [Fact]
    public void Constructor_NullMetadata_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CachedPage("/test", "content", null!));
    }
}
