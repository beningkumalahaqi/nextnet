namespace NextNet.TemplateSecurity;

/// <summary>
/// Context for a template security validation operation.
/// </summary>
public sealed record SecurityContext
{
    /// <summary>
    /// Name of the package being validated.
    /// </summary>
    public string PackageName { get; init; } = "";

    /// <summary>
    /// Version of the package being validated.
    /// </summary>
    public string PackageVersion { get; init; } = "";

    /// <summary>
    /// Identifier of the publisher claiming the package.
    /// </summary>
    public string PublisherId { get; init; } = "";

    /// <summary>
    /// Expected SHA-256 checksum of the package content.
    /// </summary>
    public string? ExpectedChecksum { get; init; }

    /// <summary>
    /// Cryptographic signature over the package content.
    /// </summary>
    public byte[]? Signature { get; init; }

    /// <summary>
    /// Publisher's public key for signature verification.
    /// </summary>
    public PublisherKey? PublisherKey { get; init; }

    /// <summary>
    /// Security level to enforce during validation.
    /// </summary>
    public SecurityLevel SecurityLevel { get; init; } = SecurityLevel.Standard;
}
