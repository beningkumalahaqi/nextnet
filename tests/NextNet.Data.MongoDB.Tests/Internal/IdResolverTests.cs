using NextNet.Data.MongoDB.Tests.Fixtures;

namespace NextNet.Data.MongoDB.Tests.Internal;

/// <summary>
/// Tests for <see cref="IdResolver"/>.
/// </summary>
public sealed class IdResolverTests
{
    [Fact]
    public void For_ShouldFindBsonIdAttribute()
    {
        var info = IdResolver.For<TestEntity>();
        Assert.NotNull(info);
        Assert.Equal("Id", info!.PropertyName);
        Assert.Equal(typeof(string), info.PropertyType);
        Assert.True(info.HasStringObjectIdRepresentation);
    }

    [Fact]
    public void For_ShouldFindObjectIdProperty()
    {
        var info = IdResolver.For<TestEntityWithObjectId>();
        Assert.NotNull(info);
        Assert.Equal("Id", info!.PropertyName);
        Assert.Equal(typeof(ObjectId), info.PropertyType);
        Assert.True(info.IsObjectId);
    }

    [Fact]
    public void For_ShouldFindIntIdProperty()
    {
        var info = IdResolver.For<TestEntityWithIntId>();
        Assert.NotNull(info);
        Assert.Equal("Id", info!.PropertyName);
        Assert.Equal(typeof(int), info.PropertyType);
        Assert.False(info.IsObjectId);
        Assert.False(info.IsString);
    }

    [Fact]
    public void For_ShouldReturnNull_WhenNoIdFound()
    {
        // A type with no ID-like property
        var info = IdResolver.For<NoIdEntity>();
        Assert.Null(info);
    }

    [Fact]
    public void For_ShouldBeThreadSafe()
    {
        var results = new IdPropertyInfo?[100];
        var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
        Parallel.For(0, 100, options, i =>
        {
            results[i] = IdResolver.For<TestEntity>();
        });

        Assert.All(results, r => Assert.NotNull(r));
    }

    private sealed class NoIdEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
