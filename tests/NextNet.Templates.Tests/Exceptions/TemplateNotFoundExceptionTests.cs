using NextNet.Templates.Exceptions;
using Xunit;

namespace NextNet.Templates.Tests.Exceptions;

public class TemplateNotFoundExceptionTests
{
    [Fact]
    public void Constructor_Should_SetErrorCode()
    {
        // Arrange & Act
        var ex = new TemplateNotFoundException("my-template");

        // Assert
        Assert.Equal("NN-101", ex.ErrorCode);
    }

    [Fact]
    public void Constructor_Should_FormatMessage_WithAndWithoutVersion()
    {
        // Arrange & Act
        var exWithoutVersion = new TemplateNotFoundException("my-template");
        var exWithVersion = new TemplateNotFoundException("my-template", "2.0.0");

        // Assert
        Assert.Equal("Template 'my-template' was not found.", exWithoutVersion.Message);
        Assert.Equal("Template 'my-template' version '2.0.0' was not found.", exWithVersion.Message);
        Assert.Equal("my-template", exWithoutVersion.TemplateName);
        Assert.Null(exWithoutVersion.Version);
        Assert.Equal("my-template", exWithVersion.TemplateName);
        Assert.Equal("2.0.0", exWithVersion.Version);
    }
}
