using System.Collections.Concurrent;

namespace NextNet.Data.MultiDb.Internal;

/// <summary>
/// Internal registry of named connection pools.
/// Each named connection gets its own pool entry, which may contain
/// a provider-specific pool object (e.g., DbContext pool, Npgsql pool).
/// </summary>
/// <remarks>
/// <para>
/// This registry is registered as a singleton in the DI container.
/// It manages the lifecycle of connection pool entries, including
/// registration, lookup, removal, and bulk disposal.
/// </para>
/// </remarks>
internal sealed class ConnectionPoolRegistry : IDisposable
{
    private readonly ConcurrentDictionary<string, ConnectionPoolEntry> _pools = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// Registers a connection pool entry for the given name.
    /// If a pool with the same name already exists, it is replaced.
    /// </summary>
    /// <param name="name">The logical connection name.</param>
    /// <param name="entry">The pool entry to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="entry"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the registry has been disposed.</exception>
    internal void Register(string name, ConnectionPoolEntry entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(entry);

        _pools[name] = entry;
    }

    /// <summary>
    /// Gets the pool entry for the given connection name.
    /// </summary>
    /// <param name="name">The connection name.</param>
    /// <returns>The pool entry.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no pool with the given name is registered.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the registry has been disposed.</exception>
    internal ConnectionPoolEntry Get(string name)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pools.TryGetValue(name, out var entry))
        {
            return entry;
        }

        throw new KeyNotFoundException($"No pool entry found for connection '{name}'.");
    }

    /// <summary>
    /// Tries to get the pool entry for the given connection name.
    /// </summary>
    /// <param name="name">The connection name.</param>
    /// <param name="entry">When this method returns, contains the pool entry if found; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the entry was found; otherwise <c>false</c>.</returns>
    internal bool TryGet(string name, out ConnectionPoolEntry? entry)
    {
        return _pools.TryGetValue(name, out entry);
    }

    /// <summary>
    /// Removes and disposes the pool entry for the given name.
    /// </summary>
    /// <param name="name">The connection name to remove.</param>
    /// <returns><c>true</c> if the entry was found and removed; otherwise <c>false</c>.</returns>
    internal bool Remove(string name)
    {
        if (_pools.TryRemove(name, out var entry))
        {
            if (entry.Pool is IDisposable disposable)
            {
                disposable.Dispose();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets all registered pool names.
    /// </summary>
    internal IReadOnlyCollection<string> Names => _pools.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Gets the number of registered pools.
    /// </summary>
    internal int Count => _pools.Count;

    /// <summary>
    /// Disposes all registered pools and clears the registry.
    /// </summary>
    internal void DisposeAll()
    {
        if (_disposed) return;

        foreach (var kvp in _pools)
        {
            if (kvp.Value.Pool is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // Swallow disposal exceptions to ensure all entries are cleaned up
                }
            }
        }

        _pools.Clear();
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAll();
        GC.SuppressFinalize(this);
    }
}
