namespace NextNet.TemplateEngine.Variables;

/// <summary>
/// Detects whether file content is binary using heuristic analysis.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BinaryDetector"/> provides two complementary methods for determining
/// whether content is binary:
/// <list type="bullet">
///   <item><see cref="IsBinary(byte[])"/> — analyzes the byte content for null bytes
///   and non-printable characters.</item>
///   <item><see cref="IsKnownBinaryExtension(string?)"/> — checks the file extension
///   against a known list of binary file formats.</item>
/// </list>
/// </para>
/// <para>
/// The content analysis samples the first 8 KB of data and classifies content as
/// binary if more than 30% of bytes are non-printable (excluding common whitespace).
/// A null byte anywhere in the sample immediately triggers binary detection.
/// </para>
/// <example>
/// <code>
/// bool isBinary = BinaryDetector.IsBinary(File.ReadAllBytes("image.png"));
/// bool isKnown = BinaryDetector.IsKnownBinaryExtension(".png");
/// </code>
/// </example>
/// </remarks>
public static class BinaryDetector
{
    /// <summary>
    /// The default number of bytes to sample from the beginning of the content.
    /// </summary>
    public const int DefaultSampleSize = 8192;

    /// <summary>
    /// The default threshold ratio of non-printable bytes above which content is
    /// classified as binary.
    /// </summary>
    public const double DefaultThreshold = 0.30;

    private static readonly HashSet<string> _knownBinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".svgz",
        ".pdf", ".zip", ".tar", ".gz", ".bz2", ".7z", ".rar",
        ".dll", ".exe", ".so", ".dylib", ".bin",
        ".woff", ".woff2", ".eot", ".ttf", ".otf",
        ".mp3", ".mp4", ".avi", ".mov", ".wav", ".ogg", ".flac",
        ".class", ".jar", ".war"
    };

    /// <summary>
    /// Gets the set of known binary file extensions.
    /// </summary>
    public static IReadOnlySet<string> KnownBinaryExtensions => _knownBinaryExtensions;

    /// <summary>
    /// Determines whether the specified content is binary by analyzing byte patterns.
    /// </summary>
    /// <param name="content">The content to analyze. Must not be null.</param>
    /// <returns><c>true</c> if the content is likely binary; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    /// <example>
    /// <code>
    /// byte[] data = File.ReadAllBytes("document.pdf");
    /// if (BinaryDetector.IsBinary(data))
    /// {
    ///     // Skip variable replacement for binary files
    /// }
    /// </code>
    /// </example>
    public static bool IsBinary(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        // Empty content is not binary
        if (content.Length == 0)
            return false;

        var sampleSize = Math.Min(content.Length, DefaultSampleSize);
        int nonPrintable = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            byte b = content[i];

            // Null byte strongly indicates binary
            if (b == 0x00)
                return true;

            // Count non-printable characters
            // Printable: 0x20-0x7E, plus common whitespace: \t (0x09), \n (0x0A), \r (0x0D)
            if (b != 0x09 && b != 0x0A && b != 0x0D && (b < 0x20 || b > 0x7E))
            {
                nonPrintable++;
            }
        }

        var ratio = (double)nonPrintable / sampleSize;
        return ratio > DefaultThreshold;
    }

    /// <summary>
    /// Determines whether the specified file extension corresponds to a known binary type.
    /// </summary>
    /// <param name="extension">The file extension (e.g., ".png"). Leading dot is optional.</param>
    /// <returns><c>true</c> if the extension is a known binary type; otherwise <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (BinaryDetector.IsKnownBinaryExtension(".cs"))
    ///     // false - C# source files are text
    ///
    /// if (BinaryDetector.IsKnownBinaryExtension("png"))
    ///     // true - PNG images are binary
    /// </code>
    /// </example>
    public static bool IsKnownBinaryExtension(string? extension)
    {
        if (string.IsNullOrEmpty(extension))
            return false;

        // Normalize: ensure leading dot, lowercase
        var ext = extension.StartsWith('.') ? extension : "." + extension;
        return _knownBinaryExtensions.Contains(ext);
    }
}
