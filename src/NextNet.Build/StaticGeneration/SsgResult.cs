namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Represents the result of a static site generation run.
/// Contains information about generated files, errors, duration, and size.
/// </summary>
/// <param name="GeneratedFiles">The list of file paths that were generated, relative to the output directory.</param>
/// <param name="Errors">The list of errors that occurred during generation.</param>
/// <param name="Duration">The total duration of the generation run.</param>
/// <param name="TotalBytes">The total number of bytes written to disk across all generated files.</param>
/// <param name="PageCount">The number of pages successfully generated.</param>
/// <param name="AssetCount">The number of static assets copied.</param>
/// <example>
/// <code>
/// var result = new SsgResult(files, errors, duration, totalBytes, pageCount, assetCount);
/// if (result.Success) { Console.WriteLine($"Generated {result.PageCount} pages"); }
/// </code>
/// </example>
public sealed record SsgResult(
    IReadOnlyList<string> GeneratedFiles,
    IReadOnlyList<SsgError> Errors,
    TimeSpan Duration,
    long TotalBytes,
    int PageCount,
    int AssetCount)
{
    /// <summary>
    /// Gets a value indicating whether the generation completed without errors.
    /// </summary>
    public bool Success => Errors.Count == 0;

    /// <summary>
    /// Returns an empty successful result (no files generated, no errors).
    /// </summary>
    public static SsgResult Empty { get; } = new SsgResult(
        Array.Empty<string>(),
        Array.Empty<SsgError>(),
        TimeSpan.Zero,
        0,
        0,
        0);
}

/// <summary>
/// Describes a single error that occurred during static site generation for a specific route.
/// </summary>
/// <param name="Route">The route pattern on which the error occurred (e.g. <c>"/blog/{slug}"</c>).</param>
/// <param name="Message">A human-readable error message.</param>
/// <param name="Exception">The optional underlying exception that caused the error.</param>
public sealed record SsgError(string Route, string Message, Exception? Exception = null);
