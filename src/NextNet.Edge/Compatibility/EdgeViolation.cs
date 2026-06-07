namespace NextNet.Edge.Compatibility;

/// <summary>
/// Represents a single compatibility violation found during edge compatibility checking.
/// </summary>
public class EdgeViolation
{
    /// <summary>
    /// Gets the severity level of the violation.
    /// </summary>
    public EdgeViolationSeverity Severity { get; }

    /// <summary>
    /// Gets a human-readable description of the violation.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the file path where the violation was found, if available.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the line number in the source file where the violation was found, if available.
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Gets the fully-qualified type name that caused the violation.
    /// </summary>
    public string? TypeName { get; }

    /// <summary>
    /// Gets the member name that caused the violation (method, property, etc.).
    /// </summary>
    public string? MemberName { get; }

    /// <summary>
    /// Gets an alternative API suggestion, if available.
    /// </summary>
    public string? Suggestion { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeViolation"/>.
    /// </summary>
    /// <param name="severity">The violation severity.</param>
    /// <param name="message">The violation message.</param>
    /// <param name="filePath">Optional file path.</param>
    /// <param name="lineNumber">Optional line number.</param>
    /// <param name="typeName">Optional fully-qualified type name.</param>
    /// <param name="memberName">Optional member name.</param>
    /// <param name="suggestion">Optional alternative API suggestion.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public EdgeViolation(
        EdgeViolationSeverity severity,
        string message,
        string? filePath = null,
        int? lineNumber = null,
        string? typeName = null,
        string? memberName = null,
        string? suggestion = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Severity = severity;
        FilePath = filePath;
        LineNumber = lineNumber;
        TypeName = typeName;
        MemberName = memberName;
        Suggestion = suggestion;
    }

    /// <summary>
    /// Returns a formatted string representation of the violation.
    /// </summary>
    public override string ToString()
    {
        var location = FilePath != null
            ? $"{FilePath}{(LineNumber.HasValue ? $"({LineNumber.Value})" : "")}"
            : "(unknown)";
        var typeInfo = TypeName != null ? $" [{TypeName}]" : "";
        var suggestion = Suggestion != null ? $" — Suggestion: {Suggestion}" : "";
        return $"[{Severity}] {location}: {Message}{typeInfo}{suggestion}";
    }
}

/// <summary>
/// Defines severity levels for edge compatibility violations.
/// </summary>
public enum EdgeViolationSeverity
{
    /// <summary>
    /// Informational — may affect edge compatibility but not necessarily blocking.
    /// </summary>
    Info,

    /// <summary>
    /// Warning — may cause issues on certain edge providers.
    /// </summary>
    Warning,

    /// <summary>
    /// Error — definitively incompatible with edge runtimes.
    /// </summary>
    Error,
}
