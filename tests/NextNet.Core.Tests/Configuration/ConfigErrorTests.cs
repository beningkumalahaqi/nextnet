using NextNet.Configuration;
using Xunit;

namespace NextNet.Core.Tests.Configuration;

public class ConfigErrorTests
{
    [Fact]
    public void Constructor_Should_SetProperties_When_AllArgumentsProvided()
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
    public void Constructor_Should_CreateInstance_When_CodeIsNull()
    {
        // The record's non-nullable string parameters do not throw at runtime;
        // null-safety is enforced at compile time.
        var error = new ConfigError(null!, "message", ConfigErrorSeverity.Error);
        Assert.Null(error.Code);
    }

    [Fact]
    public void Constructor_Should_CreateInstance_When_MessageIsNull()
    {
        // The record's non-nullable string parameters do not throw at runtime;
        // null-safety is enforced at compile time.
        var error = new ConfigError("CODE", null!, ConfigErrorSeverity.Warning);
        Assert.Null(error.Message);
    }

    [Fact]
    public void Constructor_Should_SetPathToNull_When_NotProvided()
    {
        var error = new ConfigError("CODE", "msg", ConfigErrorSeverity.Warning);
        Assert.Null(error.Path);
    }

    [Fact]
    public void Constructor_Should_SetWarningSeverity_When_WarningSpecified()
    {
        var error = new ConfigError("CODE", "msg", ConfigErrorSeverity.Warning);
        Assert.Equal(ConfigErrorSeverity.Warning, error.Severity);
    }

    [Fact]
    public void Severity_Should_HaveDistinctValues_When_ComparingErrorAndWarning()
    {
        Assert.NotEqual(
            (int)ConfigErrorSeverity.Error,
            (int)ConfigErrorSeverity.Warning);
    }
}
