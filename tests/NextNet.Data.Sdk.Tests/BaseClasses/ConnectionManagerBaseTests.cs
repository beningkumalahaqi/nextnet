using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Sdk.Base;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Tests.BaseClasses;

/// <summary>
/// Tests for <see cref="ConnectionManagerBase"/>.
/// </summary>
public class ConnectionManagerBaseTests
{
    [Fact]
    public void GetConnection_Should_ReturnConnection_WhenConfigured()
    {
        var manager = CreateManager();

        var connection = manager.GetConnection("Primary");

        Assert.NotNull(connection);
        Assert.Equal("Primary", connection.Name);
        Assert.Equal("Server=primary;", connection.ConnectionString);
    }

    [Fact]
    public void GetConnection_Should_Throw_WhenNotConfigured()
    {
        var manager = CreateManager();

        Assert.Throws<KeyNotFoundException>(() => manager.GetConnection("NonExistent"));
    }

    [Fact]
    public void GetConnection_Should_Throw_WhenDisabled()
    {
        var config = new DataConfig(
            DefaultConnection: "Default",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Default"] = new ConnectionConfig("Server=test;", "Test", Enabled: false)
            });
        var manager = CreateManager(config);

        Assert.Throws<InvalidOperationException>(() => manager.GetConnection("Default"));
    }

    [Fact]
    public void GetDefaultConnection_Should_ReturnDefault()
    {
        var manager = CreateManager();

        var connection = manager.GetDefaultConnection();

        Assert.NotNull(connection);
        Assert.Equal("Primary", connection.Name);
    }

    [Fact]
    public void GetDefaultConnection_Should_Throw_WhenNoDefault()
    {
        var config = new DataConfig(
            DefaultConnection: string.Empty,
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Primary"] = new ConnectionConfig("Server=primary;")
            });
        var manager = CreateManager(config);

        Assert.Throws<InvalidOperationException>(() => manager.GetDefaultConnection());
    }

    [Fact]
    public void GetConnectionNames_Should_ReturnAllNames()
    {
        var manager = CreateManager();

        // Access connections first to populate the lazy-loaded cache
        manager.GetConnection("Primary");
        manager.GetConnection("Secondary");

        var names = manager.GetConnectionNames();

        Assert.Contains("Primary", names);
        Assert.Contains("Secondary", names);
    }

    [Fact]
    public void Dispose_Should_NotThrow()
    {
        var manager = CreateManager();
        manager.GetConnection("Primary");

        var exception = Record.Exception(() => manager.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void GetConnection_Should_Throw_AfterDispose()
    {
        var manager = CreateManager();
        manager.Dispose();

        Assert.Throws<ObjectDisposedException>(() => manager.GetConnection("Primary"));
    }

    [Fact]
    public void Constructor_Should_Throw_WhenConfigIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TestConnectionManager(null!));
    }

    private static ConnectionManagerBase CreateManager(DataConfig? config = null)
    {
        config ??= new DataConfig(
            DefaultConnection: "Primary",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Primary"] = new ConnectionConfig("Server=primary;"),
                ["Secondary"] = new ConnectionConfig("Server=secondary;")
            });

        var options = Options.Create(config);
        return new TestConnectionManager(options);
    }

    /// <summary>
    /// Test connection manager implementation.
    /// </summary>
    private sealed class TestConnectionManager : ConnectionManagerBase
    {
        public TestConnectionManager(IOptions<DataConfig> config, ILogger? logger = null)
            : base(config, logger) { }

        protected override object CreateConnectionCore(string connectionString, string name)
        {
            return new TestConnection(name, connectionString);
        }
    }

    /// <summary>
    /// Test connection object.
    /// </summary>
    private sealed class TestConnection : IDisposable
    {
        public string Name { get; }
        public string ConnectionString { get; }
        public bool IsDisposed { get; private set; }

        public TestConnection(string name, string connectionString)
        {
            Name = name;
            ConnectionString = connectionString;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
