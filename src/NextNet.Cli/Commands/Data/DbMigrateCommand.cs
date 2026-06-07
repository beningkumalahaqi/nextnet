using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Abstractions;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db migrate</c> command — applies all pending database
/// migrations using the configured provider's <see cref="IMigrationEngine"/>.
/// </summary>
public static class DbMigrateCommand
{
    /// <summary>
    /// Creates the <c>migrate</c> command with options for dry-run, connection, and confirmation.
    /// </summary>
    public static Command Create()
    {
        var dryRunOption = new Option<bool>("--dry-run", "Show pending migrations without applying");
        var connectionOption = new Option<string>("--connection", "Named connection string to use");
        var confirmOption = new Option<bool>("--confirm", "Skip confirmation prompt");

        var command = new Command("migrate", "Apply all pending database migrations")
        {
            dryRunOption,
            connectionOption,
            confirmOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var connection = context.ParseResult.GetValueForOption(connectionOption);
            var confirm = context.ParseResult.GetValueForOption(confirmOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(dryRun, connection, confirm, plain, noColor, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the migration apply command — resolves the migration engine, handles
    /// dry-run preview and confirmation prompts, then applies pending migrations.
    /// </summary>
    /// <param name="dryRun">If true, shows pending migrations without applying.</param>
    /// <param name="connection">Optional named connection string.</param>
    /// <param name="confirm">If true, skips the confirmation prompt.</param>
    /// <param name="plain">Plain text output (no Unicode, no colors).</param>
    /// <param name="noColor">Disable color output.</param>
    /// <param name="verbose">Enable verbose output.</param>
    /// <returns>Exit code (0 = success, 2 = input error, 4 = execution error).</returns>
    public static async Task<int> ExecuteAsync(
        bool dryRun = false,
        string? connection = null,
        bool confirm = false,
        bool plain = false,
        bool noColor = false,
        bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // If dry-run, show preview and return early (no config needed)
            if (dryRun)
            {
                console.WriteInfo("Dry-run: Would check for and apply pending migrations");
                console.WriteLine();
                console.WriteMuted("Re-run without --dry-run to apply pending migrations.");
                return 0;
            }

            // Load config to verify this is a NextNet project
            var config = ConfigLoader.Load();
            if (config is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound,
                    "Not a NextNet project. Run 'nextnet new' or initialize a project first.");
                return 2;
            }

            // If connection specified, show which connection
            if (!string.IsNullOrWhiteSpace(connection) && verbose)
                console.WriteLine($"Using connection: {connection}");

            // Resolve the migration engine
            var engine = ResolveEngine(connection);
            if (engine is null)
            {
                ErrorMessage.Write(console, ErrorCodes.MigrationApplyFailed,
                    "No migration engine registered for the configured provider. " +
                    "Ensure a data provider with migration support is installed.");
                return 4;
            }

            // Confirmation prompt (skip if --confirm or --dry-run)
            if (!confirm)
            {
                console.WriteWarning("This will apply pending migrations to your database.");
                console.WriteInfo("Use --confirm to skip this prompt in non-interactive environments.");
                console.WriteLine();

                // In interactive mode, prompt for confirmation
                // For now, we require --confirm explicitly since we can't prompt in all environments
                console.WriteMuted("Re-run with --confirm to apply migrations.");
                return 0;
            }

            // Apply migrations
            if (verbose)
                console.WriteLine("Applying pending migrations...");

            var result = await engine.ApplyAsync();

            if (result.Success)
            {
                if (result.MigrationsApplied > 0)
                {
                    console.WriteSuccess($"Applied {result.MigrationsApplied} migration(s) successfully.");
                }
                else
                {
                    console.WriteInfo("Database is already up to date. No migrations to apply.");
                }

                if (verbose && !string.IsNullOrEmpty(result.Message))
                    console.WriteLine($"  {result.Message}");

                return 0;
            }
            else
            {
                var errorDetail = result.Message;
                if (result.Errors is { Count: > 0 })
                    errorDetail = string.Join("; ", result.Errors);

                ErrorMessage.Write(console, ErrorCodes.MigrationApplyFailed,
                    $"Failed to apply migrations: {errorDetail}");
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
    /// Attempts to resolve <see cref="IMigrationEngine"/> for the specified connection.
    /// </summary>
    private static IMigrationEngine? ResolveEngine(string? connectionName)
    {
        // TODO: Build a lightweight service provider from the project's startup
        // to resolve IMigrationEngine from DI.
        return null;
    }
}
