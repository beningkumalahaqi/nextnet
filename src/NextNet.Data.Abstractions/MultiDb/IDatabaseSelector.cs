using NextNet.Data.Abstractions.Abstractions;

namespace NextNet.Data.Abstractions.MultiDb;

/// <summary>
/// Resolves named database connections for multi-database scenarios.
/// Enables application code to route queries to different databases by logical name.
/// </summary>
/// <remarks>
/// <para>
/// Register via <c>services.AddNextNetData().WithDatabase(...)</c>.
/// The selector supports both synchronous and asynchronous resolution,
/// and maintains a registry of all configured named connections.
/// </para>
/// <example>
/// <code>
/// public class ReportsController
/// {
///     private readonly IDatabaseSelector _db;
///
///     public ReportsController(IDatabaseSelector db) => _db = db;
///
///     public async Task&lt;IActionResult&gt; GetDashboard()
///     {
///         // Resolve the Analytics database connection
///         var analytics = _db.For("Analytics");
///
///         // Use the connection to query (provider-specific)
///         var repo = analytics.GetRepository&lt;SalesSummary&gt;();
///         var data = await repo.GetAllAsync();
///         return Ok(data);
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface IDatabaseSelector
{
    /// <summary>
    /// Resolves a named database context for the specified logical name.
    /// Returns a <see cref="IDatabaseContext"/> that provides access to
    /// the connection, provider, and scoped repositories.
    /// </summary>
    /// <param name="name">The logical connection name (e.g., "Analytics", "Primary").</param>
    /// <returns>A database context scoped to the named connection.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    IDatabaseContext For(string name);

    /// <summary>
    /// Resolves a named database context for the specified logical name,
    /// with async support for lazy connection initialization.
    /// </summary>
    /// <param name="name">The logical connection name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task containing the database context.</returns>
    Task<IDatabaseContext> ForAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default database context as configured by the
    /// <c>DefaultConnection</c> setting in <c>nextnet.config.json</c>.
    /// </summary>
    /// <returns>The default database context.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the default connection is not configured.</exception>
    IDatabaseContext Default { get; }

    /// <summary>
    /// Gets all registered connection names.
    /// </summary>
    IReadOnlyCollection<string> ConnectionNames { get; }

    /// <summary>
    /// Checks whether a connection with the specified name is registered.
    /// </summary>
    /// <param name="name">The connection name to check.</param>
    /// <returns><c>true</c> if the connection is registered; otherwise <c>false</c>.</returns>
    bool HasConnection(string name);

    /// <summary>
    /// Gets a <see cref="IDataConnection"/> for the specified name without
    /// creating a full database context. Useful for direct ADO.NET access.
    /// </summary>
    /// <param name="name">The logical connection name.</param>
    /// <returns>The connection metadata, including the resolved connection string and provider name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no connection with the given name is registered.</exception>
    IDataConnection GetConnection(string name);
}
