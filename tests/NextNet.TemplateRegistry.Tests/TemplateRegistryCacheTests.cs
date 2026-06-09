namespace NextNet.TemplateRegistry.Tests;

using NextNet.TemplateRegistry;
using Xunit;

public class TemplateRegistryCacheTests
{
    private static RegistryOptions CreateTempOptions()
    {
        return new RegistryOptions
        {
            CacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        };
    }

    [Fact]
    public async Task SetAsync_Then_GetAsync_Should_RoundTrip_When_KeyExists()
    {
        var options = CreateTempOptions();
        var cache = new TemplateRegistryCache(options);

        var data = new { Name = "test", Value = 42 };
        await cache.SetAsync("test_key", data);
        var result = await cache.GetAsync<object>("test_key");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_When_KeyNotFound()
    {
        var options = CreateTempOptions();
        var cache = new TemplateRegistryCache(options);
        var result = await cache.GetAsync<object>("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task Invalidate_Should_RemoveKey_When_KeyExists()
    {
        var options = CreateTempOptions();
        var cache = new TemplateRegistryCache(options);
        await cache.SetAsync("test", new { x = 1 });
        cache.Invalidate("test");
        var result = await cache.GetAsync<object>("test");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_Should_ReturnNull_When_CacheExpired()
    {
        var options = CreateTempOptions();
        options.CacheTtl = TimeSpan.FromMilliseconds(1);
        var cache = new TemplateRegistryCache(options);
        await cache.SetAsync("test", new { x = 1 });
        await Task.Delay(50);
        var result = await cache.GetAsync<object>("test");
        Assert.Null(result);
    }

    [Fact]
    public async Task Clear_Should_RemoveAllKeys_When_CacheHasEntries()
    {
        var options = CreateTempOptions();
        var cache = new TemplateRegistryCache(options);
        await cache.SetAsync("key1", new { a = 1 });
        await cache.SetAsync("key2", new { b = 2 });
        cache.Clear();
        Assert.Null(await cache.GetAsync<object>("key1"));
        Assert.Null(await cache.GetAsync<object>("key2"));
    }
}
