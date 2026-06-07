namespace NextNet.Isr.Tests;

public class IsrOptionsTests
{
    [Fact]
    public void DefaultValues()
    {
        var options = new IsrOptions();

        Assert.Null(options.Revalidate);
        Assert.Null(options.RevalidateTags);
        Assert.Equal(1, options.MaxConcurrentRegenerations);
        Assert.True(options.ServeStaleWhileRevalidate);
    }

    [Fact]
    public void Validate_WithNegativeRevalidate_Throws()
    {
        var options = new IsrOptions { Revalidate = -1 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithZeroRevalidate_DoesNotThrow()
    {
        var options = new IsrOptions { Revalidate = 0 };
        options.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_WithNullRevalidate_DoesNotThrow()
    {
        var options = new IsrOptions { Revalidate = null };
        options.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_WithZeroMaxConcurrent_Throws()
    {
        var options = new IsrOptions { MaxConcurrentRegenerations = 0 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        var options = new IsrOptions
        {
            Revalidate = 300,
            RevalidateTags = new[] { "blog" },
            MaxConcurrentRegenerations = 2,
            ServeStaleWhileRevalidate = false
        };
        options.Validate(); // Should not throw
    }
}
