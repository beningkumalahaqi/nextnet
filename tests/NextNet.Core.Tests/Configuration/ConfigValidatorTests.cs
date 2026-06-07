using NextNet.Configuration;
using Xunit;

namespace NextNet.Core.Tests.Configuration;

public class ConfigValidatorTests
{
    [Fact]
    public void Validate_WithDefaultConfig_ReturnsNoErrors()
    {
        // Arrange
        var config = new NextNetConfig();

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithNullConfig_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ConfigValidator.Validate(null!));
    }

    [Fact]
    public void Validate_WithEmptyAppDir_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { AppDir = "" };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e =>
            e.Code == "APP_DIR_EMPTY" &&
            e.Severity == ConfigErrorSeverity.Error);
    }

    [Fact]
    public void Validate_WithWhitespaceAppDir_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { AppDir = "   " };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == "APP_DIR_EMPTY");
    }

    [Fact]
    public void Validate_WithDevPortZero_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = 0 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e =>
            e.Code == "DEV_PORT_OUT_OF_RANGE" &&
            e.Severity == ConfigErrorSeverity.Error);
    }

    [Fact]
    public void Validate_WithDevPortNegative_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = -1 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == "DEV_PORT_OUT_OF_RANGE");
    }

    [Fact]
    public void Validate_WithDevPortOver65535_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = 70000 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == "DEV_PORT_OUT_OF_RANGE");
    }

    [Fact]
    public void Validate_WithDevPortValid_NoError()
    {
        // Arrange
        var config = new NextNetConfig { DevPort = 8080 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.DoesNotContain(errors, e => e.Code == "DEV_PORT_OUT_OF_RANGE");
    }

    [Fact]
    public void Validate_WithWatchDebounceMsZero_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { WatchDebounceMs = 0 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e =>
            e.Code == "WATCH_DEBOUNCE_INVALID" &&
            e.Severity == ConfigErrorSeverity.Error);
    }

    [Fact]
    public void Validate_WithWatchDebounceMsNegative_ReturnsError()
    {
        // Arrange
        var config = new NextNetConfig { WatchDebounceMs = -100 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.Contains(errors, e => e.Code == "WATCH_DEBOUNCE_INVALID");
    }

    [Fact]
    public void Validate_WithWatchDebounceMsValid_NoError()
    {
        // Arrange
        var config = new NextNetConfig { WatchDebounceMs = 500 };

        // Act
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.DoesNotContain(errors, e => e.Code == "WATCH_DEBOUNCE_INVALID");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ReturnsAllErrors()
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
    public void Validate_ResultIsReadOnly()
    {
        // Arrange
        var config = new NextNetConfig { AppDir = "", DevPort = -1, WatchDebounceMs = 0 };
        var errors = ConfigValidator.Validate(config);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<ConfigError>>(errors);
    }
}
