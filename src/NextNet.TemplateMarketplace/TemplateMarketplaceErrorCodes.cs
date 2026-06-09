namespace NextNet.TemplateMarketplace;

/// <summary>
/// Defines standardized error codes for the NextNet Template Marketplace layer.
/// Each code corresponds to a specific exception type or error condition
/// and is embedded in exception messages as a prefix (e.g., "[DS-920]").
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used throughout the NextNet Template Marketplace framework
/// to enable programmatic error identification, automated handling, and diagnostic tooling.
/// Exception messages are prefixed with the error code in square brackets.
/// </para>
/// </remarks>
public static class TemplateMarketplaceErrorCodes
{
    // ──────────────────────────────────────────────────────────────
    //  Error codes (DS-920 … DS-929 range)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// The marketplace API is unreachable or returns a server error.
    /// </summary>
    public const string MarketplaceUnavailable = "DS-920";

    /// <summary>
    /// Submitting a rating or review to the marketplace failed.
    /// </summary>
    public const string RatingSubmitFailed = "DS-921";
}
