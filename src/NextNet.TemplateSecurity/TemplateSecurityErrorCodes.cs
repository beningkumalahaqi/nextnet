namespace NextNet.TemplateSecurity;

/// <summary>
/// Defines standardized error codes for the NextNet Template Security layer.
/// Each code corresponds to a specific exception type or error condition
/// and is embedded in exception messages as a prefix (e.g., "[DS-820]").
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used throughout the NextNet Template Security framework
/// to enable programmatic error identification, automated handling, and diagnostic tooling.
/// Exception messages are prefixed with the error code in square brackets.
/// </para>
/// </remarks>
public static class TemplateSecurityErrorCodes
{
    // ──────────────────────────────────────────────────────────────
    //  Error codes (DS-820 … DS-829 range)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// A computed checksum does not match the expected value.
    /// </summary>
    public const string ChecksumMismatch = "DS-820";

    /// <summary>
    /// No key was found for a given publisher.
    /// </summary>
    public const string KeyNotFound = "DS-821";

    /// <summary>
    /// A signature is invalid or cannot be verified.
    /// </summary>
    public const string SignatureInvalid = "DS-822";

    /// <summary>
    /// A publisher has been revoked.
    /// </summary>
    public const string PublisherRevoked = "DS-823";

    /// <summary>
    /// A publisher is not in the trusted list.
    /// </summary>
    public const string PublisherNotTrusted = "DS-824";
}
