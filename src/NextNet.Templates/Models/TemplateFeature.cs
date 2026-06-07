using System.Text.Json.Serialization;

namespace NextNet.Templates.Models;

/// <summary>
/// Represents an optional feature that a template supports, which users can enable or
/// disable at generation time.
/// </summary>
/// <remarks>
/// <para>
/// Features allow templates to conditionally include groups of files, variables, or
/// configuration. For example, a web API template might have features like "auth",
/// "swagger", or "logging" that users can toggle.
/// </para>
/// <para>
/// Features can declare <see cref="Dependencies"/> (other features that must be enabled
/// for this feature to work) and <see cref="Conflicts"/> (features that cannot be enabled
/// simultaneously with this one).
/// </para>
/// <example>
/// <code>
/// var feature = new TemplateFeature(
///     "auth",
///     "Include JWT-based authentication",
///     new[] { "identity" },
///     new[] { "no-auth" }
/// );
/// </code>
/// </example>
/// </remarks>
/// <param name="Name">The unique name of this feature (e.g., "auth", "logging").</param>
/// <param name="Description">A human-readable description of what this feature adds.</param>
/// <param name="Dependencies">Optional list of feature names that must also be enabled.</param>
/// <param name="Conflicts">Optional list of feature names that cannot be enabled with this one.</param>
public sealed record TemplateFeature(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<string>? Dependencies = null,
    [property: JsonPropertyName("conflicts")] IReadOnlyList<string>? Conflicts = null
);
