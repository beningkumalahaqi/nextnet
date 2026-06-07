using System.Text.Json.Serialization;

namespace NextNet.Templates.Abstractions;

/// <summary>
/// Describes the outcome of a template validation operation, indicating whether the
/// template is valid and listing any errors or warnings that were found.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ValidationResult"/> is returned by <c>ITemplateValidator</c> and
/// <c>ITemplateEngine.ValidateAsync</c> to report the results of structural and
/// semantic validation checks.
/// </para>
/// <para>
/// A result is considered valid (<see cref="IsValid"/> is <c>true</c>) only when
/// there are no errors. Warnings alone do not invalidate a template but should be
/// surfaced to the user.
/// </para>
/// <example>
/// <code>
/// var result = new ValidationResult(
///     false,
///     new[] { "Variable 'projectName' is required but not provided." },
///     new[] { "Variable 'framework' has no default value." });
///
/// if (!result.IsValid)
///     foreach (var error in result.Errors)
///         Console.WriteLine($"  Error: {error}");
/// </code>
/// </example>
/// </remarks>
/// <param name="IsValid"><c>true</c> if validation passed with no errors; otherwise <c>false</c>.</param>
/// <param name="Errors">A list of error messages describing validation failures.</param>
/// <param name="Warnings">A list of warning messages that do not block generation.</param>
public sealed record ValidationResult(
    [property: JsonPropertyName("isValid")] bool IsValid,
    [property: JsonPropertyName("errors")] IReadOnlyList<string>? Errors = null,
    [property: JsonPropertyName("warnings")] IReadOnlyList<string>? Warnings = null
);
