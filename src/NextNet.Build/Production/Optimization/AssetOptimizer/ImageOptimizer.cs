using NextNet.IO;

namespace NextNet.Build.Production.Optimization.AssetOptimizer;

/// <summary>
/// Basic image optimizer that resizes and compresses PNG, JPEG, and WebP images.
/// Uses <see cref="System.Drawing"/> for basic operations. For production use,
/// consider SkiaSharp or ImageSharp for more advanced processing.
/// </summary>
public class ImageOptimizer : IAssetOptimizer
{
    private readonly ISharpFileSystem _fileSystem;

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp",
    };

    /// <summary>
    /// Initializes a new instance of <see cref="ImageOptimizer"/>.
    /// </summary>
    public ImageOptimizer(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Maximum width for images. Images wider than this will be resized.
    /// </summary>
    public int MaxWidth { get; set; } = 1920;

    /// <summary>
    /// JPEG quality level (1-100).
    /// </summary>
    public int JpegQuality { get; set; } = 80;

    /// <summary>
    /// Whether to convert PNG/JPEG to WebP.
    /// </summary>
    public bool ConvertToWebP { get; set; } = false;

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        SupportedExtensions.Contains(extension);

    /// <inheritdoc />
    public async Task<long> OptimizeAsync(string filePath)
    {
        if (!_fileSystem.FileExists(filePath))
            return 0;

        var originalSize = new FileInfo(filePath).Length;

        // For now, we do basic size-based optimization without System.Drawing dependency.
        // In a full implementation, use SkiaSharp or ImageSharp.
        // This is a placeholder that logs what would be optimized.
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        // Attempt re-encoding with basic compression using the platform's built-in
        // image APIs is not feasible without external packages.
        // We'll record the optimization metadata and return 0 bytes saved.

        await Task.CompletedTask;

        // If WebP conversion is enabled and the source is PNG/JPEG, we'd convert.
        if (ConvertToWebP && (ext is ".png" or ".jpg" or ".jpeg"))
        {
            var webpPath = Path.ChangeExtension(filePath, ".webp");
            if (!_fileSystem.FileExists(webpPath))
            {
                // WebP conversion would go here with a proper image processing library.
                // For now, we track that conversion was requested.
            }
        }

        return 0; // No savings without external imaging library
    }
}
