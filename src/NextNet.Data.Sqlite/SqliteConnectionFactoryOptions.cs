namespace NextNet.Data.Sqlite;

/// <summary>
/// Options for configuring the <see cref="SqliteConnectionFactory"/> behavior.
/// Mapped from the <c>"data.connections.default"</c> section of <c>nextnet.config.json</c>
/// or set explicitly via <c>UseSqlite()</c>.
/// </summary>
/// <remarks>
/// <para>
/// The connection string is resolved with the following priority:
/// <list type="number">
///   <item><description><see cref="ConnectionString"/> (explicitly set)</description></item>
///   <item><description><see cref="DataSource"/> + <see cref="Mode"/> (auto-built)</description></item>
///   <item><description><c>"nextnet.config.json" → data.connections.default.connectionString</c></description></item>
/// </list>
/// </para>
/// <para>
/// When <see cref="InMemory"/> is <c>true</c>, <see cref="DataSource"/> is ignored and
/// an in-memory connection string is used instead.
/// </para>
/// </remarks>
public sealed class SqliteConnectionFactoryOptions
{
    /// <summary>
    /// Explicit connection string (e.g., <c>"Data Source=app.db;Cache=Shared"</c>).
    /// When set, takes precedence over <see cref="DataSource"/> and <see cref="Mode"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Path to the SQLite database file (e.g., <c>"app.db"</c>, <c>"/data/myapp.db"</c>).
    /// Used to build a connection string when <see cref="ConnectionString"/> is not set.
    /// Defaults to <c>"{project-name}.db"</c> in the project root.
    /// </summary>
    public string? DataSource { get; set; }

    /// <summary>
    /// SQLite open mode. Defaults to <see cref="SqliteOpenMode.ReadWriteCreate"/>.
    /// </summary>
    public SqliteOpenMode Mode { get; set; } = SqliteOpenMode.ReadWriteCreate;

    /// <summary>
    /// Whether to use an in-memory database instead of a file-based one.
    /// When <c>true</c>, <see cref="DataSource"/> is ignored and the connection string
    /// is set to <c>"Data Source=:memory:"</c> or a named in-memory database.
    /// </summary>
    public bool InMemory { get; set; }

    /// <summary>
    /// Cache mode for the SQLite connection. Defaults to <see cref="SqliteCacheMode.Default"/>.
    /// Use <see cref="SqliteCacheMode.Shared"/> for better concurrent read performance.
    /// </summary>
    public SqliteCacheMode Cache { get; set; } = SqliteCacheMode.Default;

    /// <summary>
    /// Whether to automatically create the database file if it does not exist.
    /// When <c>true</c>, the factory creates an empty file before returning a connection.
    /// Defaults to <c>true</c>.
    /// Ignored for in-memory connections.
    /// </summary>
    public bool EnsureDatabaseCreated { get; set; } = true;
}
