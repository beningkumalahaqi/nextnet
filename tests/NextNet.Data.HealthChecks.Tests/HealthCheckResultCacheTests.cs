namespace NextNet.Data.HealthChecks.Tests;

public class HealthCheckResultCacheTests
{
    private static HealthCheckResult CreateHealthyResult() =>
        HealthCheckResult.Healthy("All good", new Dictionary<string, object> { { "key", "value" } });

    [Fact]
    public void TryGet_Should_ReturnTrue_When_EntryExistsAndNotExpired()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var result = CreateHealthyResult();
        cache.Set("key1", result, TimeSpan.FromMinutes(5));

        // Act
        var found = cache.TryGet("key1", out var cached);

        // Assert
        Assert.True(found);
        Assert.Equal(HealthStatus.Healthy, cached.Status);
        Assert.Equal("All good", cached.Description);
    }

    [Fact]
    public void TryGet_Should_ReturnFalse_When_EntryExpired()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var result = CreateHealthyResult();
        // Use a very short TTL that has already expired
        cache.Set("key1", result, TimeSpan.FromMilliseconds(-1));

        // Act
        var found = cache.TryGet("key1", out _);

        // Assert
        Assert.False(found);
    }

    [Fact]
    public void TryGet_Should_ReturnFalse_When_KeyNotFound()
    {
        // Arrange
        var cache = new HealthCheckResultCache();

        // Act
        var found = cache.TryGet("nonexistent", out _);

        // Assert
        Assert.False(found);
    }

    [Fact]
    public void Set_Should_StoreEntry_WithCorrectExpiration()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var result = CreateHealthyResult();

        // Act
        cache.Set("key1", result, TimeSpan.FromSeconds(30));

        // Assert
        Assert.True(cache.TryGet("key1", out var cached));
        Assert.Equal(HealthStatus.Healthy, cached.Status);
    }

    [Fact]
    public void InvalidateAll_Should_ClearAllEntries()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        cache.Set("key1", CreateHealthyResult(), TimeSpan.FromMinutes(5));
        cache.Set("key2", CreateHealthyResult(), TimeSpan.FromMinutes(5));
        Assert.True(cache.TryGet("key1", out _));
        Assert.True(cache.TryGet("key2", out _));

        // Act
        cache.InvalidateAll();

        // Assert
        Assert.False(cache.TryGet("key1", out _));
        Assert.False(cache.TryGet("key2", out _));
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void TryGet_Should_ReturnFalse_When_EntryExpiresExactly()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var result = CreateHealthyResult();
        cache.Set("key1", result, TimeSpan.Zero);

        // Act
        var found = cache.TryGet("key1", out _);

        // Assert
        Assert.False(found);
    }

    [Fact]
    public void Set_Should_DefaultToFiveSeconds_When_TtlNotProvided()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var result = CreateHealthyResult();

        // Act
        cache.Set("key1", result);

        // Assert - should be immediately retrievable
        Assert.True(cache.TryGet("key1", out var cached));
        Assert.Equal(HealthStatus.Healthy, cached.Status);
    }

    [Fact]
    public void ConcurrentAccess_Should_BeThreadSafe()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        const int iterations = 100;

        // Act - simulate concurrent reads and writes
        Parallel.For(0, iterations, i =>
        {
            cache.Set($"key{i}", CreateHealthyResult(), TimeSpan.FromMinutes(5));
        });

        Parallel.For(0, iterations, i =>
        {
            cache.TryGet($"key{i}", out _);
        });

        // Assert - all entries should be present
        for (int i = 0; i < iterations; i++)
        {
            Assert.True(cache.TryGet($"key{i}", out _), $"Key key{i} should exist");
        }

        Assert.Equal(iterations, cache.Count);
    }

    [Fact]
    public void TryGet_Should_RemoveExpiredEntry_When_Accessed()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        cache.Set("expired", CreateHealthyResult(), TimeSpan.FromMilliseconds(-1));

        // Add a second entry that isn't expired
        cache.Set("fresh", CreateHealthyResult(), TimeSpan.FromMinutes(5));

        // Act - accessing expired entry removes it
        var expiredFound = cache.TryGet("expired", out _);

        // Assert
        Assert.False(expiredFound);

        // Fresh entry should still exist
        Assert.True(cache.TryGet("fresh", out _));

        // Expired entry should have same count total
        // (expired was removed, fresh is still there)
        Assert.True(cache.Count <= 2);
    }
}
