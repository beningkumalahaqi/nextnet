using Microsoft.Extensions.DependencyInjection;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Official;
using System.CommandLine;

namespace NextNet.Cli.Commands.Template;

/// <summary>
/// Implements the <c>nextnet template list</c> command — lists all available templates.
/// </summary>
public sealed class TemplateListCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateListCommand"/> class.
    /// </summary>
    public TemplateListCommand() : base("list", "List available templates")
    {
        this.SetHandler(HandleAsync);
    }

    private static async Task<int> HandleAsync()
    {
        var services = new ServiceCollection();
        services.AddNextNetOfficialTemplates();

        await using var sp = services.BuildServiceProvider();
        var providers = sp.GetServices<ITemplateProvider>();

        Console.WriteLine("Available templates:");
        foreach (var provider in providers)
        {
            try
            {
                // Derive the friendly name by stripping "-official" suffix
                var name = provider.Name.EndsWith("-official", StringComparison.OrdinalIgnoreCase)
                    ? provider.Name[..^"-official".Length]
                    : provider.Name;

                var manifest = await provider.GetManifestAsync(name);
                Console.WriteLine($"  {name,-15} v{manifest.Version,-10} {manifest.Description ?? "(no description)"}");
            }
            catch
            {
                Console.WriteLine($"  {provider.Name,-15} (error loading)");
            }
        }
        return 0;
    }
}
