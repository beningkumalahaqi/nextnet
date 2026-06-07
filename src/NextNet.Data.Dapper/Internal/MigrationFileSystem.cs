namespace NextNet.Data.Dapper.Internal;

/// <summary>
/// Discovers and reads SQL migration files from the file system.
/// </summary>
/// <remarks>
/// <para>
/// Migration files follow the naming convention: <c>YYYYMMDDHHmmss_description.sql</c>.
/// Rollback scripts follow: <c>YYYYMMDDHHmmss_description.down.sql</c>.
/// Files are ordered by their timestamp prefix for deterministic application order.
/// </para>
/// </remarks>
internal sealed class MigrationFileSystem
{
    private readonly string _migrationsDirectory;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationFileSystem"/>.
    /// </summary>
    /// <param name="migrationsDirectory">The directory containing migration files.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="migrationsDirectory"/> is null or empty.</exception>
    internal MigrationFileSystem(string migrationsDirectory, ILogger? logger = null)
    {
        _migrationsDirectory = migrationsDirectory ?? throw new ArgumentNullException(nameof(migrationsDirectory));
        _logger = logger;
    }

    /// <summary>
    /// Gets the full path to the migrations directory.
    /// </summary>
    internal string MigrationsDirectoryPath =>
        Path.GetFullPath(_migrationsDirectory);

    /// <summary>
    /// Discovers all pending migration files (those with .sql extension, not .down.sql, and not yet applied).
    /// </summary>
    /// <param name="appliedMigrations">The set of already-applied migration names.</param>
    /// <returns>A list of pending migration file paths, ordered by timestamp.</returns>
    internal IReadOnlyList<string> GetPendingMigrations(HashSet<string> appliedMigrations)
    {
        if (!Directory.Exists(_migrationsDirectory))
        {
            _logger?.LogWarning("Migrations directory '{Directory}' does not exist.", _migrationsDirectory);
            return Array.Empty<string>();
        }

        var files = Directory.GetFiles(_migrationsDirectory, "*.sql")
            .Where(f => !f.EndsWith(".down.sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f)
            .ToList();

        var pending = new List<string>();

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (!appliedMigrations.Contains(fileName))
            {
                pending.Add(file);
            }
        }

        _logger?.LogDebug("Found {PendingCount} pending migration(s) out of {TotalCount} total.", pending.Count, files.Count);
        return pending;
    }

    /// <summary>
    /// Reads the content of a migration SQL file.
    /// </summary>
    /// <param name="filePath">The full path to the migration file.</param>
    /// <returns>The SQL content of the file.</returns>
    internal async Task<string> ReadMigrationScriptAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    /// <summary>
    /// Gets the migration name (filename without extension) from a file path.
    /// </summary>
    /// <param name="filePath">The full path to the migration file.</param>
    /// <returns>The migration name.</returns>
    internal static string GetMigrationName(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Gets the corresponding down script path for an up migration file.
    /// </summary>
    /// <param name="upFilePath">The full path to the up migration file.</param>
    /// <returns>The path to the down script, or null if it does not exist.</returns>
    internal string? GetDownScriptPath(string upFilePath)
    {
        var downPath = Path.ChangeExtension(upFilePath, null) + ".down.sql";
        return File.Exists(downPath) ? downPath : null;
    }

    /// <summary>
    /// Creates a new migration file pair (up and down) with the given name.
    /// </summary>
    /// <param name="migrationName">The descriptive name for the migration.</param>
    /// <returns>A tuple with the up and down file paths.</returns>
    internal (string UpPath, string DownPath) CreateMigrationFiles(string migrationName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var safeName = SanitizeMigrationName(migrationName);
        var fileName = $"{timestamp}_{safeName}";

        Directory.CreateDirectory(_migrationsDirectory);

        var upPath = Path.Combine(_migrationsDirectory, $"{fileName}.sql");
        var downPath = Path.Combine(_migrationsDirectory, $"{fileName}.down.sql");

        // Write template content
        File.WriteAllText(upPath,
            $"-- Migration: {fileName}\n-- Created: {DateTime.UtcNow:O}\n\n-- Write your UP migration SQL here\n\n");
        File.WriteAllText(downPath,
            $"-- Rollback: {fileName}\n-- Created: {DateTime.UtcNow:O}\n\n-- Write your DOWN migration SQL here\n\n");

        _logger?.LogInformation("Created migration files: '{UpPath}' and '{DownPath}'.", upPath, downPath);

        return (upPath, downPath);
    }

    /// <summary>
    /// Reads a down script.
    /// </summary>
    /// <param name="downPath">The full path to the down script.</param>
    /// <returns>The SQL content, or null if the file does not exist.</returns>
    internal async Task<string?> ReadDownScriptAsync(string downPath)
    {
        return File.Exists(downPath) ? await File.ReadAllTextAsync(downPath) : null;
    }

    private static string SanitizeMigrationName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "Migration" : sanitized;
    }
}
