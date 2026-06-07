using NextNet.Templates.Abstractions;
using Xunit;

namespace NextNet.Templates.Tests.Abstractions;

public class GenerationResultTests
{
    [Fact]
    public void Constructor_Should_SetGeneratedFiles()
    {
        // Arrange
        var generated = new[] { "Program.cs", "Startup.cs" };
        var skipped = Array.Empty<string>();
        var warnings = Array.Empty<string>();
        var elapsed = TimeSpan.FromMilliseconds(100);

        // Act
        var result = new GenerationResult(generated, skipped, warnings, elapsed);

        // Assert
        Assert.Equal(generated, result.GeneratedFiles);
        Assert.Equal(skipped, result.SkippedFiles);
        Assert.Equal(warnings, result.Warnings);
        Assert.Equal(elapsed, result.Elapsed);
    }
}
