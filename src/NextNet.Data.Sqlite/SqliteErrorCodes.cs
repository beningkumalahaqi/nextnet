namespace NextNet.Data.Sqlite;

/// <summary>
/// Defines standardized error codes for the NextNet.Data.Sqlite package (DS-530 to DS-539).
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used as prefixes in exception messages to enable structured
/// error identification, logging, and monitoring. Each code maps to a specific error
/// scenario within the SQLite data provider.
/// </para>
/// </remarks>
public static class SqliteErrorCodes
{
    /// <summary>
    /// The connection to the SQLite database could not be established.
    /// </summary>
    public const string ConnectionFailed = "DS-530";

    /// <summary>
    /// The SQLite database file was not found at the specified path.
    /// </summary>
    public const string DatabaseFileNotFound = "DS-531";

    /// <summary>
    /// The SQLite database is locked by another process or connection.
    /// </summary>
    public const string DatabaseLocked = "DS-532";

    /// <summary>
    /// An Entity Framework migration operation on SQLite failed.
    /// </summary>
    public const string MigrationFailed = "DS-533";

    /// <summary>
    /// The SQLite provider configuration is invalid.
    /// </summary>
    public const string ConfigurationInvalid = "DS-534";

    /// <summary>
    /// An in-memory SQLite database has expired or been disposed.
    /// </summary>
    public const string InMemoryDatabaseExpired = "DS-535";

    /// <summary>
    /// Failed to set the SQLite journal mode.
    /// </summary>
    public const string JournalModeFailed = "DS-536";

    /// <summary>
    /// A foreign key constraint was violated.
    /// </summary>
    public const string ForeignKeyConstraintFailed = "DS-537";

    /// <summary>
    /// A WAL (Write-Ahead Log) checkpoint operation failed.
    /// </summary>
    public const string WALCheckpointFailed = "DS-538";

    /// <summary>
    /// A VACUUM operation on the SQLite database failed.
    /// </summary>
    public const string VacuumFailed = "DS-539";
}
