using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Models;

public class TemplateFileTests
{
    [Fact]
    public void Constructor_Should_SetSourceAndTarget()
    {
        // Arrange & Act
        var file = new TemplateFile("src/Program.cs", "Program.cs");

        // Assert
        Assert.Equal("src/Program.cs", file.SourcePath);
        Assert.Equal("Program.cs", file.TargetPath);
    }

    [Fact]
    public void IsBinary_Should_DefaultToFalse()
    {
        // Arrange & Act
        var file = new TemplateFile("src/app.exe", "app.exe");

        // Assert
        Assert.False(file.IsBinary);
    }
}
