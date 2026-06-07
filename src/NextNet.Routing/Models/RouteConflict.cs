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
public class RouteConflict
{
    /// <summary>
    /// Gets a human-readable description of the conflict.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the route pattern associated with the conflict.
    /// </summary>
    public string RoutePattern { get; }

    /// <summary>
    /// Gets the list of file paths that are involved in the conflict.
    /// </summary>
    public IReadOnlyList<string> ConflictingFiles { get; }

    /// <summary>
    /// Gets the severity of the conflict.
    /// </summary>
    public ConflictSeverity Severity { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RouteConflict"/>.
    /// </summary>
    /// <param name="message">A human-readable description of the conflict.</param>
    /// <param name="routePattern">The route pattern associated with the conflict.</param>
    /// <param name="conflictingFiles">The file paths involved in the conflict.</param>
    /// <param name="severity">The severity level.</param>
    public RouteConflict(
        string message,
        string routePattern,
        IReadOnlyList<string> conflictingFiles,
        ConflictSeverity severity)
    {
        Message = message;
        RoutePattern = routePattern;
        ConflictingFiles = conflictingFiles;
        Severity = severity;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"[{Severity}] {RoutePattern}: {Message}";
}
