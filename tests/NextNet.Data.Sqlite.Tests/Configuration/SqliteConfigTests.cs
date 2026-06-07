using System.Text.Json;
using NextNet.Data.Sqlite.Configuration;

namespace NextNet.Data.Sqlite.Tests.Configuration;

/// <summary>
/// Tests for <see cref="SqliteConfig"/> JSON serialization and deserialization.
/// </summary>
public sealed class SqliteConfigTests
{
    [Fact]
    public void Serialize_Should_RoundTrip_When_AllPropertiesSet()
    {
        // Arrange
        var config = new SqliteConfig
        {
            DataSource = "app.db",
            InMemory = false,
            Cache = "Shared"
        };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<SqliteConfig>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(config.DataSource, deserialized.DataSource);
        Assert.Equal(config.InMemory, deserialized.InMemory);
        Assert.Equal(config.Cache, deserialized.Cache);
    }

    [Fact]
    public void Serialize_Should_UseJsonPropertyNames()
    {
        // Arrange
        var config = new SqliteConfig
        {
            DataSource = "app.db",
            InMemory = true,
            Cache = "Shared"
        };

        // Act
        var json = JsonSerializer.Serialize(config);

        // Assert
        Assert.Contains("\"dataSource\"", json);
        Assert.Contains("\"inMemory\"", json);
        Assert.Contains("\"cache\"", json);
    }

    [Fact]
    public void Deserialize_Should_ApplyDefaults_When_PropertiesOmitted()
    {
        // Arrange
        var json = "{}";

        // Act
        var config = JsonSerializer.Deserialize<SqliteConfig>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Null(config.DataSource);
        Assert.False(config.InMemory);
        Assert.Null(config.Cache);
    }

    [Fact]
    public void Deserialize_Should_PopulateFrom_When_AllPropertiesPresent()
    {
        // Arrange
        var json = """
        {
            "dataSource": "myapp.db",
            "inMemory": true,
            "cache": "Private"
        }
        """;

        // Act
        var config = JsonSerializer.Deserialize<SqliteConfig>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("myapp.db", config.DataSource);
        Assert.True(config.InMemory);
        Assert.Equal("Private", config.Cache);
    }

    [Fact]
    public void Deserialize_Should_Handle_When_CacheIsMissing()
    {
        // Arrange
        var json = """
        {
            "dataSource": "app.db",
            "inMemory": false
        }
        """;

        // Act
        var config = JsonSerializer.Deserialize<SqliteConfig>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("app.db", config.DataSource);
        Assert.False(config.InMemory);
        Assert.Null(config.Cache);
    }
}
