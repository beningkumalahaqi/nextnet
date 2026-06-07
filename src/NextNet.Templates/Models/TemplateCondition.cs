using System.Text.Json.Serialization;

namespace NextNet.Templates.Models;

/// <summary>
/// Defines a conditional expression that controls whether a template element (file, block,
/// or section) is included during generation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateCondition"/> allows template authors to define fine-grained control
/// over what gets generated based on variable values, enabled features, or custom expressions.
/// </para>
/// <para>
/// The <see cref="Expression"/> is evaluated by the template engine at generation time.
/// The <see cref="Type"/> property indicates the expression language or evaluation strategy
/// (default: "expression"). Future implementations may support additional types like
/// "feature" or "variable" for optimized evaluation.
/// </para>
/// <example>
/// <code>
/// var condition = new TemplateCondition(
///     "features.auth == true",
///     "expression"
/// );
/// </code>
/// </example>
/// </remarks>
/// <param name="Expression">The conditional expression string to evaluate.</param>
/// <param name="Type">The type of condition evaluation (default: "expression").</param>
public sealed record TemplateCondition(
    [property: JsonPropertyName("expression")] string Expression,
    [property: JsonPropertyName("type")] string Type = "expression"
);
