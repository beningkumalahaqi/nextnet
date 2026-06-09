using NextNet.Data.MultiDb.Exceptions;

namespace NextNet.Data.MultiDb;

/// <summary>
/// Default implementation of <see cref="IDatabaseContext"/>.
/// Provides a scoped wrapper around a named connection with access to
/// the provider, connection metadata, and repository factory.
/// </summary>
/// <remarks>
/// <para>
/// Instances are created by <see cref="DatabaseSelector"/> and cached when
/// <see cref="MultiDbOptions.CacheContexts"/> is enabled. Each context is bound
/// to a single named connection and its associated provider.
/// </para>
/// <para>
/// Dispose the context when it is no longer needed to release resources.
/// When contexts are cached, disposal is managed by the selector.
/// </para>
/// </remarks>
internal sealed class DatabaseContext : IDatabaseContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _connectionString;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
    /// </summary>
    /// <param name="name">The logical connection name.</param>
    /// <param name="connectionString">The resolved connection string.</param>
    /// <param name="providerName">The name of the data provider.</param>
    /// <param name="provider">The data provider instance.</param>
    /// <param name="serviceProvider">The service provider for resolving repository implementations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/>, <paramref name="connectionString"/>,
    /// <paramref name="providerName"/>, <paramref name="provider"/>, or
    /// <paramref name="serviceProvider"/> is <c>null</c>.
    /// </exception>
    public DatabaseContext(
        string name,
        string connectionString,
        string providerName,
        NextNet.Data.Abstractions.Abstractions.IDataProvider provider,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(providerName);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        Name = name;
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        Provider = provider;
        Connection = new DataConnection(name, connectionString, providerName);
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IDataConnection Connection { get; }

    /// <inheritdoc />
    public NextNet.Data.Abstractions.Abstractions.IDataProvider Provider { get; }

    /// <inheritdoc />
    public IRepository<T> GetRepository<T>() where T : class
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // First, try to resolve a named repository registered via AddRepository<T>(connectionName)
        var repository = _serviceProvider.GetService<IRepository<T>>();
        if (repository is not null)
        {
            return repository;
        }

        // If no named repository is registered, throw a clear error
        throw new InvalidOperationException(
            $"[DS-556] No repository registered for entity type '{typeof(T).Name}' on connection '{Name}'. " +
            $"Use AddRepository<{typeof(T).Name}>(\"{Name}\") during service registration to register a repository " +
            $"scoped to this connection.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Internal connection record that implements <see cref="IDataConnection"/>.
    /// </summary>
    private sealed record DataConnection : IDataConnection
    {
        public DataConnection(string name, string connectionString, string providerName)
        {
            Name = name;
            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        public string Name { get; }
        public string ConnectionString { get; }
        public string ProviderName { get; }
    }
}
