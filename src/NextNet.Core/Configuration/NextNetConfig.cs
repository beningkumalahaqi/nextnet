using System.Text.Json.Serialization;

namespace NextNet.Configuration;

/// <summary>
/// Represents the root configuration for a NextNet application.
/// Loaded from <c>nextnet.config.json</c> at project root.
/// </summary>
/// <example>
/// <code>
/// // nextnet.config.json
/// {
///     "appDir": "src/app",
///     "outputDir": "build",
///     "devPort": 5000,
///     "ssr": true,
///     "rendering": {
///         "prettyPrint": true,
///         "maxRecursionDepth": 64
///     }
/// }
/// </code>
/// </example>
public sealed class NextNetConfig
{
    /// <summary>
    /// Gets or sets the application source directory (relative to project root).
    /// Defaults to <c>"app"</c>.
    /// </summary>
    [JsonPropertyName("appDir")]
    public string AppDir { get; set; } = "app";

    /// <summary>
    /// Gets or sets the build output directory (relative to project root).
    /// Defaults to <c>"dist"</c>.
    /// </summary>
    [JsonPropertyName("outputDir")]
    public string OutputDir { get; set; } = "dist";

    /// <summary>
    /// Gets or sets the development server port.
    /// Defaults to <c>3000</c>.
    /// </summary>
    [JsonPropertyName("devPort")]
    public int DevPort { get; set; } = 3000;

    /// <summary>
    /// Gets or sets whether server-side rendering is enabled.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("ssr")]
    public bool Ssr { get; set; } = true;

    /// <summary>
    /// Gets or sets whether static site generation is enabled.
    /// Defaults to <c>false</c>.
    /// </summary>
    [JsonPropertyName("ssg")]
    public bool Ssg { get; set; } = false;

    /// <summary>
    /// Gets or sets whether streaming SSR is enabled.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("streaming")]
    public bool Streaming { get; set; } = true;

    /// <summary>
    /// Gets or sets whether server actions are enabled.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("serverActions")]
    public bool ServerActions { get; set; } = true;

    /// <summary>
    /// Gets or sets the file system watch debounce interval in milliseconds.
    /// Used by the development server to debounce file change notifications.
    /// Defaults to <c>250</c>.
    /// </summary>
    [JsonPropertyName("watchDebounceMs")]
    public int WatchDebounceMs { get; set; } = 250;

    /// <summary>
    /// Gets or sets the rendering configuration.
    /// </summary>
    [JsonPropertyName("rendering")]
    public RenderingConfig Rendering { get; set; } = new();
}
