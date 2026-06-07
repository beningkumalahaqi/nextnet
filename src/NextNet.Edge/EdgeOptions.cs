using System.Text.Json.Serialization;

namespace NextNet.Edge;

/// <summary>
/// Configuration options for NextNet Edge runtime support.
/// Controls which edge provider is used, compatibility checking strictness,
/// and size budget limits for edge deployments.
/// </summary>
public class EdgeOptions
{
    /// <summary>
    /// Gets or sets whether edge runtime is enabled.
    /// Defaults to <c>false</c>.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the target edge provider.
    /// Supported values: <c>"cloudflare"</c>, <c>"vercel"</c>, <c>"deno"</c>, <c>"aws"</c>.
    /// Defaults to <c>"cloudflare"</c>.
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = "cloudflare";

    /// <summary>
    /// Gets or sets whether to run the compatibility checker before build/deploy.
    /// Defaults to <c>true</c>.
    /// </summary>
    [JsonPropertyName("checkCompatibility")]
    public bool CheckCompatibility { get; set; } = true;

    /// <summary>
    /// Gets or sets whether compatibility checking is strict.
    /// When strict, violations are errors; otherwise they are warnings.
    /// Defaults to <c>false</c>.
    /// </summary>
    [JsonPropertyName("strict")]
    public bool Strict { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum bundle size in bytes for edge deployments.
    /// Defaults to 1 MB (<c>1048576</c>).
    /// </summary>
    [JsonPropertyName("maxBundleSize")]
    public long MaxBundleSize { get; set; } = 1_048_576;

    /// <summary>
    /// Gets or sets the maximum number of static assets.
    /// Defaults to <c>100</c>.
    /// </summary>
    [JsonPropertyName("maxStaticAssets")]
    public int MaxStaticAssets { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum total deployment size in bytes.
    /// Defaults to 10 MB (<c>10485760</c>).
    /// </summary>
    [JsonPropertyName("maxDeploymentSize")]
    public long MaxDeploymentSize { get; set; } = 10_485_760;

    /// <summary>
    /// Gets or sets the output directory for edge bundles (relative to project root).
    /// Defaults to <c>"dist-edge"</c>.
    /// </summary>
    [JsonPropertyName("outputDir")]
    public string OutputDir { get; set; } = "dist-edge";
}
