namespace NextNet.Data.EntityFramework;

/// <summary>
/// Defines standardized error codes for Entity Framework Core data provider exceptions.
/// Each code corresponds to a specific failure scenario and is prefixed
/// in exception messages for traceability.
/// </summary>
public static class EntityFrameworkErrorCodes
{
    /// <summary>
    /// Connection to the database server failed.
    /// </summary>
    public const string ConnectionFailed = "DS-470";

    /// <summary>
    /// A database migration operation failed.
    /// </summary>
    public const string MigrationFailed = "DS-471";

    /// <summary>
    /// Translation of a LINQ query to SQL failed.
    /// </summary>
    public const string QueryTranslationFailed = "DS-472";

    /// <summary>
    /// A concurrency conflict was detected during save operations.
    /// </summary>
    public const string ConcurrencyConflict = "DS-473";

    /// <summary>
    /// The EF Core model definition is invalid or incomplete.
    /// </summary>
    public const string InvalidModelDefinition = "DS-474";

    /// <summary>
    /// The DbContext has not been configured or registered.
    /// </summary>
    public const string DbContextNotConfigured = "DS-475";

    /// <summary>
    /// A SaveChanges operation failed.
    /// </summary>
    public const string SaveChangesFailed = "DS-476";

    /// <summary>
    /// Creation of the database failed.
    /// </summary>
    public const string DatabaseCreationFailed = "DS-477";

    /// <summary>
    /// The database provider (e.g., SQL Server, SQLite) is not available.
    /// </summary>
    public const string ProviderNotAvailable = "DS-478";

    /// <summary>
    /// EF Core provider configuration is invalid or incomplete.
    /// </summary>
    public const string ConfigurationInvalid = "DS-479";
}
