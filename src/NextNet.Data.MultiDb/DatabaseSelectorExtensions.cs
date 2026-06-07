using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.MultiDb;

namespace NextNet.Data.MultiDb;

/// <summary>
/// Convenience extension methods for <see cref="IDatabaseSelector"/>.
/// Provides shorthand for common resolution and repository creation patterns.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods reduce boilerplate when working with named databases.
/// Instead of calling <c>selector.For(name).GetRepository&lt;T&gt;()</c>, you can
/// use <c>selector.GetRepository&lt;T&gt;(name)</c> directly.
/// </para>
/// <example>
/// <code>
/// // Instead of:
/// var repo1 = _db.For("Analytics").GetRepository&lt;SalesSummary&gt;();
///
/// // You can write:
/// var repo2 = _db.GetRepository&lt;SalesSummary&gt;("Analytics");
/// </code>
/// </example>
/// </remarks>
public static class DatabaseSelectorExtensions
{
    /// <summary>
    /// Resolves a named database and creates a repository for the specified entity type.
    /// Shorthand for <c>selector.For(name).GetRepository&lt;T&gt;()</c>.
    /// </summary>
    /// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
    /// <param name="selector">The database selector.</param>
    /// <param name="connectionName">The logical connection name (e.g., "Analytics", "Primary").</param>
    /// <returns>A repository scoped to the named connection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> or <paramref name="connectionName"/> is <c>null</c>.</exception>
    /// <exception cref="Exceptions.MissingConnectionException">Thrown when no connection with the given name is registered.</exception>
    public static IRepository<T> GetRepository<T>(this IDatabaseSelector selector, string connectionName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(connectionName);

        return selector.For(connectionName).GetRepository<T>();
    }

    /// <summary>
    /// Resolves the default database and creates a repository for the specified entity type.
    /// Shorthand for <c>selector.Default.GetRepository&lt;T&gt;()</c>.
    /// </summary>
    /// <typeparam name="T">The entity type. Must be a reference type.</typeparam>
    /// <param name="selector">The database selector.</param>
    /// <returns>A repository scoped to the default connection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="selector"/> is <c>null</c>.</exception>
    public static IRepository<T> GetRepository<T>(this IDatabaseSelector selector)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(selector);
        return selector.Default.GetRepository<T>();
    }
}
