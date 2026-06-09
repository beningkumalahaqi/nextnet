namespace NextNet.Templates.Exceptions;

/// <summary>
/// The exception that is thrown when a template manifest or package fails validation.
/// </summary>
/// <remarks>
/// <para>
/// Error code: DS-761. This exception is thrown by template validators and the
/// template engine when validation checks fail. It carries the full list of
/// validation errors for detailed reporting.
/// </para>
/// <example>
/// <code>
/// var errors = new List&lt;string&gt;
/// {
///     "Variable 'projectName' is required.",
///     "Feature 'auth' depends on 'identity' which is not enabled."
/// };
/// throw new TemplateValidationException(errors);
/// </code>
/// </example>
/// </remarks>
public sealed class TemplateValidationException : TemplateException
{
    private const string ErrorCodeValue = "DS-761";

    /// <summary>
    /// Gets the list of validation error messages.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateValidationException"/> class.
    /// </summary>
    /// <param name="errors">The list of validation error messages.</param>
    public TemplateValidationException(IReadOnlyList<string> errors)
        : base(ErrorCodeValue, FormatMessage(errors))
    {
        ValidationErrors = errors;
    }

    private static string FormatMessage(IReadOnlyList<string> errors)
        => $"Template validation failed with {errors.Count} error(s): {string.Join("; ", errors)}";
}
