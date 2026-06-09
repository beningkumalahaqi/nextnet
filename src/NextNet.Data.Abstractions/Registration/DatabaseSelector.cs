using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.MultiDb;

namespace NextNet.Data.Abstractions.Registration;

/// <summary>
/// Default implementation of <see cref="IDatabaseSelector"/>.
/// Resolves named connections from the registered <see cref="DataConfig"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is registered as a singleton by the <see cref="NextNetDataBuilder"/>
/// during application startup. It reads connection configurations from the
/// <see cref="DataConfig.Connections"/> dictionary and resolves them on demand.
/// </para>
/// <para>
/// For full multi-database support with provider resolution, connection pooling,
/// and cross-database routing, use the <c>NextNet.Data.MultiDb</c> package
/// which provides a more comprehensive implementation.
/// </para>
/// </remarks>
internal sealed class DatabaseSelector : IDatabaseSelector, IDisposable
{
    private readonly DataConfig _config;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSelector"/> class.
    /// </summary>
    /// <param name="config">The data configuration containing connection definitions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
    public DatabaseSelector(DataConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    /// <inheritdoc />
    public IDatabaseContext For(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_config.Connections is null || !_config.Connections.TryGetValue(name, out var connectionConfig))
        {
            throw new KeyNotFoundException($"[{DataAbstractionsErrorCodes.ConnectionFailed}] No connection with name '{name}' is registered.");
        }

        return new DatabaseContext(name, connectionConfig);
    }

    /// <inheritdoc />
    public Task<IDatabaseContext> ForAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(For(name));
    }

    /// <inheritdoc />
    public IDatabaseContext Default
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return For(_config.DefaultConnection);
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> ConnectionNames
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _config.Connections?.Keys.ToList().AsReadOnly() ?? Array.Empty<string>().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public bool HasConnection(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _config.Connections?.ContainsKey(name) == true;
    }

    /// <inheritdoc />
    public IDataConnection GetConnection(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_config.Connections is null || !_config.Connections.TryGetValue(name, out var connectionConfig))
        {
            throw new KeyNotFoundException($"[{DataAbstractionsErrorCodes.ConnectionFailed}] No connection with name '{name}' is registered.");
        }

        return new DataConnection(name, connectionConfig.ConnectionString, connectionConfig.Provider);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
    }

    /// <summary>
    /// Internal implementation of <see cref="IDataConnection"/> for use by the selector.
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

    /// <summary>
    /// Internal implementation of <see cref="IDatabaseContext"/> for use by the selector.
    /// </summary>
    private sealed class DatabaseContext : IDatabaseContext
    {
        private readonly string _name;
        private readonly ConnectionConfig _config;

        public DatabaseContext(string name, ConnectionConfig config)
        {
            _name = name;
            _config = config;
            Connection = new DataConnection(name, config.ConnectionString, config.Provider);
        }

        public string Name => _name;

        public IDataConnection Connection { get; }

        public IDataProvider Provider =>
            throw new NotSupportedException(
                $"[{DataAbstractionsErrorCodes.ProviderNotAvailable}] The base DatabaseSelector does not support runtime provider resolution. " +
                "Use NextNet.Data.MultiDb for full multi-database support with provider resolution.");

        public IRepository<T> GetRepository<T>() where T : class =>
            throw new NotSupportedException(
                $"[{DataAbstractionsErrorCodes.ProviderNotAvailable}] The base DatabaseSelector does not support runtime repository creation. " +
                "Use NextNet.Data.MultiDb for full multi-database support with repository factories.");

        public void Dispose()
        {
        }
    }
}
