namespace NextNet.Data.PostgreSQL;

/// <summary>
/// Static helpers for building Npgsql connection strings consistently.
/// Used internally by <see cref="PostgresConnectionFactory"/> and the CLI command.
/// </summary>
/// <remarks>
/// <para>
/// This class provides factory methods for constructing Npgsql connection strings
/// from individual components. All methods use <see cref="NpgsqlConnectionStringBuilder"/>
/// internally for proper escaping of special characters.
/// </para>
/// </remarks>
public static class PostgresConnectionStringBuilder
{
    /// <summary>
    /// Builds a connection string from individual components.
    /// </summary>
    /// <param name="host">Server hostname (default: localhost).</param>
    /// <param name="port">Server port (default: 5432).</param>
    /// <param name="database">Database name.</param>
    /// <param name="username">Database username.</param>
    /// <param name="password">Database password (optional for trust auth).</param>
    /// <param name="ssl">SSL options (optional).</param>
    /// <param name="pooling">Pooling options (optional).</param>
    /// <returns>An Npgsql connection string.</returns>
    /// <example>
    /// <code>
    /// var cs = PostgresConnectionStringBuilder.FromComponents(
    ///     host: "localhost",
    ///     port: 5432,
    ///     database: "myapp",
    ///     username: "postgres",
    ///     password: "secret");
    /// // → "Host=localhost;Port=5432;Database=myapp;Username=postgres;Password=secret"
    /// </code>
    /// </example>
    public static string FromComponents(
        string host = "localhost",
        int port = 5432,
        string? database = null,
        string? username = null,
        string? password = null,
        PostgresSslOptions? ssl = null,
        PostgresPoolingOptions? pooling = null)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
        };

        if (!string.IsNullOrWhiteSpace(database))
            builder.Database = database;

        if (!string.IsNullOrWhiteSpace(username))
            builder.Username = username;

        if (!string.IsNullOrWhiteSpace(password))
            builder.Password = password;

        // Apply SSL options if provided
        if (ssl is not null)
        {
            ApplySslToBuilder(builder, ssl);
        }

        // Apply pooling options if provided
        if (pooling is not null)
        {
            ApplyPoolingToBuilder(builder, pooling);
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Builds a connection string for a local Docker PostgreSQL instance.
    /// Uses default Docker PostgreSQL credentials (postgres/postgres).
    /// </summary>
    /// <param name="database">Database name.</param>
    /// <param name="port">Host-mapped port (default: 5432).</param>
    /// <returns>An Npgsql connection string for Docker.</returns>
    /// <example>
    /// <code>
    /// var cs = PostgresConnectionStringBuilder.ForDocker("myapp");
    /// // → "Host=localhost;Port=5432;Database=myapp;Username=postgres;Password=postgres"
    /// </code>
    /// </example>
    public static string ForDocker(string database, int port = 5432)
    {
        return FromComponents(
            host: "localhost",
            port: port,
            database: database,
            username: "postgres",
            password: "postgres");
    }

    /// <summary>
    /// Applies SSL options to an existing connection string.
    /// Adds or replaces SSL-related parameters.
    /// </summary>
    /// <param name="connectionString">The base connection string.</param>
    /// <param name="ssl">SSL options to apply.</param>
    /// <returns>The connection string with SSL parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ssl"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    public static string ApplySsl(string connectionString, PostgresSslOptions ssl)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        ArgumentNullException.ThrowIfNull(ssl);

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        ApplySslToBuilder(builder, ssl);
        return builder.ConnectionString;
    }

    /// <summary>
    /// Applies pooling options to an existing connection string.
    /// Adds or replaces pool-related parameters.
    /// </summary>
    /// <param name="connectionString">The base connection string.</param>
    /// <param name="pooling">Pooling options to apply.</param>
    /// <returns>The connection string with pooling parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pooling"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    public static string ApplyPooling(string connectionString, PostgresPoolingOptions pooling)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        ArgumentNullException.ThrowIfNull(pooling);

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        ApplyPoolingToBuilder(builder, pooling);
        return builder.ConnectionString;
    }

    /// <summary>
    /// Applies additional options (ApplicationName, Timeout, KeepAlive) to a connection string.
    /// </summary>
    /// <param name="connectionString">The base connection string.</param>
    /// <param name="options">The options to apply.</param>
    /// <returns>The connection string with additional parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    internal static string ApplyOptions(string connectionString, PostgresConnectionFactoryOptions options)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        ArgumentNullException.ThrowIfNull(options);

        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        // Application name
        if (!string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            builder.ApplicationName = options.ApplicationName;
        }

        // Timeout
        if (options.ConnectionTimeoutSeconds > 0)
        {
            builder.Timeout = options.ConnectionTimeoutSeconds;
        }

        // Keepalive
        if (options.EnableKeepAlive && options.KeepAliveIntervalSeconds > 0)
        {
            builder.KeepAlive = options.KeepAliveIntervalSeconds;
        }

        return builder.ConnectionString;
    }

    private static void ApplySslToBuilder(NpgsqlConnectionStringBuilder builder, PostgresSslOptions ssl)
    {
        builder.SslMode = (SslMode)(int)ssl.Mode;

        if (!string.IsNullOrWhiteSpace(ssl.ClientCertificatePath))
        {
            builder.SslCertificate = ssl.ClientCertificatePath;
        }

        if (!string.IsNullOrWhiteSpace(ssl.RootCertificatePath))
        {
            builder.RootCertificate = ssl.RootCertificatePath;
        }

        // Note: Client certificate password is not supported as a direct property on
        // NpgsqlConnectionStringBuilder in Npgsql 8.x. Use the
        // NpgsqlConnection.ProvideClientCertificatesCallback for password-protected
        // client certificates.
    }

    private static void ApplyPoolingToBuilder(NpgsqlConnectionStringBuilder builder, PostgresPoolingOptions pooling)
    {
        builder.Pooling = pooling.Enabled;

        if (pooling.Enabled)
        {
            builder.MinPoolSize = pooling.MinPoolSize;
            builder.MaxPoolSize = pooling.MaxPoolSize;
            builder.ConnectionLifetime = pooling.ConnectionLifetimeSeconds;
            builder.ConnectionIdleLifetime = pooling.ConnectionIdleLifetimeSeconds;
        }
    }
}
