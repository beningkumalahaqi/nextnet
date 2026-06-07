using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Defines the contract for database migration management.
/// Each provider implements this to support its specific migration technology
/// (EF Core migrations, raw SQL scripts, MongoDB index migrations, etc.).
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IMigrationEngine"/> interface provides a unified migration lifecycle
/// across all NextNet data providers. Implementations wrap provider-specific migration
/// tooling behind this common contract.
/// </para>
/// <para>
/// Migration configuration is provided by <see cref="Configuration.MigrationConfig"/>
/// and consumed during engine initialization.
/// </para>
/// <example>
/// <code>
/// public class MigrationService
/// {
///     private readonly IMigrationEngine _engine;
///
///     public MigrationService(IMigrationEngine engine)
///     {
///         _engine = engine;
///     }
///
///     public async Task&lt;MigrationResult&gt; CreateUserTableMigration() =>
///         await _engine.AddMigrationAsync("AddUserTable");
///
///     public async Task&lt;MigrationResult&gt; ApplyPending() =>
///         await _engine.ApplyAsync();
/// }
/// </code>
/// </example>
/// </remarks>
public interface IMigrationEngine
{
    /// <summary>
    /// Creates a new migration with the specified name.
    /// </summary>
    /// <param name="name">A descriptive name for the migration (e.g., "AddUserTable").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating success or failure of migration creation.</returns>
    Task<MigrationResult> AddMigrationAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies all pending migrations to the database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating how many migrations were applied.</returns>
    Task<MigrationResult> ApplyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the most recent applied migration.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating success or failure of the rollback.</returns>
    Task<MigrationResult> RollbackAsync(CancellationToken cancellationToken = default);
}
