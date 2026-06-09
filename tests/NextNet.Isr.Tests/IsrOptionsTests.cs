namespace NextNet.Isr.Tests;

public class IsrOptionsTests
{
    [Fact]
    public void DefaultValues_Should_HaveCorrectDefaults()
    {
        var options = new IsrOptions();

        Assert.Null(options.Revalidate);
        Assert.Null(options.RevalidateTags);
        Assert.Equal(1, options.MaxConcurrentRegenerations);
        Assert.True(options.ServeStaleWhileRevalidate);
    }

    [Fact]
    public void Validate_Should_Throw_When_RevalidateIsNegative()
    {
        var options = new IsrOptions { Revalidate = -1 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_Should_NotThrow_When_RevalidateIsZero()
    {
        var options = new IsrOptions { Revalidate = 0 };
        options.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_Should_NotThrow_When_RevalidateIsNull()
    {
        var options = new IsrOptions { Revalidate = null };
        options.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_Should_Throw_When_MaxConcurrentIsZero()
    {
        var options = new IsrOptions { MaxConcurrentRegenerations = 0 };
        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Validate_Should_NotThrow_When_OptionsAreValid()
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
