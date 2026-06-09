#if NET8_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Base;

/// <summary>
/// Base class for managing named database connections. Implements <see cref="IDataConnection"/>
/// lifecycle with connection pooling, dispose tracking, and configurable timeouts.
/// </summary>
/// <remarks>
/// <para>
/// Provider authors override <see cref="CreateConnectionCore"/> to return a
/// provider-specific connection object. The base class handles connection string
/// resolution, pool lifecycle, and dispose tracking for leak detection.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyCustomConnectionManager : ConnectionManagerBase
/// {
///     public MyCustomConnectionManager(IOptions&lt;DataConfig&gt; config, ILogger logger)
///         : base(config, logger) { }
///
///     protected override object CreateConnectionCore(string connectionString, string name)
///     {
///         return new MyCustomConnection(connectionString);
///     }
/// }
/// </code>
/// </example>
public abstract class ConnectionManagerBase : IDisposable
{
    private readonly IOptions<DataConfig> _config;
    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<string, IDataConnection> _connections = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// Gets the data configuration used by this connection manager.
    /// </summary>
    protected IOptions<DataConfig> Config => _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionManagerBase"/> class.
    /// </summary>
    /// <param name="config">The data configuration containing connection definitions.</param>
    /// <param name="logger">An optional logger for diagnostic output.</param>
    protected ConnectionManagerBase(IOptions<DataConfig> config, ILogger? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }

    /// <summary>
    /// Gets a connection by its logical name.
    /// </summary>
    /// <param name="name">The connection name (e.g., "Default", "Analytics").</param>
    /// <returns>The connection object as <see cref="IDataConnection"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the name is not registered.</exception>
    public IDataConnection GetConnection(string name)
    {
        if (_disposed)
            throw new ObjectDisposedException($"[DS-604] {nameof(ConnectionManagerBase)}");

        return _connections.GetOrAdd(name, CreateConnection);
    }

    /// <summary>
    /// Gets the default connection as specified in <see cref="DataConfig.DefaultConnection"/>.
    /// </summary>
    /// <returns>The default connection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no default connection is configured.</exception>
    public IDataConnection GetDefaultConnection()
    {
        var defaultName = _config.Value.DefaultConnection;
        if (string.IsNullOrWhiteSpace(defaultName))
            throw new InvalidOperationException("[DS-595] DefaultConnection is not configured in DataConfig.");

        return GetConnection(defaultName);
    }

    /// <summary>
    /// Gets all registered connection names.
    /// </summary>
    /// <returns>A read-only collection of connection names.</returns>
    public IReadOnlyCollection<string> GetConnectionNames()
    {
        return _connections.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Creates a provider-specific connection object from a connection string.
    /// The returned object is wrapped in an internal <see cref="IDataConnection"/> adapter.
    /// </summary>
    /// <param name="connectionString">The resolved connection string.</param>
    /// <param name="name">The logical name of the connection.</param>
    /// <returns>A provider-specific connection object (e.g., <c>SqlConnection</c>, <c>MongoClient</c>).</returns>
    protected abstract object CreateConnectionCore(string connectionString, string name);

    /// <summary>
    /// Disposes all tracked connections. Provider authors override to add custom cleanup.
    /// </summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/>; <c>false</c> from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            foreach (var kvp in _connections)
            {
                if (kvp.Value is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error disposing connection '{ConnectionName}'.", kvp.Key);
                    }
                }
            }
            _connections.Clear();
        }

        _disposed = true;
    }

    /// <summary>
    /// Disposes all tracked connections and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private IDataConnection CreateConnection(string name)
    {
        if (_config.Value.Connections == null || !_config.Value.Connections.TryGetValue(name, out var connectionConfig))
            throw new KeyNotFoundException($"[DS-601] Connection '{name}' is not configured in DataConfig.Connections.");

        if (!connectionConfig.Enabled)
            throw new InvalidOperationException($"[DS-604] Connection '{name}' is disabled in configuration.");

        _logger?.LogDebug("Creating connection '{ConnectionName}'.", name);

        var provider = CreateConnectionCore(connectionConfig.ConnectionString, name);
        return new ConnectionAdapter(provider, name, connectionConfig.ConnectionString, GetType().Name);
    }

    /// <summary>
    /// Internal adapter that wraps a provider-specific connection object in an <see cref="IDataConnection"/>.
    /// </summary>
    private sealed class ConnectionAdapter : IDataConnection
    {
        public string Name { get; }
        public string ConnectionString { get; }
        public string ProviderName { get; }
        public object InnerConnection { get; }

        public ConnectionAdapter(object innerConnection, string name, string connectionString, string providerName)
        {
            InnerConnection = innerConnection;
            Name = name;
            ConnectionString = connectionString;
            ProviderName = providerName;
        }
    }
}
#endif
