using NextNet.Templates.Abstractions;
using Xunit;

namespace NextNet.Templates.Tests.Abstractions;

public class ValidationResultTests
{
    [Fact]
    public void Constructor_Should_SetIsValid()
    {
        // Arrange & Act
        var result = new ValidationResult(true);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Errors);
        Assert.Null(result.Warnings);
    }

    [Fact]
    public void Constructor_Should_SetErrors()
    {
        // Arrange
        var errors = new[] { "Error 1" };
        var warnings = new[] { "Warning 1" };

        // Act
        var result = new ValidationResult(false, errors, warnings);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(errors, result.Errors);
        Assert.Equal(warnings, result.Warnings);
    }
}
