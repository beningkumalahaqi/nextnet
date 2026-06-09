namespace NextNet.Templates.Exceptions;

/// <summary>
/// The exception that is thrown when a requested template cannot be located.
/// </summary>
/// <remarks>
/// <para>
/// Error code: DS-760. This exception is thrown by template providers and the
/// template registry when a template with the specified name (and optional version)
/// does not exist.
/// </para>
/// <para>
/// The <see cref="TemplateName"/> and <see cref="Version"/> properties carry the
/// exact values that were used in the lookup, enabling precise error reporting.
/// </para>
/// <example>
/// <code>
/// throw new TemplateNotFoundException("my-template", "1.0.0");
/// // Message: "Template 'my-template' version '1.0.0' was not found."
///
/// throw new TemplateNotFoundException("my-template");
/// // Message: "Template 'my-template' was not found."
/// </code>
/// </example>
/// </remarks>
public sealed class TemplateNotFoundException : TemplateException
{
    private const string ErrorCodeValue = "DS-760";

    /// <summary>
    /// Gets the name of the template that was not found.
    /// </summary>
    public string TemplateName { get; }

    /// <summary>
    /// Gets the optional version that was requested.
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateNotFoundException"/> class.
    /// </summary>
    /// <param name="templateName">The name of the template that was not found.</param>
    /// <param name="version">An optional version that was requested.</param>
    public TemplateNotFoundException(string templateName, string? version = null)
        : base(ErrorCodeValue, FormatMessage(templateName, version))
    {
        TemplateName = templateName;
        Version = version;
    }

    private static string FormatMessage(string templateName, string? version)
        => version is not null
            ? $"Template '{templateName}' version '{version}' was not found."
            : $"Template '{templateName}' was not found.";
}
