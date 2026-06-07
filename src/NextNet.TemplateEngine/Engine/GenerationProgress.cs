namespace NextNet.TemplateEngine;

/// <summary>
/// Reports the current state of a template generation operation, enabling progress
/// tracking and user feedback.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GenerationProgress"/> is passed to the <c>IProgress&lt;GenerationProgress&gt;</c>
/// callback during template generation. Consumers can use it to display progress bars,
/// status messages, or log the generation process.
/// </para>
/// <para>
/// The <see cref="Percentage"/> property provides a 0–100 scale for overall progress,
/// while <see cref="Stage"/> and <see cref="CurrentFile"/> give context about what is
/// currently being processed.
/// </para>
/// <example>
/// <code>
/// var progress = new Progress&lt;GenerationProgress&gt;(p =>
/// {
///     Console.WriteLine($"[{p.Stage}] {p.CurrentFile} ({p.Percentage:F0}%)");
/// });
/// var result = await engine.GenerateAsync(package, variables, options, progress);
/// </code>
/// </example>
/// </remarks>
public sealed record GenerationProgress
{
    /// <summary>
    /// Gets the name of the current generation stage
    /// (e.g., "Validating", "Resolving features", "Filtering files", "Generating").
    /// </summary>
    public string Stage { get; init; } = "";

    /// <summary>
    /// Gets the source path of the file currently being processed, if any.
    /// </summary>
    public string CurrentFile { get; init; } = "";

    /// <summary>
    /// Gets the number of files processed so far in the current stage.
    /// </summary>
    public int FilesProcessed { get; init; }

    /// <summary>
    /// Gets the total number of files to process in the current stage.
    /// </summary>
    public int TotalFiles { get; init; }

    /// <summary>
    /// Gets the overall completion percentage (0.0 to 100.0).
    /// </summary>
    public double Percentage { get; init; }
}
