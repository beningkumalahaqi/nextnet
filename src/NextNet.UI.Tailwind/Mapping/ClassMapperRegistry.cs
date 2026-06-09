using System.Collections.Concurrent;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Tailwind.Mapping;

/// <summary>
/// Registry that maps component types (<see cref="Type"/>) to their corresponding
/// <see cref="IComponentClassMapper{T}"/> implementations. Enables runtime resolution
/// of class mappers for any <see cref="IComponent"/> type.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ClassMapperRegistry"/> provides a central point for registering and
/// resolving Tailwind CSS class mappers. Mappers can be registered for specific
/// interface types (e.g., <see cref="IButton"/>, <see cref="ICard"/>) and are
/// resolved based on the runtime type of the component.
/// </para>
/// <para>
/// The registry is thread-safe and intended to be configured once during application
/// startup, typically via the <c>AddNextNetTailwind</c> DI extension method.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var registry = new ClassMapperRegistry();
/// registry.Register&lt;IButton&gt;(new ButtonClassMapper());
/// registry.Register&lt;ICard&gt;(new CardClassMapper());
///
/// var mapper = registry.Resolve&lt;IButton&gt;();
/// var classes = mapper.MapClasses(button, context);
/// </code>
/// </example>
public sealed class ClassMapperRegistry
{
    private readonly ConcurrentDictionary<Type, object> _mappers = new();

    /// <summary>
    /// Registers a class mapper for the specified component type.
    /// If a mapper is already registered for the same type, it is replaced.
    /// </summary>
    /// <typeparam name="T">The component interface type (e.g., <see cref="IButton"/>).</typeparam>
    /// <param name="mapper">The mapper instance to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="mapper"/> is null.</exception>
    public void Register<T>(IComponentClassMapper<T> mapper) where T : IComponent
    {
        ArgumentNullException.ThrowIfNull(mapper);
        _mappers[typeof(T)] = mapper;
    }

    /// <summary>
    /// Resolves the class mapper for the specified component type.
    /// </summary>
    /// <typeparam name="T">The component interface type (e.g., <see cref="IButton"/>).</typeparam>
    /// <returns>The registered mapper instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no mapper is registered for the specified type.
    /// Error code: DS-400.
    /// </exception>
    public IComponentClassMapper<T> Resolve<T>() where T : IComponent
    {
        if (_mappers.TryGetValue(typeof(T), out var mapper))
        {
            return (IComponentClassMapper<T>)mapper;
        }

        throw new InvalidOperationException(
            $"No class mapper registered for component type '{typeof(T).FullName}'. " +
            "Register a mapper via Register<T>() or ensure AddNextNetTailwind() was called during startup. " +
            "(Error code: DS-400)");
    }

    /// <summary>
    /// Attempts to resolve a class mapper for the specified component type.
    /// </summary>
    /// <typeparam name="T">The component interface type (e.g., <see cref="IButton"/>).</typeparam>
    /// <param name="mapper">When this method returns, contains the registered mapper if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if a mapper was found; otherwise, <c>false</c>.</returns>
    public bool TryResolve<T>(out IComponentClassMapper<T>? mapper) where T : IComponent
    {
        if (_mappers.TryGetValue(typeof(T), out var obj))
        {
            mapper = (IComponentClassMapper<T>)obj;
            return true;
        }

        mapper = null;
        return false;
    }

    /// <summary>
    /// Maps the specified component's properties to Tailwind CSS classes by resolving
    /// the appropriate mapper and invoking it.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="component">The component instance to map.</param>
    /// <param name="context">The rendering context.</param>
    /// <returns>A space-separated string of Tailwind CSS class names.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no mapper is registered for the specified type (error code: DS-400).
    /// </exception>
    public string MapClasses<T>(T component, RenderContext context) where T : IComponent
    {
        var mapper = Resolve<T>();
        return mapper.MapClasses(component, context);
    }

    /// <summary>
    /// Gets the count of registered mappers in the registry.
    /// </summary>
    public int Count => _mappers.Count;
}
