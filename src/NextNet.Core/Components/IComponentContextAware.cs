namespace NextNet.Components;

/// <summary>
/// Marks a component as requiring a <see cref="ComponentContext"/> before rendering.
/// </summary>
/// <remarks>
/// This interface is implemented automatically by the <see cref="Page"/> base class.
/// The SSR pipeline detects components that implement this interface and calls
/// <see cref="SetContext"/> prior to invoking <c>Render()</c>.
/// Custom implementations should be rare; prefer inheriting from <see cref="Page"/>.
/// </remarks>
public interface IComponentContextAware
{
    /// <summary>
    /// Sets the component context for the current request.
    /// Called by the rendering pipeline before <c>Render()</c>.
    /// </summary>
    /// <param name="context">The component context containing HTTP context, route params, and query params.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    void SetContext(ComponentContext context);
}
