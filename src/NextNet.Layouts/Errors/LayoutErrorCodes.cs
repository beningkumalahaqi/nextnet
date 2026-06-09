namespace NextNet.Layouts.Errors;

/// <summary>
/// Defines error codes for the NextNet.Layouts library.
/// Range: DS-100 through DS-105.
/// Each code follows the DS-NNN format used across the framework.
/// </summary>
public static class LayoutErrorCodes
{
    /// <summary>
    /// The layout chain exceeds the maximum allowed depth, indicating a possible circular reference.
    /// </summary>
    public const string LayoutChainTooDeep = "DS-100";

    /// <summary>
    /// A layout type could not be resolved from the route manifest for the given path.
    /// </summary>
    public const string LayoutTypeNotResolved = "DS-101";

    /// <summary>
    /// The resolved type does not implement <c>ILayout</c>.
    /// </summary>
    public const string LayoutDoesNotImplementILayout = "DS-102";

    /// <summary>
    /// The layout type is not registered in the DI container.
    /// </summary>
    public const string LayoutNotInContainer = "DS-103";

    /// <summary>
    /// The custom error page render operation failed unexpectedly.
    /// </summary>
    public const string ErrorPageRenderFailed = "DS-104";

    /// <summary>
    /// The error boundary fell back to the built-in error page after all fallback paths were exhausted.
    /// </summary>
    public const string ErrorBoundaryFallback = "DS-105";
}
