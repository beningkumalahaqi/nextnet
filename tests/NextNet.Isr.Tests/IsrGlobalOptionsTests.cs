namespace NextNet.Isr.Tests;

public class IsrGlobalOptionsTests
{
    [Fact]
    public void DefaultValues()
    {
        var options = new IsrGlobalOptions();

        Assert.Equal(60, options.DefaultRevalidateSeconds);
        Assert.Equal(4, options.MaxConcurrentRegenerations);
        Assert.Equal(100, options.MaxPendingRevalidations);
        Assert.Equal(30, options.DeduplicationWindowSeconds);
        Assert.Null(options.RevalidationSecret);
        Assert.Null(options.WebhookSecret);
        Assert.True(options.ServeStaleWhileRevalidate);
    }

    [Fact]
    public void Validate_WithNegativeDefaultRevalidate_Throws()
    {
        var options = new IsrGlobalOptions { DefaultRevalidateSeconds = -1 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithZeroMaxConcurrent_Throws()
    {
        var options = new IsrGlobalOptions { MaxConcurrentRegenerations = 0 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithZeroMaxPending_Throws()
    {
        var options = new IsrGlobalOptions { MaxPendingRevalidations = 0 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithNegativeDeduplicationWindow_Throws()
    {
        var options = new IsrGlobalOptions { DeduplicationWindowSeconds = -1 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        var options = new IsrGlobalOptions
        {
            DefaultRevalidateSeconds = 120,
            MaxConcurrentRegenerations = 8,
            MaxPendingRevalidations = 200,
            DeduplicationWindowSeconds = 60,
            RevalidationSecret = "my-secret",
            WebhookSecret = "whsec_test"
        };
        options.Validate(); // Should not throw
    }
}
