using System.Text.Json.Serialization;

namespace NextNet.Isr.Endpoints;

/// <summary>
/// The request body for the on-demand revalidation endpoint (<c>POST /_isr/revalidate</c>).
/// </summary>
public sealed record IsrRevalidationRequest
{
    /// <summary>
    /// Gets or sets the specific route to revalidate (e.g. <c>"/blog/hello-world"</c>).
    /// When set, only this route is revalidated.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; init; }

    /// <summary>
    /// Gets or sets the tags to invalidate. When specified, all routes with
    /// matching tags will be revalidated.
    /// </summary>
    [JsonPropertyName("tags")]
    public string[]? Tags { get; init; }

    /// <summary>
    /// Gets or sets the revalidation secret for authentication.
    /// Must match the configured <see cref="IsrGlobalOptions.RevalidationSecret"/>.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; init; }
}
