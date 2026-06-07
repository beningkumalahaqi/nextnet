using System.Text.Json.Serialization;

namespace NextNet.Templates.Models;

/// <summary>
/// Describes a single file entry within a template manifest, mapping a source path
/// inside the template package to a target path in the generated output.
/// </summary>
/// <remarks>
/// <para>
/// Each <see cref="TemplateFile"/> entry specifies where the source file lives inside
/// the template package (<see cref="SourcePath"/>) and where it should be written
/// in the generated project (<see cref="TargetPath"/>).
/// </para>
/// <para>
/// File generation can be conditionally skipped by providing a <see cref="Condition"/>
/// expression. If the condition evaluates to <c>false</c> at generation time, the
/// file is omitted from the output. Binary files (images, binaries) should set
/// <see cref="IsBinary"/> to <c>true</c> so the engine treats them as raw byte copies
/// rather than text templates.
/// </para>
/// <example>
/// <code>
/// var file = new TemplateFile(
///     "templates/Controllers/WeatherController.cs",
///     "Controllers/WeatherController.cs",
///     "features.api",
///     false
/// );
/// </code>
/// </example>
/// </remarks>
/// <param name="SourcePath">The relative path to the source file inside the template package.</param>
/// <param name="TargetPath">The relative path where the file should be written in the target project.</param>
/// <param name="Condition">An optional expression that must evaluate to true for this file to be generated.</param>
/// <param name="IsBinary">Whether this file is a binary file (default: false). Binary files are copied as-is without text processing.</param>
public sealed record TemplateFile(
    [property: JsonPropertyName("source")] string SourcePath,
    [property: JsonPropertyName("target")] string TargetPath,
    [property: JsonPropertyName("condition")] string? Condition = null,
    [property: JsonPropertyName("binary")] bool IsBinary = false
);
