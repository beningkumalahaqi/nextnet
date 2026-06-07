using NextNet.Data.PostgreSQL.Configuration;

namespace NextNet.Data.PostgreSQL.Internal;

/// <summary>
/// Internal helper that implements the connection string resolution priority chain.
/// </summary>
/// <remarks>
/// <para>
/// The resolution priority is:
/// <list type="number">
///   <item><description>Explicit connection string parameter</description></item>
///   <item><description><see cref="PostgresConnectionFactoryOptions.ConnectionString"/></description></item>
///   <item><description>Build from <see cref="PostgresConnectionFactoryOptions"/> individual components</description></item>
///   <item><description>Environment variables: <c>PGHOST</c>, <c>PGPORT</c>, <c>PGDATABASE</c>, <c>PGUSER</c>, <c>PGPASSWORD</c>, <c>PGSSLMODE</c></description></item>
///   <item><description>Configuration file (<c>nextnet.config.json</c>)</description></item>
///   <item><description>Default: <c>Host=localhost;Port=5432;Database=nextnet;Username=postgres</c></description></item>
/// </list>
/// </para>
/// </remarks>
internal sealed class ConnectionStringResolver
{
    private readonly PostgresConnectionFactoryOptions _options;
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public ConnectionStringResolver(
        PostgresConnectionFactoryOptions options,
        string? explicitConnectionString = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _explicitConnectionString = explicitConnectionString;
    }

    /// <summary>
    /// Resolves the connection string using the priority chain.
    /// The result is cached after the first invocation.
    /// </summary>
    /// <returns>The resolved Npgsql connection string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no connection string can be resolved.</exception>
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
            return ApplyOptions(_explicitConnectionString);

        // Priority 2: Options.ConnectionString
        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
            return ApplyOptions(_options.ConnectionString);

        // Priority 3: Build from individual options components
        if (!string.IsNullOrWhiteSpace(_options.Database) ||
            !string.IsNullOrWhiteSpace(_options.Username))
        {
            return BuildFromComponents();
        }

        // Priority 4: Environment variables
        var envHost = GetEnvOrDefault("PGHOST", null);
        var envPort = GetEnvOrDefault("PGPORT", null);
        var envDatabase = GetEnvOrDefault("PGDATABASE", null);
        var envUsername = GetEnvOrDefault("PGUSER", null);
        var envPassword = GetEnvOrDefault("PGPASSWORD", null);

        if (envHost is not null || envDatabase is not null || envUsername is not null)
        {
            var finalHost = envHost ?? _options.Host;
            var finalPort = envPort is not null && int.TryParse(envPort, out var p) ? p : _options.Port;
            var finalDatabase = envDatabase ?? _options.Database ?? "nextnet";
            var finalUsername = envUsername ?? _options.Username ?? "postgres";

            var cs = PostgresConnectionStringBuilder.FromComponents(
                host: finalHost,
                port: finalPort,
                database: finalDatabase,
                username: finalUsername,
                password: envPassword);

            // Apply SSL from env var PGSSLMODE if set
            var envSslMode = GetEnvOrDefault("PGSSLMODE", null);
            if (envSslMode is not null && Enum.TryParse<PostgresSslMode>(envSslMode, ignoreCase: true, out var sslMode))
            {
                var sslOptions = new PostgresSslOptions { Mode = sslMode };
                cs = PostgresConnectionStringBuilder.ApplySsl(cs, sslOptions);
            }

            return cs;
        }

        // Priority 5: Config file (future: reads from nextnet.config.json)
        // For now, we skip config file reading since NextNetDataBuilder handles it.

        // Priority 6: Default fallback
        return BuildDefaultConnectionString();
    }

    private string BuildFromComponents()
    {
        var database = _options.Database
            ?? GetEnvOrDefault("PGDATABASE", null)
            ?? "nextnet";

        var username = _options.Username
            ?? GetEnvOrDefault("PGUSER", null)
            ?? "postgres";

        var password = _options.Password
            ?? GetEnvOrDefault("PGPASSWORD", null);

        var envPort = GetEnvOrDefault("PGPORT", null);
        var host = GetEnvOrDefault("PGHOST", null) ?? _options.Host;
        var port = envPort is not null && int.TryParse(envPort, out var p) ? p : _options.Port;

        var baseConnectionString = PostgresConnectionStringBuilder.FromComponents(
            host: host,
            port: port,
            database: database,
            username: username,
            password: password);

        // Apply SSL from options
        baseConnectionString = PostgresConnectionStringBuilder.ApplySsl(baseConnectionString, _options.Ssl);

        // Apply pooling from options
        if (_options.Pooling is not null)
        {
            baseConnectionString = PostgresConnectionStringBuilder.ApplyPooling(baseConnectionString, _options.Pooling);
        }

        // Apply additional options
        baseConnectionString = PostgresConnectionStringBuilder.ApplyOptions(baseConnectionString, _options);

        return baseConnectionString;
    }

    private string BuildDefaultConnectionString()
    {
        var database = GetEnvOrDefault("PGDATABASE", null) ?? "nextnet";
        var username = GetEnvOrDefault("PGUSER", null) ?? "postgres";
        var password = GetEnvOrDefault("PGPASSWORD", null);

        var envPort = GetEnvOrDefault("PGPORT", null);
        var host = GetEnvOrDefault("PGHOST", null) ?? "localhost";
        var port = envPort is not null && int.TryParse(envPort, out var p) ? p : 5432;

        var cs = PostgresConnectionStringBuilder.FromComponents(
            host: host,
            port: port,
            database: database,
            username: username,
            password: password);

        // Apply SSL from env var if set
        var envSslMode = GetEnvOrDefault("PGSSLMODE", null);
        if (envSslMode is not null && Enum.TryParse<PostgresSslMode>(envSslMode, ignoreCase: true, out var sslMode))
        {
            var sslOptions = new PostgresSslOptions { Mode = sslMode };
            cs = PostgresConnectionStringBuilder.ApplySsl(cs, sslOptions);
        }

        return cs;
    }

    private string ApplyOptions(string connectionString)
    {
        var result = connectionString;

        // Apply SSL from options if not already in the connection string
        result = PostgresConnectionStringBuilder.ApplySsl(result, _options.Ssl);

        // Apply pooling from options
        if (_options.Pooling is not null)
        {
            result = PostgresConnectionStringBuilder.ApplyPooling(result, _options.Pooling);
        }

        // Apply additional options
        result = PostgresConnectionStringBuilder.ApplyOptions(result, _options);

        return result;
    }

    private static string? GetEnvOrDefault(string variable, string? defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }
}
