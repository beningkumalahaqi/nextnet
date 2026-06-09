namespace NextNet.Data.HealthChecks;

/// <summary>
/// Defines standardized error codes for the NextNet.Data.HealthChecks package (DS-570 to DS-574).
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used as prefixes in exception messages to enable structured
/// error identification, logging, and monitoring. Each code maps to a specific error
/// scenario within the health check subsystem.
/// </para>
/// </remarks>
public static class HealthCheckErrorCodes
{
    /// <summary>
    /// A health check operation has exceeded its configured timeout.
    /// </summary>
    public const string HealthCheckTimeout = "DS-570";

    /// <summary>
    /// A database connection test within a health check failed.
    /// </summary>
    public const string ConnectionTestFailed = "DS-571";

    /// <summary>
    /// The health check configuration is invalid.
    /// </summary>
    public const string ConfigurationInvalid = "DS-572";

    /// <summary>
    /// A custom health check implementation returned a failure result.
    /// </summary>
    public const string CustomCheckFailed = "DS-573";

    /// <summary>
    /// A dependency required by a health check is unavailable.
    /// </summary>
    public const string DependencyUnavailable = "DS-574";
}
