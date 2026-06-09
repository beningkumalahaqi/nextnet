using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Rendering;

/// <summary>
/// Stores and resolves <see cref="IComponentRenderer{T}"/> instances by component type.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentRendererRegistry"/> provides a non-generic lookup mechanism for
/// resolving component renderers at runtime. Renderers are registered by the component
/// type they handle and can be resolved via <see cref="Resolve{T}"/> or the non-generic
/// <see cref="Resolve(Type)"/> overload.
/// </para>
/// <para>
/// The registry is typically populated during application startup through the
/// <c>AddNextNetDesignSystem</c> service registration method, which registers
/// <see cref="DefaultComponentRenderer{T}"/> for each standard component type.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var registry = new ComponentRendererRegistry();
/// registry.Register&lt;IButton&gt;(new DefaultComponentRenderer&lt;IButton&gt;());
///
/// var renderer = registry.Resolve(typeof(IButton));
/// var result = ((IComponentRenderer&lt;IButton&gt;)renderer).Render(button, context);
/// </code>
/// </example>
public class ComponentRendererRegistry
{
    private readonly Dictionary<Type, object> _renderers = new();

    /// <summary>
    /// Registers a component renderer for the specified component type.
    /// </summary>
    /// <typeparam name="T">The component type. Must implement <see cref="IComponent"/>.</typeparam>
    /// <param name="renderer">The renderer instance to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="renderer"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a renderer is already registered for type <typeparamref name="T"/>. (Error DS-151)
    /// </exception>
    public void Register<T>(IComponentRenderer<T> renderer)
        where T : IComponent
    {
        ArgumentNullException.ThrowIfNull(renderer);

        var type = typeof(T);
        if (_renderers.ContainsKey(type))
        {
            throw new InvalidOperationException(
                $"DS-151: A renderer is already registered for component type '{type.FullName}'.");
        }

        _renderers[type] = renderer;
    }

    /// <summary>
    /// Registers a component renderer for the specified component type, replacing any existing registration.
    /// </summary>
    /// <typeparam name="T">The component type. Must implement <see cref="IComponent"/>.</typeparam>
    /// <param name="renderer">The renderer instance to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="renderer"/> is null.</exception>
    public void RegisterOrReplace<T>(IComponentRenderer<T> renderer)
        where T : IComponent
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderers[typeof(T)] = renderer;
    }

    /// <summary>
    /// Resolves a component renderer for the specified component type.
    /// </summary>
    /// <typeparam name="T">The component type. Must implement <see cref="IComponent"/>.</typeparam>
    /// <returns>The registered <see cref="IComponentRenderer{T}"/> instance.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if no renderer is registered for type <typeparamref name="T"/>. (Error DS-152)
    /// </exception>
    public IComponentRenderer<T> Resolve<T>()
        where T : IComponent
    {
        var type = typeof(T);
        if (_renderers.TryGetValue(type, out var renderer))
        {
            return (IComponentRenderer<T>)renderer;
        }

        throw new KeyNotFoundException(
            $"DS-152: No renderer registered for component type '{type.FullName}'.");
    }

    /// <summary>
    /// Resolves a component renderer for the specified component type using a non-generic API.
    /// </summary>
    /// <param name="componentType">The component type to resolve a renderer for.</param>
    /// <returns>The registered renderer as an <see cref="object"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if no renderer is registered for the specified type. (Error DS-152)
    /// </exception>
    public object Resolve(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        if (_renderers.TryGetValue(componentType, out var renderer))
        {
            return renderer;
        }

        throw new KeyNotFoundException(
            $"DS-152: No renderer registered for component type '{componentType.FullName}'.");
    }

    /// <summary>
    /// Gets a value indicating whether a renderer is registered for the specified component type.
    /// </summary>
    /// <typeparam name="T">The component type to check.</typeparam>
    /// <returns><c>true</c> if a renderer is registered; otherwise, <c>false</c>.</returns>
    public bool IsRegistered<T>()
        where T : IComponent
        => _renderers.ContainsKey(typeof(T));

    /// <summary>
    /// Gets the total number of registered renderers.
    /// </summary>
    public int Count => _renderers.Count;

    /// <summary>
    /// Removes all registered renderers.
    /// </summary>
    public void Clear() => _renderers.Clear();
}
