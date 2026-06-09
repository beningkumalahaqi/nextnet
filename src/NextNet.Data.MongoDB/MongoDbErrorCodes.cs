namespace NextNet.Data.MongoDB;

/// <summary>
/// Defines standardized error codes for MongoDB data provider exceptions.
/// Each code corresponds to a specific failure scenario and is prefixed
/// in exception messages for traceability.
/// </summary>
public static class MongoDbErrorCodes
{
    /// <summary>
    /// Connection to MongoDB server failed.
    /// </summary>
    public const string ConnectionFailed = "DS-490";

    /// <summary>
    /// The requested collection was not found in the database.
    /// </summary>
    public const string CollectionNotFound = "DS-491";

    /// <summary>
    /// Serialization or deserialization of a BSON document failed.
    /// </summary>
    public const string DocumentSerializationFailed = "DS-492";

    /// <summary>
    /// A duplicate key error occurred during an insert or update operation.
    /// </summary>
    public const string DuplicateKeyError = "DS-493";

    /// <summary>
    /// A MongoDB query exceeded the configured timeout.
    /// </summary>
    public const string QueryTimeout = "DS-494";

    /// <summary>
    /// The provided MongoDB connection string is invalid or malformed.
    /// </summary>
    public const string InvalidConnectionString = "DS-495";

    /// <summary>
    /// A bulk write operation failed.
    /// </summary>
    public const string BulkWriteFailed = "DS-496";

    /// <summary>
    /// Index creation or management failed.
    /// </summary>
    public const string IndexCreationFailed = "DS-497";

    /// <summary>
    /// Change stream subscription or processing failed.
    /// </summary>
    public const string ChangeStreamFailed = "DS-498";

    /// <summary>
    /// A multi-document transaction failed.
    /// </summary>
    public const string TransactionFailed = "DS-499";

    /// <summary>
    /// A GridFS file operation failed.
    /// </summary>
    public const string GridFsError = "DS-500";

    /// <summary>
    /// An aggregation pipeline operation failed.
    /// </summary>
    public const string AggregationFailed = "DS-501";

    /// <summary>
    /// MongoDB provider configuration is invalid or incomplete.
    /// </summary>
    public const string ConfigurationInvalid = "DS-502";

    /// <summary>
    /// The MongoDB provider has not been configured or initialized.
    /// </summary>
    public const string ProviderNotConfigured = "DS-503";

    /// <summary>
    /// The MongoDB repository has not been properly initialized.
    /// </summary>
    public const string RepositoryNotInitialized = "DS-504";
}
