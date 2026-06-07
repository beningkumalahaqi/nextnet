using NextNet.Data.MultiDb.Internal;
using NextNet.Data.MultiDb.Tests.Fixtures;

namespace NextNet.Data.MultiDb.Tests;

public class ConnectionPoolRegistryTests
{
    private static ConnectionPoolEntry CreateEntry(string name, string provider = "TestProvider", string cs = "Server=test;")
    {
        return new ConnectionPoolEntry(
            ConnectionName: name,
            ProviderName: provider,
            ConnectionString: cs,
            Provider: new FakeDataProvider(name));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_StorePoolEntry()
    {
        var registry = new ConnectionPoolRegistry();
        var entry = CreateEntry("Analytics");

        registry.Register("Analytics", entry);

        var retrieved = registry.Get("Analytics");
        Assert.NotNull(retrieved);
        Assert.Equal("Analytics", retrieved.ConnectionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Get_Should_ReturnEntry_When_Exists()
    {
        var registry = new ConnectionPoolRegistry();
        var entry = CreateEntry("Primary");
        registry.Register("Primary", entry);

        var result = registry.Get("Primary");

        Assert.Same(entry, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Get_Should_Throw_When_Missing()
    {
        var registry = new ConnectionPoolRegistry();

        var ex = Assert.Throws<KeyNotFoundException>(() => registry.Get("NonExistent"));
        Assert.Contains("NonExistent", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TryGet_Should_ReturnTrueAndEntry_When_Exists()
    {
        var registry = new ConnectionPoolRegistry();
        registry.Register("Test", CreateEntry("Test"));

        var found = registry.TryGet("Test", out var entry);

        Assert.True(found);
        Assert.NotNull(entry);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TryGet_Should_ReturnFalse_When_Missing()
    {
        var registry = new ConnectionPoolRegistry();

        var found = registry.TryGet("Missing", out var entry);

        Assert.False(found);
        Assert.Null(entry);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Remove_Should_RemoveEntry()
    {
        var registry = new ConnectionPoolRegistry();
        registry.Register("Test", CreateEntry("Test"));

        var removed = registry.Remove("Test");

        Assert.True(removed);
        Assert.Throws<KeyNotFoundException>(() => registry.Get("Test"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Remove_Should_ReturnFalse_When_Missing()
    {
        var registry = new ConnectionPoolRegistry();

        var removed = registry.Remove("NonExistent");

        Assert.False(removed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Names_Should_ReturnAllKeys()
    {
        var registry = new ConnectionPoolRegistry();
        registry.Register("A", CreateEntry("A"));
        registry.Register("B", CreateEntry("B"));

        var names = registry.Names;

        Assert.Equal(2, names.Count);
        Assert.Contains("A", names);
        Assert.Contains("B", names);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DisposeAll_Should_ClearRegistry()
    {
        var registry = new ConnectionPoolRegistry();
        registry.Register("Test", CreateEntry("Test"));

        registry.DisposeAll();

        Assert.Equal(0, registry.Count);
        Assert.Throws<ObjectDisposedException>(() => registry.Get("Test"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_Throw_When_NameIsNull()
    {
        var registry = new ConnectionPoolRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Register(null!, CreateEntry("Test")));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_Throw_When_EntryIsNull()
    {
        var registry = new ConnectionPoolRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Register("Test", null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dispose_Should_CallDisposeAll()
    {
        var registry = new ConnectionPoolRegistry();
        registry.Register("Test", CreateEntry("Test"));

        registry.Dispose();

        Assert.Equal(0, registry.Count);
    }
}
