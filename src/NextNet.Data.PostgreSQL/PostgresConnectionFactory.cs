using NextNet.Data.PostgreSQL.Internal;

namespace NextNet.Data.PostgreSQL;

/// <summary>
/// Factory for creating <see cref="NpgsqlConnection"/> instances
/// configured for NextNet PostgreSQL connections.
/// Resolves connection strings from named configurations, explicit values,
/// or environment variables.
/// </summary>
/// <remarks>
/// <para>
/// The factory is registered as a singleton and resolves the connection string
/// once at construction time. Connection objects are not pooled by the factory —
/// Npgsql's built-in connection pooling is configured via the connection string
/// or <see cref="PostgresConnectionFactoryOptions"/>.
/// </para>
/// <para>
/// Each call to <see cref="CreateConnection()"/> returns a new <c>NpgsqlConnection</c>
/// instance. Connections are returned unopened by default — the caller is responsible
/// for opening, using, and disposing the connection. Use <see cref="CreateConnectionAsync"/>
/// to create and open in one step.
/// </para>
/// <para>
/// SSL/TLS configuration is applied from <see cref="PostgresSslOptions"/> before
/// the connection string is finalized.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class UserService
/// {
///     private readonly PostgresConnectionFactory _factory;
///
///     public UserService(PostgresConnectionFactory factory)
///         => _factory = factory;
///
///     public async Task DoSomething()
///     {
///         await using var connection = await _factory.CreateConnectionAsync();
///         // Use NpgsqlConnection directly...
///     }
/// }
/// </code>
/// </example>
public sealed class PostgresConnectionFactory : IDisposable
{
    private readonly PostgresConnectionFactoryOptions _options;
    private readonly ConnectionStringResolver _resolver;
    private readonly ILogger<PostgresConnectionFactory>? _logger;
    private readonly string _resolvedConnectionString;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresConnectionFactory"/> class.
    /// Resolves the connection string from the provided options, config, or environment.
    /// </summary>
    /// <param name="options">The factory configuration options.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no connection string can be resolved.</exception>
    public PostgresConnectionFactory(
        IOptions<PostgresConnectionFactoryOptions> options,
        ILogger<PostgresConnectionFactory>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _logger = logger;
        _resolver = new ConnectionStringResolver(_options);
        _resolvedConnectionString = _resolver.Resolve();

        if (string.IsNullOrWhiteSpace(_resolvedConnectionString))
        {
            throw new InvalidOperationException(
                "Failed to resolve a PostgreSQL connection string. " +
                "Ensure a connection string is provided via UsePostgreSQL(), configuration, or environment variables.");
        }

        _logger?.LogDebug("PostgresConnectionFactory initialized with connection string.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresConnectionFactory"/> class
    /// with an explicit connection string (bypasses options resolution).
    /// </summary>
    /// <param name="connectionString">The explicit connection string to use.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    internal PostgresConnectionFactory(
        string connectionString,
        ILogger<PostgresConnectionFactory>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        _options = new PostgresConnectionFactoryOptions();
        _logger = logger;
        _resolver = new ConnectionStringResolver(_options, connectionString);
        _resolvedConnectionString = _resolver.Resolve();

        _logger?.LogDebug("PostgresConnectionFactory initialized with explicit connection string.");
    }

    /// <summary>
    /// Gets the resolved connection string that the factory will use for the default connection.
    /// </summary>
    public string ConnectionString => _resolvedConnectionString;

    /// <summary>
    /// Creates a new <see cref="NpgsqlConnection"/> using the resolved connection string.
    /// The connection is returned unopened.
    /// </summary>
    /// <returns>A new, unopened <c>NpgsqlConnection</c>.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the factory has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the resolved connection string is null or empty.</exception>
    public NpgsqlConnection CreateConnection()
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(_resolvedConnectionString))
            throw new InvalidOperationException(
                "No connection string is configured. Ensure UsePostgreSQL() is called with a connection string or configuration.");

        return new NpgsqlConnection(_resolvedConnectionString);
    }

    /// <summary>
    /// Creates a new <see cref="NpgsqlConnection"/> and opens it asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the open operation.</param>
    /// <returns>A task containing the opened <c>NpgsqlConnection</c>.</returns>
    /// <exception cref="NpgsqlException">Thrown when the connection cannot be established.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the factory has been disposed.</exception>
    public async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    /// <summary>
    /// Creates a new <see cref="NpgsqlConnection"/> for the specified named connection.
    /// </summary>
    /// <param name="connectionName">The logical connection name (e.g., "Default", "Analytics").</param>
    /// <returns>A new, unopened <c>NpgsqlConnection</c>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is configured.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionName"/> is null or empty.</exception>
    public NpgsqlConnection CreateConnection(string connectionName)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("Connection name must not be null or empty.", nameof(connectionName));

        if (string.Equals(connectionName, "Default", StringComparison.OrdinalIgnoreCase))
            return CreateConnection();

        // Future: support named connections via additional configuration
        throw new KeyNotFoundException(
            $"No PostgreSQL connection with name '{connectionName}' is configured. " +
            "Only the 'Default' connection is currently supported.");
    }

    /// <summary>
    /// Tests database connectivity by opening and immediately closing a connection.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the connection was successfully opened and closed.</returns>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            _logger?.LogDebug("PostgreSQL connection test succeeded.");
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.LogWarning(ex, "PostgreSQL connection test failed.");
            return false;
        }
    }

    /// <summary>
    /// Disposes the factory, releasing any held resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PostgresConnectionFactory));
    }
}
