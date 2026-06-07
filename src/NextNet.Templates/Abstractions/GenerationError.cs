namespace NextNet.Templates.Abstractions;

/// <summary>
/// Represents an error that occurred during template file generation, providing
/// context about which file and stage failed.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GenerationError"/> is used in <c>GenerationResult.Errors</c> to report
/// individual file-level failures without aborting the entire generation process.
/// Non-critical errors (e.g., a single file failing due to template syntax) are captured
/// here so the caller can inspect and handle them after generation completes.
/// </para>
/// <example>
/// <code>
/// var result = await engine.GenerateAsync(package, variables, options);
/// if (result.Errors?.Count &gt; 0)
/// {
///     foreach (var error in result.Errors)
///     {
///         Console.WriteLine($"Error in {error.File} at {error.Stage}: {error.Message}");
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public sealed record GenerationError
{
    /// <summary>
    /// Gets the relative path of the file that caused the error.
    /// </summary>
    public string File { get; init; } = "";

    /// <summary>
    /// Gets the name of the stage during which the error occurred
    /// (e.g., "Generate", "Validate", "Replace").
    /// </summary>
    public string Stage { get; init; } = "";

    /// <summary>
    /// Gets a human-readable description of the error.
    /// </summary>
    public string Message { get; init; } = "";
}
