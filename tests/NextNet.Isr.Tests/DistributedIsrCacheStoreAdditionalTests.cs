using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using NextNet.Isr.Cache;

namespace NextNet.Isr.Tests;

/// <summary>
/// Additional tests for DistributedIsrCacheStore covering GetMetadataAsync, RemoveTagAsync, etc.
/// </summary>
public class DistributedIsrCacheStoreAdditionalTests
{
    private readonly Mock<IDistributedCache> _mockDistributed;
    private readonly DistributedIsrCacheStore _store;

    public DistributedIsrCacheStoreAdditionalTests()
    {
        _mockDistributed = new Mock<IDistributedCache>(MockBehavior.Strict);
        _store = new DistributedIsrCacheStore(_mockDistributed.Object);
    }

    [Fact]
    public async Task GetMetadataAsync_Should_ReturnEntry_When_Found()
    {
        var metaBytes = Encoding.UTF8.GetBytes(
            "{\"route\":\"/test\",\"generatedAt\":\"2026-06-06T12:00:00Z\",\"revalidateAfter\":\"2026-06-06T12:01:00Z\",\"revalidateIntervalSeconds\":60,\"tags\":[],\"hash\":\"\",\"size\":0}");

        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(metaBytes);

        var result = await _store.GetMetadataAsync("/test");

        Assert.NotNull(result);
        Assert.Equal("/test", result.Route);
        Assert.Equal(60, result.RevalidateIntervalSeconds);
    }

    [Fact]
    public async Task GetMetadataAsync_Should_ReturnNull_When_NotFound()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _store.GetMetadataAsync("/test");

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveTagAsync_Should_RemoveRoute_When_TagExists()
    {
        var existingRoutes = Encoding.UTF8.GetBytes("[\"/route1\",\"/route2\"]");

        _mockDistributed.Setup(d => d.GetAsync("isr:tag:test-tag", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRoutes);

        // After removing one route, we save the remaining (via the interface method with options)
        _mockDistributed.Setup(d => d.SetAsync(
                "isr:tag:test-tag",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _store.RemoveTagAsync("/route1", "test-tag");

        _mockDistributed.Verify(d => d.SetAsync(
            "isr:tag:test-tag",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTagAsync_Should_RemoveTag_When_LastRouteRemoved()
    {
        var existingRoutes = Encoding.UTF8.GetBytes("[\"/route1\"]");

        _mockDistributed.Setup(d => d.GetAsync("isr:tag:test-tag", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRoutes);

        _mockDistributed.Setup(d => d.RemoveAsync("isr:tag:test-tag", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _store.RemoveTagAsync("/route1", "test-tag");

        _mockDistributed.Verify(d => d.RemoveAsync("isr:tag:test-tag", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTagAsync_Should_NotThrow_When_TagDoesNotExist()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:tag:nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        await _store.RemoveTagAsync("/route1", "nonexistent");
        // Should not throw
    }

    [Fact]
    public async Task RemoveAsync_Should_CleanUpTags_When_ExistingTagsPresent()
    {
        var metaBytes = Encoding.UTF8.GetBytes(
            "{\"route\":\"/test\",\"generatedAt\":\"2026-06-06T12:00:00Z\",\"revalidateAfter\":\"2026-06-06T12:01:00Z\",\"revalidateIntervalSeconds\":60,\"tags\":[\"blog\"],\"hash\":\"\",\"size\":0}");

        _mockDistributed.Setup(d => d.GetAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(metaBytes);

        _mockDistributed.Setup(d => d.GetAsync("isr:tag:blog", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null); // No existing tag data

        _mockDistributed.Setup(d => d.RemoveAsync("isr:content:/test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockDistributed.Setup(d => d.RemoveAsync("isr:meta:/test", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _store.RemoveAsync("/test");
        Assert.True(result);
    }

    [Fact]
    public async Task AddTagAsync_Should_CreateTagEntry_When_NewTagAdded()
    {
        _mockDistributed.Setup(d => d.GetAsync("isr:tag:new-tag", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _mockDistributed.Setup(d => d.SetAsync(
                "isr:tag:new-tag",
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _store.AddTagAsync("/route1", "new-tag");

        _mockDistributed.Verify(d => d.SetAsync(
            "isr:tag:new-tag",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
