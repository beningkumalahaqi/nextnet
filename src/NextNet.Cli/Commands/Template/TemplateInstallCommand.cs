using NextNet.Cli.Community;
using NextNet.IO;
using System.CommandLine;
using RegistryOptions = NextNet.TemplateRegistry.RegistryOptions;

namespace NextNet.Cli.Commands.Template;

/// <summary>
/// Implements the <c>nextnet template install</c> command — installs a community
/// template from the NextNet template registry.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template install</c> command downloads and installs a community template
/// by name (e.g., <c>nextnet template install author/my-template</c>). It uses
/// <see cref="CommunityTemplateManager"/> to resolve the latest version, download
/// the manifest, and register the template locally.
/// </para>
/// <para>
/// Use <c>--version</c> to pin a specific version, or <c>--force</c> to reinstall
/// an already-installed template.
/// </para>
/// </remarks>
public sealed class TemplateInstallCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateInstallCommand"/> class.
    /// </summary>
    public TemplateInstallCommand() : base("install", "Install a community template")
    {
        var nameArg = new Argument<string>("name", "Template name (e.g., author/template)");
        var versionOption = new Option<string?>("--version", "Specific version to install");
        var forceOption = new Option<bool>("--force", "Force reinstall if already installed");

        AddArgument(nameArg);
        AddOption(versionOption);
        AddOption(forceOption);

        this.SetHandler(HandleAsync, nameArg, versionOption, forceOption);
    }

    private static async Task<int> HandleAsync(string name, string? version, bool force)
    {
        try
        {
            var manager = CreateManager();

            var options = new InstallOptions
            {
                Version = version,
                Force = force
            };

            var result = await manager.InstallAsync(name, options);

            if (result.Success)
            {
                Console.WriteLine($"Installed template '{result.Name}' v{result.Version}");
                Console.WriteLine($"  Location: {result.InstallPath}");
                return 0;
            }

            Console.Error.WriteLine($"Installation failed: {result.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Creates a configured <see cref="CommunityTemplateManager"/> instance.
    /// </summary>
    internal static CommunityTemplateManager CreateManager()
    {
        var options = new RegistryOptions();
        var httpClient = new HttpClient();
        var client = new NextNet.TemplateRegistry.HttpTemplateRegistryClient(httpClient, options);
        var cache = new NextNet.TemplateRegistry.TemplateRegistryCache(options);
        var registry = new NextNet.TemplateRegistry.TemplateRegistry(client, cache);
        var installed = new InstalledTemplateRegistry();
        var fileSystem = new DefaultSharpFileSystem();

        return new CommunityTemplateManager(registry, installed, fileSystem);
    }
}
