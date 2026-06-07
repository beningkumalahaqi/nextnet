namespace NextNet.Middleware.Attributes;

/// <summary>
/// Specifies the execution order for a middleware component.
/// Middleware with lower order values execute earlier in the pipeline.
/// This attribute is applied to middleware implementations to set their default priority.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MiddlewareOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the execution order. Lower values run first.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiddlewareOrderAttribute"/> class.
    /// </summary>
    /// <param name="order">The execution order for the middleware.</param>
    public MiddlewareOrderAttribute(int order)
    {
        Order = order;
    }
}
