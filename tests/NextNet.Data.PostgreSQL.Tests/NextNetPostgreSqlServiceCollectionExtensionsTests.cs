namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="NextNetPostgreSqlServiceCollectionExtensions"/> registration methods.
/// </summary>
public sealed class NextNetPostgreSqlServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetPostgreSql_Should_RegisterPostgresConnectionFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetPostgreSql();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<PostgresConnectionFactory>();
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddNextNetPostgreSql_Should_RegisterOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetPostgreSql();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<PostgresConnectionFactoryOptions>>();
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddNextNetPostgreSql_Should_ApplyConfiguration_When_ConfigureProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetPostgreSql(options =>
        {
            options.Host = "db.example.com";
            options.Database = "testdb";
            options.Username = "testuser";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<PostgresConnectionFactoryOptions>>();
        Assert.Equal("db.example.com", options.Value.Host);
        Assert.Equal("testdb", options.Value.Database);
        Assert.Equal("testuser", options.Value.Username);
    }

    [Fact]
    public void AddNextNetPostgreSql_Should_Throw_When_ServicesNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            NextNetPostgreSqlServiceCollectionExtensions.AddNextNetPostgreSql(null!));
    }

    [Fact]
    public void AddNextNetPostgreSql_Should_RegisterFactoryAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddNextNetPostgreSql();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var factory1 = serviceProvider.GetRequiredService<PostgresConnectionFactory>();
        var factory2 = serviceProvider.GetRequiredService<PostgresConnectionFactory>();

        // Assert - same instance (singleton)
        Assert.Same(factory1, factory2);
    }

    [Fact]
    public void AddNextNetPostgreSql_Should_RegisterRawOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetPostgreSql(options =>
        {
            options.ConnectionString = "Host=test;Database=test";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rawOptions = serviceProvider.GetService<PostgresConnectionFactoryOptions>();
        Assert.NotNull(rawOptions);
        Assert.Equal("Host=test;Database=test", rawOptions.ConnectionString);
    }
}
