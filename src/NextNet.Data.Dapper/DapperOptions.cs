namespace NextNet.Data.Dapper;

/// <summary>
/// Provider-specific options for configuring the Dapper data provider.
/// Passed to <c>UseDapper()</c> via the setup delegate.
/// </summary>
/// <remarks>
/// <para>
/// These options control connection pooling behavior, command timeouts, health check registration,
/// and repository lifetimes. Most options map to SQL Client connection string properties.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData()
///     .UseDapper(options =>
///     {
///         options.ConnectionName = "Default";
///         options.CommandTimeoutSeconds = 60;
///         options.EnablePooling = true;
///         options.MaxPoolSize = 50;
///         options.EnableSqlLogging = true;
///     });
/// </code>
/// </example>
/// </remarks>
public sealed record DapperOptions
{
    /// <summary>
    /// Gets or sets the default command timeout in seconds. Defaults to 30.
    /// Applied to all queries executed through this provider.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the name of the default connection from <see cref="DataConfig.Connections"/>.
    /// Defaults to <c>"Default"</c>.
    /// </summary>
    public string ConnectionName { get; set; } = "Default";

    /// <summary>
    /// Gets or sets whether to enable SQL Client connection pooling.
    /// Defaults to <c>true</c>. Disable for short-lived or test scenarios.
    /// </summary>
    public bool EnablePooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum pool size. Defaults to 0.
    /// </summary>
    public int MinPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum pool size. Defaults to 100.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the connection lifetime in seconds. Defaults to 0 (no limit).
    /// When exceeded, the connection is removed from the pool.
    /// </summary>
    public int ConnectionLifetimeSeconds { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether to enable retry on transient failures.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableRetryOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum retry count for transient failures. Defaults to 3.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to register default health checks. Defaults to <c>true</c>.
    /// </summary>
    public bool RegisterHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the service lifetime for generated repositories. Defaults to <see cref="ServiceLifetime.Scoped"/>.
    /// </summary>
    public ServiceLifetime RepositoryLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Gets or sets whether to log SQL queries. Defaults to <c>false</c>.
    /// Enable in development for query diagnostics.
    /// </summary>
    public bool EnableSqlLogging { get; set; } = false;
}

/// <summary>
/// Dapper-specific options for scaffold code generation.
/// Controls SQL dialect, pagination style, and naming conventions.
/// </summary>
/// <remarks>
/// <para>
/// These options are consumed by <see cref="DapperScaffoldProvider"/> when generating
/// model classes, repository classes, and CRUD server actions.
/// </para>
/// </remarks>
public sealed record DapperScaffoldOptions
{
    /// <summary>
    /// Gets or sets the pagination SQL style for generated repositories.
    /// Defaults to <see cref="PaginationStyle.OffsetFetch"/> (SQL Server).
    /// </summary>
    public PaginationStyle PaginationStyle { get; set; } = PaginationStyle.OffsetFetch;

    /// <summary>
    /// Gets or sets whether to generate <c>SCOPE_IDENTITY()</c> calls in insert methods.
    /// Defaults to <c>true</c>. Disable for databases that don't support it.
    /// </summary>
    public bool UseScopeIdentity { get; set; } = true;

    /// <summary>
    /// Gets or sets the column name quoting character. Defaults to <c>[]</c> for SQL Server.
    /// Set to <c>""</c> for PostgreSQL, <c>``</c> for MySQL.
    /// </summary>
    public string QuoteCharacter { get; set; } = "[]";

    /// <summary>
    /// Gets or sets whether to generate the <c>[TableName]</c> attribute on models.
    /// Defaults to <c>true</c>. Dapper does not require it but it aids documentation.
    /// </summary>
    public bool GenerateTableAttribute { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to generate a <c>[Column]</c> attribute on each property.
    /// Defaults to <c>false</c> — Dapper maps by convention.
    /// </summary>
    public bool GenerateColumnAttributes { get; set; } = false;
}
