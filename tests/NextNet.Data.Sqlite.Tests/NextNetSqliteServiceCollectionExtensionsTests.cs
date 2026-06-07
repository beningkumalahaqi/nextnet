using NextNet.Data.Sqlite;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for <see cref="NextNetSqliteServiceCollectionExtensions"/>.
/// </summary>
public sealed class NextNetSqliteServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetSqlite_Should_RegisterSqliteConnectionFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetSqlite();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<SqliteConnectionFactory>();
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddNextNetSqlite_Should_RegisterOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetSqlite(options =>
        {
            options.DataSource = "test.db";
            options.Cache = SqliteCacheMode.Shared;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<SqliteConnectionFactoryOptions>>();
        Assert.NotNull(options);
        Assert.Equal("test.db", options.Value.DataSource);
        Assert.Equal(SqliteCacheMode.Shared, options.Value.Cache);
    }

    [Fact]
    public void AddNextNetSqlite_Should_ReturnServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetSqlite();

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddNextNetSqlite_Should_Throw_When_ServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetSqlite());
    }

    [Fact]
    public void AddNextNetSqlite_Should_RegisterDefaultOptions_When_NoConfigure()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetSqlite();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<SqliteConnectionFactoryOptions>>();
        Assert.NotNull(options);
        Assert.Null(options.Value.ConnectionString);
        Assert.False(options.Value.InMemory);
    }
}
