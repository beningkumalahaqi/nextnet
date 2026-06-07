using Xunit;

namespace NextNet.Data.Providers.Tests;

public class DataProviderHealthResultTests
{
    [Fact]
    public void Healthy_Should_CreateHealthyResult()
    {
        // Act
        var result = DataProviderHealthResult.Healthy();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Message);
        Assert.Null(result.Exception);
        Assert.NotEqual(default, result.CheckedAt);
    }

    [Fact]
    public void Healthy_WithMessage_Should_SetMessage()
    {
        // Act
        var result = DataProviderHealthResult.Healthy("All systems go");

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("All systems go", result.Message);
    }

    [Fact]
    public void Unhealthy_Should_CreateUnhealthyResult()
    {
        // Act
        var result = DataProviderHealthResult.Unhealthy("Connection failed");

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("Connection failed", result.Message);
        Assert.Null(result.Exception);
        Assert.NotEqual(default, result.CheckedAt);
    }

    [Fact]
    public void Unhealthy_WithException_Should_SetException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Something went wrong");

        // Act
        var result = DataProviderHealthResult.Unhealthy("Error occurred", innerException);

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("Error occurred", result.Message);
        Assert.Same(innerException, result.Exception);
    }

    [Fact]
    public void Healthy_DefaultMessage_Should_BeHealthy()
    {
        // Act
        var result = DataProviderHealthResult.Healthy();

        // Assert
        Assert.Equal("Healthy", result.Message);
    }

    [Fact]
    public void Unhealthy_NullMessage_Should_DefaultToUnhealthy()
    {
        // Act
        var result = DataProviderHealthResult.Unhealthy(null!);

        // Assert
        Assert.Equal("Unhealthy", result.Message);
    }

    [Fact]
    public void CheckedAt_Should_BeRecent()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        // Act
        var result = DataProviderHealthResult.Healthy();

        // Assert
        Assert.True(result.CheckedAt >= before);
        Assert.True(result.CheckedAt <= DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Properties_Should_BeMutable()
    {
        // Use with-expressions on record
        var result1 = DataProviderHealthResult.Healthy("Initial");
        var result2 = result1 with { Message = "Updated" };

        Assert.Equal("Initial", result1.Message);
        Assert.Equal("Updated", result2.Message);
    }
}
