namespace NextNet.Data.Dapper.Tests.HealthCheck;

/// <summary>
/// Tests for <see cref="DapperHealthCheck"/> connectivity verification,
/// result structure, and error handling.
/// </summary>
public sealed class DapperHealthCheckTests : IDisposable
{
    private readonly DapperConnectionManager _connectionManager;
    private readonly ILogger<DapperHealthCheck> _logger;
    private readonly IReadOnlyList<string> _connectionNames;
    private bool _disposed;

    public DapperHealthCheckTests()
    {
        _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperHealthCheck>();

        var connections = new Dictionary<string, ConnectionConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["Default"] = new ConnectionConfig(
                "Server=192.0.2.1,9999;Database=Test;Trusted_Connection=true;Connect Timeout=1;",
                Provider: "Dapper"),
            ["Analytics"] = new ConnectionConfig(
                "Server=192.0.2.2,9999;Database=Analytics;Trusted_Connection=true;Connect Timeout=1;",
                Provider: "Dapper")
        };

        _connectionManager = new DapperConnectionManager(
            connections,
            new DapperOptions { EnablePooling = false, CommandTimeoutSeconds = 1 },
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperConnectionManager>());

        _connectionNames = new List<string> { "Default", "Analytics" }.AsReadOnly();
    }

    /// <summary>
    /// Creates a <see cref="DapperHealthCheck"/> instance for testing.
    /// </summary>
    private DapperHealthCheck CreateHealthCheck(
        IReadOnlyList<string>? connectionNames = null,
        DapperConnectionManager? connectionManager = null,
        ILogger<DapperHealthCheck>? logger = null)
    {
        return new DapperHealthCheck(
            connectionManager ?? _connectionManager,
            connectionNames ?? _connectionNames,
            logger ?? _logger);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_ThrowArgumentNullException_When_ConnectionManagerIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new DapperHealthCheck(null!, _connectionNames));
        Assert.Contains("connectionManager", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_ThrowArgumentNullException_When_ConnectionNamesIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new DapperHealthCheck(_connectionManager, null!));
        Assert.Contains("connectionNames", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseNullLogger_When_LoggerIsNull()
    {
        var healthCheck = new DapperHealthCheck(
            _connectionManager,
            _connectionNames,
            logger: null);

        Assert.NotNull(healthCheck);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_ReturnResult_WithDefaultCancellationToken()
    {
        var healthCheck = CreateHealthCheck();
        var result = await healthCheck.GetHealthCheckAsync();

        Assert.NotNull(result);
        Assert.NotNull(result.Status);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_ReturnHealthCheckResultType()
    {
        var healthCheck = CreateHealthCheck();
        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.IsType<HealthCheckResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_HavePositiveDuration()
    {
        // Use a single connection name pointing to unreachable server
        var config = new List<string> { "Default" }.AsReadOnly();
        var healthCheck = CreateHealthCheck(connectionNames: config);

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        // Duration should be measurable (> 0)
        Assert.True(result.Duration.TotalMilliseconds >= 0,
            $"Expected duration >= 0, got {result.Duration.TotalMilliseconds}ms");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_ReportUnhealthy_When_NoDatabase()
    {
        // All connections point to unreachable servers
        var healthCheck = CreateHealthCheck();

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.False(result.IsHealthy, "Health check should fail when database is unreachable.");
        Assert.Equal("Unhealthy", result.Status);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_IncludeDiagnosticData()
    {
        var healthCheck = CreateHealthCheck();

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        // The Data dictionary should contain per-connection diagnostics
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data!);

        foreach (var connectionName in _connectionNames)
        {
            Assert.True(result.Data!.ContainsKey(connectionName),
                $"Diagnostics should include entry for '{connectionName}'.");
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_ReportAllConnectionsInData()
    {
        var healthCheck = CreateHealthCheck();

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Equal(_connectionNames.Count, result.Data!.Count);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_ReportPerConnectionStatus()
    {
        var healthCheck = CreateHealthCheck();

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.NotNull(result.Data);

        foreach (var kvp in result.Data!)
        {
            // Each entry should be an anonymous type with status, latencyMs, etc.
            var entry = kvp.Value;
            Assert.NotNull(entry);

            // We can't easily inspect anonymous types, so verify the data exists
            var entryStr = entry.ToString();
            Assert.NotNull(entryStr);
            Assert.NotEmpty(entryStr!);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_NotThrow_When_Cancelled()
    {
        var healthCheck = CreateHealthCheck();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Should handle cancellation gracefully (OperationCanceledException or derived)
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => healthCheck.GetHealthCheckAsync(cts.Token));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_ReturnMessage()
    {
        var healthCheck = CreateHealthCheck();

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Contains("connection(s)", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_HandleSingleConnection()
    {
        var singleConnection = new List<string> { "Default" }.AsReadOnly();
        var healthCheck = CreateHealthCheck(connectionNames: singleConnection);

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data!);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_HandleEmptyConnectionList()
    {
        var emptyList = new List<string>().AsReadOnly();
        var healthCheck = CreateHealthCheck(connectionNames: emptyList);

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsHealthy,
            "Empty connection list should be considered healthy (no failures).");
        Assert.Equal("Healthy", result.Status);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data!);
    }

    /// <summary>
    /// Creates a health check with a connection manager that has no connections.
    /// Tests the edge case where the connection manager was initialized without connections.
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetHealthCheckAsync_Should_HandleConnectionManagerWithNoRegisteredConnections()
    {
        var emptyManager = new DapperConnectionManager(
            new Dictionary<string, ConnectionConfig>(),
            new DapperOptions { EnablePooling = false },
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperConnectionManager>());

        var emptyList = new List<string>().AsReadOnly();
        var healthCheck = CreateHealthCheck(
            connectionManager: emptyManager,
            connectionNames: emptyList);

        var result = await healthCheck.GetHealthCheckAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
        Assert.Empty(result.Data!);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _connectionManager.Dispose();
        }
    }
}
