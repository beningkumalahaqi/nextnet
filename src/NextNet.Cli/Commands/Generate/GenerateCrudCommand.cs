using System.CommandLine;
using System.CommandLine.Invocation;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.Services;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;

namespace NextNet.Cli.Commands.Generate;

/// <summary>
/// Implements the <c>nextnet generate crud &lt;name&gt;</c> command — generates
/// a full set of CRUD server actions (model, repository, and API route) for
/// the specified entity.
/// </summary>
public static class GenerateCrudCommand
{
    /// <summary>
    /// Creates the <c>crud</c> subcommand with entity name argument and options.
    /// </summary>
    public static Command Create()
    {
        var nameArg = new Argument<string>("name", "Entity name (PascalCase, e.g., \"User\", \"Product\")");
        var outputOption = new Option<string>("--output", () => ".", "Output directory");
        outputOption.AddAlias("-o");
        var propertyOption = new Option<string[]>("--property", "Property in format \"Name:Type\" (can be repeated)");
        propertyOption.AddAlias("-p");
        var namespaceOption = new Option<string>("--namespace", "Override the target namespace");
        var forceOption = new Option<bool>("--force", "Overwrite existing files without confirmation");
        var dryRunOption = new Option<bool>("--dry-run", "Show what would be generated without writing");

        var command = new Command("crud", "Generate model, repository, and CRUD API route for the specified entity")
        {
            nameArg,
            outputOption,
            propertyOption,
            namespaceOption,
            forceOption,
            dryRunOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var name = context.ParseResult.GetValueForArgument(nameArg);
            var output = context.ParseResult.GetValueForOption(outputOption) ?? ".";
            var properties = context.ParseResult.GetValueForOption(propertyOption);
            var ns = context.ParseResult.GetValueForOption(namespaceOption);
            var force = context.ParseResult.GetValueForOption(forceOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);

            var exitCode = ExecuteAsync(name, output, properties, ns, force, dryRun, verbose, plain, noColor).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the <c>generate crud</c> command logic.
    /// </summary>
    public static async Task<int> ExecuteAsync(
        string? name,
        string output = ".",
        string[]? properties = null,
        string? namespaceOverride = null,
        bool force = false,
        bool dryRun = false,
        bool verbose = false,
        bool plain = false,
        bool noColor = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Step 1: Validate entity name
            if (!ScaffoldService.ValidateEntityName(name))
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    ErrorMessage.Write(console, ErrorCodes.InvalidProjectName,
                        "Entity name is required. Provide a PascalCase name (e.g., \"User\").");
                }
                else
                {
                    ErrorMessage.Write(console, ErrorCodes.InvalidProjectName,
                        $"Invalid entity name '{name}'. Names must start with a letter and contain only alphanumeric characters.");
                }
                return 2;
            }

            // Step 2: Load config and resolve provider
            var (config, provider, errorCode) = ScaffoldService.LoadConfigAndProvider(console);
            if (config is null || provider is null)
                return errorCode ?? 2;

            // Step 3: Check if provider is model-only (no provider configured)
            if (provider.ProviderName == "ModelOnly")
            {
                ErrorMessage.Write(console, ErrorCodes.ScaffoldCrudFailed,
                    "No data provider configured. Run 'nextnet add data <provider>' first to enable CRUD generation.");
                return 2;
            }

            // Step 4: Parse properties
            var (parsedProps, parseError) = ScaffoldService.ParseProperties(properties);
            if (parseError is not null)
            {
                console.WriteError(parseError);
                return 2;
            }

            // Step 5: Build options
            var options = ScaffoldService.BuildOptions(
                config, output, namespaceOverride, force, dryRun, parsedProps);

            // Step 6: Generate CRUD
            if (verbose)
                console.WriteLine($"Generating CRUD for '{name}'...");

            var artifacts = await provider.GenerateCrudAsync(name!, options);

            if (artifacts.Length == 0)
            {
                ErrorMessage.Write(console, ErrorCodes.ScaffoldCrudFailed, "No files were generated.");
                return 4;
            }

            // Check for errors
            foreach (var artifact in artifacts)
            {
                if (artifact.ErrorMessage is not null)
                {
                    ErrorMessage.Write(console, ErrorCodes.ScaffoldCrudFailed, artifact.ErrorMessage);
                    return 4;
                }
            }

            // Step 7: Display results
            if (dryRun)
            {
                console.WriteInfo($"Would create {artifacts.Length} files:");
                foreach (var artifact in artifacts)
                {
                    console.WriteMuted($"  {artifact.RelativePath}");
                }
            }
            else
            {
                var created = artifacts.Where(a => !a.WasSkipped).ToList();
                var skipped = artifacts.Where(a => a.WasSkipped).ToList();

                if (created.Count > 0)
                {
                    console.WriteSuccess($"Generated {created.Count} file(s):");
                    foreach (var artifact in created)
                    {
                        console.WriteMuted($"  {artifact.RelativePath}  ({artifact.LinesOfCode} lines)");
                    }
                }

                if (skipped.Count > 0)
                {
                    console.WriteWarning($"Skipped {skipped.Count} existing file(s) (use --force to overwrite):");
                    foreach (var artifact in skipped)
                    {
                        console.WriteMuted($"  {artifact.RelativePath}");
                    }
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }
}
