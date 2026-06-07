using System.Text.Json;
using NextNet.Data.PostgreSQL.Configuration;

namespace NextNet.Data.PostgreSQL.Tests.Configuration;

/// <summary>
/// Tests for <see cref="PostgresConfig"/> JSON serialization and deserialization.
/// </summary>
public sealed class PostgresConfigTests
{
    [Fact]
    public void Serialize_Should_RoundTrip_When_AllPropertiesSet()
    {
        // Arrange
        var config = new PostgresConfig
        {
            Host = "db.example.com",
            Port = 5433,
            Database = "testdb",
            Username = "testuser",
            Password = "testpass",
            SslMode = "Require",
            UseDocker = true,
            ContainerName = "my-postgres"
        };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<PostgresConfig>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("db.example.com", deserialized.Host);
        Assert.Equal(5433, deserialized.Port);
        Assert.Equal("testdb", deserialized.Database);
        Assert.Equal("testuser", deserialized.Username);
        Assert.Equal("testpass", deserialized.Password);
        Assert.Equal("Require", deserialized.SslMode);
        Assert.True(deserialized.UseDocker);
        Assert.Equal("my-postgres", deserialized.ContainerName);
    }

    [Fact]
    public void Deserialize_Should_ApplyDefaults_When_PropertiesOmitted()
    {
        // Arrange
        var json = @"{}";

        // Act
        var config = JsonSerializer.Deserialize<PostgresConfig>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Null(config.Host);
        Assert.Null(config.Port);
        Assert.Null(config.Database);
        Assert.Null(config.Username);
        Assert.Null(config.Password);
        Assert.Null(config.SslMode);
        Assert.False(config.UseDocker);
        Assert.Null(config.ContainerName);
    }

    [Fact]
    public void Deserialize_Should_ReadSslMode_When_Provided()
    {
        // Arrange
        var json = @"{""sslMode"": ""VerifyFull""}";

        // Act
        var config = JsonSerializer.Deserialize<PostgresConfig>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("VerifyFull", config.SslMode);
    }

    [Fact]
    public void Serialize_Should_UseJsonPropertyNames()
    {
        // Arrange
        var config = new PostgresConfig
        {
            Host = "localhost",
            Port = 5432,
            Database = "myapp",
            Username = "postgres",
            Password = "secret",
            SslMode = "Prefer",
            UseDocker = true,
            ContainerName = "nextnet-postgres"
        };

        // Act
        var json = JsonSerializer.Serialize(config);

        // Assert the JSON property names match expected conventions
        Assert.Contains("\"host\"", json);
        Assert.Contains("\"port\"", json);
        Assert.Contains("\"database\"", json);
        Assert.Contains("\"username\"", json);
        Assert.Contains("\"password\"", json);
        Assert.Contains("\"sslMode\"", json);
        Assert.Contains("\"useDocker\"", json);
        Assert.Contains("\"containerName\"", json);
    }

    [Fact]
    public void Deserialize_Should_HandleFullConfig()
    {
        // Arrange
        var json = @"
        {
            ""host"": ""pg.example.com"",
            ""port"": 5432,
            ""database"": ""production"",
            ""username"": ""app_user"",
            ""password"": ""encrypted"",
            ""sslMode"": ""VerifyFull"",
            ""useDocker"": false,
            ""containerName"": null
        }";

        // Act
        var config = JsonSerializer.Deserialize<PostgresConfig>(json);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("pg.example.com", config.Host);
        Assert.Equal(5432, config.Port);
        Assert.Equal("production", config.Database);
        Assert.Equal("app_user", config.Username);
        Assert.Equal("encrypted", config.Password);
        Assert.Equal("VerifyFull", config.SslMode);
        Assert.False(config.UseDocker);
        Assert.Null(config.ContainerName);
    }
}
