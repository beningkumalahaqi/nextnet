namespace NextNet.Middleware.Attributes;

/// <summary>
/// Specifies middleware to apply to a page or route handler class.
/// Can be applied multiple times to add multiple middleware components.
/// Route-level middleware runs after global middleware and before the route handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class UseMiddlewareAttribute : Attribute
{
    /// <summary>
    /// Gets the middleware type to apply.
    /// </summary>
    public Type MiddlewareType { get; }

    /// <summary>
    /// Gets or sets optional route patterns to scope the middleware.
    /// If not set, the middleware applies to all routes handled by the class.
    /// Supports wildcard patterns (e.g., "/admin/*").
    /// </summary>
    public string[]? Routes { get; set; }

    /// <summary>
    /// Gets or sets the execution order within the route-level middleware set.
    /// Lower values run first.
    /// </summary>
    public int Order { get; set; } = MiddlewareOrder.Normal;

    /// <summary>
    /// Initializes a new instance of the <see cref="UseMiddlewareAttribute"/> class.
    /// </summary>
    /// <param name="middlewareType">The middleware type implementing <see cref="IMiddleware"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middlewareType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="middlewareType"/> does not implement <see cref="IMiddleware"/>.</exception>
    public UseMiddlewareAttribute(Type middlewareType)
    {
        if (middlewareType == null) throw new ArgumentNullException(nameof(middlewareType));
        if (!typeof(IMiddleware).IsAssignableFrom(middlewareType))
            throw new ArgumentException(
                $"Type '{middlewareType.FullName}' must implement {nameof(IMiddleware)}.",
                nameof(middlewareType));

        MiddlewareType = middlewareType;
    }
}
