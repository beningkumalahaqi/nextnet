using System.CommandLine;
using System.CommandLine.Invocation;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.Services;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;

namespace NextNet.Cli.Commands.Generate;

/// <summary>
/// Implements the <c>nextnet generate model &lt;name&gt;</c> command — generates
/// a model class for the specified entity.
/// </summary>
public static class GenerateModelCommand
{
    /// <summary>
    /// Creates the <c>model</c> subcommand with entity name argument and options.
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

        var command = new Command("model", "Generate a model class for the specified entity")
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
    /// Executes the <c>generate model</c> command logic.
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

            // Step 3: Parse properties
            var (parsedProps, parseError) = ScaffoldService.ParseProperties(properties);
            if (parseError is not null)
            {
                console.WriteError(parseError);
                return 2;
            }

            // Step 4: Build options
            var options = ScaffoldService.BuildOptions(
                config, output, namespaceOverride, force, dryRun, parsedProps);

            // Step 5: Generate model
            if (verbose)
                console.WriteLine($"Generating model '{name}'...");

            var outputDir = Path.GetFullPath(options.OutputDirectory);
            var artifact = await provider.GenerateModelAsync(name!, options);

            if (artifact.ErrorMessage is not null)
            {
                ErrorMessage.Write(console, ErrorCodes.ScaffoldModelFailed, artifact.ErrorMessage);
                return 4;
            }

            // Step 6: Display result
            if (dryRun)
            {
                console.WriteInfo($"Would create: {artifact.RelativePath}");
            }
            else if (artifact.WasSkipped)
            {
                console.WriteWarning($"Skipped (already exists): {artifact.RelativePath}");
                console.WriteInfo("Use --force to overwrite.");
            }
            else
            {
                console.WriteSuccess($"Generated model: {artifact.RelativePath}");
            }

            if (verbose)
                console.WriteMuted($"  ({artifact.LinesOfCode} lines)");

            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }
}
