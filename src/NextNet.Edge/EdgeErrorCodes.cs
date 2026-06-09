namespace NextNet.Edge;

/// <summary>
/// Defines standardized error codes used throughout the NextNet Edge runtime.
/// Error codes follow the DS-3xx range for edge-related errors.
/// </summary>
public static class EdgeErrorCodes
{
    /// <summary>
    /// An adapter for the specified provider is already registered.
    /// </summary>
    public const string AdapterAlreadyRegistered = "DS-350";

    /// <summary>
    /// No adapter is registered for the specified provider.
    /// </summary>
    public const string NoAdapterRegisteredForProvider = "DS-351";

    /// <summary>
    /// The specified edge provider is not supported.
    /// </summary>
    public const string UnsupportedProvider = "DS-352";

    /// <summary>
    /// The compatibility check failed due to one or more violations.
    /// </summary>
    public const string CompatibilityCheckFailed = "DS-353";

    /// <summary>
    /// The static asset count exceeds the configured maximum.
    /// </summary>
    public const string StaticAssetCountExceeded = "DS-354";

    /// <summary>
    /// The response size budget has been exceeded.
    /// </summary>
    public const string SizeBudgetExceeded = "DS-355";

    /// <summary>
    /// The edge stream has already been completed and cannot be written to.
    /// </summary>
    public const string StreamAlreadyCompleted = "DS-356";

    /// <summary>
    /// Cannot convert an EdgeHttpContext to an ASP.NET Core HttpContext on a pure edge runtime.
    /// </summary>
    public const string CannotConvertToAspNetCoreContext = "DS-357";

    /// <summary>
    /// A compatibility error was found that blocked the operation.
    /// </summary>
    public const string CompatibilityError = "DS-358";
}
