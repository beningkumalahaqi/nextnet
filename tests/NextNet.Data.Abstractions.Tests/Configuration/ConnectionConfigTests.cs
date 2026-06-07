using System.Text.Json;
using NextNet.Data.Abstractions.Configuration;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Configuration;

public class ConnectionConfigTests
{
    [Fact]
    public void Constructor_Should_SetRequiredProperties()
    {
        // Arrange & Act
        var config = new ConnectionConfig("Server=.;Database=Test;");

        // Assert
        Assert.Equal("Server=.;Database=Test;", config.ConnectionString);
    }

    [Fact]
    public void Constructor_Should_ApplyDefaults_When_OptionalParametersOmitted()
    {
        // Arrange & Act
        var config = new ConnectionConfig("Server=.;Database=Test;");

        // Assert
        Assert.Equal("EntityFramework", config.Provider);
        Assert.Equal(30, config.TimeoutSeconds);
        Assert.True(config.Enabled);
    }

    [Fact]
    public void Constructor_Should_SetAllProperties_When_Provided()
    {
        // Arrange & Act
        var config = new ConnectionConfig(
            "Server=.;Database=Test;",
            "Dapper",
            60,
            false);

        // Assert
        Assert.Equal("Server=.;Database=Test;", config.ConnectionString);
        Assert.Equal("Dapper", config.Provider);
        Assert.Equal(60, config.TimeoutSeconds);
        Assert.False(config.Enabled);
    }

    [Fact]
    public void Serialize_Should_UseJsonPropertyNames()
    {
        // Arrange
        var config = new ConnectionConfig("Server=.;Database=Test;");

        // Act
        var json = JsonSerializer.Serialize(config);

        // Assert
        Assert.Contains("connectionString", json);
        Assert.DoesNotContain("ConnectionString", json);
        Assert.Contains("provider", json);
        Assert.Contains("timeoutSeconds", json);
        Assert.Contains("enabled", json);
    }

    [Fact]
    public void Serialize_Should_RoundTrip_When_AllPropertiesSet()
    {
        // Arrange
        var config = new ConnectionConfig("Server=.;Database=Test;", "MongoDB", 120, false);

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ConnectionConfig>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(config.ConnectionString, deserialized.ConnectionString);
        Assert.Equal(config.Provider, deserialized.Provider);
        Assert.Equal(config.TimeoutSeconds, deserialized.TimeoutSeconds);
        Assert.Equal(config.Enabled, deserialized.Enabled);
    }
}
