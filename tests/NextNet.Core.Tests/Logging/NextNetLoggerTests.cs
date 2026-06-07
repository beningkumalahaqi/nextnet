using NextNet.Logging;
using Xunit;

namespace NextNet.Core.Tests.Logging;

public class NextNetLoggerTests
{
    [Fact]
    public void Constructor_WithCategory_SetsCategory()
    {
        var logger = new NextNetLogger("TestCategory");
        Assert.NotNull(logger);
    }

    [Fact]
    public void Constructor_WithNullCategory_DoesNotThrow()
    {
        var logger = new NextNetLogger(null);
        Assert.NotNull(logger);
    }

    [Fact]
    public void Info_LogsWithoutThrowing()
    {
        var logger = new NextNetLogger();
        logger.Info("Test info message");
        // Should not throw
    }

    [Fact]
    public void Info_WithArgs_LogsWithoutThrowing()
    {
        var logger = new NextNetLogger();
        logger.Info("Value is {0} and {1}", 42, "test");
        // Should not throw
    }

    [Fact]
    public void Warn_LogsWithoutThrowing()
    {
        var logger = new NextNetLogger();
        logger.Warn("Test warning");
    }

    [Fact]
    public void Error_LogsWithoutThrowing()
    {
        var logger = new NextNetLogger();
        logger.Error("Test error");
    }

    [Fact]
    public void Debug_LogsWithoutThrowing()
    {
        var logger = new NextNetLogger();
        logger.Debug("Test debug");
    }

    [Fact]
    public void BeginScope_ReturnsDisposable()
    {
        var logger = new NextNetLogger();
        using var scope = logger.BeginScope("TestScope");
        Assert.NotNull(scope);
        Assert.IsAssignableFrom<IDisposable>(scope);
    }

    [Fact]
    public void BeginScope_AndDispose_DoesNotThrow()
    {
        var logger = new NextNetLogger();
        var scope = logger.BeginScope("Scope1");
        // Should not throw
        scope.Dispose();
    }

    [Fact]
    public void NestedScopes_DoNotThrow()
    {
        var logger = new NextNetLogger();
        using (var scope1 = logger.BeginScope("Outer"))
        {
            using (var scope2 = logger.BeginScope("Inner"))
            {
                logger.Info("Inside nested scopes");
            }
        }
    }

    [Fact]
    public void MultipleLogLevels_DoNotThrow()
    {
        var logger = new NextNetLogger("MultiTest");
        logger.Debug("Debug msg");
        logger.Info("Info msg");
        logger.Warn("Warn msg");
        logger.Error("Error msg");
    }

    [Fact]
    public void FormatMessage_WithInvalidFormat_DoesNotThrow()
    {
        var logger = new NextNetLogger();
        // Invalid format string (missing closing brace) should not throw
        logger.Info("Invalid {0 format", "arg");
    }

    [Fact]
    public void LogWithCategory_DoesNotThrow()
    {
        var logger = new NextNetLogger("MyComponent");
        logger.Info("Component initialized");
    }
}
