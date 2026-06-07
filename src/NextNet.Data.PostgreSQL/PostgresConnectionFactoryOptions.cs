namespace NextNet.Data.PostgreSQL;

/// <summary>
/// Options for configuring the <see cref="PostgresConnectionFactory"/> behavior and
/// the underlying Npgsql connection string.
/// </summary>
/// <remarks>
/// <para>
/// The final connection string is built by combining the explicit <see cref="ConnectionString"/>
/// (highest priority) or by composing individual components (<see cref="Host"/>, <see cref="Port"/>,
/// <see cref="Database"/>, <see cref="Username"/>, <see cref="Password"/>) with pooling and SSL
/// settings appended.
/// </para>
/// <para>
/// If neither explicit connection string nor individual components are set, the factory falls back
/// to environment variables (<c>PGHOST</c>, <c>PGPORT</c>, <c>PGDATABASE</c>, <c>PGUSER</c>,
/// <c>PGPASSWORD</c>) and finally to <c>nextnet.config.json</c>.
/// </para>
/// </remarks>
public sealed class PostgresConnectionFactoryOptions
{
    /// <summary>
    /// Explicit Npgsql connection string (e.g., <c>"Host=localhost;Port=5432;Database=myapp;Username=postgres;Password=secret"</c>).
    /// When set, takes precedence over individual components and environment variables.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// PostgreSQL server hostname or IP address. Defaults to <c>"localhost"</c>.
    /// Ignored when <see cref="ConnectionString"/> is set.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// PostgreSQL server port. Defaults to <c>5432</c>.
    /// Ignored when <see cref="ConnectionString"/> is set.
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// The database name to connect to.
    /// If not set, resolved from <c>PGDATABASE</c> environment variable or
    /// the project name from <c>nextnet.config.json</c>.
    /// Ignored when <see cref="ConnectionString"/> is set.
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// The database username.
    /// If not set, resolved from <c>PGUSER</c> environment variable or defaults to <c>"postgres"</c>.
    /// Ignored when <see cref="ConnectionString"/> is set.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// The database password.
    /// If not set, resolved from <c>PGPASSWORD</c> environment variable.
    /// Ignored when <see cref="ConnectionString"/> is set.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Connection pooling configuration. Defaults to pooled with standard settings.
    /// Set <c>null</c> to disable pooling entirely.
    /// </summary>
    public PostgresPoolingOptions? Pooling { get; set; } = new();

    /// <summary>
    /// SSL/TLS configuration. Defaults to <see cref="PostgresSslMode.Prefer"/>.
    /// </summary>
    public PostgresSslOptions Ssl { get; set; } = new();

    /// <summary>
    /// The application name reported to PostgreSQL for diagnostics.
    /// Visible in <c>pg_stat_activity</c> and logs.
    /// Defaults to the entry assembly name.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Connection timeout in seconds. Defaults to <c>15</c>.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 15;

    /// <summary>
    /// Command timeout in seconds. Defaults to <c>30</c>.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to enable TCP keepalive. Defaults to <c>true</c>.
    /// </summary>
    public bool EnableKeepAlive { get; set; } = true;

    /// <summary>
    /// TCP keepalive interval in seconds. Defaults to <c>60</c>.
    /// Only applies when <see cref="EnableKeepAlive"/> is <c>true</c>.
    /// </summary>
    public int KeepAliveIntervalSeconds { get; set; } = 60;
}
