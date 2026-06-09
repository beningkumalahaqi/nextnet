namespace NextNet.Data.MultiDb;

/// <summary>
/// Defines standardized error codes for the NextNet.Data.MultiDb package (DS-550 to DS-559).
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used as prefixes in exception messages to enable structured
/// error identification, logging, and monitoring. Each code maps to a specific error
/// scenario within the multi-database subsystem.
/// </para>
/// </remarks>
public static class MultiDbErrorCodes
{
    /// <summary>
    /// A data provider has not been registered for the requested connection.
    /// </summary>
    public const string ProviderNotRegistered = "DS-550";

    /// <summary>
    /// Multiple providers are configured for the same connection name.
    /// </summary>
    public const string MultipleProvidersConfigured = "DS-551";

    /// <summary>
    /// The default provider has not been set in the configuration.
    /// </summary>
    public const string DefaultProviderNotSet = "DS-552";

    /// <summary>
    /// Switching between databases or providers failed at runtime.
    /// </summary>
    public const string DatabaseSwitchFailed = "DS-553";

    /// <summary>
    /// The connection pool has been exhausted (all connections in use).
    /// </summary>
    public const string ConnectionPoolExhausted = "DS-554";

    /// <summary>
    /// The multi-database configuration is invalid or missing required settings.
    /// </summary>
    public const string ConfigurationInvalid = "DS-555";

    /// <summary>
    /// Resolution of the data provider for a connection failed.
    /// </summary>
    public const string ProviderResolutionFailed = "DS-556";

    /// <summary>
    /// A routing rule is invalid or cannot be applied.
    /// </summary>
    public const string RoutingRuleInvalid = "DS-557";

    /// <summary>
    /// A health check for a multi-database connection failed.
    /// </summary>
    public const string HealthCheckFailed = "DS-558";

    /// <summary>
    /// Automatic failover to a backup connection failed.
    /// </summary>
    public const string FailoverFailed = "DS-559";
}
