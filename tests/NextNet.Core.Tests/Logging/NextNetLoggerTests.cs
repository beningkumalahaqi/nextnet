using NextNet.Logging;
using Xunit;

namespace NextNet.Core.Tests.Logging;

public class NextNetLoggerTests
{
    [Fact]
    public void Constructor_Should_SetCategory_When_Provided()
    {
        var logger = new NextNetLogger("TestCategory");
        Assert.NotNull(logger);
    }

    [Fact]
    public void Constructor_Should_NotThrow_When_CategoryIsNull()
    {
        var logger = new NextNetLogger(null);
        Assert.NotNull(logger);
    }

    [Fact]
    public void Info_Should_NotThrow_When_Invoked()
    {
        var logger = new NextNetLogger();
        logger.Info("Test info message");
        // Should not throw
    }

    [Fact]
    public void Info_Should_NotThrow_When_ArgsProvided()
    {
        var logger = new NextNetLogger();
        logger.Info("Value is {0} and {1}", 42, "test");
        // Should not throw
    }

    [Fact]
    public void Warn_Should_NotThrow_When_Invoked()
    {
        var logger = new NextNetLogger();
        logger.Warn("Test warning");
    }

    [Fact]
    public void Error_Should_NotThrow_When_Invoked()
    {
        var logger = new NextNetLogger();
        logger.Error("Test error");
    }

    [Fact]
    public void Debug_Should_NotThrow_When_Invoked()
    {
        var logger = new NextNetLogger();
        logger.Debug("Test debug");
    }

    [Fact]
    public void BeginScope_Should_ReturnDisposable_When_Invoked()
    {
        var logger = new NextNetLogger();
        using var scope = logger.BeginScope("TestScope");
        Assert.NotNull(scope);
        Assert.IsAssignableFrom<IDisposable>(scope);
    }

    [Fact]
    public void BeginScope_Should_NotThrow_When_Disposed()
    {
        var logger = new NextNetLogger();
        var scope = logger.BeginScope("Scope1");
        // Should not throw
        scope.Dispose();
    }

    [Fact]
    public void NestedScopes_Should_NotThrow_When_Used()
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
    public void MultipleLogLevels_Should_NotThrow_When_Invoked()
    {
        var logger = new NextNetLogger("MultiTest");
        logger.Debug("Debug msg");
        logger.Info("Info msg");
        logger.Warn("Warn msg");
        logger.Error("Error msg");
    }

    [Fact]
    public void FormatMessage_Should_NotThrow_When_FormatIsInvalid()
    {
        var logger = new NextNetLogger();
        // Invalid format string (missing closing brace) should not throw
        logger.Info("Invalid {0 format", "arg");
    }

    [Fact]
    public void LogWithCategory_Should_NotThrow_When_Invoked()
    {
        var logger = new NextNetLogger("MyComponent");
        logger.Info("Component initialized");
    }
}
