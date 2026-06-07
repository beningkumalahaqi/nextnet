namespace NextNet.Isr.Manifest;

/// <summary>
/// Per-route ISR metadata that controls revalidation behaviour.
/// This metadata is generated at build time by <see cref="IsrManifestGenerator"/>
/// and consumed at runtime by the ISR middleware and revalidation manager.
/// </summary>
public class IsrRouteMetadata
{
    /// <summary>
    /// Gets or sets the route pattern (e.g. <c>"/blog/{slug}"</c>).
    /// </summary>
    public string RoutePattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the revalidation interval in seconds.
    /// When <c>null</c>, the global default is used.
    /// When <c>0</c>, the page is never automatically revalidated (static).
    /// </summary>
    public int? RevalidateSeconds { get; set; }

    /// <summary>
    /// Gets or sets the cache tags for grouped invalidation.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent regenerations for this route.
    /// Defaults to <c>1</c>.
    /// </summary>
    public int MaxConcurrentRegenerations { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether stale content is served while revalidating.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ServeStaleWhileRevalidate { get; set; } = true;

    /// <summary>
    /// Gets or sets the file path to the source page component.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Converts this metadata to an <see cref="IsrOptions"/> instance for runtime use.
    /// </summary>
    public IsrOptions ToOptions()
    {
        return new IsrOptions
        {
            Revalidate = RevalidateSeconds,
            RevalidateTags = Tags,
            MaxConcurrentRegenerations = MaxConcurrentRegenerations,
            ServeStaleWhileRevalidate = ServeStaleWhileRevalidate
        };
    }
}
