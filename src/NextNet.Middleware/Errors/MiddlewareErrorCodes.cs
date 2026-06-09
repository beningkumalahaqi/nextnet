namespace NextNet.Middleware.Errors;

/// <summary>
/// Defines error codes for the NextNet.Middleware package.
/// Error codes range from DS-700 to DS-709.
/// </summary>
/// <example>
/// <code>
/// // Throwing an exception with an error code
/// throw new ArgumentException(
///     $"{MiddlewareErrorCodes.InvalidMiddlewareType}: Type '{type.FullName}' does not implement IMiddleware.",
///     nameof(middlewareType));
/// </code>
/// </example>
public static class MiddlewareErrorCodes
{
    /// <summary>
    /// DS-700: Invalid middleware type — the type does not implement <see cref="IMiddleware"/>.
    /// </summary>
    public const string InvalidMiddlewareType = "DS-700";

    /// <summary>
    /// DS-701: Pipeline already built — cannot modify the pipeline after it has been built.
    /// </summary>
    public const string PipelineAlreadyBuilt = "DS-701";

    /// <summary>
    /// DS-702: Route pattern invalid — the route pattern string is null, empty, or malformed.
    /// </summary>
    public const string RoutePatternInvalid = "DS-702";

    /// <summary>
    /// DS-703: Middleware resolution failed — unable to resolve middleware instance from the service provider.
    /// </summary>
    public const string MiddlewareResolutionFailed = "DS-703";

    /// <summary>
    /// DS-704: Circular branch pipeline — a branch pipeline contains a reference back to the parent, creating a cycle.
    /// </summary>
    public const string CircularBranchPipeline = "DS-704";

    /// <summary>
    /// DS-705: Middleware instance is null — a required middleware instance was null.
    /// </summary>
    public const string MiddlewareInstanceIsNull = "DS-705";

    /// <summary>
    /// DS-706: Predicate is null — a required predicate function was null.
    /// </summary>
    public const string PredicateIsNull = "DS-706";

    /// <summary>
    /// DS-707: Configuration delegate is null — a required configuration delegate was null.
    /// </summary>
    public const string ConfigurationDelegateIsNull = "DS-707";

    /// <summary>
    /// DS-708: Service provider is null — the service provider used to resolve middleware was null.
    /// </summary>
    public const string ServiceProviderIsNull = "DS-708";

    /// <summary>
    /// DS-709: Terminal delegate error — the terminal delegate threw an unhandled exception.
    /// </summary>
    public const string TerminalDelegateError = "DS-709";
}
