using Xunit;

namespace NextNet.Edge.Tests;

public class EdgeOptionsTests
{
    [Fact]
    public void Constructor_DefaultsAreSet()
    {
        // Arrange & Act
        var options = new EdgeOptions();

        // Assert
        Assert.False(options.Enabled);
        Assert.Equal("cloudflare", options.Provider);
        Assert.True(options.CheckCompatibility);
        Assert.False(options.Strict);
        Assert.Equal(1_048_576, options.MaxBundleSize);
        Assert.Equal(100, options.MaxStaticAssets);
        Assert.Equal(10_485_760, options.MaxDeploymentSize);
        Assert.Equal("dist-edge", options.OutputDir);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var options = new EdgeOptions
        {
            Enabled = true,
            Provider = "aws",
            CheckCompatibility = false,
            Strict = true,
            MaxBundleSize = 500_000,
            MaxStaticAssets = 50,
            MaxDeploymentSize = 5_000_000,
            OutputDir = "custom-edge-output",
        };

        // Assert
        Assert.True(options.Enabled);
        Assert.Equal("aws", options.Provider);
        Assert.False(options.CheckCompatibility);
        Assert.True(options.Strict);
        Assert.Equal(500_000, options.MaxBundleSize);
        Assert.Equal(50, options.MaxStaticAssets);
        Assert.Equal(5_000_000, options.MaxDeploymentSize);
        Assert.Equal("custom-edge-output", options.OutputDir);
    }
}
