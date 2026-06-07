using NextNet.Data.Abstractions.Configuration;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Configuration;

public class DataConfigValidatorTests
{
    private readonly DataConfigValidator _validator = new();

    [Fact]
    public void Validate_Should_ReturnEmpty_When_ConfigIsValid()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new("Server=.;Database=Test;")
        };
        var config = new DataConfig(
            "Default",
            connections,
            new MigrationConfig(),
            new ScaffoldingConfig());

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Should_Throw_When_ConfigIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_DefaultConnectionIsEmpty()
    {
        // Arrange
        var config = new DataConfig(DefaultConnection: "");

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("DefaultConnection"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_DefaultConnectionIsWhitespace()
    {
        // Arrange
        var config = new DataConfig(DefaultConnection: "   ");

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("DefaultConnection"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ConnectionStringIsEmpty()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new(ConnectionString: "")
        };
        var config = new DataConfig("Default", connections);

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("ConnectionString"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_TimeoutOutOfRange()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new("Server=.;Database=Test;", TimeoutSeconds: 5000)
        };
        var config = new DataConfig("Default", connections);

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("TimeoutSeconds") && e.Contains("5000"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_TimeoutIsZero()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new("Server=.;Database=Test;", TimeoutSeconds: 0)
        };
        var config = new DataConfig("Default", connections);

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("TimeoutSeconds"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_DefaultConnectionMissingFromConnections()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Analytics"] = new("Server=.;Database=Analytics;")
        };
        var config = new DataConfig("Default", connections);

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("DefaultConnection") && e.Contains("Default"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ProviderIsEmpty()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            ["Default"] = new("Server=.;Database=Test;", Provider: "")
        };
        var config = new DataConfig("Default", connections);

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("Provider"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_MigrationDirectoryIsEmpty()
    {
        // Arrange
        var config = new DataConfig(
            Migration: new MigrationConfig(Directory: ""));

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("Migration") && e.Contains("Directory"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_MigrationTimeoutIsZero()
    {
        // Arrange
        var config = new DataConfig(
            Migration: new MigrationConfig(TimeoutSeconds: 0));

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("Migration") && e.Contains("TimeoutSeconds"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ScaffoldingModelsNamespaceIsEmpty()
    {
        // Arrange
        var config = new DataConfig(
            Scaffolding: new ScaffoldingConfig(ModelsNamespace: ""));

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("ModelsNamespace"));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ScaffoldingRepositoriesNamespaceIsEmpty()
    {
        // Arrange
        var config = new DataConfig(
            Scaffolding: new ScaffoldingConfig(RepositoriesNamespace: ""));

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Contains("RepositoriesNamespace"));
    }

    [Fact]
    public void Validate_Should_ReturnMultipleErrors_When_ConfigHasMultipleIssues()
    {
        // Arrange
        var connections = new Dictionary<string, ConnectionConfig>
        {
            [""] = new("", "EntityFramework", 0, true)
        };
        var config = new DataConfig(
            "",
            connections,
            new MigrationConfig(Directory: ""),
            new ScaffoldingConfig(ModelsNamespace: ""));

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.True(errors.Count >= 4, $"Expected at least 4 errors, got {errors.Count}");
    }

    [Fact]
    public void Validate_Should_ReturnEmpty_When_ConnectionsIsNull()
    {
        // Arrange
        var config = new DataConfig(DefaultConnection: "Default", Connections: null);

        // Act
        var errors = _validator.Validate(config);

        // Assert
        Assert.Empty(errors);
    }
}
