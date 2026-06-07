namespace NextNet.Build.Production.Optimization.AssetOptimizer;

/// <summary>
/// Contract for optimizing individual assets during the production build.
/// </summary>
public interface IAssetOptimizer
{
    /// <summary>
    /// Optimizes the asset at the given path.
    /// </summary>
    /// <param name="filePath">Absolute path to the asset file.</param>
    /// <returns>The number of bytes saved, or 0 if no optimization occurred.</returns>
    Task<long> OptimizeAsync(string filePath);

    /// <summary>
    /// Whether this optimizer can handle the given file extension.
    /// </summary>
    bool CanHandle(string extension);
}
