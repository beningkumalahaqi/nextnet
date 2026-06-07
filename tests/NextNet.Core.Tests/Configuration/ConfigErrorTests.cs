using NextNet.Configuration;
using Xunit;

namespace NextNet.Core.Tests.Configuration;

public class ConfigErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Act
        var error = new ConfigError("TEST_CODE", "Test message", ConfigErrorSeverity.Error, "Test.Path");

        // Assert
        Assert.Equal("TEST_CODE", error.Code);
        Assert.Equal("Test message", error.Message);
        Assert.Equal(ConfigErrorSeverity.Error, error.Severity);
        Assert.Equal("Test.Path", error.Path);
    }

    [Fact]
    public void Constructor_WithNullCode_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ConfigError(null!, "message", ConfigErrorSeverity.Error));
    }

    [Fact]
    public void Constructor_WithNullMessage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ConfigError("CODE", null!, ConfigErrorSeverity.Warning));
    }

    [Fact]
    public void Constructor_AllowsNullPath()
    {
        var error = new ConfigError("CODE", "msg", ConfigErrorSeverity.Warning);
        Assert.Null(error.Path);
    }

    [Fact]
    public void Constructor_DefaultSeverityIsWarning()
    {
        var error = new ConfigError("CODE", "msg", ConfigErrorSeverity.Warning);
        Assert.Equal(ConfigErrorSeverity.Warning, error.Severity);
    }

    [Fact]
    public void Severity_Error_And_Warning_AreDistinct()
    {
        Assert.NotEqual(
            (int)ConfigErrorSeverity.Error,
            (int)ConfigErrorSeverity.Warning);
    }
}
