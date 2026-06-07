namespace NextNet.Configuration;

/// <summary>
/// Represents a configuration validation error or warning.
/// </summary>
public sealed class ConfigError
{
    /// <summary>
    /// Gets a machine-readable error code identifying the validation issue.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets a human-readable description of the validation issue.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the severity level of the validation issue.
    /// </summary>
    public ConfigErrorSeverity Severity { get; }

    /// <summary>
    /// Gets the configuration property path where the issue was detected,
    /// or <c>null</c> if not applicable.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigError"/>.
    /// </summary>
    /// <param name="code">A machine-readable error code.</param>
    /// <param name="message">A human-readable description.</param>
    /// <param name="severity">The severity of the issue.</param>
    /// <param name="path">Optional configuration property path.</param>
    public ConfigError(string code, string message, ConfigErrorSeverity severity, string? path = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Severity = severity;
        Path = path;
    }
}

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
