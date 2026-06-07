using Microsoft.Extensions.DependencyInjection;

namespace NextNet.Edge.Adapters;

/// <summary>
/// Registry for edge runtime adapters. Provides factory-style resolution
/// of adapters by provider identifier.
/// </summary>
public class AdapterRegistry
{
    private readonly Dictionary<string, Func<IEdgeRuntimeAdapter>> _adapterFactories = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="AdapterRegistry"/>.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider for resolving adapter instances.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    public AdapterRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Registers an adapter factory for the specified provider.
    /// </summary>
    /// <param name="providerId">The provider identifier (e.g., "cloudflare", "aws").</param>
    /// <param name="factory">A factory function that creates an adapter instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providerId"/> or <paramref name="factory"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="providerId"/> is already registered.</exception>
    public void Register(string providerId, Func<IEdgeRuntimeAdapter> factory)
    {
        if (providerId == null) throw new ArgumentNullException(nameof(providerId));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        if (_adapterFactories.ContainsKey(providerId))
            throw new ArgumentException($"Adapter for provider '{providerId}' is already registered.", nameof(providerId));

        _adapterFactories[providerId] = factory;
    }

    /// <summary>
    /// Registers an adapter type for the specified provider. The adapter is resolved via DI.
    /// </summary>
    /// <typeparam name="T">The adapter type implementing <see cref="IEdgeRuntimeAdapter"/>.</typeparam>
    /// <param name="providerId">The provider identifier. If null, uses the adapter's <see cref="IEdgeRuntimeAdapter.ProviderId"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="providerId"/> is already registered.</exception>
    public void Register<T>(string? providerId = null) where T : IEdgeRuntimeAdapter
    {
        // Create a temporary instance to get the provider ID
        var temp = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
        var id = providerId ?? temp.ProviderId;

        if (_adapterFactories.ContainsKey(id))
            throw new ArgumentException($"Adapter for provider '{id}' is already registered.", nameof(providerId));

        _adapterFactories[id] = () => ActivatorUtilities.CreateInstance<T>(_serviceProvider);
    }

    /// <summary>
    /// Gets the adapter for the specified provider.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <returns>The edge runtime adapter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providerId"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no adapter is registered for the provider.</exception>
    public IEdgeRuntimeAdapter GetAdapter(string providerId)
    {
        if (providerId == null) throw new ArgumentNullException(nameof(providerId));

        if (_adapterFactories.TryGetValue(providerId, out var factory))
            return factory();

        throw new KeyNotFoundException($"No adapter registered for edge provider '{providerId}'. " +
            $"Available providers: {string.Join(", ", _adapterFactories.Keys)}");
    }

    /// <summary>
    /// Attempts to get the adapter for the specified provider.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="adapter">When this method returns, contains the adapter if found; otherwise null.</param>
    /// <returns><c>true</c> if an adapter was found; otherwise <c>false</c>.</returns>
    public bool TryGetAdapter(string providerId, out IEdgeRuntimeAdapter? adapter)
    {
        if (providerId == null)
        {
            adapter = null;
            return false;
        }

        if (_adapterFactories.TryGetValue(providerId, out var factory))
        {
            adapter = factory();
            return true;
        }

        adapter = null;
        return false;
    }

    /// <summary>
    /// Gets all registered provider IDs.
    /// </summary>
    public IEnumerable<string> GetRegisteredProviders() => _adapterFactories.Keys;
}
