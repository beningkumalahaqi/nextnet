namespace NextNet.TemplateSecurity;

/// <summary>
/// Defines the security level for template validation.
/// </summary>
public enum SecurityLevel
{
    /// <summary>
    /// Skip signature checks, only verify checksums.
    /// </summary>
    Permissive,

    /// <summary>
    /// Verify checksums and signatures if available.
    /// </summary>
    Standard,

    /// <summary>
    /// Require signatures and trusted publisher verification.
    /// </summary>
    Strict
}

/// <summary>
/// Configuration options for template security validation.
/// </summary>
public sealed class SecurityOptions
{
    /// <summary>
    /// The security level to enforce during validation.
    /// </summary>
    public SecurityLevel Level { get; set; } = SecurityLevel.Standard;

    /// <summary>
    /// Whether checksum verification is required.
    /// </summary>
    public bool RequireChecksums { get; set; } = true;

    /// <summary>
    /// Whether signature verification is required.
    /// </summary>
    public bool RequireSignatures { get; set; } = false;

    /// <summary>
    /// Whether trusted publisher enforcement is active.
    /// </summary>
    public bool EnforceTrustedPublishers { get; set; } = false;

    /// <summary>
    /// Path to the trusted publishers JSON file.
    /// </summary>
    public string TrustedPublishersFile { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nextnet", "trusted-publishers.json");

    /// <summary>
    /// Path to the security audit log file.
    /// </summary>
    public string AuditLogFile { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nextnet", "security-audit.log");
}
