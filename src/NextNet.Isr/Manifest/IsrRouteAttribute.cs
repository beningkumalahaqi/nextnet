namespace NextNet.Isr.Manifest;

/// <summary>
/// Attribute that can be applied to page classes to configure ISR behaviour
/// for that specific route. When present, the attribute values override
/// the global ISR defaults.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class IsrRouteAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the revalidation interval in seconds.
    /// After this many seconds, the page is considered stale and will be revalidated.
    /// Set to <c>0</c> for static pages that never revalidate.
    /// Set to <c>null</c> to inherit the global default.
    /// </summary>
    public int? RevalidateSeconds { get; set; }

    /// <summary>
    /// Gets or sets the cache tags for grouped invalidation.
    /// Tags enable operations like "revalidate all blog posts".
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent regenerations for this route.
    /// Defaults to <c>1</c>.
    /// </summary>
    public int MaxConcurrentRegenerations { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether stale content may be served while a background
    /// revalidation is in progress. Defaults to <c>true</c>.
    /// </summary>
    public bool ServeStaleWhileRevalidate { get; set; } = true;
}
