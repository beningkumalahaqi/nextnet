namespace NextNet.TemplateMarketplace.Tests;

using Xunit;

public class MarketplaceCacheTests
{
    [Fact]
    public async Task SetAsync_Then_GetAsync_Should_RoundTrip_When_KeyExists()
    {
        var options = new MarketplaceOptions
        {
            CacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };
        var cache = new MarketplaceCache(options);

        var data = new PublisherProfile { PublisherId = "test", DisplayName = "Test" };
        await cache.SetAsync("test_key", data);

        var retrieved = await cache.GetAsync<PublisherProfile>("test_key");
        Assert.NotNull(retrieved);
        Assert.Equal("test", retrieved!.PublisherId);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_When_NotFound()
    {
        var options = new MarketplaceOptions
        {
            CacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };
        var cache = new MarketplaceCache(options);

        Assert.Null(await cache.GetAsync<PublisherProfile>("nonexistent"));
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_When_Expired()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = new MarketplaceOptions
        {
            CacheDirectory = tempDir,
            CacheTtl = TimeSpan.FromMilliseconds(1)
        };
        var cache = new MarketplaceCache(options);

        var data = new PublisherProfile { PublisherId = "test", DisplayName = "Test" };
        await cache.SetAsync("test_key", data);

        // Wait for TTL to expire
        await Task.Delay(10);

        var retrieved = await cache.GetAsync<PublisherProfile>("test_key");
        Assert.Null(retrieved);
    }
}
