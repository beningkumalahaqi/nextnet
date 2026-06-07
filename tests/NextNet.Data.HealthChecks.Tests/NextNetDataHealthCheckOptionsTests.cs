namespace NextNet.Data.HealthChecks.Tests;

public class NextNetDataHealthCheckOptionsTests
{
    [Fact]
    public void Constructor_Should_SetDefaultValues()
    {
        // Arrange & Act
        var options = new NextNetDataHealthCheckOptions();

        // Assert
        Assert.Equal("/health", options.EndpointPath);
        Assert.False(options.ShowDetails);
        Assert.Equal(TimeSpan.FromSeconds(5), options.CacheTtl);
        Assert.False(options.IncludeExceptionDetails);
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var options = new NextNetDataHealthCheckOptions();

        // Act
        options.EndpointPath = "/health/data";
        options.ShowDetails = true;
        options.CacheTtl = TimeSpan.FromSeconds(10);
        options.IncludeExceptionDetails = true;

        // Assert
        Assert.Equal("/health/data", options.EndpointPath);
        Assert.True(options.ShowDetails);
        Assert.Equal(TimeSpan.FromSeconds(10), options.CacheTtl);
        Assert.True(options.IncludeExceptionDetails);
    }

    [Fact]
    public void CacheTtl_Should_SupportZeroToDisableCaching()
    {
        // Arrange & Act
        var options = new NextNetDataHealthCheckOptions { CacheTtl = TimeSpan.Zero };

        // Assert
        Assert.Equal(TimeSpan.Zero, options.CacheTtl);
    }

    [Fact]
    public void EndpointPath_Should_HandleCustomPath()
    {
        // Arrange & Act
        var options = new NextNetDataHealthCheckOptions { EndpointPath = "/custom/health" };

        // Assert
        Assert.Equal("/custom/health", options.EndpointPath);
    }
}
