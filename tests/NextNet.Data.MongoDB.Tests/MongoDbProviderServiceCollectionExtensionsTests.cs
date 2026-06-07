namespace NextNet.Data.MongoDB.Tests;

/// <summary>
/// Tests for <see cref="MongoDbNextNetDataExtensions"/>.
/// </summary>
public sealed class MongoDbProviderServiceCollectionExtensionsTests
{
    [Fact]
    public void UseMongoDB_ShouldThrow_WhenBuilderNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            MongoDbNextNetDataExtensions.UseMongoDB(null!));
    }

    [Fact]
    public void UseMongoDB_ShouldReturnSameBuilder()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        var result = builder.UseMongoDB();
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseMongoDB_ShouldRegisterMongoDbOptions()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        builder.UseMongoDB(options =>
        {
            options.ConnectionName = "Test";
            options.DefaultDatabaseName = "testdb";
        });

        var serviceProvider = BuildServiceProvider(services, builder);
        var options = serviceProvider.GetService<MongoDbOptions>();
        Assert.NotNull(options);
        Assert.Equal("Test", options!.ConnectionName);
        Assert.Equal("testdb", options.DefaultDatabaseName);
    }

    [Fact]
    public void UseMongoDB_ShouldRegisterMongoClientManager()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseMongoDB();

        var serviceProvider = BuildServiceProvider(services, builder);
        var manager = serviceProvider.GetService<MongoClientManager>();
        Assert.NotNull(manager);
    }

    [Fact]
    public void UseMongoDB_ShouldRegisterHealthCheck()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseMongoDB(options => options.RegisterHealthChecks = true);

        var serviceProvider = BuildServiceProvider(services, builder);
        var healthCheck = serviceProvider.GetService<IHealthCheckProvider>();
        Assert.NotNull(healthCheck);
        Assert.IsType<MongoDbHealthCheck>(healthCheck);
    }

    [Fact]
    public void UseMongoDB_ShouldRegisterMigrationEngine()
    {
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseMongoDB();

        var serviceProvider = BuildServiceProvider(services, builder);
        var engine = serviceProvider.GetService<IMigrationEngine>();
        Assert.NotNull(engine);
        Assert.IsType<MongoDbMigrationEngine>(engine);
    }

    private static ServiceProvider BuildServiceProvider(IServiceCollection services, NextNetDataBuilder builder)
    {
        builder.Build();
        return services.BuildServiceProvider();
    }
}
