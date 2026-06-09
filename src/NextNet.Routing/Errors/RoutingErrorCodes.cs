namespace NextNet.Routing.Errors;

/// <summary>
/// Defines error codes for the NextNet.Routing component.
/// Error codes are in the DS-050 to DS-059 range.
/// </summary>
public static class RoutingErrorCodes
{
    /// <summary>
    /// Route parsing failed.
    /// </summary>
    public const string RouteParseFailed = "DS-050";

    /// <summary>
    /// API route parsing failed.
    /// </summary>
    public const string ApiRouteParseFailed = "DS-051";

    /// <summary>
    /// Directory access denied.
    /// </summary>
    public const string DirectoryAccessDenied = "DS-052";

    /// <summary>
    /// Route pattern is invalid.
    /// </summary>
    public const string RoutePatternInvalid = "DS-053";

    /// <summary>
    /// Duplicate static route detected.
    /// </summary>
    public const string DuplicateStaticRoute = "DS-054";

    /// <summary>
    /// Static route overlaps with dynamic route.
    /// </summary>
    public const string StaticDynamicOverlap = "DS-055";

    /// <summary>
    /// Dynamic route overlaps with catch-all route.
    /// </summary>
    public const string DynamicCatchAllOverlap = "DS-056";

    /// <summary>
    /// Missing root layout.
    /// </summary>
    public const string MissingRootLayout = "DS-057";

    /// <summary>
    /// Orphaned layout detected.
    /// </summary>
    public const string OrphanedLayout = "DS-058";

    /// <summary>
    /// File watcher directory not found.
    /// </summary>
    public const string FileWatcherDirectoryNotFound = "DS-059";
}
