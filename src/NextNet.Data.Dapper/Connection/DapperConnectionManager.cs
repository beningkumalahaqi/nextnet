using System.Collections.Concurrent;
using NextNet.Data.Dapper.Internal;

namespace NextNet.Data.Dapper;

/// <summary>
/// Manages named <see cref="SqlConnection"/> pools for the Dapper provider.
/// Each named connection string maps to a pool managed by SQL Client's
/// built-in connection pooling (enabled by default via <c>Pooling=true</c>).
/// </summary>
/// <remarks>
/// <para>
/// Connections are not individually pooled by this manager — SQL Client's
/// <see cref="SqlConnection"/> pool management handles that transparently.
/// This class creates <see cref="SqlConnection"/> instances with the correct
/// connection string and optional pool configuration.
/// </para>
/// <para>
/// Thread-safe: all connection string lookups use <see cref="ConcurrentDictionary{TKey, TValue}"/>
/// and builder caching. Designed for singleton lifetime.
/// </para>
/// <example>
/// <code>
/// // Open a named connection
/// using var conn = await connectionManager.GetConnectionAsync("Default");
///
/// // Open the default connection
/// using var conn = await connectionManager.GetDefaultConnectionAsync();
///
/// // Or for fine-grained control:
/// using var conn = await connectionManager.OpenConnectionAsync("Analytics");
/// </code>
/// </example>
/// </remarks>
public sealed class DapperConnectionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, ConnectionConfig> _connections;
    private readonly ConcurrentDictionary<string, string> _builtConnectionStrings;
    private readonly DapperOptions _options;
    private readonly ILogger? _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="DapperConnectionManager"/>.
    /// </summary>
    /// <param name="connections">The named connection configurations.</param>
    /// <param name="options">Provider options controlling pooling behavior.</param>
    /// <param name="logger">Optional logger for connection events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connections"/> is null.</exception>
    public DapperConnectionManager(
        IReadOnlyDictionary<string, ConnectionConfig> connections,
        DapperOptions? options = null,
        ILogger<DapperConnectionManager>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(connections);

        _connections = new ConcurrentDictionary<string, ConnectionConfig>(connections, StringComparer.OrdinalIgnoreCase);
        _builtConnectionStrings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _options = options ?? new DapperOptions();
        _logger = logger;
    }

    /// <summary>
    /// Creates and opens a <see cref="SqlConnection"/> for the specified named database.
    /// The connection is configured with the provider's command timeout and pool settings.
    /// This is a convenience wrapper around <see cref="OpenConnectionAsync"/>.
    /// </summary>
    /// <param name="name">The logical connection name (e.g., "Default", "Analytics").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An opened <see cref="SqlConnection"/> ready for querying.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the manager has not been initialized.</exception>
    public async Task<SqlConnection> GetConnectionAsync(string name, CancellationToken cancellationToken = default)
    {
        return await OpenConnectionAsync(name, cancellationToken);
    }

    /// <summary>
    /// Creates and opens a <see cref="SqlConnection"/> for the default database connection.
    /// The default connection name is configured via <see cref="DapperOptions.ConnectionName"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An opened <see cref="SqlConnection"/> ready for querying.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the default connection is not registered.</exception>
    /// <example>
    /// <code>
    /// using var conn = await connectionManager.GetDefaultConnectionAsync();
    /// var users = await conn.QueryAsync&lt;User&gt;("SELECT * FROM Users");
    /// </code>
    /// </example>
    public async Task<SqlConnection> GetDefaultConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await OpenConnectionAsync(_options.ConnectionName, cancellationToken);
    }

    /// <summary>
    /// Creates and opens a <see cref="SqlConnection"/> for the specified named database.
    /// The connection is configured with the provider's command timeout and pool settings.
    /// </summary>
    /// <param name="connectionName">The logical connection name (e.g., "Default", "Analytics").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An opened <see cref="SqlConnection"/> ready for querying.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the manager has not been initialized.</exception>
    public async Task<SqlConnection> OpenConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connectionString = GetConnectionString(connectionName);

        _logger?.LogDebug("Opening connection '{ConnectionName}'...", connectionName);

        var connection = new SqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            _logger?.LogDebug("Connection '{ConnectionName}' opened successfully.", connectionName);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Creates a <see cref="SqlConnection"/> for the specified named database without opening it.
    /// Caller is responsible for opening and disposing the connection.
    /// </summary>
    /// <param name="connectionName">The logical connection name.</param>
    /// <returns>An unopened <see cref="SqlConnection"/>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    public SqlConnection CreateConnection(string connectionName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connectionString = GetConnectionString(connectionName);
        _logger?.LogDebug("Created unopened connection '{ConnectionName}'.", connectionName);
        return new SqlConnection(connectionString);
    }

    /// <summary>
    /// Gets the connection string for the specified named database.
    /// The result is cached in the <see cref="ConcurrentDictionary{TKey, TValue}"/>
    /// for subsequent lookups.
    /// </summary>
    /// <param name="connectionName">The logical connection name.</param>
    /// <returns>The connection string with pool settings applied.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    public string GetConnectionString(string connectionName)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DapperConnectionManager));

        if (!_connections.TryGetValue(connectionName, out var config))
        {
            throw new KeyNotFoundException($"No connection named '{connectionName}' is registered. " +
                $"Available connections: {string.Join(", ", _connections.Keys)}");
        }

        return _builtConnectionStrings.GetOrAdd(connectionName, _ =>
            DefaultConnectionStrings.BuildConnectionString(config.ConnectionString, _options));
    }

    /// <summary>
    /// Gets all registered connection names.
    /// </summary>
    /// <returns>A read-only collection of connection names.</returns>
    public IReadOnlyCollection<string> GetConnectionNames() =>
        _connections.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Validates that all configured connections are reachable by executing a <c>SELECT 1</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A dictionary mapping connection name to a boolean health status.</returns>
    public async Task<IReadOnlyDictionary<string, bool>> ValidateConnectionsAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var results = new Dictionary<string, bool>();

        foreach (var connectionName in _connections.Keys)
        {
            try
            {
                using var conn = await OpenConnectionAsync(connectionName, cancellationToken);
                await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT 1", commandTimeout: 5, cancellationToken: cancellationToken));
                results[connectionName] = true;
                _logger?.LogDebug("Connection '{ConnectionName}' validated successfully.", connectionName);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                results[connectionName] = false;
                _logger?.LogWarning(ex, "Connection '{ConnectionName}' validation failed.", connectionName);
            }
        }

        return results;
    }

    /// <summary>
    /// Disposes all managed resources. Connections managed by the SQL Client pool
    /// are released when the pool is cleared.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            SqlConnection.ClearAllPools();
            _builtConnectionStrings.Clear();
            _logger?.LogDebug("DapperConnectionManager disposed and SQL Client pools cleared.");
        }
    }
}
