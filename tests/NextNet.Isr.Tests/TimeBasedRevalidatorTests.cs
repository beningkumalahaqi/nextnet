using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class TimeBasedRevalidatorTests
{
    private readonly IsrGlobalOptions _globalOptions;
    private readonly TimeBasedRevalidator _revalidator;

    public TimeBasedRevalidatorTests()
    {
        _globalOptions = new IsrGlobalOptions { DefaultRevalidateSeconds = 60 };
        _revalidator = new TimeBasedRevalidator(_globalOptions);
    }

    [Fact]
    public void IsStale_AgeExceedsInterval_ReturnsTrue()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(61);

        Assert.True(_revalidator.IsStale(generatedAt, 60, now));
    }

    [Fact]
    public void IsStale_AgeWithinInterval_ReturnsFalse()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(30);

        Assert.False(_revalidator.IsStale(generatedAt, 60, now));
    }

    [Fact]
    public void IsStale_ZeroInterval_NeverStale()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddDays(365);

        Assert.False(_revalidator.IsStale(generatedAt, 0, now));
    }

    [Fact]
    public void IsStale_NullInterval_UsesGlobalDefault()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(61);

        Assert.True(_revalidator.IsStale(generatedAt, null, now));
    }

    [Fact]
    public void IsStale_NullNow_UsesUtcNow()
    {
        var generatedAt = DateTime.UtcNow.AddSeconds(-120);

        Assert.True(_revalidator.IsStale(generatedAt, 60, null));
    }

    [Fact]
    public void IsStale_NegativeInterval_NeverStale()
    {
        var generatedAt = DateTime.UtcNow.AddDays(-1);

        Assert.False(_revalidator.IsStale(generatedAt, -1, DateTime.UtcNow));
    }

    [Fact]
    public void GetTtlSeconds_ReturnsCorrectRemainingTime()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(10);

        var ttl = _revalidator.GetTtlSeconds(generatedAt, 60, now);

        Assert.Equal(50, ttl, 1); // 50 seconds remaining
    }

    [Fact]
    public void GetTtlSeconds_WhenStale_ReturnsZero()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(120);

        var ttl = _revalidator.GetTtlSeconds(generatedAt, 60, now);

        Assert.Equal(0, ttl);
    }

    [Fact]
    public void GetTtlSeconds_ZeroInterval_ReturnsMaxValue()
    {
        var generatedAt = DateTime.UtcNow;

        var ttl = _revalidator.GetTtlSeconds(generatedAt, 0);

        Assert.Equal(double.MaxValue, ttl);
    }
}
