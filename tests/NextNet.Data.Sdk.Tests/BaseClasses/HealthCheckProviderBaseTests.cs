using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.Sdk.Base;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Tests.BaseClasses;

/// <summary>
/// Tests for <see cref="HealthCheckProviderBase"/>.
/// </summary>
public class HealthCheckProviderBaseTests
{
    [Fact]
    public async Task GetHealthCheckAsync_Should_ReturnHealthy_WhenAllConnectionsHealthy()
    {
        var (provider, _) = CreateHealthCheckProvider();
        var result = await provider.GetHealthCheckAsync();

        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Status);
    }

    [Fact]
    public async Task GetHealthCheckAsync_Should_ReturnDegraded_WhenSomeConnectionsFail()
    {
        var (provider, manager) = CreateHealthCheckProvider();

        // Force unhealthy by adding a failing connection
        var config = new DataConfig(
            DefaultConnection: "Primary",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Primary"] = new ConnectionConfig("Server=primary;"),
                ["Failing"] = new ConnectionConfig("Server=failing;")
            });

        var options = Options.Create(config);
        var failingProvider = new TestHealthCheckProvider(manager, options);

        var result = await failingProvider.GetHealthCheckAsync();

        Assert.False(result.IsHealthy);
        Assert.Equal("Degraded", result.Status);
    }

    [Fact]
    public async Task GetHealthCheckAsync_Should_ReturnUnhealthy_WhenNoConnections()
    {
        var config = new DataConfig(
            DefaultConnection: string.Empty,
            Connections: null);
        var options = Options.Create(config);
        var manager = new TestConnectionManager(options);
        var provider = new TestHealthCheckProvider(manager, options);

        var result = await provider.GetHealthCheckAsync();

        Assert.False(result.IsHealthy);
        Assert.Equal("Unhealthy", result.Status);
    }

    [Fact]
    public void Constructor_Should_Throw_WhenConnectionManagerIsNull()
    {
        var config = new DataConfig();
        var options = Options.Create(config);

        Assert.Throws<ArgumentNullException>(() => new TestHealthCheckProvider(null!, options));
    }

    [Fact]
    public void Constructor_Should_Throw_WhenConfigIsNull()
    {
        var config = new DataConfig();
        var options = Options.Create(config);
        var manager = new TestConnectionManager(options);

        Assert.Throws<ArgumentNullException>(() => new TestHealthCheckProvider(manager, null!));
    }

    private static (HealthCheckProviderBase, ConnectionManagerBase) CreateHealthCheckProvider()
    {
        var config = new DataConfig(
            DefaultConnection: "Primary",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Primary"] = new ConnectionConfig("Server=primary;"),
                ["Secondary"] = new ConnectionConfig("Server=secondary;")
            });

        var options = Options.Create(config);
        var manager = new TestConnectionManager(options);
        var provider = new TestHealthCheckProvider(manager, options);

        return (provider, manager);
    }

    /// <summary>
    /// Test health check provider that checks connections.
    /// </summary>
    private sealed class TestHealthCheckProvider : HealthCheckProviderBase
    {
        public TestHealthCheckProvider(
            ConnectionManagerBase connectionManager,
            IOptions<DataConfig> config,
            ILogger? logger = null)
            : base(connectionManager, config, logger) { }

        protected override async Task<HealthCheckResult> CheckConnectionAsync(
            string connectionName,
            IDataConnection connection,
            CancellationToken cancellationToken)
        {
            // If the connection string contains "failing", simulate a failure
            if (connection.ConnectionString.Contains("failing", StringComparison.OrdinalIgnoreCase))
            {
                return UnhealthyResult(connectionName, TimeSpan.FromMilliseconds(1), "Simulated failure");
            }

            await Task.Delay(1, cancellationToken);
            return HealthyResult(connectionName, TimeSpan.FromMilliseconds(1));
        }
    }

    /// <summary>
    /// Test connection manager.
    /// </summary>
    private sealed class TestConnectionManager : ConnectionManagerBase
    {
        public TestConnectionManager(IOptions<DataConfig> config, ILogger? logger = null)
            : base(config, logger) { }

        protected override object CreateConnectionCore(string connectionString, string name)
        {
            return name;
        }
    }
}
