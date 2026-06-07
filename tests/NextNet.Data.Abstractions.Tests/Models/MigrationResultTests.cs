using NextNet.Data.Abstractions.Models;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Models;

public class MigrationResultTests
{
    [Fact]
    public void Constructor_Should_SetAllProperties()
    {
        // Arrange
        var errors = new[] { "Syntax error in migration script." };

        // Act
        var result = new MigrationResult(
            true,
            "Migration created successfully.",
            0,
            "AddUserTable",
            errors);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Migration created successfully.", result.Message);
        Assert.Equal(0, result.MigrationsApplied);
        Assert.Equal("AddUserTable", result.MigrationName);
        Assert.Same(errors, result.Errors);
    }

    [Fact]
    public void Constructor_Should_AllowDefaults_When_OptionalParametersOmitted()
    {
        // Arrange & Act
        var result = new MigrationResult(true, "Success");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Success", result.Message);
        Assert.Equal(0, result.MigrationsApplied);
        Assert.Null(result.MigrationName);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Constructor_Should_ReportMigrationsApplied()
    {
        // Arrange & Act
        var result = new MigrationResult(true, "Applied 3 migrations", 3);

        // Assert
        Assert.Equal(3, result.MigrationsApplied);
        Assert.True(result.Success);
    }

    [Fact]
    public void Constructor_Should_StoreErrors_When_Present()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = new MigrationResult(false, "Migration failed", Errors: errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(2, result.Errors?.Count);
    }
}
