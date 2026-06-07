using System.Text.Json.Serialization;

namespace NextNet.Templates.Abstractions;

/// <summary>
/// Describes the outcome of a template generation operation, including which files were
/// created, skipped, and any warnings that were raised.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GenerationResult"/> is returned by <c>ITemplateEngine.GenerateAsync</c>
/// and provides a complete report of what happened during code generation.
/// </para>
/// <para>
/// The <see cref="Elapsed"/> property captures the total wall-clock time spent in
/// generation, which is useful for performance diagnostics and user feedback.
/// </para>
/// <example>
/// <code>
/// var result = new GenerationResult(
///     new[] { "Program.cs", "Startup.cs" },
///     Array.Empty&lt;string&gt;(),
///     new[] { "File 'appsettings.json' already exists, skipped." },
///     TimeSpan.FromMilliseconds(250));
/// </code>
/// </example>
/// </remarks>
/// <param name="GeneratedFiles">The list of relative paths for files that were successfully generated.</param>
/// <param name="SkippedFiles">The list of relative paths for files that were skipped (e.g., existing files when overwrite is disabled).</param>
/// <param name="Warnings">A list of warning messages that occurred during generation.</param>
/// <param name="Elapsed">The total time elapsed during the generation operation.</param>
public sealed record GenerationResult(
    [property: JsonPropertyName("generatedFiles")] IReadOnlyList<string> GeneratedFiles,
    [property: JsonPropertyName("skippedFiles")] IReadOnlyList<string> SkippedFiles,
    [property: JsonPropertyName("warnings")] IReadOnlyList<string> Warnings,
    [property: JsonPropertyName("elapsed")] TimeSpan Elapsed
)
{
    /// <summary>
    /// Gets whether the generation completed successfully (no errors).
    /// </summary>
    [property: JsonPropertyName("success")]
    public bool Success { get; init; } = true;

    /// <summary>
    /// Gets the list of <see cref="GenerationError"/> instances that occurred during
    /// generation, if any. When non-null, <see cref="Success"/> will be <c>false</c>.
    /// </summary>
    [property: JsonPropertyName("errors")]
    public IReadOnlyList<GenerationError>? Errors { get; init; }
}
