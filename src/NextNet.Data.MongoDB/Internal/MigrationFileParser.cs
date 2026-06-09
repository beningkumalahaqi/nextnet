using System.Text.Json;

namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Discovers and parses JSON migration definition files for MongoDB index management.
/// </summary>
/// <remarks>
/// <para>
/// Migration files are JSON documents describing index creation/drop operations.
/// Files are named with a timestamp prefix for ordering: <c>YYYYMMDDHHmmss_name.json</c>.
/// </para>
/// <para>
/// File format:
/// <code>
/// {
///   "name": "20260606000001_AddUserIndexes",
///   "createdAt": "2026-06-06T00:00:01Z",
///   "operations": [
///     { "type": "createIndex", "collection": "users", "keys": { "email": 1 }, "options": { "unique": true } }
///   ],
///   "rollback": [
///     { "type": "dropIndex", "collection": "users", "indexName": "idx_users_email_unique" }
///   ]
/// }
/// </code>
/// </para>
/// </remarks>
internal sealed class MigrationFileParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly string _migrationsDirectory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationFileParser"/>.
    /// </summary>
    /// <param name="migrationsDirectory">The directory containing migration JSON files.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public MigrationFileParser(string migrationsDirectory, ILogger? logger = null)
    {
        _migrationsDirectory = migrationsDirectory;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Gets all pending migration files that have not been applied.
    /// </summary>
    /// <param name="appliedMigrationNames">The set of already-applied migration names.</param>
    /// <returns>A sorted list of pending migration file paths.</returns>
    public IReadOnlyList<string> GetPendingMigrations(ISet<string> appliedMigrationNames)
    {
        if (!Directory.Exists(_migrationsDirectory))
        {
            _logger.LogInformation("Migrations directory '{Directory}' does not exist.", _migrationsDirectory);
            return Array.Empty<string>();
        }

        var files = Directory.GetFiles(_migrationsDirectory, "*.json")
            .OrderBy(f => f)
            .ToList();

        var pending = new List<string>();

        foreach (var file in files)
        {
            var migrationName = Path.GetFileNameWithoutExtension(file);

            // Extract name after timestamp prefix: "20260606000001_AddUserIndexes" → "AddUserIndexes"
            // The full filename (without ext) is the migration name
            if (!appliedMigrationNames.Contains(migrationName))
            {
                pending.Add(file);
            }
        }

        return pending;
    }

    /// <summary>
    /// Parses a migration JSON file into a <see cref="MigrationDefinition"/>.
    /// </summary>
    /// <param name="filePath">The path to the migration JSON file.</param>
    /// <returns>The parsed migration definition.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the JSON is malformed.</exception>
    public async Task<MigrationDefinition> ParseAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"[{MongoDbErrorCodes.CollectionNotFound}] Migration file not found.", filePath);
        }

        try
        {
            await using var stream = File.OpenRead(filePath);
            var definition = await JsonSerializer.DeserializeAsync<MigrationDefinition>(stream, JsonOptions);

            if (definition is null)
            {
                throw new InvalidOperationException($"[{MongoDbErrorCodes.DocumentSerializationFailed}] Failed to parse migration file: {filePath}");
            }

            return definition;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"[{MongoDbErrorCodes.DocumentSerializationFailed}] Migration file '{filePath}' contains invalid JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a new migration JSON template file.
    /// </summary>
    /// <param name="name">The migration name.</param>
    /// <param name="timestamp">The timestamp for ordering.</param>
    /// <returns>The full path to the created file.</returns>
    public string CreateMigrationFile(string name, DateTimeOffset timestamp)
    {
        if (!Directory.Exists(_migrationsDirectory))
        {
            Directory.CreateDirectory(_migrationsDirectory);
        }

        var timestampStr = timestamp.ToString("yyyyMMddHHmmss");
        var fileName = $"{timestampStr}_{name}.json";
        var filePath = Path.Combine(_migrationsDirectory, fileName);

        var template = new MigrationDefinition
        {
            Name = $"{timestampStr}_{name}",
            CreatedAt = timestamp,
            Operations = new List<MigrationOperation>(),
            Rollback = new List<MigrationOperation>(),
        };

        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);

        _logger.LogInformation("Created migration file: {FilePath}", filePath);
        return filePath;
    }
}

/// <summary>
/// Represents a MongoDB migration definition parsed from a JSON file.
/// </summary>
internal sealed record MigrationDefinition
{
    /// <summary>
    /// Gets or sets the migration name (e.g., "20260606000001_AddUserIndexes").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the migration was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of operations to apply.
    /// </summary>
    public List<MigrationOperation> Operations { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of operations to roll back.
    /// </summary>
    public List<MigrationOperation> Rollback { get; set; } = new();
}

/// <summary>
/// Represents a single MongoDB migration operation (create/drop index, create/drop collection).
/// </summary>
internal sealed record MigrationOperation
{
    /// <summary>
    /// Gets or sets the operation type: "createIndex", "dropIndex", "createCollection", "dropCollection".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection name.
    /// </summary>
    public string Collection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index key specification document (for createIndex operations).
    /// </summary>
    public Dictionary<string, object>? Keys { get; set; }

    /// <summary>
    /// Gets or sets the index options (for createIndex operations).
    /// </summary>
    public Dictionary<string, object>? Options { get; set; }

    /// <summary>
    /// Gets or sets the index name (for dropIndex operations).
    /// </summary>
    public string? IndexName { get; set; }
}
