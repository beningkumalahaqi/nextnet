namespace NextNet.TemplateSecurity;

/// <summary>
/// Represents a trusted publisher in the registry.
/// </summary>
public sealed record TrustedPublisher
{
    /// <summary>
    /// Unique identifier for the publisher.
    /// </summary>
    public string PublisherId { get; init; } = "";

    /// <summary>
    /// Optional human-readable display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Optional public key associated with this publisher.
    /// </summary>
    public string? PublicKey { get; init; }

    /// <summary>
    /// When the publisher was added to the trusted list.
    /// </summary>
    public DateTime AddedAt { get; init; }

    /// <summary>
    /// Who added this publisher to the trusted list.
    /// </summary>
    public string? AddedBy { get; init; }
}
