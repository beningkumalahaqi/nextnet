using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db migration add &lt;name&gt;</c> command — creates a new
/// database migration with the specified name using the configured provider's
/// <see cref="IMigrationEngine"/>.
/// </summary>
public static class DbMigrationAddCommand
{
    /// <summary>
    /// Creates the <c>add</c> subcommand with required name argument and options
    /// for provider, output directory, and dry-run preview.
    /// </summary>
    public static Command Create()
    {
        var nameArg = new Argument<string>("name", "Migration name (e.g., \"AddUserTable\", \"RemoveOldColumn\")");
        var providerOption = new Option<string>("--provider", "Target provider name if multiple are registered");
        var outputDirOption = new Option<string>("--output-dir", "Output directory for migration files");
        var dryRunOption = new Option<bool>("--dry-run", "Show what would be created without writing files");

        var command = new Command("add", "Create a new database migration")
        {
            nameArg,
            providerOption,
            outputDirOption,
            dryRunOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var name = context.ParseResult.GetValueForArgument(nameArg);
            var provider = context.ParseResult.GetValueForOption(providerOption);
            var outputDir = context.ParseResult.GetValueForOption(outputDirOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(name, provider, outputDir, dryRun, plain, noColor, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the migration add command — validates the name, resolves the migration engine,
    /// and creates the migration file.
    /// </summary>
    /// <param name="name">The migration name (required, non-empty).</param>
    /// <param name="provider">Optional provider name for multi-provider projects.</param>
    /// <param name="outputDir">Override for the migration output directory.</param>
    /// <param name="dryRun">If true, shows what would be created without writing files.</param>
    /// <param name="plain">Plain text output (no Unicode, no colors).</param>
    /// <param name="noColor">Disable color output.</param>
    /// <param name="verbose">Enable verbose output.</param>
    /// <returns>Exit code (0 = success, 2 = input error, 4 = execution error).</returns>
    public static async Task<int> ExecuteAsync(
        string? name,
        string? provider = null,
        string? outputDir = null,
        bool dryRun = false,
        bool plain = false,
        bool noColor = false,
        bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Validate name is non-empty
            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorMessage.Write(console, ErrorCodes.MigrationAddFailed,
                    "Migration name is required. Provide a name like 'AddUserTable'.");
                return 2;
            }

            // Load config (nullable — dry-run works without it)
            var config = ConfigLoader.Load();

            // Resolve migration directory from options or config
            var migrationDir = ResolveMigrationDirectory(outputDir, config);

            // If dry-run, show preview and return early
            if (dryRun)
            {
                console.WriteInfo($"Dry-run: Would create migration '{name}'");
                console.WriteLine($"  Output directory: {migrationDir}");
                console.WriteLine($"  Provider: {provider ?? config?.Data?.Provider ?? "default"}");
                console.WriteLine();
                console.WriteMuted("Re-run without --dry-run to create the migration.");
                return 0;
            }

            // Require config for actual execution
            if (config is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound,
                    "Not a NextNet project. Run 'nextnet new' or initialize a project first.");
                return 2;
            }

            // Resolve the migration engine
            var engine = ResolveEngine(provider);
            if (engine is null)
            {
                ErrorMessage.Write(console, ErrorCodes.MigrationAddFailed,
                    "No migration engine registered for the configured provider. " +
                    "Ensure a data provider with migration support is installed.");
                return 4;
            }

            // Ensure migration directory exists
            if (!string.IsNullOrEmpty(migrationDir) && !Directory.Exists(migrationDir))
            {
                try
                {
                    Directory.CreateDirectory(migrationDir);
                    if (verbose)
                        console.WriteLine($"Created directory: {migrationDir}");
                }
                catch (Exception ex)
                {
                    ErrorMessage.Write(console, ErrorCodes.MigrationAddFailed,
                        $"Failed to create migration directory '{migrationDir}': {ex.Message}");
                    return 4;
                }
            }

            // Call the engine
            if (verbose)
                console.WriteLine($"Creating migration '{name}'...");

            var result = await engine.AddMigrationAsync(name);

            if (result.Success)
            {
                var migrationName = result.MigrationName ?? name;
                console.WriteSuccess($"Created migration: {migrationName}");
                if (verbose && !string.IsNullOrEmpty(migrationDir))
                    console.WriteLine($"  Location: {migrationDir}");
                return 0;
            }
            else
            {
                var errorDetail = result.Message;
                if (result.Errors is { Count: > 0 })
                    errorDetail = string.Join("; ", result.Errors);

                ErrorMessage.Write(console, ErrorCodes.MigrationAddFailed,
                    $"Failed to create migration '{name}': {errorDetail}");
                return 4;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return 4;
        }
    }

    /// <summary>
    /// Resolves the effective migration output directory from options or config.
    /// </summary>
    private static string? ResolveMigrationDirectory(string? outputDir, NextNetProjectConfig? config)
    {
        if (!string.IsNullOrEmpty(outputDir))
            return outputDir;

        if (config is null)
            return Path.Combine(Environment.CurrentDirectory, "Migrations");

        // Default from config or hard-coded default
        var dir = config.Migration?.Directory ?? "Migrations";
        return Path.IsPathRooted(dir) ? dir : Path.Combine(Environment.CurrentDirectory, dir);
    }

    /// <summary>
    /// Attempts to resolve <see cref="IMigrationEngine"/> for the specified provider.
    /// </summary>
    private static IMigrationEngine? ResolveEngine(string? providerName)
    {
        // TODO: Build a lightweight service provider from the project's startup
        // to resolve IMigrationEngine from DI. For now, this returns null,
        // which causes a helpful error message to be shown.
        //
        // Future implementation will:
        // 1. Load nextnet.config.json
        // 2. Build a temporary service provider from the project's Startup/Program
        // 3. Resolve IMigrationEngine by provider name or default
        return null;
    }
}
