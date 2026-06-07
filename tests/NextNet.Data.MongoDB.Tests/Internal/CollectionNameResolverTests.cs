using NextNet.Data.MongoDB.Tests.Fixtures;

namespace NextNet.Data.MongoDB.Tests.Internal;

/// <summary>
/// Tests for <see cref="CollectionNameResolver"/>.
/// </summary>
public sealed class CollectionNameResolverTests
{
    [Fact]
    public void Resolve_ShouldUsePluralizedCamelCase_WhenNoOptionsOrAttribute()
    {
        var name = CollectionNameResolver.Resolve<TestEntity>();
        Assert.Equal("testEntities", name);
    }

    [Fact]
    public void Resolve_ShouldUseCollectionNameAttribute_WhenPresent()
    {
        var name = CollectionNameResolver.Resolve<TestEntityWithCollectionAttribute>();
        Assert.Equal("custom_test_collection", name);
    }

    [Fact]
    public void Resolve_ShouldUseOptionsOverride_WhenProvided()
    {
        var options = new MongoDbRepositoryOptions { CollectionName = "my_custom_collection" };
        var name = CollectionNameResolver.Resolve<TestEntity>(options);
        Assert.Equal("my_custom_collection", name);
    }

    [Fact]
    public void Resolve_ShouldPreferOptionsOverAttribute()
    {
        var options = new MongoDbRepositoryOptions { CollectionName = "option_override" };
        var name = CollectionNameResolver.Resolve<TestEntityWithCollectionAttribute>(options);
        Assert.Equal("option_override", name);
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenTypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => CollectionNameResolver.Resolve(null!));
    }

    [Fact]
    public void Resolve_ShouldUseTypeName_ForNonAttributedType()
    {
        var name = CollectionNameResolver.Resolve<TestEntityWithObjectId>();
        Assert.Equal("testEntityWithObjectIds", name);
    }
}
