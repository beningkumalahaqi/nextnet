using NextNet.Data.Internal;
using NextNet.Data.MultiDb.Internal;
using NextNet.Data.MultiDb.Tests.Fixtures;

namespace NextNet.Data.MultiDb.Tests;

public class MultiDbServiceCollectionExtensionsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabaseSelector_Should_RegisterServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = new NextNetDataBuilder(services);

        builder.WithDatabaseSelector();

        var sp = services.BuildServiceProvider();

        var selector = sp.GetService<IDatabaseSelector>();
        Assert.NotNull(selector);

        var nameRegistry = sp.GetService<ConnectionNameRegistry>();
        Assert.NotNull(nameRegistry);

        var poolRegistry = sp.GetService<ConnectionPoolRegistry>();
        Assert.NotNull(poolRegistry);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabaseSelector_Should_ConfigureOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = new NextNetDataBuilder(services);

        builder.WithDatabaseSelector(opts =>
        {
            opts.ValidateOnStartup = false;
            opts.CacheContexts = false;
            opts.FallbackToDefault = true;
        });

        var sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<IOptions<MultiDbOptions>>();
        Assert.False(options.Value.ValidateOnStartup);
        Assert.False(options.Value.CacheContexts);
        Assert.True(options.Value.FallbackToDefault);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabaseSelector_Should_RegisterGuard_When_ValidateOnStartup()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = new NextNetDataBuilder(services);

        builder.WithDatabaseSelector(opts => opts.ValidateOnStartup = true);

        var sp = services.BuildServiceProvider();

        var guard = sp.GetService<DatabaseSelectorGuard>();
        Assert.NotNull(guard);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabaseSelector_Should_NotRegisterGuard_When_ValidateOnStartupFalse()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var builder = new NextNetDataBuilder(services);

        builder.WithDatabaseSelector(opts => opts.ValidateOnStartup = false);

        var sp = services.BuildServiceProvider();

        var guard = sp.GetService<DatabaseSelectorGuard>();
        Assert.Null(guard);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabase_Should_Throw_When_NameIsEmpty()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        Assert.Throws<ArgumentException>(() => builder.WithDatabase("", "Server=test;"));
        Assert.Throws<ArgumentException>(() => builder.WithDatabase("   ", "Server=test;"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabase_Should_Throw_When_ConnectionStringIsEmpty()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        Assert.Throws<ArgumentException>(() => builder.WithDatabase("Test", ""));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabase_Should_Throw_When_BuilderIsNull()
    {
        NextNetDataBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.WithDatabase("Test", "Server=test;"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WithDatabaseSelector_Should_Throw_When_BuilderIsNull()
    {
        NextNetDataBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.WithDatabaseSelector());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AddNamedProvider_Should_RegisterConnection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = new NextNetDataBuilder(services);

        builder.AddNamedProvider<FakeDataProvider>("Analytics", "Host=analytics;");

        builder.Build();

        var registry = services.BuildServiceProvider().GetRequiredService<IDataProviderRegistry>();
        var providers = registry.GetAll();
        Assert.NotEmpty(providers);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AddRepository_Should_Throw_When_BuilderIsNull()
    {
        NextNetDataBuilder? builder = null;
        Assert.Throws<ArgumentNullException>(() => builder!.AddRepository<object>("Test"));
    }
}
