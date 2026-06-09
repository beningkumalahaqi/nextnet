using NextNet.Configuration;

namespace NextNet.Rendering;

/// <summary>
/// Configuration options for the SSR rendering engine.
/// Controls buffering, streaming, caching, and timeout behaviour.
/// </summary>
public sealed class SsrOptions
{
    /// <summary>
    /// Gets or sets whether streaming responses are enabled.
    /// When enabled, the response starts before the page is fully rendered.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool Streaming { get; set; } = true;

    /// <summary>
    /// Gets or sets the buffer size in bytes for streaming chunks.
    /// Defaults to <c>8192</c>.
    /// </summary>
    public int BufferSize { get; set; } = 8192;

    /// <summary>
    /// Gets or sets whether response compression is enabled.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Gets or sets whether HTTP caching headers should be generated.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableCaching { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum time allowed for a single page render.
    /// Defaults to 10 seconds.
    /// </summary>
    public TimeSpan RenderTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Applies merging from an external <see cref="RenderingConfig"/> section.
    /// Maps relevant rendering configuration to SSR options.
    /// </summary>
    /// <param name="config">The configuration to merge from.</param>
    public void Apply(RenderingConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        // BufferSize is independently configurable and not derived from recursion depth.
        // If the config specifies a maxRecursionDepth, we respect it but do not couple
        // unrelated settings. Users configure BufferSize directly on SsrOptions.
    }
}
