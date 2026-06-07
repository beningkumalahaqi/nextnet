namespace NextNet.Conventions;

/// <summary>
/// Defines the file naming conventions and directory structure conventions
/// used by NextNet for file-based routing and component discovery.
/// </summary>
public static class NextNetConventions
{
    /// <summary>
    /// The configuration file name loaded by <c>NextNetConfigLoader</c>.
    /// </summary>
    public const string ConfigFileName = "nextnet.config.json";

    /// <summary>
    /// The default application source directory name.
    /// </summary>
    public const string AppDirectory = "app";

    /// <summary>
    /// The default build output directory name.
    /// </summary>
    public const string OutputDirectory = "dist";

    /// <summary>
    /// The public/static assets directory name.
    /// </summary>
    public const string PublicDirectory = "public";

    /// <summary>
    /// The file name for page components (e.g. <c>app/index/page.cs</c>).
    /// </summary>
    public const string PageFileName = "page.cs";

    /// <summary>
    /// The file name for layout components (e.g. <c>app/blog/layout.cs</c>).
    /// </summary>
    public const string LayoutFileName = "layout.cs";

    /// <summary>
    /// The file name for API route handlers (e.g. <c>app/api/users/route.cs</c>).
    /// </summary>
    public const string RouteFileName = "route.cs";

    /// <summary>
    /// The file name for error boundary pages (e.g. <c>app/error.cs</c>).
    /// </summary>
    public const string ErrorFileName = "error.cs";

    /// <summary>
    /// The file name for loading/suspense components (e.g. <c>app/loading.cs</c>).
    /// </summary>
    public const string LoadingFileName = "loading.cs";

    /// <summary>
    /// Array of reserved file names that have special meaning in NextNet's routing convention.
    /// </summary>
    public static readonly string[] ReservedFileNames =
    {
        PageFileName,
        LayoutFileName,
        RouteFileName,
        ErrorFileName,
        LoadingFileName,
    };

    /// <summary>
    /// Determines whether the specified file name is a page component file.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><c>true</c> if the file is a page component; otherwise <c>false</c>.</returns>
    public static bool IsPageFile(string fileName)
        => string.Equals(fileName, PageFileName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified file name is a layout component file.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><c>true</c> if the file is a layout component; otherwise <c>false</c>.</returns>
    public static bool IsLayoutFile(string fileName)
        => string.Equals(fileName, LayoutFileName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified file name is an API route handler file.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><c>true</c> if the file is an API route handler; otherwise <c>false</c>.</returns>
    public static bool IsRouteFile(string fileName)
        => string.Equals(fileName, RouteFileName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified file name is an error boundary file.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><c>true</c> if the file is an error page; otherwise <c>false</c>.</returns>
    public static bool IsErrorFile(string fileName)
        => string.Equals(fileName, ErrorFileName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified file name has special meaning in NextNet conventions.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns><c>true</c> if the file is a reserved file; otherwise <c>false</c>.</returns>
    public static bool IsReservedFile(string fileName)
        => ReservedFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
}
