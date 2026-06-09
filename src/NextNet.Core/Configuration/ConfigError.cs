namespace NextNet.Configuration;

/// <summary>
/// Represents a configuration validation error or warning.
/// </summary>
/// <param name="Code">A machine-readable error code identifying the validation issue.</param>
/// <param name="Message">A human-readable description of the validation issue.</param>
/// <param name="Severity">The severity level of the validation issue.</param>
/// <param name="Path">Optional configuration property path where the issue was detected.</param>
public sealed record ConfigError(
    string Code,
    string Message,
    ConfigErrorSeverity Severity,
    string? Path = null);

/// <summary>
/// Defines the severity level of a configuration validation issue.
/// </summary>
public enum ConfigErrorSeverity
{
    /// <summary>
    /// A warning that does not prevent the application from running.
    /// </summary>
    Warning,

    /// <summary>
    /// An error that should be addressed before running the application.
    /// </summary>
    Error,
}
