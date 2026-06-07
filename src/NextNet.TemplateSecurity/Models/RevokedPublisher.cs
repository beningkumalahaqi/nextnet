namespace NextNet.TemplateSecurity;

/// <summary>
/// Represents a publisher whose trust has been revoked.
/// </summary>
public sealed record RevokedPublisher
{
    /// <summary>
    /// Unique identifier of the revoked publisher.
    /// </summary>
    public string PublisherId { get; init; } = "";

    /// <summary>
    /// Reason for the revocation.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// When the publisher was revoked.
    /// </summary>
    public DateTime RevokedAt { get; init; }
}
