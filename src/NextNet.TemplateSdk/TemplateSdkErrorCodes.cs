namespace NextNet.TemplateSdk;

/// <summary>
/// Defines error code constants used throughout the NextNet.TemplateSdk package.
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
///   <item><term>DS-740</term><description>Template source directory not found.</description></item>
///   <item><term>DS-741</term><description>Template manifest (template.json) not found.</description></item>
///   <item><term>DS-742</term><description>Package creation failed.</description></item>
///   <item><term>DS-743</term><description>Publish operation failed (no API token).</description></item>
///   <item><term>DS-744</term><description>Publish operation failed (server error).</description></item>
///   <item><term>DS-745</term><description>Scaffold operation failed.</description></item>
///   <item><term>DS-746</term><description>Template validation error.</description></item>
///   <item><term>DS-747</term><description>Invalid argument to SDK operation.</description></item>
///   <item><term>DS-748</term><description>Author profile error.</description></item>
///   <item><term>DS-749</term><description>Template SDK CLI error.</description></item>
/// </list>
/// </remarks>
public static class TemplateSdkErrorCodes
{
    /// <summary>
    /// Error code for template source directory not found (DS-740).
    /// </summary>
    public const string SourceDirectoryNotFound = "DS-740";

    /// <summary>
    /// Error code for template manifest not found (DS-741).
    /// </summary>
    public const string ManifestNotFound = "DS-741";

    /// <summary>
    /// Error code for package creation failure (DS-742).
    /// </summary>
    public const string PackageCreationFailed = "DS-742";

    /// <summary>
    /// Error code for publish failure due to missing API token (DS-743).
    /// </summary>
    public const string PublishMissingToken = "DS-743";

    /// <summary>
    /// Error code for publish failure due to server error (DS-744).
    /// </summary>
    public const string PublishServerError = "DS-744";

    /// <summary>
    /// Error code for scaffold operation failure (DS-745).
    /// </summary>
    public const string ScaffoldFailed = "DS-745";

    /// <summary>
    /// Error code for template validation errors (DS-746).
    /// </summary>
    public const string ValidationError = "DS-746";

    /// <summary>
    /// Error code for invalid argument to SDK operation (DS-747).
    /// </summary>
    public const string InvalidArgument = "DS-747";

    /// <summary>
    /// Error code for author profile errors (DS-748).
    /// </summary>
    public const string AuthorProfileError = "DS-748";

    /// <summary>
    /// Error code for Template SDK CLI errors (DS-749).
    /// </summary>
    public const string CliError = "DS-749";
}
