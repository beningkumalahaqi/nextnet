namespace NextNet.TemplateSecurity;

/// <summary>
/// Result of a template security validation operation.
/// </summary>
public sealed record SecurityReport
{
    /// <summary>
    /// Whether the package passed all required security checks.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Whether the checksum verification passed.
    /// </summary>
    public bool ChecksumValid { get; init; }

    /// <summary>
    /// Whether the signature verification passed.
    /// </summary>
    public bool SignatureValid { get; init; }

    /// <summary>
    /// Whether the publisher is in the trusted list.
    /// </summary>
    public bool PublisherTrusted { get; init; }

    /// <summary>
    /// Whether the publisher has been revoked.
    /// </summary>
    public bool PublisherRevoked { get; init; }

    /// <summary>
    /// Warning messages from the validation.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Error messages from the validation.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Timestamp when the validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; init; } = DateTime.UtcNow;
}
