using NextNet.Templates.Exceptions;
using Xunit;

namespace NextNet.Templates.Tests.Exceptions;

public sealed class TemplateGenerationExceptionTests
{
    [Fact]
    public void Constructor_Should_SetErrorCode_ToSK103()
    {
        // Arrange & Act
        var ex = new TemplateGenerationException("Something went wrong");

        // Assert
        Assert.Equal("NN-103", ex.ErrorCode);
    }

    [Fact]
    public void Constructor_Should_StoreFilePath_When_Provided()
    {
        // Arrange & Act
        var ex = new TemplateGenerationException("Failed to process template syntax", "Controllers/WeatherController.cs");

        // Assert
        Assert.Equal("Controllers/WeatherController.cs", ex.FilePath);
    }

    [Fact]
    public void Constructor_Should_HandleNullFilePath()
    {
        // Arrange & Act
        var ex = new TemplateGenerationException("Something went wrong");

        // Assert
        Assert.Null(ex.FilePath);
    }

    [Fact]
    public void Constructor_Should_FormatMessage_WithFilePath()
    {
        // Arrange & Act
        var ex = new TemplateGenerationException("Failed to process template syntax", "Controllers/WeatherController.cs");

        // Assert
        Assert.Equal("Template generation failed for 'Controllers/WeatherController.cs': Failed to process template syntax", ex.Message);
    }

    [Fact]
    public void Constructor_Should_FormatMessage_WithoutFilePath()
    {
        // Arrange & Act
        var ex = new TemplateGenerationException("Something went wrong");

        // Assert
        Assert.Equal("Template generation failed: Something went wrong", ex.Message);
    }
}
