using NextNet.Isr.Cache;

namespace NextNet.Isr.Tests;

public class FileSystemCacheProviderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileSystemCacheProvider _store;

    public FileSystemCacheProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _store = new FileSystemCacheProvider(_tempDir);
    }

    public void Dispose()
    {
        _store.Dispose();
        try
        {
            Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Ignore cleanup failures
        }
    }

    [Fact]
    public async Task SetAndGet_Should_StoreAndRetrievePage_When_Called()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60);
        await _store.SetAsync("/test", "<html>content</html>", entry);

        var cached = await _store.GetAsync("/test");

        Assert.NotNull(cached);
        Assert.Equal("/test", cached.Route);
        Assert.Equal("<html>content</html>", cached.Content);
        Assert.Equal(entry.RevalidateAfter, cached.Metadata.RevalidateAfter);
    }

    [Fact]
    public async Task Get_Should_ReturnNull_When_RouteDoesNotExist()
    {
        var cached = await _store.GetAsync("/nonexistent");
        Assert.Null(cached);
    }

    [Fact]
    public async Task Exists_Should_ReturnTrue_When_PageIsCached()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60);
        await _store.SetAsync("/test", "content", entry);

        Assert.True(await _store.ExistsAsync("/test"));
    }

    [Fact]
    public async Task Exists_Should_ReturnFalse_When_PageIsNotCached()
    {
        Assert.False(await _store.ExistsAsync("/test"));
    }

    [Fact]
    public async Task Remove_Should_ReturnTrueAndRemove_When_EntryExists()
    {
        var entry = new CacheEntry("/test", DateTime.UtcNow, 60);
        await _store.SetAsync("/test", "content", entry);

        Assert.True(await _store.RemoveAsync("/test"));
        Assert.Null(await _store.GetAsync("/test"));
    }

    [Fact]
    public async Task Remove_Should_ReturnFalse_When_EntryDoesNotExist()
    {
        Assert.False(await _store.RemoveAsync("/test"));
    }

    [Fact]
    public async Task GetMetadata_Should_ReturnEntryMetadata_When_Called()
    {
        var now = DateTime.UtcNow;
        var entry = new CacheEntry("/test", now, 60, tags: new[] { "tag1" });
        await _store.SetAsync("/test", "content", entry);

        var metadata = await _store.GetMetadataAsync("/test");

        Assert.NotNull(metadata);
        Assert.Equal("/test", metadata.Route);
        Assert.Equal(now, metadata.GeneratedAt, TimeSpan.FromSeconds(1));
        Assert.Contains("tag1", metadata.Tags);
    }

    [Fact]
    public async Task TagIndex_Should_AddAndRetrieveByTag_When_Called()
    {
        var entry1 = new CacheEntry("/blog/post-1", DateTime.UtcNow, 60, tags: new[] { "blog" });
        var entry2 = new CacheEntry("/blog/post-2", DateTime.UtcNow, 60, tags: new[] { "blog" });
        var entry3 = new CacheEntry("/about", DateTime.UtcNow, 60);

        await _store.SetAsync("/blog/post-1", "content1", entry1);
        await _store.SetAsync("/blog/post-2", "content2", entry2);
        await _store.SetAsync("/about", "content3", entry3);

        var routes = await _store.GetRoutesByTagAsync(new[] { "blog" });

        Assert.Contains("/blog/post-1", routes);
        Assert.Contains("/blog/post-2", routes);
        Assert.DoesNotContain("/about", routes);
    }

    [Fact]
    public async Task Remove_Should_CleanUpTagIndex_When_EntryRemoved()
    {
        var entry = new CacheEntry("/blog/post", DateTime.UtcNow, 60, tags: new[] { "blog" });
        await _store.SetAsync("/blog/post", "content", entry);
        await _store.RemoveAsync("/blog/post");

        var routes = await _store.GetRoutesByTagAsync(new[] { "blog" });
        Assert.Empty(routes);
    }

    [Fact]
    public async Task AddTag_And_RemoveTag_Should_ManageIndices_When_Called()
    {
        await _store.AddTagAsync("/route1", "news");
        await _store.AddTagAsync("/route2", "news");

        var routes = await _store.GetRoutesByTagAsync(new[] { "news" });
        Assert.Contains("/route1", routes);
        Assert.Contains("/route2", routes);

        await _store.RemoveTagAsync("/route1", "news");
        routes = await _store.GetRoutesByTagAsync(new[] { "news" });
        Assert.DoesNotContain("/route1", routes);
        Assert.Contains("/route2", routes);
    }

    [Fact]
    public async Task Update_Should_ReplaceContent_When_RouteAlreadyExists()
    {
        var entry1 = new CacheEntry("/test", DateTime.UtcNow, 60);
        await _store.SetAsync("/test", "old-content", entry1);

        var entry2 = new CacheEntry("/test", DateTime.UtcNow, 60);
        await _store.SetAsync("/test", "new-content", entry2);

        var cached = await _store.GetAsync("/test");
        Assert.NotNull(cached);
        Assert.Equal("new-content", cached.Content);
    }

    [Fact]
    public async Task PersistsAcrossRestarts_Should_RetainData_When_SameDirectoryUsed()
    {
        // First instance
        var entry = new CacheEntry("/persist-test", DateTime.UtcNow, 60, tags: new[] { "persist" });
        await _store.SetAsync("/persist-test", "persistent-content", entry);

        // Dispose and create a new instance pointing to the same directory
        _store.Dispose();
        using var store2 = new FileSystemCacheProvider(_tempDir);

        var cached = await store2.GetAsync("/persist-test");
        Assert.NotNull(cached);
        Assert.Equal("persistent-content", cached.Content);

        // Tags should also be persisted
        var routes = await store2.GetRoutesByTagAsync(new[] { "persist" });
        Assert.Contains("/persist-test", routes);
    }

    [Fact]
    public async Task Clear_Should_RemoveAllEntries_When_Called()
    {
        // Set some entries
        var entry1 = new CacheEntry("/a", DateTime.UtcNow, 60);
        var entry2 = new CacheEntry("/b", DateTime.UtcNow, 60);

        await _store.SetAsync("/a", "content-a", entry1);
        await _store.SetAsync("/b", "content-b", entry2);

        Assert.Equal(2, _store.Count);

        _store.Clear();

        Assert.Equal(0, _store.Count);
        Assert.Null(await _store.GetAsync("/a"));
        Assert.Null(await _store.GetAsync("/b"));
    }

    [Fact]
    public async Task GetRoutesByTag_Should_ReturnUnion_When_MultipleTagsProvided()
    {
        await _store.AddTagAsync("/blog/post-1", "blog");
        await _store.AddTagAsync("/blog/post-1", "content");
        await _store.AddTagAsync("/news/article-1", "news");

        var routes = await _store.GetRoutesByTagAsync(new[] { "blog", "news" });

        Assert.Contains("/blog/post-1", routes);
        Assert.Contains("/news/article-1", routes);
    }

    [Fact]
    public async Task DeeplyNestedRoutes_Should_StoreAndRetrieve_When_Called()
    {
        var entry = new CacheEntry("/blog/2024/hello-world", DateTime.UtcNow, 120);
        await _store.SetAsync("/blog/2024/hello-world", "deep content", entry);

        var cached = await _store.GetAsync("/blog/2024/hello-world");
        Assert.NotNull(cached);
        Assert.Equal("deep content", cached.Content);
    }

    [Fact]
    public async Task CancellationToken_Should_BeRespected_When_Cancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _store.GetAsync("/test", cts.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _store.SetAsync("/test", "content", new CacheEntry("/test", DateTime.UtcNow, 60), cts.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _store.ExistsAsync("/test", cts.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _store.RemoveAsync("/test", cts.Token));
    }
}
