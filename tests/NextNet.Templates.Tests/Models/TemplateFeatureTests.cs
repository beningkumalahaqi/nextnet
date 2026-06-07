using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Models;

public class TemplateFeatureTests
{
    [Fact]
    public void Constructor_Should_SetNameAndDependencies()
    {
        // Arrange & Act
        var feature = new TemplateFeature(
            "auth",
            "Include authentication",
            new[] { "identity" },
            new[] { "no-auth" }
        );

        // Assert
        Assert.Equal("auth", feature.Name);
        Assert.Equal("Include authentication", feature.Description);
        Assert.Equal(new[] { "identity" }, feature.Dependencies);
        Assert.Equal(new[] { "no-auth" }, feature.Conflicts);
    }
}
