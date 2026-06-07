using NextNet.Data.Exceptions;
using Xunit;

namespace NextNet.Data.Providers.Tests;

public class ExceptionTests
{
    [Fact]
    public void NextNetDataException_Should_HaveErrorCode()
    {
        // Act
        var ex = new NextNetDataException("TEST_001", "Test message");

        // Assert
        Assert.Equal("TEST_001", ex.ErrorCode);
        Assert.Equal("Test message", ex.Message);
    }

    [Fact]
    public void NextNetDataException_Should_SupportInnerException()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner");

        // Act
        var ex = new NextNetDataException("TEST_002", "Outer", inner);

        // Assert
        Assert.Equal("TEST_002", ex.ErrorCode);
        Assert.Equal("Outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void ProviderRegistrationException_Should_HaveCorrectErrorCode()
    {
        // Act
        var ex = new ProviderRegistrationException("MyProvider", "Duplicate name");

        // Assert
        Assert.Equal("SKDATA_PROVIDER_001", ex.ErrorCode);
        Assert.Contains("MyProvider", ex.Message);
        Assert.Equal("MyProvider", ex.ProviderName);
    }

    [Fact]
    public void ProviderInitializationException_Should_HaveCorrectErrorCode()
    {
        // Arrange
        var inner = new InvalidOperationException("Failed to connect");

        // Act
        var ex = new ProviderInitializationException("MyProvider", inner);

        // Assert
        Assert.Equal("SKDATA_PROVIDER_002", ex.ErrorCode);
        Assert.Contains("MyProvider", ex.Message);
        Assert.Equal("MyProvider", ex.ProviderName);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void ProviderNotFoundException_Should_HaveCorrectErrorCode()
    {
        // Act
        var ex = new ProviderNotFoundException("MissingProvider");

        // Assert
        Assert.Equal("SKDATA_PROVIDER_003", ex.ErrorCode);
        Assert.Contains("MissingProvider", ex.Message);
        Assert.Equal("MissingProvider", ex.ProviderName);
    }

    [Fact]
    public void ProviderHealthCheckException_Should_HaveCorrectErrorCode()
    {
        // Arrange
        var inner = new TimeoutException("Health check timed out");

        // Act
        var ex = new ProviderHealthCheckException("MyProvider", inner);

        // Assert
        Assert.Equal("SKDATA_PROVIDER_004", ex.ErrorCode);
        Assert.Contains("MyProvider", ex.Message);
        Assert.Equal("MyProvider", ex.ProviderName);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void ProviderConfigurationException_Should_HaveCorrectErrorCode()
    {
        // Act
        var ex = new ProviderConfigurationException("MyProvider", "Missing connection string");

        // Assert
        Assert.Equal("SKDATA_PROVIDER_005", ex.ErrorCode);
        Assert.Contains("MyProvider", ex.Message);
        Assert.Contains("Missing connection string", ex.Message);
        Assert.Equal("MyProvider", ex.ProviderName);
    }

    [Fact]
    public void AllExceptions_Should_InheritFromNextNetDataException()
    {
        // Assert that all provider exceptions share the base type
        Assert.IsAssignableFrom<NextNetDataException>(
            new ProviderRegistrationException("A", "msg"));
        Assert.IsAssignableFrom<NextNetDataException>(
            new ProviderInitializationException("A", new Exception()));
        Assert.IsAssignableFrom<NextNetDataException>(
            new ProviderNotFoundException("A"));
        Assert.IsAssignableFrom<NextNetDataException>(
            new ProviderHealthCheckException("A", new Exception()));
        Assert.IsAssignableFrom<NextNetDataException>(
            new ProviderConfigurationException("A", "detail"));
    }
}
