using NextNet.Isr.Cache;

namespace NextNet.Isr.Tests;

public class CacheEntryTests
{
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        var now = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);

        var entry = new CacheEntry(
            route: "/blog/hello-world",
            generatedAt: now,
            revalidateIntervalSeconds: 300,
            tags: new[] { "blog", "content" },
            hash: "abc123",
            size: 4096);

        Assert.Equal("/blog/hello-world", entry.Route);
        Assert.Equal(now, entry.GeneratedAt);
        Assert.Equal(300, entry.RevalidateIntervalSeconds);
        Assert.Equal(now.AddSeconds(300), entry.RevalidateAfter);
        Assert.Equal(2, entry.Tags.Count);
        Assert.Contains("blog", entry.Tags);
        Assert.Contains("content", entry.Tags);
        Assert.Equal("abc123", entry.Hash);
        Assert.Equal(4096, entry.Size);
    }

    [Fact]
    public void Constructor_NullRoute_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CacheEntry(null!, DateTime.UtcNow, 60));
    }

    [Fact]
    public void Constructor_NegativeInterval_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CacheEntry("/test", DateTime.UtcNow, -1));
    }

    [Fact]
    public void Constructor_ZeroInterval_SetsRevalidateAfterToMaxValue()
    {
        var entry = new CacheEntry("/static", DateTime.UtcNow, 0);
        Assert.Equal(DateTime.MaxValue, entry.RevalidateAfter);
    }

    [Fact]
    public void Constructor_NullTags_DefaultsToEmpty()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60, null);
        Assert.Empty(entry.Tags);
    }

    [Fact]
    public void IsStale_PastRevalidateAfter_ReturnsTrue()
    {
        var now = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var entry = new CacheEntry("/test", now, 60);

        Assert.True(entry.IsStale(now.AddSeconds(61)));
    }

    [Fact]
    public void IsStale_BeforeRevalidateAfter_ReturnsFalse()
    {
        var now = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var entry = new CacheEntry("/test", now, 60);

        Assert.False(entry.IsStale(now.AddSeconds(30)));
    }

    [Fact]
    public void IsStale_ExactlyAtRevalidateAfter_ReturnsTrue()
    {
        var now = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var entry = new CacheEntry("/test", now, 60);

        Assert.True(entry.IsStale(now.AddSeconds(60)));
    }

    [Fact]
    public void HasTag_WithMatchingTag_ReturnsTrue()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60,
            tags: new[] { "blog", "news" });

        Assert.True(entry.HasTag("blog"));
        Assert.True(entry.HasTag("news"));
    }

    [Fact]
    public void HasTag_WithNonMatchingTag_ReturnsFalse()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60,
            tags: new[] { "blog" });

        Assert.False(entry.HasTag("other"));
    }

    [Fact]
    public void HasTag_CaseInsensitive_ReturnsTrue()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60,
            tags: new[] { "Blog" });

        Assert.True(entry.HasTag("blog"));
        Assert.True(entry.HasTag("BLOG"));
    }
}
