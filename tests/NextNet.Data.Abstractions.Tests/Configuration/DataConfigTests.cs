using System.Text.Json;
using NextNet.Data.Abstractions.Configuration;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Configuration;

public class DataConfigTests
{
    [Fact]
    public void DefaultConstructor_Should_SetExpectedDefaults()
    {
        // Arrange & Act
        var config = new DataConfig();

        // Assert
        Assert.Equal("Default", config.DefaultConnection);
        Assert.Null(config.Connections);
        Assert.Null(config.Migration);
        Assert.Null(config.Scaffolding);
    }

    [Fact]
    public void ParameterizedConstructor_Should_SetAllProperties()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new("Server=.;Database=Test;")
        };
        var migration = new MigrationConfig(AutoApply: true);
        var scaffolding = new ScaffoldingConfig(ModelsNamespace: "App.Models");

        // Act
        var config = new DataConfig(
            "Primary",
            connections,
            migration,
            scaffolding);

        // Assert
        Assert.Equal("Primary", config.DefaultConnection);
        Assert.Same(connections, config.Connections);
        Assert.Same(migration, config.Migration);
        Assert.Same(scaffolding, config.Scaffolding);
    }

    [Fact]
    public void Serialize_Should_RoundTrip_When_AllPropertiesSet()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new("Server=.;Database=Test;", "EntityFramework", 30, true)
        };
        var config = new DataConfig(
            "Default",
            connections,
            new MigrationConfig(true, "Migrations", "__NextNetMigrations", 60),
            new ScaffoldingConfig("Models", "Repositories", "Actions", "Models", "Repositories", "app/api", false));

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<DataConfig>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(config.DefaultConnection, deserialized.DefaultConnection);
        Assert.NotNull(deserialized.Connections);
        Assert.True(deserialized.Connections.ContainsKey("Default"));
        Assert.Equal(config.Connections!["Default"].ConnectionString, deserialized.Connections["Default"].ConnectionString);
        Assert.NotNull(deserialized.Migration);
        Assert.Equal(config.Migration!.AutoApply, deserialized.Migration.AutoApply);
        Assert.NotNull(deserialized.Scaffolding);
        Assert.Equal(config.Scaffolding!.ModelsNamespace, deserialized.Scaffolding.ModelsNamespace);
    }

    [Fact]
    public void Serialize_Should_UseJsonPropertyNames()
    {
        // Arrange
        var config = new DataConfig();

        // Act
        var json = JsonSerializer.Serialize(config);

        // Assert
        Assert.Contains("defaultConnection", json);
        Assert.DoesNotContain("DefaultConnection", json);
    }
}
