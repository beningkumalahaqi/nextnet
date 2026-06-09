using System.Text.Json.Serialization;

namespace NextNet.Configuration;

/// <summary>
/// Configuration options for HTML rendering.
/// </summary>
public sealed class RenderingConfig
{
    /// <summary>
    /// Gets or sets whether rendered HTML should be pretty-printed with indentation.
    /// Defaults to <c>false</c>.
    /// </summary>
    [JsonPropertyName("prettyPrint")]
    public bool PrettyPrint { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum recursion depth for rendering nested components.
    /// Defaults to <c>128</c>.
    /// </summary>
    [JsonPropertyName("maxRecursionDepth")]
    public int MaxRecursionDepth { get; set; } = 128;

    /// <summary>
    /// Gets or sets whether to minify the rendered HTML output.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("minify")]
    public bool Minify { get; set; } = true;
}
