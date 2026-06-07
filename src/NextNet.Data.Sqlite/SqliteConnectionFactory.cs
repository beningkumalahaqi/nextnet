using NextNet.Data.Sqlite.Internal;

namespace NextNet.Data.Sqlite;

/// <summary>
/// Factory for creating <see cref="SqliteConnection"/> instances
/// configured for NextNet SQLite connections.
/// Resolves connection strings from named configurations or explicit values.
/// </summary>
/// <remarks>
/// <para>
/// The factory is registered as a singleton and caches connection string lookups
/// for efficiency. Connection objects are not cached — each call to
/// <see cref="CreateConnection()"/> or <see cref="CreateConnectionAsync"/> creates a
/// new <c>SqliteConnection</c> instance.
/// </para>
/// <para>
/// In file-based mode, the factory ensures the directory containing the database file
/// exists before creating a connection. If <c>EnsureDatabaseCreated</c> is set in options,
/// it also creates an empty database file if one does not exist.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyService
/// {
///     private readonly SqliteConnectionFactory _factory;
///
///     public MyService(SqliteConnectionFactory factory)
///         => _factory = factory;
///
///     public async Task DoSomething()
///     {
///         await using var connection = await _factory.CreateConnectionAsync();
///         // Use connection directly...
///     }
/// }
/// </code>
/// </example>
public sealed class SqliteConnectionFactory
{
    private readonly SqliteConnectionFactoryOptions _options;
    private readonly ConnectionStringResolver _resolver;
    private readonly ILogger<SqliteConnectionFactory>? _logger;
    private readonly string _resolvedConnectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteConnectionFactory"/> class.
    /// </summary>
    /// <param name="options">The factory configuration options.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public SqliteConnectionFactory(
        IOptions<SqliteConnectionFactoryOptions> options,
        ILogger<SqliteConnectionFactory>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _logger = logger;
        _resolver = new ConnectionStringResolver(_options);
        _resolvedConnectionString = _resolver.Resolve();

        _logger?.LogDebug("SqliteConnectionFactory initialized with connection string: {ConnectionString}",
            _resolvedConnectionString);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteConnectionFactory"/> class
    /// with an explicit connection string (bypasses options resolution).
    /// </summary>
    /// <param name="connectionString">The explicit connection string to use.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    internal SqliteConnectionFactory(
        string connectionString,
        ILogger<SqliteConnectionFactory>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        _options = new SqliteConnectionFactoryOptions();
        _logger = logger;
        _resolver = new ConnectionStringResolver(_options, connectionString);
        _resolvedConnectionString = _resolver.Resolve();

        _logger?.LogDebug("SqliteConnectionFactory initialized with explicit connection string: {ConnectionString}",
            _resolvedConnectionString);
    }

    /// <summary>
    /// Gets the resolved connection string that the factory will use.
    /// </summary>
    public string ConnectionString => _resolvedConnectionString;

    /// <summary>
    /// Creates a new <see cref="SqliteConnection"/> using the configured connection string.
    /// The connection is returned unopened — the caller manages its lifetime and opening.
    /// </summary>
    /// <returns>A new, unopened <c>SqliteConnection</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resolved connection string is null or empty.</exception>
    public SqliteConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_resolvedConnectionString))
            throw new InvalidOperationException("No connection string is configured. Ensure UseSqlite() is called with a connection string or configuration.");

        return new SqliteConnection(_resolvedConnectionString);
    }

    /// <summary>
    /// Creates a new <see cref="SqliteConnection"/> and opens it asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task containing the opened <c>SqliteConnection</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resolved connection string is null or empty.</exception>
    public async Task<SqliteConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    /// <summary>
    /// Creates a new <see cref="SqliteConnection"/> for the specified named connection.
    /// Currently supports the <c>"Default"</c> connection name, which returns the
    /// factory's default connection. Named connections beyond "Default" require
    /// additional configuration and will throw if not found.
    /// </summary>
    /// <param name="connectionName">The logical connection name (e.g., "Default", "Analytics").</param>
    /// <returns>A new, unopened <c>SqliteConnection</c>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is configured.</exception>
    public SqliteConnection CreateConnection(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("Connection name must not be null or empty.", nameof(connectionName));

        if (string.Equals(connectionName, "Default", StringComparison.OrdinalIgnoreCase))
            return CreateConnection();

        // Future: support named connections via additional configuration
        throw new KeyNotFoundException(
            $"No SQLite connection with name '{connectionName}' is configured. " +
            "Only the 'Default' connection is currently supported.");
    }

    /// <summary>
    /// Tests whether the configured database file exists on disk (for file-based connections).
    /// Returns <c>true</c> for in-memory connections without checking disk.
    /// </summary>
    /// <returns><c>true</c> if the database file exists or the connection is in-memory.</returns>
    public bool DatabaseExists()
    {
        if (_options.InMemory)
            return true;

        var dataSource = ExtractDataSource(_resolvedConnectionString);
        if (string.IsNullOrWhiteSpace(dataSource))
            return false;

        // In-memory check via connection string
        if (dataSource == ":memory:" || IsInMemoryConnectionString(_resolvedConnectionString))
            return true;

        return File.Exists(dataSource);
    }

    /// <summary>
    /// Ensures the database file exists on disk by creating an empty file if needed.
    /// No-op for in-memory connections.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (_options.InMemory || IsInMemoryConnectionString(_resolvedConnectionString))
        {
            _logger?.LogDebug("Skipping EnsureDatabaseAsync for in-memory SQLite connection.");
            return;
        }

        var dataSource = ExtractDataSource(_resolvedConnectionString);
        if (string.IsNullOrWhiteSpace(dataSource))
        {
            _logger?.LogWarning("Cannot ensure database: Unable to parse Data Source from connection string.");
            return;
        }

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _logger?.LogDebug("Creating directory: {Directory}", directory);
            Directory.CreateDirectory(directory);
        }

        // Create the file if it does not exist
        if (!File.Exists(dataSource))
        {
            _logger?.LogDebug("Creating SQLite database file: {FilePath}", dataSource);
            await Task.Run(() =>
            {
                try
                {
                    using var stream = File.Create(dataSource);
                    // Create empty zero-byte file
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new InvalidOperationException(
                        $"Cannot create SQLite database file at '{dataSource}'. " +
                        "The application does not have write permission for the target directory.", ex);
                }
                catch (IOException ex)
                {
                    throw new InvalidOperationException(
                        $"Cannot create SQLite database file at '{dataSource}'. " +
                        "The path may be invalid, or the file is already in use.", ex);
                }
            }, cancellationToken).ConfigureAwait(false);

            _logger?.LogInformation("SQLite database file created: {FilePath}", dataSource);
        }
    }

    /// <summary>
    /// Extracts the Data Source value from a SQLite connection string.
    /// </summary>
    private static string? ExtractDataSource(string connectionString)
    {
        try
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether the connection string targets an in-memory database.
    /// </summary>
    private static bool IsInMemoryConnectionString(string connectionString)
    {
        try
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
            return builder.DataSource == ":memory:" || builder.Mode == SqliteOpenMode.Memory;
        }
        catch
        {
            return false;
        }
    }
}
