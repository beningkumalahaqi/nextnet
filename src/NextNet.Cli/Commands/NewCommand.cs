using Microsoft.Extensions.DependencyInjection;
using NextNet.Cli.Errors;
using NextNet.Cli.Interactive;
using NextNet.Core.Extensions;
using NextNet.TemplateEngine;
using NextNet.TemplateEngine.Variables;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;
using NextNet.Templates.Official;
using System.CommandLine;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet new</c> command — scaffolds a new NextNet project
/// from a V3 template using the TemplateEngine.
/// </summary>
/// <remarks>
/// <para>
/// Usage: <c>nextnet new &lt;template&gt; &lt;name&gt;</c>
/// </para>
/// <example>
/// nextnet new blog my-blog
/// nextnet new api my-api --output ./projects
/// nextnet new --interactive
/// nextnet new --no-restore
/// </example>
/// </remarks>
public sealed class NewCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewCommand"/> class.
    /// </summary>
    public NewCommand() : base("new", "Create a new NextNet project from a template")
    {
        var templateArg = new Argument<string?>("template", () => null, "Template name (blog, api)");
        var nameArg = new Argument<string?>("name", () => null, "Project name");
        var outputOpt = new Option<string?>("--output", "Output directory");
        var noRestoreOpt = new Option<bool>("--no-restore", "Skip dotnet restore");
        var interactiveOpt = new Option<bool>("--interactive", "Launch interactive project generator");

        AddArgument(templateArg);
        AddArgument(nameArg);
        AddOption(outputOpt);
        AddOption(noRestoreOpt);
        AddOption(interactiveOpt);

        this.SetHandler(HandleAsync, templateArg, nameArg, outputOpt, noRestoreOpt, interactiveOpt);
    }

    /// <summary>
    /// Handles the <c>nextnet new</c> command execution.
    /// </summary>
    private static async Task<int> HandleAsync(string? template, string? name, string? output, bool noRestore, bool interactive)
    {
        try
        {
            // ── DI setup ──────────────────────────────────────────────
            var services = new ServiceCollection();
            services.AddNextNetTemplateEngine();
            services.AddNextNetOfficialTemplates();

            await using var sp = services.BuildServiceProvider();
            var providers = sp.GetServices<ITemplateProvider>().ToList();
            var engine = sp.GetRequiredService<ITemplateEngine>();

            // ── Interactive mode ──────────────────────────────────────
            if (interactive || (template is null && name is null))
            {
                var gen = new InteractiveProjectGenerator(providers);
                var interactiveResult = await gen.GenerateAsync(new InteractiveOptions
                {
                    ProjectName = name,
                    TemplateName = template
                });

                template = interactiveResult.TemplateName;
                name = interactiveResult.VariableContext.Get<string>("projectName");
                output ??= $"./{name}";

                // Already have the manifest from interactive result
                var manifest = interactiveResult.Manifest;
                var files = await FindProvider(providers, template)!.GetFilesAsync(manifest);
                var package = new TemplatePackage(manifest, files);

                return await GenerateProject(engine, package, interactiveResult.VariableContext, name!, template, output, noRestore);
            }

            // ── Argument-based mode ───────────────────────────────────
            if (name is null)
            {
                Console.Write("Project name: ");
                name = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.Error.WriteLine("Error: Project name is required.");
                    return 1;
                }
            }

            template ??= "blog";
            output ??= $"./{name}";

            // ── Find matching provider ────────────────────────────────
            ITemplateProvider? provider = FindProvider(providers, template);

            if (provider is null)
            {
                Console.Error.WriteLine($"Error [NN-100]: Template '{template}' not found.");
                Console.Error.WriteLine("Run 'nextnet template list' to see available templates.");
                return 1;
            }

            // ── Load manifest and files ───────────────────────────────
            var manifestArg = await provider.GetManifestAsync(template);
            var filesArg = await provider.GetFilesAsync(manifestArg);
            var packageArg = new TemplatePackage(manifestArg, filesArg);

            // ── Build variable context ────────────────────────────────
            var variables = await BuildVariablesAsync(manifestArg, name!);

            return await GenerateProject(engine, packageArg, variables, name!, template, output, noRestore);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error [NN-103]: Unexpected error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Generates the project files using the template engine and optionally runs restore.
    /// </summary>
    private static async Task<int> GenerateProject(
        ITemplateEngine engine,
        TemplatePackage package,
        IVariableContext variables,
        string projectName,
        string templateName,
        string output,
        bool noRestore)
    {
        Console.WriteLine($"Generating {projectName} from {templateName} template...");
        var result = await engine.GenerateAsync(package, variables, output);

        if (!result.Success)
        {
            var errorCount = result.Errors?.Count ?? 0;
            Console.Error.WriteLine($"Error [NN-102]: Project generation failed with {errorCount} error(s).");
            if (result.Errors is not null)
            {
                foreach (var error in result.Errors)
                {
                    Console.Error.WriteLine($"  - {error.File}: {error.Message}");
                }
            }
            return 1;
        }

        Console.WriteLine($"\u2713 Generated {result.GeneratedFiles.Count} files in {output}");

        // ── Restore ───────────────────────────────────────────────
        if (!noRestore)
        {
            Console.WriteLine("Restoring dependencies...");
            await RunDotNetRestore(output);
        }

        return 0;
    }

    /// <summary>
    /// Finds the <see cref="ITemplateProvider"/> that can serve the specified template name.
    /// </summary>
    private static ITemplateProvider? FindProvider(IReadOnlyList<ITemplateProvider> providers, string template)
    {
        foreach (var p in providers)
        {
            if (string.Equals(p.Name, $"{template}Official", StringComparison.OrdinalIgnoreCase) ||
                p.ExistsAsync(template).GetAwaiter().GetResult())
            {
                return p;
            }
        }
        return null;
    }

    /// <summary>
    /// Prompts the user for variable values defined in the template manifest.
    /// </summary>
    private static Task<IVariableContext> BuildVariablesAsync(TemplateManifest manifest, string projectName)
    {
        var builder = VariableContext.CreateBuilder();
        builder.Set("projectName", projectName);
        builder.Set("namespaceName", StringCaseHelper.ToPascalCase(projectName));

        foreach (var variable in manifest.Variables ?? Enumerable.Empty<TemplateVariable>())
        {
            if (string.Equals(variable.Name, "projectName", StringComparison.OrdinalIgnoreCase))
                continue;

            object? value = variable.Type switch
            {
                "bool" => PromptBool(variable),
                "enum" => PromptEnum(variable),
                _ => PromptString(variable)
            };

            if (value is not null)
            {
                builder.Set(variable.Name, value);
            }
        }

        return Task.FromResult<IVariableContext>(builder.Build());
    }

    /// <summary>
    /// Prompts for a string variable value.
    /// </summary>
    private static string? PromptString(TemplateVariable v)
    {
        var defaultStr = v.Default?.ToString() ?? "";
        Console.Write($"{v.Description ?? v.Name} [{defaultStr}]: ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? null : input;
    }

    /// <summary>
    /// Prompts for a boolean variable value.
    /// </summary>
    private static bool PromptBool(TemplateVariable v)
    {
        var defaultVal = v.Default?.GetBoolean() == true;
        Console.Write($"{v.Description ?? v.Name} (y/n) [{(defaultVal ? "y" : "n")}]: ");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();
        return input is "y" or "yes";
    }

    /// <summary>
    /// Prompts for an enum variable value.
    /// </summary>
    private static string? PromptEnum(TemplateVariable v)
    {
        Console.WriteLine($"{v.Description ?? v.Name}:");
        if (v.AllowedValues is not null)
        {
            for (int i = 0; i < v.AllowedValues.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {v.AllowedValues[i]}");
            }
        }
        Console.Write("Select [1]: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
            return v.AllowedValues?.FirstOrDefault();
        if (int.TryParse(input, out var idx) && v.AllowedValues is not null && idx >= 1 && idx <= v.AllowedValues.Count)
            return v.AllowedValues[idx - 1];
        return input;
    }

    /// <summary>
    /// Runs <c>dotnet restore</c> in the specified output directory.
    /// </summary>
    private static async Task RunDotNetRestore(string outputDir)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("dotnet", "restore")
            {
                WorkingDirectory = outputDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc is not null)
            {
                await proc.WaitForExitAsync();
            }
        }
        catch
        {
            Console.WriteLine("Warning: dotnet restore failed. Run 'dotnet restore' manually.");
        }
    }
}
