using Microsoft.Extensions.DependencyInjection;
using NextNet.Cli.Errors;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;
using NextNet.Templates.Official;
using System.CommandLine;

namespace NextNet.Cli.Commands.Template;

/// <summary>
/// Implements the <c>nextnet template info</c> command — shows detailed information
/// about a specific template.
/// </summary>
public sealed class TemplateInfoCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateInfoCommand"/> class.
    /// </summary>
    public TemplateInfoCommand() : base("info", "Show template information")
    {
        var nameArg = new Argument<string>("name", "Template name");
        AddArgument(nameArg);
        this.SetHandler(HandleAsync, nameArg);
    }

    private static async Task<int> HandleAsync(string name)
    {
        var services = new ServiceCollection();
        services.AddNextNetOfficialTemplates();

        await using var sp = services.BuildServiceProvider();
        var providers = sp.GetServices<ITemplateProvider>().ToList();

        // Find the provider that can serve this template
        ITemplateProvider? provider = null;
        foreach (var p in providers)
        {
            if (string.Equals(p.Name, $"{name}-official", StringComparison.OrdinalIgnoreCase) ||
                await p.ExistsAsync(name))
            {
                provider = p;
                break;
            }
        }

        if (provider is null)
        {
            Console.Error.WriteLine($"Error [NN-100]: Template '{name}' not found.");
            Console.Error.WriteLine("Run 'nextnet template list' to see available templates.");
            return 1;
        }

        var manifest = await provider.GetManifestAsync(name);
        Console.WriteLine($"Name:         {manifest.Name}");
        Console.WriteLine($"Version:      {manifest.Version}");
        Console.WriteLine($"Author:       {manifest.Author ?? "(none)"}");
        Console.WriteLine($"Description:  {manifest.Description ?? "(none)"}");
        Console.WriteLine($"NextNet:     {manifest.NextNetVersion}");
        if (manifest.Tags is { Count: > 0 })
        {
            Console.WriteLine($"Tags:         {string.Join(", ", manifest.Tags)}");
        }
        Console.WriteLine();
        Console.WriteLine("Variables:");
        foreach (var v in manifest.Variables ?? Enumerable.Empty<TemplateVariable>())
        {
            var required = v.Required ? " (required)" : "";
            var defaultStr = v.Default is not null ? $" (default: {v.Default})" : "";
            Console.WriteLine($"  {v.Name,-15} [{v.Type}]{required}{defaultStr}");
            if (v.Description is not null)
                Console.WriteLine($"    {v.Description}");
        }
        Console.WriteLine();
        Console.WriteLine($"Files: {manifest.Files?.Count ?? 0}");
        return 0;
    }
}
