using NextNet.Data.MultiDb.Exceptions;

namespace NextNet.Data.MultiDb.Internal;

/// <summary>
/// Factory for creating <see cref="IDatabaseContext"/> instances from pool entries.
/// Handles the wiring of provider, connection, and repository factory.
/// </summary>
internal sealed class DatabaseContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContextFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    public DatabaseContextFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Creates a new <see cref="IDatabaseContext"/> from the given pool entry.
    /// </summary>
    /// <param name="entry">The pool entry containing provider and connection information.</param>
    /// <returns>A new database context scoped to the named connection.</returns>
    /// <exception cref="ProviderMismatchException">Thrown when the provider cannot be resolved.</exception>
    public IDatabaseContext Create(ConnectionPoolEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new DatabaseContext(
            entry.ConnectionName,
            entry.ConnectionString,
            entry.ProviderName,
            entry.Provider,
            _serviceProvider);
    }
}
