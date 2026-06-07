using NextNet.Data.MultiDb.Exceptions;

namespace NextNet.Data.MultiDb.Tests.Fixtures;

/// <summary>
/// A fake <see cref="IDatabaseSelector"/> for testing components that
/// depend on the selector. Provides canned responses for all methods.
/// </summary>
internal sealed class FakeDatabaseSelector : IDatabaseSelector
{
    private readonly Dictionary<string, IDatabaseContext> _contexts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IDataConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

    public IDatabaseContext DefaultContext { get; set; } = null!;

    public IDatabaseContext For(string name)
    {
        if (_contexts.TryGetValue(name, out var context))
            return context;

        throw new MissingConnectionException(name);
    }

    public Task<IDatabaseContext> ForAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(For(name));
    }

    public IDatabaseContext Default => DefaultContext;

    public IReadOnlyCollection<string> ConnectionNames => _contexts.Keys.ToList().AsReadOnly();

    public bool HasConnection(string name) => _contexts.ContainsKey(name);

    public IDataConnection GetConnection(string name)
    {
        if (_connections.TryGetValue(name, out var conn))
            return conn;

        throw new MissingConnectionException(name);
    }

    public void AddContext(string name, IDatabaseContext context)
    {
        _contexts[name] = context;
        _connections[name] = context.Connection;
    }
}
