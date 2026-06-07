using System.Text.Json.Serialization;

namespace NextNet.Data.Abstractions.Configuration;

/// <summary>
/// Configuration options for database migration behavior.
/// </summary>
/// <remarks>
/// <para>
/// Migration settings control how database migrations are created, applied, and tracked.
/// These options are consumed by the <see cref="Abstractions.IMigrationEngine"/> implementation.
/// </para>
/// <example>
/// <code>
/// var config = new MigrationConfig(
///     autoApply: true,
///     directory: "Data/Migrations",
///     historyTableName: "__MyMigrations",
///     timeoutSeconds: 120);
/// </code>
/// </example>
/// </remarks>
/// <param name="AutoApply">Whether to automatically apply pending migrations on startup. Defaults to <c>false</c>.</param>
/// <param name="Directory">The directory where migration files are stored, relative to project root. Defaults to <c>"Migrations"</c>.</param>
/// <param name="HistoryTableName">The name of the migration history table. Defaults to <c>"__NextNetMigrations"</c>.</param>
/// <param name="TimeoutSeconds">Timeout for migration operations in seconds. Must be >= 1. Defaults to 60.</param>
public sealed record MigrationConfig(
    [property: JsonPropertyName("autoApply")]
    bool AutoApply = false,

    [property: JsonPropertyName("directory")]
    string Directory = "Migrations",

    [property: JsonPropertyName("historyTableName")]
    string HistoryTableName = "__NextNetMigrations",

    [property: JsonPropertyName("timeoutSeconds")]
    int TimeoutSeconds = 60
);
