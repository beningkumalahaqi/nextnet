using NextNet.Templates.Exceptions;

namespace NextNet.TemplateEngine.Conditionals;

/// <summary>
/// Represents an error that occurs during evaluation of a conditional expression.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EvaluationException"/> is thrown by <see cref="ConditionEvaluator"/> when
/// an expression cannot be evaluated, such as when an unknown operator is encountered
/// or type mismatch prevents comparison.
/// </para>
/// <para>
/// Error code: <c>DS-701</c>
/// </para>
/// </remarks>
public sealed class EvaluationException : TemplateException
{
    /// <summary>
    /// Gets the original expression text that caused the evaluation error, if available.
    /// </summary>
    public string? ExpressionText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationException"/> class.
    /// </summary>
    /// <param name="message">A human-readable description of the evaluation error.</param>
    /// <param name="expressionText">The original expression text that caused the error (optional).</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    public EvaluationException(string message, string? expressionText = null, Exception? inner = null)
        : base(TemplateEngineErrorCodes.EvaluationError, FormatMessage(message, expressionText), inner)
    {
        ExpressionText = expressionText;
    }

    /// <summary>
    /// Formats the error message including optional expression text.
    /// </summary>
    private static string FormatMessage(string message, string? expressionText) =>
        expressionText is not null
            ? $"Evaluation failed for expression '{expressionText}': {message}"
            : $"Expression evaluation failed: {message}";
}
