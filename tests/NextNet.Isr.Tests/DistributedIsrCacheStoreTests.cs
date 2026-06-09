using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NextNet.Isr.Cache;

namespace NextNet.Isr.Tests;

public class DistributedIsrCacheStoreTests
{
    private readonly Mock<IDistributedCache> _mockDistributed;
    private readonly DistributedIsrCacheStore _store;

    public DistributedIsrCacheStoreTests()
    {
        _mockDistributed = new Mock<IDistributedCache>(MockBehavior.Strict);
        _store = new DistributedIsrCacheStore(_mockDistributed.Object);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnCachedPage_When_CacheHasContentAndMetadata()
    {
        var contentBytes = System.Text.Encoding.UTF8.GetBytes("<html>hello</html>");
        var metaBytes = System.Text.Encoding.UTF8.GetBytes(
            "{\"route\":\"/test\",\"generatedAt\":\"2026-06-06T12:00:00Z\",\"revalidateAfter\":\"2026-06-06T12:01:00Z\",\"revalidateIntervalSeconds\":60,\"hash\":\"\",\"size\":0,\"tags\":[]}");

        _mockDistributed.Setup(d => d.GetAsync("isr:content:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentBytes);
        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(metaBytes);

        var result = await _store.GetAsync("/test");

        Assert.NotNull(result);
        Assert.Equal("/test", result.Route);
        Assert.Equal("<html>hello</html>", result.Content);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_When_ContentIsMissing()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:content:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _store.GetAsync("/test");

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_MetadataExists()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        Assert.True(await _store.ExistsAsync("/test"));
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_MetadataIsMissing()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        Assert.False(await _store.ExistsAsync("/test"));
    }

    [Fact]
    public async Task SetAsync_Should_StoreContentMetadataAndTags_When_Called()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60,
            tags: new[] { "blog" }, hash: "abc", size: 100);

        // Setup for content and metadata storage
        _mockDistributed.Setup(d => d.SetAsync(
                "isr:content:/test",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockDistributed.Setup(d => d.SetAsync(
                "isr:meta:/test",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Tag setup: Get returns null (new tag), Set stores it
        _mockDistributed.Setup(d => d.GetAsync("isr:tag:blog", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _mockDistributed.Setup(d => d.SetAsync(
                "isr:tag:blog",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _store.SetAsync("/test", "<html>content</html>", entry);

        // Verify all calls were made
        _mockDistributed.Verify(d => d.SetAsync(
            "isr:content:/test",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockDistributed.Verify(d => d.SetAsync(
            "isr:meta:/test",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_Should_RemoveContentAndMetadata_When_Called()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null); // No metadata, so no tags to clean up

        _mockDistributed.Setup(d => d.RemoveAsync("isr:content:/test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockDistributed.Setup(d => d.RemoveAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _store.RemoveAsync("/test");

        Assert.True(result);
    }

    [Fact]
    public async Task GetRoutesByTagAsync_Should_ReturnRoutes_When_Called()
    {
        var routesBytes = System.Text.Encoding.UTF8.GetBytes("[\"/blog/post-1\"]");

        _mockDistributed.Setup(d => d.GetAsync("isr:tag:blog", It.IsAny<CancellationToken>()))
            .ReturnsAsync(routesBytes);

        var routes = await _store.GetRoutesByTagAsync(new[] { "blog" });

        Assert.Single(routes);
        Assert.Contains("/blog/post-1", routes);
    }
}
