namespace NextNet.Templates.Exceptions;

/// <summary>
/// The exception that is thrown when an error occurs during template file generation.
/// </summary>
/// <remarks>
/// <para>
/// Error code: DS-762. This exception is thrown by the template engine when a file
/// cannot be generated due to I/O errors, template syntax errors, or other runtime
/// failures during the generation process.
/// </para>
/// <para>
/// If the error is specific to a particular file, the <see cref="FilePath"/> property
/// carries the relative path of the file that caused the failure.
/// </para>
/// <example>
/// <code>
/// throw new TemplateGenerationException(
///     "Failed to process template syntax",
///     "Controllers/WeatherController.cs");
/// </code>
/// </example>
/// </remarks>
public sealed class TemplateGenerationException : TemplateException
{
    private const string ErrorCodeValue = "DS-762";

    /// <summary>
    /// Gets the relative path of the file that caused the generation error, if applicable.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateGenerationException"/> class.
    /// </summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="filePath">An optional relative path to the file that caused the error.</param>
    /// <param name="inner">An optional inner exception that caused this error.</param>
    public TemplateGenerationException(string message, string? filePath = null, Exception? inner = null)
        : base(ErrorCodeValue, FormatMessage(message, filePath), inner)
    {
        FilePath = filePath;
    }

    private static string FormatMessage(string message, string? filePath)
        => filePath is not null
            ? $"Template generation failed for '{filePath}': {message}"
            : $"Template generation failed: {message}";
}
