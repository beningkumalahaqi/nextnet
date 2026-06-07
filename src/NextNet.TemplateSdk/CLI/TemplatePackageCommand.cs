using System.CommandLine;

namespace NextNet.TemplateSdk.CLI;

/// <summary>
/// Implements the <c>template package</c> command — packages a template directory
/// into a distributable <c>.nntemplate</c> archive.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template package</c> command uses <see cref="TemplatePackager"/> to create a
/// compressed ZIP archive containing all template files. The output file uses the
/// <c>.nntemplate</c> extension and includes a SHA-256 checksum for integrity.
/// </para>
/// <para>
/// After packaging, the template can be distributed via the NextNet registry or
/// installed directly from the file system.
/// </para>
/// </remarks>
public sealed class TemplatePackageCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplatePackageCommand"/> class.
    /// </summary>
    public TemplatePackageCommand() : base("package", "Package a template into .nntemplate")
    {
        var sourceArg = new Argument<string>("source-dir", "Template source directory");
        var outputOption = new Option<string?>("--output", "Output file path");

        AddArgument(sourceArg);
        AddOption(outputOption);

        this.SetHandler(HandleAsync, sourceArg, outputOption);
    }

    private static async Task<int> HandleAsync(string sourceDir, string? outputPath)
    {
        try
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                var dirName = new DirectoryInfo(sourceDir).Name;
                outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"{dirName}.nntemplate");
            }

            var packager = new TemplatePackager();
            var result = await packager.PackageAsync(sourceDir, outputPath);

            Console.WriteLine($"Package created: {result.PackagePath}");
            Console.WriteLine($"  Size:     {result.SizeBytes:N0} bytes");
            Console.WriteLine($"  SHA-256:  {result.ChecksumSha256}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
