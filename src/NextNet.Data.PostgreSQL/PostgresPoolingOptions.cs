namespace NextNet.Data.PostgreSQL;

/// <summary>
/// Connection pooling configuration for Npgsql connections.
/// Maps to Npgsql connection string parameters.
/// </summary>
/// <remarks>
/// <para>
/// Connection pooling is enabled by default in Npgsql. These options allow
/// tuning pool behavior for different deployment scenarios:
/// </para>
/// <list type="bullet">
///   <item>Development: small pool (Min=1, Max=10) to conserve resources</item>
///   <item>Production: larger pool (Min=5, Max=100) for concurrent workloads</item>
///   <item>Serverless: aggressive recycling (ConnectionIdleLifetime=60s)</item>
/// </list>
/// </remarks>
public sealed class PostgresPoolingOptions
{
    /// <summary>
    /// Whether connection pooling is enabled. Defaults to <c>true</c>.
    /// Set to <c>false</c> to disable pooling entirely (each connection is physical).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Minimum number of connections maintained in the pool. Defaults to <c>1</c>.
    /// </summary>
    public int MinPoolSize { get; set; } = 1;

    /// <summary>
    /// Maximum number of connections allowed in the pool. Defaults to <c>100</c>.
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Maximum lifetime (in seconds) for a connection in the pool. Defaults to <c>300</c> (5 minutes).
    /// Connections older than this are recycled on return to the pool.
    /// </summary>
    public int ConnectionLifetimeSeconds { get; set; } = 300;

    /// <summary>
    /// Idle lifetime (in seconds) before an idle connection is removed from the pool.
    /// Defaults to <c>60</c> (1 minute).
    /// </summary>
    public int ConnectionIdleLifetimeSeconds { get; set; } = 60;

    /// <summary>
    /// Whether to retry when a connection is obtained from the pool after a failed validation.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool RetryOnFailure { get; set; } = true;

    /// <summary>
    /// Whether to validate connections when they are retrieved from the pool.
    /// Defaults to <c>false</c> for performance; enable for environments with
    /// aggressive network timeouts (e.g., cloud load balancers).
    /// </summary>
    public bool ValidateOnRetrieve { get; set; } = false;
}
