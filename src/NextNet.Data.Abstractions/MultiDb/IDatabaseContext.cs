using NextNet.Data.Abstractions.Abstractions;

namespace NextNet.Data.Abstractions.MultiDb;

/// <summary>
/// A scoped context for a named database connection, providing access to
/// the underlying connection, provider, and scoped repositories.
/// </summary>
/// <remarks>
/// <para>
/// Obtained from <see cref="IDatabaseSelector.For(string)"/>.
/// The context is tied to a single named connection and provides
/// factory methods for creating provider-specific repository instances.
/// </para>
/// <para>
/// Dispose the context when it is no longer needed to release
/// any provider-specific resources.
/// </para>
/// <example>
/// <code>
/// using var ctx = _db.For("Analytics");
/// var repo = ctx.GetRepository&lt;SalesSummary&gt;();
/// var data = await repo.GetAllAsync();
/// </code>
/// </example>
/// </remarks>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// Gets the logical name of this database (e.g., "Analytics", "Primary").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the connection metadata (connection string, provider name).
    /// </summary>
    IDataConnection Connection { get; }

    /// <summary>
    /// Gets the underlying data provider instance for this database.
    /// </summary>
    IDataProvider Provider { get; }

    /// <summary>
    /// Creates a scoped repository for the specified entity type.
    /// The repository is bound to this database's connection.
    /// </summary>
    /// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
    /// <returns>A repository instance scoped to this database.</returns>
    IRepository<T> GetRepository<T>() where T : class;
}
