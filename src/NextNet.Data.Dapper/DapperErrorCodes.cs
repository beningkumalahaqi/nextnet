namespace NextNet.Data.Dapper;

/// <summary>
/// Defines standardized error codes for Dapper data provider exceptions.
/// Each code corresponds to a specific failure scenario and is prefixed
/// in exception messages for traceability.
/// </summary>
public static class DapperErrorCodes
{
    /// <summary>
    /// Connection to the database server failed.
    /// </summary>
    public const string ConnectionFailed = "DS-450";

    /// <summary>
    /// A SQL query execution failed.
    /// </summary>
    public const string QueryExecutionFailed = "DS-451";

    /// <summary>
    /// Mapping between query parameters and object properties failed.
    /// </summary>
    public const string ParameterMappingFailed = "DS-452";

    /// <summary>
    /// A .NET type mapping is not supported by the Dapper provider.
    /// </summary>
    public const string TypeMappingNotSupported = "DS-453";

    /// <summary>
    /// The specified stored procedure was not found in the database.
    /// </summary>
    public const string StoredProcedureNotFound = "DS-454";

    /// <summary>
    /// A database transaction failed.
    /// </summary>
    public const string TransactionFailed = "DS-455";

    /// <summary>
    /// The provided connection string is invalid or malformed.
    /// </summary>
    public const string InvalidConnectionString = "DS-456";

    /// <summary>
    /// Dapper provider configuration is invalid or incomplete.
    /// </summary>
    public const string ConfigurationInvalid = "DS-457";

    /// <summary>
    /// The SQL query contains a syntax error.
    /// </summary>
    public const string SqlSyntaxError = "DS-458";

    /// <summary>
    /// Mapping query results to the target type failed.
    /// </summary>
    public const string ResultMappingFailed = "DS-459";

    /// <summary>
    /// The requested entity was not found in the database.
    /// </summary>
    public const string EntityNotFound = "DS-460";
}
