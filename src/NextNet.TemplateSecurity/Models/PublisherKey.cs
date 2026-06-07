namespace NextNet.TemplateSecurity;

/// <summary>
/// Supported cryptographic key algorithms for template signing.
/// </summary>
public enum KeyAlgorithm
{
    /// <summary>
    /// RSA with 2048-bit key length.
    /// </summary>
    Rsa2048,

    /// <summary>
    /// Ed25519 signature algorithm.
    /// </summary>
    Ed25519
}

/// <summary>
/// Represents a publisher's cryptographic key pair.
/// </summary>
public sealed record PublisherKey
{
    /// <summary>
    /// Unique identifier for the publisher.
    /// </summary>
    public string PublisherId { get; init; } = "";

    /// <summary>
    /// The cryptographic algorithm used for this key.
    /// </summary>
    public KeyAlgorithm Algorithm { get; init; } = KeyAlgorithm.Rsa2048;

    /// <summary>
    /// Public key encoded as a Base64 string.
    /// </summary>
    public string PublicKeyBase64 { get; init; } = "";

    /// <summary>
    /// When the key was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Private key encoded as a Base64 string. <c>null</c> for public-only keys.
    /// </summary>
    public string? PrivateKeyBase64 { get; init; }
}
