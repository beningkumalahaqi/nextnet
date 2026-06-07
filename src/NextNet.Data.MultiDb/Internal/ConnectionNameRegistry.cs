using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace NextNet.Data.MultiDb.Internal;

/// <summary>
/// Thread-safe registry that maps logical connection names to their provider and configuration.
/// Used internally by <see cref="DatabaseSelector"/> for connection resolution.
/// </summary>
/// <remarks>
/// <para>
/// This registry is registered as a singleton in the DI container. It uses a
/// <see cref="ConcurrentDictionary{TKey, TValue}"/> with case-insensitive name comparison
/// to ensure thread safety and consistency.
/// </para>
/// </remarks>
internal sealed class ConnectionNameRegistry
{
    private readonly ConcurrentDictionary<string, ConnectionRegistration> _registrations = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a named connection.
    /// </summary>
    /// <param name="name">The logical connection name.</param>
    /// <param name="registration">The connection registration details.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="registration"/> is <c>null</c>.</exception>
    internal void Register(string name, ConnectionRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(registration);

        _registrations[name] = registration;
    }

    /// <summary>
    /// Tries to get the registration for the given connection name.
    /// </summary>
    /// <param name="name">The connection name to look up.</param>
    /// <param name="registration">When this method returns, contains the registration if found; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the registration was found; otherwise <c>false</c>.</returns>
    internal bool TryGet(string name, [MaybeNullWhen(false)] out ConnectionRegistration registration)
    {
        return _registrations.TryGetValue(name, out registration);
    }

    /// <summary>
    /// Gets all registered connection names.
    /// </summary>
    internal IReadOnlyCollection<string> Names => _registrations.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Checks if a connection name is registered.
    /// </summary>
    /// <param name="name">The connection name to check.</param>
    /// <returns><c>true</c> if the name is registered; otherwise <c>false</c>.</returns>
    internal bool Exists(string name)
    {
        return _registrations.ContainsKey(name);
    }

    /// <summary>
    /// Gets the number of registered connections.
    /// </summary>
    internal int Count => _registrations.Count;
}
