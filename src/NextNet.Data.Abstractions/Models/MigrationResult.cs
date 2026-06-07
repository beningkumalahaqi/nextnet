namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Represents the outcome of a migration operation.
/// </summary>
/// <remarks>
/// <para>
/// Returned by <see cref="Abstractions.IMigrationEngine"/> methods to indicate whether
/// the migration operation succeeded, how many migrations were applied, and any errors
/// that occurred during the process.
/// </para>
/// <example>
/// <code>
/// var result = new MigrationResult(
///     success: true,
///     message: "Migration applied successfully.",
///     migrationsApplied: 3,
///     migrationName: "AddUserTable");
/// </code>
/// </example>
/// </remarks>
/// <param name="Success">Whether the migration operation completed successfully.</param>
/// <param name="Message">A human-readable description of the result.</param>
/// <param name="MigrationsApplied">The number of migrations that were applied (for <c>ApplyAsync</c>).</param>
/// <param name="MigrationName">The name of the migration that was created or rolled back.</param>
/// <param name="Errors">Any errors that occurred during the operation.</param>
public sealed record MigrationResult(
    bool Success,
    string Message,
    int MigrationsApplied = 0,
    string? MigrationName = null,
    IReadOnlyList<string>? Errors = null
);
