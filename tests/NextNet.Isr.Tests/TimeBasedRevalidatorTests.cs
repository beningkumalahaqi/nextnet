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
    public void IsStale_Should_ReturnTrue_When_AgeExceedsInterval()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(61);

        Assert.True(_revalidator.IsStale(generatedAt, 60, now));
    }

    [Fact]
    public void IsStale_Should_ReturnFalse_When_AgeWithinInterval()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(30);

        Assert.False(_revalidator.IsStale(generatedAt, 60, now));
    }

    [Fact]
    public void IsStale_Should_ReturnFalse_When_IntervalIsZero()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddDays(365);

        Assert.False(_revalidator.IsStale(generatedAt, 0, now));
    }

    [Fact]
    public void IsStale_Should_UseGlobalDefault_When_IntervalIsNull()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(61);

        Assert.True(_revalidator.IsStale(generatedAt, null, now));
    }

    [Fact]
    public void IsStale_Should_UseUtcNow_When_NowIsNull()
    {
        var generatedAt = DateTime.UtcNow.AddSeconds(-120);

        Assert.True(_revalidator.IsStale(generatedAt, 60, null));
    }

    [Fact]
    public void IsStale_Should_ReturnFalse_When_IntervalIsNegative()
    {
        var generatedAt = DateTime.UtcNow.AddDays(-1);

        Assert.False(_revalidator.IsStale(generatedAt, -1, DateTime.UtcNow));
    }

    [Fact]
    public void GetTtlSeconds_Should_ReturnCorrectRemainingTime_When_Called()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(10);

        var ttl = _revalidator.GetTtlSeconds(generatedAt, 60, now);

        Assert.Equal(50, ttl, 1); // 50 seconds remaining
    }

    [Fact]
    public void GetTtlSeconds_Should_ReturnZero_When_Stale()
    {
        var generatedAt = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);
        var now = generatedAt.AddSeconds(120);

        var ttl = _revalidator.GetTtlSeconds(generatedAt, 60, now);

        Assert.Equal(0, ttl);
    }

    [Fact]
    public void GetTtlSeconds_Should_ReturnMaxValue_When_IntervalIsZero()
    {
        var generatedAt = DateTime.UtcNow;

        var ttl = _revalidator.GetTtlSeconds(generatedAt, 0);

        Assert.Equal(double.MaxValue, ttl);
    }
}
