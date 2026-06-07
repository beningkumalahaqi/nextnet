namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Manages the <c>_nextnetMigrations</c> collection that tracks applied migration history.
/// </summary>
/// <remarks>
/// <para>
/// The migration history collection stores documents with the following structure:
/// <code>
/// {
///   "_id": ObjectId,
///   "migrationName": "20260606000001_AddUserIndexes",
///   "appliedAt": ISODate("2026-06-06T00:00:01Z")
/// }
/// </code>
/// </para>
/// </remarks>
internal sealed class MigrationHistoryRepository
{
    private readonly IMongoDatabase _database;
    private readonly string _collectionName;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationHistoryRepository"/>.
    /// </summary>
    /// <param name="database">The MongoDB database.</param>
    /// <param name="collectionName">The name of the history collection. Defaults to "_nextnetMigrations".</param>
    /// <param name="logger">Optional logger.</param>
    public MigrationHistoryRepository(
        IMongoDatabase database,
        string? collectionName = null,
        ILogger? logger = null)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _collectionName = collectionName ?? "_nextnetMigrations";
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Ensures the migration history collection exists.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
    {
        var filter = new BsonDocument("name", _collectionName);
        var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter }, cancellationToken);
        var exists = await collections.AnyAsync(cancellationToken);

        if (!exists)
        {
            await _database.CreateCollectionAsync(_collectionName, cancellationToken: cancellationToken);
            _logger.LogInformation("Created migration history collection '{CollectionName}'.", _collectionName);
        }
    }

    /// <summary>
    /// Gets the set of already-applied migration names.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A set of migration names.</returns>
    public async Task<HashSet<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        var collection = _database.GetCollection<MigrationHistoryRecord>(_collectionName);
        var records = await collection.Find(FilterDefinition<MigrationHistoryRecord>.Empty)
            .SortBy(r => r.MigrationName)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(records.Select(r => r.MigrationName));
    }

    /// <summary>
    /// Gets the most recently applied migration name.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The migration name, or <c>null</c> if no migrations have been applied.</returns>
    public async Task<string?> GetLastMigrationNameAsync(CancellationToken cancellationToken = default)
    {
        var collection = _database.GetCollection<MigrationHistoryRecord>(_collectionName);
        var last = await collection.Find(FilterDefinition<MigrationHistoryRecord>.Empty)
            .SortByDescending(r => r.AppliedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return last?.MigrationName;
    }

    /// <summary>
    /// Records a migration as applied.
    /// </summary>
    /// <param name="migrationName">The migration name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task RecordMigrationAsync(string migrationName, CancellationToken cancellationToken = default)
    {
        var collection = _database.GetCollection<MigrationHistoryRecord>(_collectionName);
        var record = new MigrationHistoryRecord
        {
            MigrationName = migrationName,
            AppliedAt = DateTime.UtcNow,
        };

        await collection.InsertOneAsync(record, cancellationToken: cancellationToken);
        _logger.LogInformation("Recorded migration '{MigrationName}' in history.", migrationName);
    }

    /// <summary>
    /// Removes a migration record (for rollback).
    /// </summary>
    /// <param name="migrationName">The migration name to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the record was removed; <c>false</c> if not found.</returns>
    public async Task<bool> RemoveMigrationAsync(string migrationName, CancellationToken cancellationToken = default)
    {
        var collection = _database.GetCollection<MigrationHistoryRecord>(_collectionName);
        var filter = Builders<MigrationHistoryRecord>.Filter.Eq(r => r.MigrationName, migrationName);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount > 0)
        {
            _logger.LogInformation("Removed migration '{MigrationName}' from history.", migrationName);
            return true;
        }

        return false;
    }
}

/// <summary>
/// Represents a migration history record in the <c>_nextnetMigrations</c> collection.
/// </summary>
internal sealed class MigrationHistoryRecord
{
    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    /// Gets or sets the migration name.
    /// </summary>
    public string MigrationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the migration was applied.
    /// </summary>
    public DateTime AppliedAt { get; set; }
}
