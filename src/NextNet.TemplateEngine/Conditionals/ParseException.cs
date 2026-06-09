using NextNet.Templates.Exceptions;

namespace NextNet.TemplateEngine.Conditionals;

/// <summary>
/// Represents an error that occurs during parsing of a conditional expression.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ParseException"/> is thrown by <see cref="ConditionParser"/> when the
/// input expression string contains a syntax error. It includes the character position
/// where the error was detected and, optionally, the surrounding context text.
/// </para>
/// <para>
/// Error code: <c>DS-700</c>
/// </para>
/// </remarks>
public sealed class ParseException : TemplateException
{
    /// <summary>
    /// Gets the character position in the expression where the error was detected.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the optional context text (typically the full expression) surrounding the error.
    /// </summary>
    public string? Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// </summary>
    /// <param name="message">A human-readable description of the parse error.</param>
    /// <param name="position">The character position where the error occurred.</param>
    /// <param name="context">Optional context text for richer error messages.</param>
    public ParseException(string message, int position, string? context = null)
        : base(TemplateEngineErrorCodes.ParseError, FormatMessage(message, position, context))
    {
        Position = position;
        Context = context;
    }

    /// <summary>
    /// Formats the error message including position and optional context.
    /// </summary>
    private static string FormatMessage(string message, int position, string? context) =>
        context is not null
            ? $"Expression parse error at position {position} ('{context}'): {message}"
            : $"Expression parse error at position {position}: {message}";
}
