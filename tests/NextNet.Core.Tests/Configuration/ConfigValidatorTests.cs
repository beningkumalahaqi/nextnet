using NextNet.Configuration;
using NextNet.Errors;
using Xunit;

namespace NextNet.Core.Tests.Configuration;

public class ConfigValidatorTests
{
    [Fact]
    public void Validate_Should_ReturnNoErrors_When_DefaultConfigProvided()
    {
        // Arrange
        var config = new NextNetConfig();

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_Should_ThrowArgumentNullException_When_ConfigIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ConfigValidator.Validate(null!));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_AppDirIsEmpty()
    {
        // Arrange
        var config = new NextNetConfig { AppDir = "" };
        var expectedCode = CoreErrorCodes.ConfigAppDirEmpty;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e =>
            e.Code == expectedCode &&
            e.Severity == ConfigErrorSeverity.Error);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_AppDirIsWhitespace()
    {
        // Arrange
        var config = new NextNetConfig { AppDir = "   " };
        var expectedCode = CoreErrorCodes.ConfigAppDirEmpty;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_DevPortIsZero()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = 0 };
        var expectedCode = CoreErrorCodes.ConfigDevPortOutOfRange;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e =>
            e.Code == expectedCode &&
            e.Severity == ConfigErrorSeverity.Error);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_DevPortIsNegative()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = -1 };
        var expectedCode = CoreErrorCodes.ConfigDevPortOutOfRange;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_DevPortExceedsMax()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = 70000 };
        var expectedCode = CoreErrorCodes.ConfigDevPortOutOfRange;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Validate_Should_NotReturnError_When_DevPortIsValid()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = 8080 };
        var expectedCode = CoreErrorCodes.ConfigDevPortOutOfRange;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.DoesNotContain(errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_WatchDebounceMsIsZero()
    {
        // Arrange
        var config = new NextNetConfig { WatchDebounceMs = 0 };
        var expectedCode = CoreErrorCodes.ConfigWatchDebounceInvalid;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e =>
            e.Code == expectedCode &&
            e.Severity == ConfigErrorSeverity.Error);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_WatchDebounceMsIsNegative()
    {
        // Arrange
        var config = new NextNetConfig { WatchDebounceMs = -100 };
        var expectedCode = CoreErrorCodes.ConfigWatchDebounceInvalid;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Validate_Should_NotReturnError_When_WatchDebounceMsIsValid()
    {
        // Arrange
        var config = new NextNetConfig { WatchDebounceMs = 500 };
        var expectedCode = CoreErrorCodes.ConfigWatchDebounceInvalid;

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.DoesNotContain(errors, e => e.Code == expectedCode);
    }

    [Fact]
    public void Validate_Should_ReturnAllErrors_When_MultipleIssuesExist()
    {
        // Arrange
        var config = new NextNetConfig
        {
            AppDir = "",
            DevPort = -1,
            WatchDebounceMs = 0,
        };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public void Validate_Should_ReturnReadOnlyList_When_Invoked()
    {
        // Arrange
        var config = new NextNetConfig { AppDir = "", DevPort = -1, WatchDebounceMs = 0 };
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<ConfigError>>(errors);
    }
}
