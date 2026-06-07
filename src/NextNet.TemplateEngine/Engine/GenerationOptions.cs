namespace NextNet.TemplateEngine;

/// <summary>
/// Configuration options that control the behavior of the template generation process.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GenerationOptions"/> allows callers to customize how <see cref="TemplateEngine"/>
/// processes template packages. Properties control output location, file overwrite behavior,
/// and dry-run mode for preview.
/// </para>
/// <example>
/// <code>
/// var options = new GenerationOptions
/// {
///     OutputDirectory = "./output",
///     Overwrite = false,
///     DryRun = true
/// };
/// var engine = new TemplateEngine(fileSystem);
/// var result = await engine.GenerateAsync(package, variables, options);
/// </code>
/// </example>
/// </remarks>
public sealed record GenerationOptions
{
    /// <summary>
    /// Gets the root directory where generated files will be written.
    /// Defaults to an empty string (current directory).
    /// </summary>
    public string OutputDirectory { get; init; } = "";

    /// <summary>
    /// Gets whether existing files in the output directory should be overwritten.
    /// When <c>false</c>, existing files are skipped and reported as warnings.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool Overwrite { get; init; } = true;

    /// <summary>
    /// Gets whether to perform a dry run without actually writing any files.
    /// When <c>true</c>, the engine validates and processes all files but does
    /// not write anything to disk. Useful for previewing what would be generated.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool DryRun { get; init; } = false;
}
