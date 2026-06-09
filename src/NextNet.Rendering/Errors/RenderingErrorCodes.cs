namespace NextNet.Rendering.Errors;

/// <summary>
/// Defines DS-150..157 error codes for the NextNet.Rendering package.
/// Codes are code-only strings used as prefixes in formatted messages.
/// </summary>
/// <example>
/// Use in error messages to provide traceable error codes:
/// <code>
/// throw new RenderException(
///     $"[{RenderingErrorCodes.RouteNotFound}] No route found for '{route}'.");
/// </code>
/// </example>
public static class RenderingErrorCodes
{
    /// <summary>
    /// DS-150: The requested route was not found in the route manifest.
    /// </summary>
    public const string RouteNotFound = "DS-150";

    /// <summary>
    /// DS-151: The page component type could not be resolved for a given route.
    /// </summary>
    public const string PageTypeNotResolved = "DS-151";

    /// <summary>
    /// DS-152: The page component type is not registered in the DI container.
    /// </summary>
    public const string PageTypeNotRegistered = "DS-152";

    /// <summary>
    /// DS-153: The layout component type could not be resolved for a given layout path.
    /// </summary>
    public const string LayoutTypeNotResolved = "DS-153";

    /// <summary>
    /// DS-154: The layout component type is not registered in the DI container.
    /// </summary>
    public const string LayoutTypeNotRegistered = "DS-154";

    /// <summary>
    /// DS-155: The page render exceeded the configured timeout.
    /// </summary>
    public const string RenderTimeout = "DS-155";

    /// <summary>
    /// DS-156: An unexpected error occurred during rendering.
    /// </summary>
    public const string RenderFailed = "DS-156";

    /// <summary>
    /// DS-157: The component resolver failed to initialize.
    /// </summary>
    public const string ComponentResolverInitFailed = "DS-157";
}
