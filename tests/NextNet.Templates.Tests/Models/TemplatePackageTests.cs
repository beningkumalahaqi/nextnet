using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Models;

public class TemplatePackageTests
{
    [Fact]
    public void Constructor_Should_RequireManifest()
    {
        // Arrange
        var manifest = new TemplateManifest("test", "1.0.0", "3.0.0");

        // Act
        var package = new TemplatePackage(manifest);

        // Assert
        Assert.Same(manifest, package.Manifest);
        Assert.Null(package.Files);
    }
}
