using Microsoft.AspNetCore.Http;
using Moq;
using NextNet.Components;
using NextNet.Isr.Cache;
using NextNet.Isr.Revalidation;
using NextNet.Rendering;

namespace NextNet.Isr.Tests;

public class IsrRevalidationManagerTests
{
    private readonly Mock<IIsrCacheStore> _mockCacheStore;
    private readonly SsrRenderer _ssrRenderer;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly IsrGlobalOptions _globalOptions;
    private readonly IsrRevalidationManager _manager;

    public IsrRevalidationManagerTests()
    {
        _mockCacheStore = new Mock<IIsrCacheStore>(MockBehavior.Strict);
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        _globalOptions = new IsrGlobalOptions { DefaultRevalidateSeconds = 60 };

        var routeManifest = new Routing.RouteManifest(
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            null,
            Array.Empty<Routing.Models.RouteConflict>());

        var serviceProvider = Mock.Of<IServiceProvider>();
        _ssrRenderer = new SsrRenderer(serviceProvider, routeManifest);

        _mockHttpContextAccessor.Setup(a => a.HttpContext)
            .Returns(new DefaultHttpContext());

        _manager = new IsrRevalidationManager(
            _mockCacheStore.Object,
            _ssrRenderer,
            _mockHttpContextAccessor.Object,
            _globalOptions);
    }

    [Fact]
    public async Task IsStaleAsync_Should_ReturnTrue_When_CacheIsMissing()
    {
        _mockCacheStore.Setup(c => c.GetMetadataAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CacheEntry?)null);

        var result = await _manager.IsStaleAsync("/test");
        Assert.True(result);
    }

    [Fact]
    public async Task IsStaleAsync_Should_ReturnFalse_When_CacheIsFresh()
    {
        var now = DateTime.UtcNow;
        var entry = new CacheEntry("/test", now, 60, null, null, 0);

        _mockCacheStore.Setup(c => c.GetMetadataAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var result = await _manager.IsStaleAsync("/test");
        Assert.False(result);
    }

    [Fact]
    public async Task IsStaleAsync_Should_ReturnTrue_When_CacheIsStale()
    {
        // Use a time well in the past to ensure the entry is stale
        var generatedAt = DateTime.UtcNow.AddMinutes(-10); // 10 minutes ago
        var entry = new CacheEntry("/test", generatedAt, 60, null, null, 0);

        _mockCacheStore.Setup(c => c.GetMetadataAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var result = await _manager.IsStaleAsync("/test");
        Assert.True(result);
    }

    [Fact]
    public async Task GetCachedAsync_Should_DelegateToCacheStore()
    {
        var cached = new CachedPage("/test", "html", new CacheEntry("/test", DateTime.UtcNow, 60));

        _mockCacheStore.Setup(c => c.GetAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _manager.GetCachedAsync("/test");

        Assert.NotNull(result);
        Assert.Same(cached, result);
    }

    [Fact]
    public async Task SetCachedAsync_Should_ComputeHashAndStore()
    {
        _mockCacheStore.Setup(c => c.SetAsync(
                "/test",
                "<html>hello</html>",
                It.IsAny<CacheEntry>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var options = new IsrOptions { Revalidate = 120, RevalidateTags = new[] { "blog" } };

        await _manager.SetCachedAsync("/test", "<html>hello</html>", options);

        _mockCacheStore.Verify(c => c.SetAsync(
            "/test",
            "<html>hello</html>",
            It.Is<CacheEntry>(e => e.RevalidateIntervalSeconds == 120),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCachedAsync_Should_UseGlobalDefault_When_IntervalIsNull()
    {
        _mockCacheStore.Setup(c => c.SetAsync(
                "/test",
                "content",
                It.IsAny<CacheEntry>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var options = new IsrOptions { Revalidate = null };

        await _manager.SetCachedAsync("/test", "content", options);

        _mockCacheStore.Verify(c => c.SetAsync(
            "/test",
            "content",
            It.Is<CacheEntry>(e => e.RevalidateIntervalSeconds == 60),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ComputeHash_Should_ReturnHexString_When_ContentIsProvided()
    {
        var hash = IsrRevalidationManager.ComputeHash("test content");
        Assert.NotNull(hash);
        Assert.Matches("^[a-f0-9]+$", hash);
        Assert.Equal(64, hash.Length); // SHA-256 produces 64 hex chars
    }

    [Fact]
    public void ComputeHash_Should_ReturnEmpty_When_ContentIsEmpty()
    {
        var hash = IsrRevalidationManager.ComputeHash("");
        Assert.Equal(string.Empty, hash);
    }

    [Fact]
    public void ComputeHash_Should_ReturnEmpty_When_ContentIsNull()
    {
        var hash = IsrRevalidationManager.ComputeHash(null!);
        Assert.Equal(string.Empty, hash);
    }

    [Fact]
    public void ComputeHash_Should_BeDeterministic_When_SameInput()
    {
        var hash1 = IsrRevalidationManager.ComputeHash("hello");
        var hash2 = IsrRevalidationManager.ComputeHash("hello");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_Should_ProduceDifferentHashes_When_DifferentInputs()
    {
        var hash1 = IsrRevalidationManager.ComputeHash("hello");
        var hash2 = IsrRevalidationManager.ComputeHash("world");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public async Task InvalidateByTagsAsync_Should_ReturnFailure_When_TagsAreEmpty()
    {
        var result = await _manager.InvalidateByTagsAsync(Array.Empty<string>());
        Assert.False(result.Success);
    }

    [Fact]
    public async Task InvalidateByTagsAsync_Should_ReturnFailure_When_TagsAreNull()
    {
        var result = await _manager.InvalidateByTagsAsync(null!);
        Assert.False(result.Success);
    }
}
