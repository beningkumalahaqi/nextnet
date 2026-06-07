using NextNet.Data.Sqlite.Internal;

namespace NextNet.Data.Sqlite.Tests.Internal;

/// <summary>
/// Tests for <see cref="ConnectionStringResolver"/> resolution priority chain.
/// </summary>
public sealed class ConnectionStringResolverTests
{
    [Fact]
    public void Resolve_Should_ReturnExplicitConnectionString_When_Provided()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            ConnectionString = "Data Source=options.db"
        };
        var resolver = new ConnectionStringResolver(options, "Data Source=explicit.db");

        // Act
        var result = resolver.Resolve();

        // Assert
        // Explicit constructor parameter takes priority
        Assert.Contains("explicit.db", result);
    }

    [Fact]
    public void Resolve_Should_UseOptionsConnectionString_When_NoExplicit()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            ConnectionString = "Data Source=options.db"
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("options.db", result);
    }

    [Fact]
    public void Resolve_Should_BuildFromDataSource_When_NoConnectionString()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = "mydata.db"
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("mydata.db", result);
        Assert.Contains("Data Source=", result);
    }

    [Fact]
    public void Resolve_Should_ReturnInMemory_When_InMemoryFlag()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            InMemory = true
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains(":memory:", result);
    }

    [Fact]
    public void Resolve_Should_FallbackToDefault_When_NothingConfigured()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Data Source=", result);
        Assert.Contains("database.db", result);
    }

    [Fact]
    public void Resolve_Should_CacheResult_When_CalledMultipleTimes()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = "cached.db"
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var first = resolver.Resolve();
        var second = resolver.Resolve();

        // Assert
        Assert.Equal(first, second);
        Assert.Contains("cached.db", first);
    }

    [Fact]
    public void Resolve_Should_UseExplicitOverOptionsDataSource()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = "options.db"
        };
        var resolver = new ConnectionStringResolver(options, "Data Source=explicit.db");

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("explicit.db", result);
        Assert.DoesNotContain("options.db", result);
    }

    [Fact]
    public void Resolve_Should_BuildFromDataSourceWithCache_When_CacheSpecified()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = "app.db",
            Cache = SqliteCacheMode.Shared
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("app.db", result);
        Assert.Contains("Cache=Shared", result);
    }
}
