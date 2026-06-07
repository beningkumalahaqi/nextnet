using NextNet.Data.MultiDb.Tests.Fixtures;

namespace NextNet.Data.MultiDb.Tests;

public class DatabaseSelectorExtensionsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_WithName_Should_Throw_When_SelectorIsNull()
    {
        IDatabaseSelector? selector = null;
        Assert.Throws<ArgumentNullException>(() => selector!.GetRepository<object>("Test"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_WithoutName_Should_Throw_When_SelectorIsNull()
    {
        IDatabaseSelector? selector = null;
        Assert.Throws<ArgumentNullException>(() => selector!.GetRepository<object>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_WithName_Should_Throw_When_ConnectionNotFound()
    {
        var fakeSelector = new FakeDatabaseSelector();
        fakeSelector.DefaultContext = CreateFakeContext("Default");

        Assert.Throws<MissingConnectionException>(() => fakeSelector.GetRepository<object>("NonExistent"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_WithoutName_Should_Throw_When_NotRegistered()
    {
        var fakeSelector = new FakeDatabaseSelector();
        var provider = new FakeDataProvider("Default");
        var services = new ServiceCollection().BuildServiceProvider();
        var context = new DatabaseContext("Default", "cs", "EF", provider, services);

        // Set Default context but don't register any repository
        fakeSelector.DefaultContext = context;

        Assert.Throws<InvalidOperationException>(() => fakeSelector.GetRepository<object>());
    }

    private static IDatabaseContext CreateFakeContext(string name)
    {
        var provider = new FakeDataProvider(name);
        var services = new ServiceCollection().BuildServiceProvider();
        return new DatabaseContext(
            name, $"Server={name};", "EntityFramework", provider, services);
    }
}
