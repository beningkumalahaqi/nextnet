using NextNet.Templates.Exceptions;
using Xunit;

namespace NextNet.Templates.Tests.Exceptions;

public sealed class TemplateValidationExceptionTests
{
    [Fact]
    public void Constructor_Should_SetErrorCode_ToSK102()
    {
        // Arrange & Act
        var ex = new TemplateValidationException(new[] { "error1" });

        // Assert
        Assert.Equal("NN-102", ex.ErrorCode);
    }

    [Fact]
    public void Constructor_Should_StoreValidationErrors()
    {
        // Arrange
        var errors = new List<string> { "Variable 'name' is required.", "Feature 'auth' is missing." };

        // Act
        var ex = new TemplateValidationException(errors);

        // Assert
        Assert.NotNull(ex.ValidationErrors);
        Assert.Equal(2, ex.ValidationErrors.Count);
        Assert.Contains("Variable 'name' is required.", ex.ValidationErrors);
        Assert.Contains("Feature 'auth' is missing.", ex.ValidationErrors);
    }

    [Fact]
    public void Constructor_Should_FormatMessage_WithErrorCount()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };

        // Act
        var ex = new TemplateValidationException(errors);

        // Assert
        Assert.Contains("3", ex.Message);
        Assert.Contains("Error 1", ex.Message);
        Assert.Contains("Error 2", ex.Message);
        Assert.Contains("Error 3", ex.Message);
    }
}
