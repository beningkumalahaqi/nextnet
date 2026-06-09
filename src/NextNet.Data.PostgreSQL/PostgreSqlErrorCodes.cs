namespace NextNet.Data.PostgreSQL;

/// <summary>
/// Defines standardized error codes for the NextNet.Data.PostgreSQL package (DS-510 to DS-519).
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used as prefixes in exception messages to enable structured
/// error identification, logging, and monitoring. Each code maps to a specific error
/// scenario within the PostgreSQL data provider.
/// </para>
/// </remarks>
public static class PostgreSqlErrorCodes
{
    /// <summary>
    /// The connection to the PostgreSQL server could not be established.
    /// </summary>
    public const string ConnectionFailed = "DS-510";

    /// <summary>
    /// SSL/TLS negotiation with the PostgreSQL server failed.
    /// </summary>
    public const string SslNegotiationFailed = "DS-511";

    /// <summary>
    /// The PostgreSQL connection string is invalid or malformed.
    /// </summary>
    public const string ConnectionStringInvalid = "DS-512";

    /// <summary>
    /// The Npgsql connection pool has been exhausted.
    /// </summary>
    public const string PoolExhausted = "DS-513";

    /// <summary>
    /// A database query or command has timed out.
    /// </summary>
    public const string QueryTimeout = "DS-514";

    /// <summary>
    /// An Entity Framework migration operation failed.
    /// </summary>
    public const string MigrationFailed = "DS-515";

    /// <summary>
    /// The Docker Compose template could not be loaded or rendered.
    /// </summary>
    public const string DockerComposeFailed = "DS-516";

    /// <summary>
    /// The PostgreSQL provider configuration is invalid.
    /// </summary>
    public const string ConfigurationInvalid = "DS-517";

    /// <summary>
    /// A PostgreSQL replication slot operation failed.
    /// </summary>
    public const string ReplicationSlotError = "DS-518";

    /// <summary>
    /// A PostgreSQL COPY operation (import/export) failed.
    /// </summary>
    public const string CopyOperationFailed = "DS-519";
}
