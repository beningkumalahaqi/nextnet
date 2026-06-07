namespace NextNet.Data.MultiDb.Tests;

public class ConnectionConfigExtensionsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void WithPoolSize_Should_SetPoolSize()
    {
        var config = new ConnectionConfig("Server=test;");
        var updated = config.WithPoolSize(20);

        Assert.Equal(20, updated.PoolSize);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithPoolSize_Should_Throw_When_ConfigIsNull()
    {
        ConnectionConfig? config = null;
        Assert.Throws<ArgumentNullException>(() => config!.WithPoolSize(10));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithPoolSize_Should_Throw_When_PoolSizeIsZero()
    {
        var config = new ConnectionConfig("Server=test;");
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithPoolSize(0));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithPoolSize_Should_Throw_When_PoolSizeIsNegative()
    {
        var config = new ConnectionConfig("Server=test;");
        Assert.Throws<ArgumentOutOfRangeException>(() => config.WithPoolSize(-1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithTags_Should_SetTags()
    {
        var config = new ConnectionConfig("Server=test;");
        var updated = config.WithTags("readonly", "reporting");

        Assert.NotNull(updated.Tags);
        Assert.Contains("readonly", updated.Tags);
        Assert.Contains("reporting", updated.Tags);
        Assert.Equal(2, updated.Tags.Count);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithTags_Should_IgnoreNull()
    {
        var config = new ConnectionConfig("Server=test;");
        var updated = config.WithTags(null!);

        Assert.Null(updated.Tags);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithTags_Should_IgnoreEmptyTags()
    {
        var config = new ConnectionConfig("Server=test;");
        var updated = config.WithTags("valid", "", null!);

        Assert.NotNull(updated.Tags);
        Assert.Single(updated.Tags);
        Assert.Contains("valid", updated.Tags);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithTags_Should_Throw_When_ConfigIsNull()
    {
        ConnectionConfig? config = null;
        Assert.Throws<ArgumentNullException>(() => config!.WithTags("tag1"));
    }
}
