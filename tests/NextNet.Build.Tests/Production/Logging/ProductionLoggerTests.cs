using Microsoft.Extensions.Logging;
using Moq;
using NextNet.Build.Production.Logging;
using Xunit;

namespace NextNet.Build.Tests.Production.Logging;

public class ProductionLoggerTests
{
    private readonly Mock<ILogger<ProductionLogger>> _mockLogger;
    private readonly ProductionLogger _logger;

    public ProductionLoggerTests()
    {
        _mockLogger = new Mock<ILogger<ProductionLogger>>();
        _logger = new ProductionLogger(_mockLogger.Object);
    }

    [Fact]
    public void LogStartup_Should_LogInformation_When_Called()
    {
        _logger.LogStartup(150, "1.0.0");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogRequest_Should_LogError_When_ErrorStatus()
    {
        _logger.LogRequest("GET", "/api/test", 500, 100, 1024);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogRequest_Should_LogWarning_When_WarningStatus()
    {
        _logger.LogRequest("GET", "/api/test", 404, 100, null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogRequest_Should_LogInformation_When_SuccessStatus()
    {
        _logger.LogRequest("GET", "/api/test", 200, 50, 2048);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogBudgetViolation_Should_LogWarning_When_Called()
    {
        _logger.LogBudgetViolation("TotalSize", "1 MB", "2 MB");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogBuildComplete_Should_LogInformation_When_Success()
    {
        _logger.LogBuildComplete(5000, true, 102400, 2048000);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogBuildComplete_Should_LogError_When_Failure()
    {
        _logger.LogBuildComplete(3000, false, 50000, 0);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogSecurityEvent_Should_LogWarning_When_Called()
    {
        _logger.LogSecurityEvent("CSP Violation", "Blocked inline script");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
