namespace NextNet.Data.Sqlite;

/// <summary>
/// Static helpers for building SQLite connection strings consistently.
/// Used internally by <see cref="SqliteConnectionFactory"/> and the CLI command.
/// </summary>
/// <remarks>
/// <para>
/// This class provides factory methods for constructing SQLite connection strings
/// from file paths or in-memory database names. It ensures consistent formatting
/// and resolves relative file paths to absolute paths.
/// </para>
/// </remarks>
public static class SqliteConnectionStringBuilder
{
    /// <summary>
    /// Builds a connection string for the specified file path.
    /// </summary>
    /// <param name="filePath">Path to the SQLite database file.</param>
    /// <param name="cache">Optional cache mode. Defaults to <see cref="SqliteCacheMode.Default"/>.</param>
    /// <returns>A SQLite connection string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    /// <example>
    /// <code>
    /// var connString = SqliteConnectionStringBuilder.FromFile("app.db");
    /// // → "Data Source=app.db"
    ///
    /// var connString = SqliteConnectionStringBuilder.FromFile("app.db", SqliteCacheMode.Shared);
    /// // → "Data Source=app.db;Cache=Shared"
    /// </code>
    /// </example>
    public static string FromFile(string filePath, SqliteCacheMode cache = SqliteCacheMode.Default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must not be null or empty.", nameof(filePath));

        var resolvedPath = Path.GetFullPath(filePath);
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = resolvedPath
        };

        if (cache != SqliteCacheMode.Default)
        {
            builder.Cache = cache;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Builds an in-memory connection string.
    /// </summary>
    /// <param name="name">
    /// Optional logical name for the in-memory database.
    /// Enables shared in-memory databases across connections.
    /// When provided, uses <c>Mode=Memory;Cache=Shared</c> to allow
    /// multiple connections to share the same in-memory database.
    /// </param>
    /// <returns>An in-memory SQLite connection string.</returns>
    /// <example>
    /// <code>
    /// var connString = SqliteConnectionStringBuilder.InMemory();
    /// // → "Data Source=:memory:"
    ///
    /// var connString = SqliteConnectionStringBuilder.InMemory("myapp");
    /// // → "Data Source=myapp;Mode=Memory;Cache=Shared"
    /// </code>
    /// </example>
    public static string InMemory(string? name = null)
    {
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();

        if (string.IsNullOrWhiteSpace(name))
        {
            builder.DataSource = ":memory:";
        }
        else
        {
            builder.DataSource = name;
            builder.Mode = SqliteOpenMode.Memory;
            builder.Cache = SqliteCacheMode.Shared;
        }

        return builder.ConnectionString;
    }
}
