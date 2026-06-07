namespace NextNet.Templates.Versioning;

/// <summary>
/// Migration scaffolding for future template schema versions.
/// </summary>
/// <remarks>
/// <para>
/// In V3.1, no migrations are needed. This class hierarchy is a placeholder for future use
/// when the manifest schema changes and older templates need to be upgraded.
/// </para>
/// <para>
/// To implement a migration, create a subclass of <see cref="VersionMigration"/> that
/// specifies the source and target versions, then register it with
/// <see cref="VersionMigrationRegistry.Register"/>.
/// </para>
/// </remarks>
public abstract class VersionMigration
{
    /// <summary>
    /// Gets the source schema version this migration upgrades FROM.
    /// </summary>
    public abstract string FromVersion { get; }

    /// <summary>
    /// Gets the target schema version this migration upgrades TO.
    /// </summary>
    public abstract string ToVersion { get; }

    /// <summary>
    /// Applies the migration to a JSON manifest string.
    /// </summary>
    /// <param name="manifestJson">The source manifest JSON to transform.</param>
    /// <returns>The migrated manifest JSON.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifestJson"/> is <c>null</c>.</exception>
    public abstract string Migrate(string manifestJson);
}

/// <summary>
/// Registry of available version migrations. Empty in V3.1.
/// </summary>
/// <remarks>
/// <para>
/// The registry maintains a collection of <see cref="VersionMigration"/> instances and
/// provides methods to discover migration paths between schema versions.
/// </para>
/// <example>
/// <code>
/// var registry = new VersionMigrationRegistry();
/// registry.Register(new V1ToV2Migration());
/// var path = registry.GetMigrationPath("1.0.0", "2.0.0");
/// foreach (var step in path)
/// {
///     manifestJson = step.Migrate(manifestJson);
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class VersionMigrationRegistry
{
    private readonly List<VersionMigration> _migrations = new();

    /// <summary>
    /// Gets the list of registered migrations.
    /// </summary>
    public IReadOnlyList<VersionMigration> Migrations => _migrations;

    /// <summary>
    /// Registers a migration for use during template upgrade.
    /// </summary>
    /// <param name="migration">The migration to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="migration"/> is <c>null</c>.</exception>
    public void Register(VersionMigration migration)
    {
        ArgumentNullException.ThrowIfNull(migration);
        _migrations.Add(migration);
    }

    /// <summary>
    /// Returns the ordered migration path from <paramref name="fromVersion"/> to <paramref name="toVersion"/>.
    /// </summary>
    /// <param name="fromVersion">The source schema version.</param>
    /// <param name="toVersion">The target schema version.</param>
    /// <returns>An ordered list of migrations to apply sequentially. Empty if no path is available.</returns>
    /// <remarks>
    /// V3.1: No migrations are registered yet, so this always returns an empty list.
    /// Future implementations will perform topological sorting to find the shortest path.
    /// </remarks>
    public IReadOnlyList<VersionMigration> GetMigrationPath(string fromVersion, string toVersion)
    {
        // V3.1: no migrations registered
        return Array.Empty<VersionMigration>();
    }
}
