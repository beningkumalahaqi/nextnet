using System.Text.Json.Serialization;
using NextNet.Templates.Abstractions;

namespace NextNet.Templates.Models;

/// <summary>
/// Describes the metadata and structure of a template package, including its variables,
/// features, files, and conditions.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TemplateManifest"/> is the root metadata document for a NextNet template.
/// It is serialized as a JSON file (typically <c>template.json</c>) inside a template package
/// and defines everything the template engine needs to evaluate, render, and apply a template.
/// </para>
/// <para>
/// Manifests support semantic versioning via <see cref="Version"/> and compatibility
/// checking via <see cref="NextNetVersion"/>. Variables are resolved from
/// <see cref="IVariableContext"/> at generation time. Features enable optional sections
/// controlled by the user (e.g., "authentication", "logging"). Conditions allow
/// file-level or block-level expression-based inclusion.
/// </para>
/// <example>
/// <code>
/// var manifest = new TemplateManifest(
///     "nextnet-webapi",
///     "1.0.0",
///     "3.0.0",
///     "NextNet",
///     "A Web API project template for NextNet",
///     new[] { "webapi", "rest", "api" },
///     new[] { new TemplateVariable("projectName", "string") },
///     new[] { new TemplateFeature("auth", "Include authentication") },
///     new[] { new TemplateFile("Program.cs", "Program.cs") },
///     null
/// );
/// </code>
/// </example>
/// </remarks>
/// <param name="Name">The unique name of the template (e.g., "nextnet-webapi").</param>
/// <param name="Version">The semantic version of this template package.</param>
/// <param name="NextNetVersion">The minimum NextNet framework version this template requires.</param>
/// <param name="Author">The author or organization that created the template.</param>
/// <param name="Description">A human-readable description of what the template generates.</param>
/// <param name="Tags">Optional tags for searching and categorizing the template.</param>
/// <param name="Variables">The set of input variables the template accepts.</param>
/// <param name="Features">The optional features the template supports.</param>
/// <param name="Files">The list of files the template will generate.</param>
/// <param name="Conditions">Global conditions that apply to the entire template.</param>
public sealed record TemplateManifest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("nextnetVersion")] string NextNetVersion,
    [property: JsonPropertyName("author")] string? Author = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags = null,
    [property: JsonPropertyName("variables")] IReadOnlyList<TemplateVariable>? Variables = null,
    [property: JsonPropertyName("features")] IReadOnlyList<TemplateFeature>? Features = null,
    [property: JsonPropertyName("files")] IReadOnlyList<TemplateFile>? Files = null,
    [property: JsonPropertyName("conditions")] IReadOnlyList<TemplateCondition>? Conditions = null
);
