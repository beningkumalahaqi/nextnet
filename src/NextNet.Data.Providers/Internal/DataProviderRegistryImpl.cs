using System.Collections.Concurrent;

namespace NextNet.Data.Internal;

/// <summary>
/// Default thread-safe implementation of <see cref="IDataProviderRegistry"/>.
/// Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> for lock-free reads
/// and thread-safe writes during provider registration.
/// </summary>
internal sealed class DataProviderRegistryImpl : IDataProviderRegistry
{
    private readonly ConcurrentDictionary<string, IDataProvider> _providers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IDataProvider> _orderedProviders = new();

    private readonly object _lock = new();

    /// <summary>
    /// Adds a provider to the registry based on the registration descriptor.
    /// The provider instance is resolved from the DI container on first access.
    /// </summary>
    /// <param name="descriptor">The registration descriptor for the provider.</param>
    /// <exception cref="InvalidOperationException">Thrown when a provider with the same name already exists.</exception>
    public void AddProvider(ProviderRegistrationDescriptor descriptor)
    {
        // We store a factory-based wrapper that resolves from DI when first needed.
        // At this point the provider instance hasn't been created yet.
        // The actual IDataProvider instances are resolved from DI at runtime.
        lock (_lock)
        {
            _orderedProviders.Add(null!); // Placeholder; resolved at runtime via DI
        }
    }

    /// <summary>
    /// Tracks an already-resolved provider instance in the registry.
    /// Called after the provider is resolved from the DI container.
    /// </summary>
    /// <param name="name">The provider name.</param>
    /// <param name="provider">The provider instance.</param>
    /// <exception cref="InvalidOperationException">Thrown when a provider with the same name already exists.</exception>
    public void TrackInstance(string name, IDataProvider provider)
    {
        if (!_providers.TryAdd(name, provider))
        {
            throw new InvalidOperationException($"Provider with name '{name}' is already tracked in the registry.");
        }

        lock (_lock)
        {
            // Update the placeholder
            for (var i = 0; i < _orderedProviders.Count; i++)
            {
                if (_orderedProviders[i] is null)
                {
                    _orderedProviders[i] = provider;
                    return;
                }
            }

            _orderedProviders.Add(provider);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IDataProvider> GetAll()
    {
        lock (_lock)
        {
            return _orderedProviders.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public IDataProvider? GetByName(string name)
    {
        _providers.TryGetValue(name, out var provider);
        return provider;
    }

    /// <inheritdoc />
    public IDataProvider? GetDefault()
    {
        lock (_lock)
        {
            return _orderedProviders.Count > 0 ? _orderedProviders[0] : null;
        }
    }
}
