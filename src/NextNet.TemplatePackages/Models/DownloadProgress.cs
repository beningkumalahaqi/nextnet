namespace NextNet.TemplatePackages;

/// <summary>
/// Reports the progress of a template package download operation.
/// Used with <see cref="IProgress{T}"/> to provide incremental download feedback.
/// </summary>
public sealed record DownloadProgress
{
    /// <summary>Number of bytes downloaded so far.</summary>
    public long BytesDownloaded { get; init; }

    /// <summary>Total number of bytes to download, if known.</summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Progress percentage calculated from <see cref="BytesDownloaded"/> and <see cref="TotalBytes"/>.
    /// Rounded to one decimal place. Returns 0 if <see cref="TotalBytes"/> is not known.
    /// </summary>
    public double Percentage => TotalBytes > 0
        ? Math.Round((double)BytesDownloaded / TotalBytes * 100, 1)
        : 0;
}
