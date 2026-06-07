using NextNet.Data.Abstractions.Models;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Models;

public class HealthCheckResultTests
{
    [Fact]
    public void Constructor_Should_SetAllProperties()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(150);
        var data = new Dictionary<string, object> { ["version"] = "1.0" };

        // Act
        var result = new HealthCheckResult(
            true,
            "Healthy",
            duration,
            "All good",
            data);

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Status);
        Assert.Equal(duration, result.Duration);
        Assert.Equal("All good", result.Message);
        Assert.Same(data, result.Data);
    }

    [Fact]
    public void Constructor_Should_AllowNullMessageAndData()
    {
        // Arrange & Act
        var result = new HealthCheckResult(true, "Healthy", TimeSpan.Zero);

        // Assert
        Assert.Null(result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public void IsHealthy_WhenStatusIsHealthy_Should_BeTrue()
    {
        // Arrange & Act
        var result = new HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(42));

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Status);
    }

    [Fact]
    public void IsHealthy_WhenStatusIsUnhealthy_Should_BeFalse()
    {
        // Arrange & Act
        var result = new HealthCheckResult(false, "Unhealthy", TimeSpan.FromMilliseconds(100), "Connection failed");

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("Unhealthy", result.Status);
        Assert.Equal("Connection failed", result.Message);
    }
}
