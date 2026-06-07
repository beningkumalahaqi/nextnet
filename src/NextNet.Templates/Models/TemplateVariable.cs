using System.Text.Json.Serialization;

namespace NextNet.Templates.Models;

/// <summary>
/// Defines an input variable that a template accepts, including its type, default value,
/// and validation constraints.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateVariable"/> entries are declared in the template manifest and
/// presented to the user at generation time. The template engine resolves each variable
/// from the provided <c>VariableContext</c> and makes it available during rendering.
/// </para>
/// <para>
/// The <see cref="Type"/> property specifies the expected CLR type name (e.g., "string",
/// "bool", "int"). If no type is specified, it defaults to "string".
/// </para>
/// <para>
/// Variables marked as <see cref="Required"/> must have a value provided; otherwise
/// validation fails. <see cref="AllowedValues"/> restricts the variable to a predefined
/// set of acceptable values. The <see cref="Default"/> value is used when no explicit
/// value is supplied and the variable is not required.
/// </para>
/// <example>
/// <code>
/// var variable = new TemplateVariable(
///     "projectName",
///     "string",
///     null,
///     "The name of the project",
///     true,
///     null
/// );
/// </code>
/// </example>
/// </remarks>
/// <param name="Name">The name of the variable (e.g., "projectName").</param>
/// <param name="Type">The CLR type name of the variable value (default: "string").</param>
/// <param name="Default">An optional default value serialized as a <see cref="JsonElement"/>.</param>
/// <param name="Description">A human-readable description of this variable.</param>
/// <param name="Required">Whether a value for this variable is required (default: false).</param>
/// <param name="AllowedValues">An optional list of allowed values for constrained choice variables.</param>
public sealed record TemplateVariable(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type = "string",
    [property: JsonPropertyName("default")] JsonElement? Default = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("required")] bool Required = false,
    [property: JsonPropertyName("allowedValues")] IReadOnlyList<string>? AllowedValues = null
);
