namespace NextNet.Data.MongoDB;

/// <summary>
/// Provider-specific options for configuring the MongoDB data provider.
/// Passed to <c>UseMongoDB()</c> via the setup delegate.
/// </summary>
/// <remarks>
/// <para>
/// These options control connection pooling behavior, timeouts, read/write preferences,
/// health check registration, and repository lifetimes. Most options map to
/// <see cref="MongoClientSettings"/> properties.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData()
///     .UseMongoDB(options =>
///     {
///         options.ConnectionName = "Default";
///         options.DefaultDatabaseName = "myapp";
///         options.MaxConnectionPoolSize = 50;
///         options.RetryWrites = true;
///     });
/// </code>
/// </example>
/// </remarks>
public sealed record MongoDbOptions
{
    /// <summary>
    /// Gets or sets the name of the default connection from <see cref="DataConfig.Connections"/>.
    /// Defaults to <c>"Default"</c>.
    /// </summary>
    public string ConnectionName { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the default database name to use when not specified
    /// in the MongoDB connection URI. If neither this nor the URI specifies
    /// a database name, initialization throws <c>ProviderConfigurationException</c>.
    /// </summary>
    public string? DefaultDatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the maximum connection pool size for each <see cref="MongoClient"/>.
    /// Defaults to <c>100</c>. Corresponds to <c>MongoClientSettings.MaxConnectionPoolSize</c>.
    /// </summary>
    public int MaxConnectionPoolSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the minimum connection pool size. Defaults to <c>0</c>.
    /// </summary>
    public int MinConnectionPoolSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum connection lifetime in seconds. Defaults to <c>0</c> (no limit).
    /// When a connection exceeds this age, it is removed from the pool.
    /// </summary>
    public int MaxConnectionLifeTimeSeconds { get; set; } = 0;

    /// <summary>
    /// Gets or sets the connection timeout in seconds. Defaults to <c>10</c>.
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the server selection timeout in seconds. Defaults to <c>30</c>.
    /// </summary>
    public int ServerSelectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the socket timeout in seconds. Defaults to <c>30</c>.
    /// </summary>
    public int SocketTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the read preference. Defaults to <see cref="ReadPreference.Primary"/>.
    /// Acceptable values: <c>"Primary"</c>, <c>"PrimaryPreferred"</c>, <c>"Secondary"</c>,
    /// <c>"SecondaryPreferred"</c>, <c>"Nearest"</c>.
    /// </summary>
    public string? ReadPreference { get; set; }

    /// <summary>
    /// Gets or sets the write concern. Defaults to <c>"Majority"</c>.
    /// Acceptable values: <c>"Acknowledged"</c>, <c>"Unacknowledged"</c>, <c>"W1"</c>,
    /// <c>"W2"</c>, <c>"W3"</c>, <c>"Majority"</c>, or a custom <c>w</c> value string.
    /// </summary>
    public string? WriteConcern { get; set; }

    /// <summary>
    /// Gets or sets whether to enable retryable writes. Defaults to <c>true</c>.
    /// </summary>
    public bool RetryWrites { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable retryable reads. Defaults to <c>true</c>.
    /// </summary>
    public bool RetryReads { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to register default health checks. Defaults to <c>true</c>.
    /// </summary>
    public bool RegisterHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the service lifetime for generated repositories. Defaults to <see cref="ServiceLifetime.Scoped"/>.
    /// </summary>
    public ServiceLifetime RepositoryLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Gets or sets whether to log MongoDB queries. Defaults to <c>false</c>.
    /// Enable in development for query diagnostics.
    /// </summary>
    public bool EnableQueryLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets a custom delegate for advanced <see cref="MongoClientSettings"/> configuration.
    /// Applied after all other options. Useful for custom cluster configurations,
    /// TLS settings, or Kerberos authentication.
    /// </summary>
    public Action<MongoClientSettings>? ConfigureClientSettings { get; set; }
}

/// <summary>
/// Options for configuring a single <see cref="MongoDbRepository{T}"/> instance.
/// </summary>
/// <remarks>
/// <para>
/// Controls collection name resolution, ID field behavior, and connection binding
/// for a specific entity type's repository.
/// </para>
/// <example>
/// <code>
/// var options = new MongoDbRepositoryOptions
/// {
///     CollectionName = "userAccounts",
///     ConnectionName = "Analytics",
///     TreatIdAsObjectId = true
/// };
/// </code>
/// </example>
/// </remarks>
public sealed record MongoDbRepositoryOptions
{
    /// <summary>
    /// Gets or sets the collection name for the entity. If not set, resolved from
    /// <c>CollectionNameAttribute</c> on the entity type, or defaults
    /// to the pluralized, camelCase version of the entity type name.
    /// </summary>
    public string? CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection to use for this repository.
    /// If not set, uses the provider's default connection.
    /// </summary>
    public string? ConnectionName { get; set; }

    /// <summary>
    /// Gets or sets the property or field name used as the document <c>_id</c>.
    /// Defaults to auto-discovery via <c>[BsonId]</c> attribute or <c>Id</c> convention.
    /// </summary>
    public string? IdFieldName { get; set; }

    /// <summary>
    /// Gets or sets whether the <c>_id</c> value should be treated as an
    /// <c>ObjectId</c> when querying. When <c>true</c>,
    /// string <c>_id</c> values are parsed as <c>ObjectId</c> for queries.
    /// Defaults to <c>true</c> to match common MongoDB conventions.
    /// </summary>
    public bool TreatIdAsObjectId { get; set; } = true;
}
