namespace NextNet.TemplateRegistry;

/// <summary>
/// Defines error code constants used throughout the NextNet.TemplateRegistry package.
/// </summary>
/// <remarks>
/// <para>
/// Error codes follow the DS-7xx naming convention, where "DS" stands for
/// "Design System / Templates" and the numeric suffix identifies the specific error.
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Code</term>
///     <description>Error</description>
///   </listheader>
///   <item><term>DS-720</term><description>Registry is unavailable or unreachable.</description></item>
///   <item><term>DS-721</term><description>Requested resource not found in the registry.</description></item>
///   <item><term>DS-722</term><description>Registry API rate limit exceeded.</description></item>
///   <item><term>DS-723</term><description>Registry authentication error.</description></item>
///   <item><term>DS-724</term><description>Registry response parsing error.</description></item>
///   <item><term>DS-725</term><description>Cache operation error.</description></item>
///   <item><term>DS-726</term><description>Invalid registry configuration.</description></item>
///   <item><term>DS-727</term><description>Download error from registry.</description></item>
///   <item><term>DS-728</term><description>Registry metadata error.</description></item>
///   <item><term>DS-729</term><description>Registry service registration error.</description></item>
/// </list>
/// </remarks>
public static class TemplateRegistryErrorCodes
{
    /// <summary>
    /// Error code for registry unavailable errors (DS-720).
    /// </summary>
    public const string RegistryUnavailable = "DS-720";

    /// <summary>
    /// Error code for registry not-found errors (DS-721).
    /// </summary>
    public const string RegistryNotFound = "DS-721";

    /// <summary>
    /// Error code for rate limit errors (DS-722).
    /// </summary>
    public const string RateLimitExceeded = "DS-722";

    /// <summary>
    /// Error code for registry authentication errors (DS-723).
    /// </summary>
    public const string AuthenticationError = "DS-723";

    /// <summary>
    /// Error code for registry response parsing errors (DS-724).
    /// </summary>
    public const string ResponseParsingError = "DS-724";

    /// <summary>
    /// Error code for cache operation errors (DS-725).
    /// </summary>
    public const string CacheError = "DS-725";

    /// <summary>
    /// Error code for invalid registry configuration (DS-726).
    /// </summary>
    public const string InvalidConfiguration = "DS-726";

    /// <summary>
    /// Error code for download errors from registry (DS-727).
    /// </summary>
    public const string DownloadError = "DS-727";

    /// <summary>
    /// Error code for registry metadata errors (DS-728).
    /// </summary>
    public const string MetadataError = "DS-728";

    /// <summary>
    /// Error code for registry service registration errors (DS-729).
    /// </summary>
    public const string ServiceRegistrationError = "DS-729";
}
