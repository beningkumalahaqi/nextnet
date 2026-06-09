namespace NextNet.Routing.Models;

/// <summary>
/// Describes the severity level of a route conflict.
/// </summary>
public enum ConflictSeverity
{
    /// <summary>
    /// A warning-level conflict that does not prevent the application from running
    /// but may indicate an unintended configuration.
    /// </summary>
    Warning,

    /// <summary>
    /// An error-level conflict that should be resolved before deployment.
    /// </summary>
    Error,
}

/// <summary>
/// Represents a conflict or issue discovered between route entries during
/// route scanning and conflict detection.
/// </summary>
/// <param name="Message">A human-readable description of the conflict.</param>
/// <param name="RoutePattern">The route pattern associated with the conflict.</param>
/// <param name="ConflictingFiles">The file paths involved in the conflict.</param>
/// <param name="Severity">The severity level.</param>
public sealed record RouteConflict(string Message, string RoutePattern, IReadOnlyList<string> ConflictingFiles, ConflictSeverity Severity)
{
    /// <inheritdoc />
    public override string ToString()
        => $"[{Severity}] {RoutePattern}: {Message}";
}
