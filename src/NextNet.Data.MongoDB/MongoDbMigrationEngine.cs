using NextNet.Data.MongoDB.Internal;

namespace NextNet.Data.MongoDB;

/// <summary>
/// MongoDB implementation of <see cref="IMigrationEngine"/>.
/// MongoDB is schema-less, so traditional migrations (DDL changes) do not apply.
/// This engine manages index creation, schema version tracking using a
/// <c>_nextnetMigrations</c> collection in each database.
/// </summary>
/// <remarks>
/// <para>
/// MongoDB does not have a schema migration system like relational databases.
/// Instead, this engine handles:
/// <list type="bullet">
///   <item><description><b>Index management</b> — creating and dropping indexes on collections</description></item>
///   <item><description><b>Schema version tracking</b> — recording applied schema versions in a
///       <c>_nextnetMigrations</c> collection</description></item>
/// </list>
/// </para>
/// <para>
/// Index definitions and schema versions are expressed as JSON migration files
/// in the <c>Migrations/</c> directory.
/// </para>
/// <para>
/// Migration file format (<c>Migrations/20260606000001_AddUserIndexes.json</c>):
/// <code>
/// {
///   "name": "20260606000001_AddUserIndexes",
///   "createdAt": "2026-06-06T00:00:01Z",
///   "operations": [
///     {
///       "type": "createIndex",
///       "collection": "users",
///       "keys": { "email": 1 },
///       "options": { "unique": true, "name": "idx_users_email_unique" }
///     }
///   ],
///   "rollback": [
///     {
///       "type": "dropIndex",
///       "collection": "users",
///       "indexName": "idx_users_email_unique"
///     }
///   ]
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class MongoDbMigrationEngine : IMigrationEngine
{
    private readonly MongoClientManager _clientManager;
    private readonly MigrationConfig? _config;
    private readonly ILogger<MongoDbMigrationEngine> _logger;
    private readonly MigrationFileParser _fileParser;
    private readonly string _connectionName;

    /// <summary>
    /// Initializes a new instance of <see cref="MongoDbMigrationEngine"/>.
    /// </summary>
    /// <param name="clientManager">The MongoDB client manager.</param>
    /// <param name="config">Optional migration configuration (directory, collection name for history).</param>
    /// <param name="connectionName">The name of the connection to use. Defaults to "Default".</param>
    /// <param name="logger">Optional logger for migration operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="clientManager"/> is null.</exception>
    public MongoDbMigrationEngine(
        MongoClientManager clientManager,
        MigrationConfig? config = null,
        string? connectionName = null,
        ILogger<MongoDbMigrationEngine>? logger = null)
    {
        _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        _config = config;
        _connectionName = connectionName ?? "Default";
        _logger = logger ?? new NullLogger<MongoDbMigrationEngine>();

        var migrationsDir = _config?.Directory ?? "Migrations";
        _fileParser = new MigrationFileParser(migrationsDir, _logger);
    }

    /// <summary>
    /// Creates a new migration file (JSON) with the specified name.
    /// Produces a migration definition file with index and validation schema stubs.
    /// </summary>
    /// <param name="name">A descriptive name for the migration (e.g., "AddUserIndexes").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public Task<MigrationResult> AddMigrationAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Migration name must not be null or empty.");
        }

        _logger.LogInformation("Creating migration '{MigrationName}'...", name);

        try
        {
            var filePath = _fileParser.CreateMigrationFile(name, DateTimeOffset.UtcNow);

            _logger.LogInformation("Migration '{MigrationName}' created successfully at {FilePath}.", name, filePath);
            return Task.FromResult(new MigrationResult(
                true,
                $"Migration '{name}' created successfully. File: {filePath}",
                MigrationName: name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create migration '{MigrationName}'.", name);
            return Task.FromResult(new MigrationResult(
                false,
                $"Failed to create migration '{name}': {ex.Message}",
                Errors: new[] { ex.ToString() }));
        }
    }

    /// <summary>
    /// Discovers and applies all pending migration definitions not yet recorded
    /// in the <c>_nextnetMigrations</c> collection. Each migration is executed
    /// within its own scope (MongoDB does not support multi-document DDL transactions).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> with the count of applied migrations.</returns>
    public async Task<MigrationResult> ApplyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying pending MongoDB migrations...");

        // Check if there are any connections configured
        if (_clientManager.GetConnectionNames().Count == 0)
        {
            _logger.LogInformation("No connections configured. No migrations to apply.");
            return new MigrationResult(true, "No connections configured. No migrations to apply.", MigrationsApplied: 0);
        }

        try
        {
            var database = await _clientManager.GetDatabaseAsync(_connectionName, cancellationToken);
            var historyRepo = new MigrationHistoryRepository(database, _config?.HistoryTableName, _logger);

            // Ensure history collection exists
            await historyRepo.EnsureCollectionExistsAsync(cancellationToken);

            // Discover applied migrations
            var appliedMigrations = await historyRepo.GetAppliedMigrationsAsync(cancellationToken);

            // Discover pending migration files
            var pendingFiles = _fileParser.GetPendingMigrations(appliedMigrations);

            if (pendingFiles.Count == 0)
            {
                _logger.LogInformation("No pending MongoDB migrations to apply.");
                return new MigrationResult(true, "No pending migrations to apply.", MigrationsApplied: 0);
            }

            var appliedCount = 0;
            var errors = new List<string>();

            foreach (var filePath in pendingFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var definition = await _fileParser.ParseAsync(filePath);
                    _logger.LogInformation("Applying migration '{MigrationName}'...", definition.Name);

                    // Execute each operation
                    foreach (var operation in definition.Operations)
                    {
                        await ExecuteOperationAsync(database, operation, cancellationToken);
                    }

                    // Record migration in history
                    await historyRepo.RecordMigrationAsync(definition.Name, cancellationToken);

                    appliedCount++;
                    _logger.LogInformation("Migration '{MigrationName}' applied successfully.", definition.Name);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Migration '{Path.GetFileNameWithoutExtension(filePath)}' failed: {ex.Message}";
                    _logger.LogError(ex, "Migration failed for file '{FilePath}'.", filePath);
                    errors.Add(errorMsg);
                }
            }

            _logger.LogInformation("Applied {Count} MongoDB migration(s) successfully.", appliedCount);
            return new MigrationResult(
                true,
                $"Applied {appliedCount} migration(s) successfully.",
                MigrationsApplied: appliedCount,
                Errors: errors.Count > 0 ? errors.AsReadOnly() : null);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Migration application was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply MongoDB migrations.");
            return new MigrationResult(
                false,
                $"Failed to apply migrations: {ex.Message}",
                Errors: new[] { ex.ToString() });
        }
    }

    /// <summary>
    /// Rolls back the most recently applied migration by reverting its index changes.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    public async Task<MigrationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rolling back the last MongoDB migration...");

        // Check if there are any connections configured
        if (_clientManager.GetConnectionNames().Count == 0)
        {
            _logger.LogInformation("No connections configured. No migrations to roll back.");
            return new MigrationResult(true, "No connections configured. No migrations to roll back.", MigrationsApplied: 0);
        }

        try
        {
            var database = await _clientManager.GetDatabaseAsync(_connectionName, cancellationToken);
            var historyRepo = new MigrationHistoryRepository(database, _config?.HistoryTableName, _logger);

            await historyRepo.EnsureCollectionExistsAsync(cancellationToken);

            var lastMigrationName = await historyRepo.GetLastMigrationNameAsync(cancellationToken);

            if (lastMigrationName is null)
            {
                _logger.LogInformation("No MongoDB migrations to roll back.");
                return new MigrationResult(true, "No migrations to roll back.", MigrationsApplied: 0);
            }

            // Find the migration file
            var migrationsDir = _config?.Directory ?? "Migrations";
            var filePath = Directory.GetFiles(migrationsDir, $"{lastMigrationName}.json").FirstOrDefault();

            if (filePath is null)
            {
                return new MigrationResult(
                    false,
                    $"Cannot find migration file for '{lastMigrationName}'.",
                    Errors: new[] { $"File '{lastMigrationName}.json' not found in '{migrationsDir}'." });
            }

            var definition = await _fileParser.ParseAsync(filePath);

            if (definition.Rollback.Count == 0)
            {
                return new MigrationResult(
                    false,
                    $"No rollback operations defined in migration '{lastMigrationName}'.",
                    MigrationName: lastMigrationName);
            }

            // Execute rollback operations in reverse order
            for (var i = definition.Rollback.Count - 1; i >= 0; i--)
            {
                await ExecuteOperationAsync(database, definition.Rollback[i], cancellationToken);
            }

            // Remove from history
            await historyRepo.RemoveMigrationAsync(lastMigrationName, cancellationToken);

            _logger.LogInformation("Migration '{MigrationName}' rolled back successfully.", lastMigrationName);
            return new MigrationResult(
                true,
                $"Migration '{lastMigrationName}' rolled back successfully.",
                MigrationsApplied: 1,
                MigrationName: lastMigrationName);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Migration rollback was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback migration.");
            return new MigrationResult(
                false,
                $"Failed to rollback migration: {ex.Message}",
                Errors: new[] { ex.ToString() });
        }
    }

    private static async Task ExecuteOperationAsync(
        IMongoDatabase database,
        MigrationOperation operation,
        CancellationToken cancellationToken)
    {
        switch (operation.Type.ToLowerInvariant())
        {
            case "createindex":
                await ExecuteCreateIndexAsync(database, operation, cancellationToken);
                break;

            case "dropindex":
                await ExecuteDropIndexAsync(database, operation, cancellationToken);
                break;

            case "createcollection":
                await database.CreateCollectionAsync(operation.Collection, cancellationToken: cancellationToken);
                break;

            case "dropcollection":
                await database.DropCollectionAsync(operation.Collection, cancellationToken);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown migration operation type: '{operation.Type}'. " +
                    "Supported types: createIndex, dropIndex, createCollection, dropCollection.");
        }
    }

    private static async Task ExecuteCreateIndexAsync(
        IMongoDatabase database,
        MigrationOperation operation,
        CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<BsonDocument>(operation.Collection);

        // Build index keys
        var keysBuilder = new BsonDocument();
        if (operation.Keys is not null)
        {
            foreach (var (field, value) in operation.Keys)
            {
                keysBuilder[field] = Convert.ToInt32(value);
            }
        }

        var keysDefinition = new BsonDocumentIndexKeysDefinition<BsonDocument>(keysBuilder);

        // Build index options
        var options = new CreateIndexOptions();
        if (operation.Options is not null)
        {
            if (operation.Options.TryGetValue("name", out var name))
                options.Name = name?.ToString();

            if (operation.Options.TryGetValue("unique", out var unique))
                options.Unique = Convert.ToBoolean(unique);

            if (operation.Options.TryGetValue("sparse", out var sparse))
                options.Sparse = Convert.ToBoolean(sparse);

            if (operation.Options.TryGetValue("background", out var background))
                options.Background = Convert.ToBoolean(background);
        }

        var model = new CreateIndexModel<BsonDocument>(keysDefinition, options);
        await collection.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);
    }

    private static async Task ExecuteDropIndexAsync(
        IMongoDatabase database,
        MigrationOperation operation,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(operation.IndexName))
        {
            throw new InvalidOperationException(
                "dropIndex operation requires an 'indexName' field.");
        }

        var collection = database.GetCollection<BsonDocument>(operation.Collection);
        await collection.Indexes.DropOneAsync(operation.IndexName, cancellationToken);
    }
}
