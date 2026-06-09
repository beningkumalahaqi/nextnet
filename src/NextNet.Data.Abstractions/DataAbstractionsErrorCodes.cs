namespace NextNet.Data.Abstractions;

/// <summary>
/// Defines standardized error codes for the NextNet Data Abstractions layer.
/// Each code corresponds to a specific error condition and is embedded in
/// exception messages as a prefix (e.g., "[DS-400]").
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used throughout the NextNet Data framework to enable
/// programmatic error identification, automated handling, and diagnostic tooling.
/// Exception messages are prefixed with the error code in square brackets.
/// </para>
/// </remarks>
public static class DataAbstractionsErrorCodes
{
    /// <summary>
    /// A repository has not been configured for the requested entity type.
    /// </summary>
    public const string RepositoryNotConfigured = "DS-400";

    /// <summary>
    /// The entity type has not been registered with the data layer.
    /// </summary>
    public const string EntityNotRegistered = "DS-401";

    /// <summary>
    /// A query execution operation failed unexpectedly.
    /// </summary>
    public const string QueryExecutionFailed = "DS-402";

    /// <summary>
    /// A connection to the database could not be established.
    /// </summary>
    public const string ConnectionFailed = "DS-403";

    /// <summary>
    /// A database transaction operation failed.
    /// </summary>
    public const string TransactionFailed = "DS-404";

    /// <summary>
    /// A migration operation failed (create, apply, or rollback).
    /// </summary>
    public const string MigrationFailed = "DS-405";

    /// <summary>
    /// The connection string is invalid or missing required values.
    /// </summary>
    public const string InvalidConnectionString = "DS-406";

    /// <summary>
    /// The requested data provider is not available or not registered.
    /// </summary>
    public const string ProviderNotAvailable = "DS-407";

    /// <summary>
    /// The data configuration is invalid or contains conflicting values.
    /// </summary>
    public const string ConfigurationInvalid = "DS-408";

    /// <summary>
    /// The requested entity was not found in the data store.
    /// </summary>
    public const string EntityNotFound = "DS-409";

    /// <summary>
    /// An entity with the same key has already been registered.
    /// </summary>
    public const string DuplicateEntityRegistration = "DS-410";

    /// <summary>
    /// The builder has already been built and cannot be modified further.
    /// </summary>
    public const string BuilderAlreadyBuilt = "DS-411";
}
