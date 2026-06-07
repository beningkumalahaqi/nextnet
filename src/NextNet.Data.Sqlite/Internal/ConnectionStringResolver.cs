namespace NextNet.Data.Sqlite.Internal;

/// <summary>
/// Internal helper that implements the connection string resolution priority chain.
/// </summary>
/// <remarks>
/// <para>
/// The resolution priority is:
/// <list type="number">
///   <item><description>Explicit connection string parameter</description></item>
///   <item><description><see cref="SqliteConnectionFactoryOptions.ConnectionString"/></description></item>
///   <item><description><see cref="SqliteConnectionFactoryOptions.DataSource"/> → built via <see cref="SqliteConnectionStringBuilder.FromFile"/></description></item>
///   <item><description><see cref="SqliteConnectionFactoryOptions.InMemory"/> → built via <see cref="SqliteConnectionStringBuilder.InMemory"/></description></item>
///   <item><description>Configuration fallback (future: reads from <c>nextnet.config.json</c>)</description></item>
///   <item><description>Default: <c>"Data Source=database.db"</c></description></item>
/// </list>
/// </para>
/// <para>
/// The resolved connection string is cached once computed.
/// </para>
/// </remarks>
internal sealed class ConnectionStringResolver
{
    private readonly SqliteConnectionFactoryOptions _options;
    private readonly string? _explicitConnectionString;
    private string? _cachedConnectionString;
    private readonly object _cacheLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringResolver"/> class.
    /// </summary>
    /// <param name="options">The factory configuration options.</param>
    /// <param name="explicitConnectionString">
    /// An optional explicit connection string that takes highest priority.
    /// </param>
    public ConnectionStringResolver(
        SqliteConnectionFactoryOptions options,
        string? explicitConnectionString = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _explicitConnectionString = explicitConnectionString;
    }

    /// <summary>
    /// Resolves the connection string using the priority chain.
    /// The result is cached after the first invocation.
    /// </summary>
    /// <returns>The resolved SQLite connection string.</returns>
    public string Resolve()
    {
        if (_cachedConnectionString is not null)
            return _cachedConnectionString;

        lock (_cacheLock)
        {
            if (_cachedConnectionString is not null)
                return _cachedConnectionString;

            _cachedConnectionString = ResolveCore();
            return _cachedConnectionString;
        }
    }

    /// <summary>
    /// Resets the cached connection string. Used for testing.
    /// </summary>
    internal void ResetCache()
    {
        lock (_cacheLock)
        {
            _cachedConnectionString = null;
        }
    }

    private string ResolveCore()
    {
        // Priority 1: Explicit connection string parameter
        if (!string.IsNullOrWhiteSpace(_explicitConnectionString))
            return _explicitConnectionString;

        // Priority 2: Options.ConnectionString
        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            return _options.ConnectionString;

        // Priority 3: InMemory flag
        if (_options.InMemory)
            return SqliteConnectionStringBuilder.InMemory();

        // Priority 4: Options.DataSource
        if (!string.IsNullOrWhiteSpace(_options.DataSource))
            return SqliteConnectionStringBuilder.FromFile(_options.DataSource, _options.Cache);

        // Priority 5: Default fallback
        return SqliteConnectionStringBuilder.FromFile("database.db", _options.Cache);
    }
}
