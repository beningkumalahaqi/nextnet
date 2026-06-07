namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Represents the result of a static site generation run.
/// Contains information about generated files, errors, duration, and size.
/// </summary>
public class SsgResult
{
    /// <summary>
    /// Gets a value indicating whether the generation completed without errors.
    /// </summary>
    public bool Success => Errors.Count == 0;

    /// <summary>
    /// Gets the list of file paths that were generated, relative to the output directory.
    /// </summary>
    public IReadOnlyList<string> GeneratedFiles { get; }

    /// <summary>
    /// Gets the list of errors that occurred during generation.
    /// </summary>
    public IReadOnlyList<SsgError> Errors { get; }

    /// <summary>
    /// Gets the total duration of the generation run.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the total number of bytes written to disk across all generated files.
    /// </summary>
    public long TotalBytes { get; }

    /// <summary>
    /// Gets the number of pages successfully generated.
    /// </summary>
    public int PageCount { get; }

    /// <summary>
    /// Gets the number of static assets copied.
    /// </summary>
    public int AssetCount { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SsgResult"/>.
    /// </summary>
    public SsgResult(
        IReadOnlyList<string> generatedFiles,
        IReadOnlyList<SsgError> errors,
        TimeSpan duration,
        long totalBytes,
        int pageCount,
        int assetCount)
    {
        GeneratedFiles = generatedFiles ?? Array.Empty<string>();
        Errors = errors ?? Array.Empty<SsgError>();
        Duration = duration;
        TotalBytes = totalBytes;
        PageCount = pageCount;
        AssetCount = assetCount;
    }

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
public class SsgError
{
    /// <summary>
    /// Gets the route pattern on which the error occurred (e.g. <c>"/blog/{slug}"</c>).
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Gets a human-readable error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the optional underlying exception that caused the error.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SsgError"/>.
    /// </summary>
    public SsgError(string route, string message, Exception? exception = null)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
    }
}
