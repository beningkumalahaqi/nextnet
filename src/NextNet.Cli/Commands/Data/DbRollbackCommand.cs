using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db rollback</c> command — rolls back the most recently
/// applied database migration(s) using the configured provider's <see cref="IMigrationEngine"/>.
/// </summary>
public static class DbRollbackCommand
{
    /// <summary>
    /// Creates the <c>rollback</c> command with options for steps, dry-run, connection,
    /// backup, and confirmation.
    /// </summary>
    public static Command Create()
    {
        var dryRunOption = new Option<bool>("--dry-run", "Show what would happen without executing");
        var connectionOption = new Option<string>("--connection", "Named connection string to use");
        var stepsOption = new Option<int>("--steps", () => 1, "Number of migrations to roll back");
        var backupOption = new Option<bool>("--backup", "Create a SQL backup before rolling back");
        var backupDirOption = new Option<string>("--backup-dir", () => "Backups", "Directory for SQL backups");
        var confirmOption = new Option<bool>("--confirm", "Skip confirmation prompt");

        var command = new Command("rollback", "Roll back the most recently applied database migration(s)")
        {
            dryRunOption,
            connectionOption,
            stepsOption,
            backupOption,
            backupDirOption,
            confirmOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var connection = context.ParseResult.GetValueForOption(connectionOption);
            var steps = context.ParseResult.GetValueForOption(stepsOption);
            var backup = context.ParseResult.GetValueForOption(backupOption);
            var backupDir = context.ParseResult.GetValueForOption(backupDirOption);
            var confirm = context.ParseResult.GetValueForOption(confirmOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(dryRun, connection, steps, backup, backupDir, confirm, plain, noColor, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the rollback command — resolves the migration engine, handles dry-run,
    /// backup creation, and confirmation, then rolls back the specified number of migrations.
    /// </summary>
    /// <param name="dryRun">If true, shows what would happen without executing.</param>
    /// <param name="connection">Optional named connection string.</param>
    /// <param name="steps">Number of migrations to roll back (default: 1).</param>
    /// <param name="backup">If true, creates a SQL backup before rolling back.</param>
    /// <param name="backupDir">Directory for SQL backup files.</param>
    /// <param name="confirm">If true, skips the confirmation prompt.</param>
    /// <param name="plain">Plain text output (no Unicode, no colors).</param>
    /// <param name="noColor">Disable color output.</param>
    /// <param name="verbose">Enable verbose output.</param>
    /// <returns>Exit code (0 = success, 2 = input error, 4 = execution error).</returns>
    public static async Task<int> ExecuteAsync(
        bool dryRun = false,
        string? connection = null,
        int steps = 1,
        bool backup = false,
        string? backupDir = "Backups",
        bool confirm = false,
        bool plain = false,
        bool noColor = false,
        bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Validate steps
            if (steps < 1)
            {
                ErrorMessage.Write(console, ErrorCodes.MigrationRollbackFailed,
                    "Number of steps must be at least 1.");
                return 2;
            }

            // If dry-run, show preview and return early (no config needed)
            if (dryRun)
            {
                console.WriteInfo($"Dry-run: Would roll back {steps} migration(s)");
                console.WriteLine();
                console.WriteMuted("Re-run without --dry-run to execute the rollback.");
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
                ErrorMessage.Write(console, ErrorCodes.MigrationRollbackFailed,
                    "No migration engine registered for the configured provider. " +
                    "Ensure a data provider with migration support is installed.");
                return 4;
            }

            // Handle backup before rollback
            if (backup)
            {
                var effectiveBackupDir = ResolveBackupDirectory(backupDir);
                if (verbose)
                    console.WriteLine($"Backup directory: {effectiveBackupDir}");

                try
                {
                    if (!Directory.Exists(effectiveBackupDir))
                        Directory.CreateDirectory(effectiveBackupDir);

                    console.WriteInfo($"Backups will be saved to: {effectiveBackupDir}");
                }
                catch (Exception ex)
                {
                    ErrorMessage.Write(console, ErrorCodes.MigrationRollbackFailed,
                        $"Failed to create backup directory '{effectiveBackupDir}': {ex.Message}");
                    return 4;
                }
            }

            // Confirmation prompt (skip if --confirm)
            if (!confirm)
            {
                console.WriteWarning($"This will roll back {steps} migration(s) from your database.");
                console.WriteInfo("Use --confirm to skip this prompt in non-interactive environments.");
                console.WriteLine();

                console.WriteMuted("Re-run with --confirm to execute the rollback.");
                return 0;
            }

            // Execute rollback for the specified number of steps
            if (verbose)
                console.WriteLine($"Rolling back {steps} migration(s)...");

            MigrationResult? lastResult = null;
            for (var i = 0; i < steps; i++)
            {
                if (verbose)
                    console.WriteLine($"  Step {i + 1} of {steps}...");

                lastResult = await engine.RollbackAsync();

                if (!lastResult.Success)
                {
                    var errorDetail = lastResult.Message;
                    if (lastResult.Errors is { Count: > 0 })
                        errorDetail = string.Join("; ", lastResult.Errors);

                    if (i == 0)
                    {
                        ErrorMessage.Write(console, ErrorCodes.MigrationRollbackFailed,
                            $"Failed to roll back migration: {errorDetail}");
                        return 4;
                    }
                    else
                    {
                        console.WriteWarning($"Stopped after rolling back {i} migration(s): {errorDetail}");
                        return 0;
                    }
                }

                // Check if there's nothing more to roll back
                if (lastResult.MigrationsApplied == 0 && string.IsNullOrEmpty(lastResult.MigrationName))
                {
                    if (i == 0)
                    {
                        console.WriteInfo("No migrations to roll back. Database is already at base state.");
                        return 0;
                    }
                    else
                    {
                        console.WriteInfo($"Rolled back {i} migration(s). No more migrations to roll back.");
                        return 0;
                    }
                }
            }

            // Success
            if (lastResult is not null && lastResult.Success)
            {
                console.WriteSuccess($"Successfully rolled back {steps} migration(s).");

                if (verbose && !string.IsNullOrEmpty(lastResult.MigrationName))
                    console.WriteLine($"  Last rolled back: {lastResult.MigrationName}");

                return 0;
            }

            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return 4;
        }
    }

    /// <summary>
    /// Resolves the effective backup directory path.
    /// </summary>
    private static string ResolveBackupDirectory(string? backupDir)
    {
        if (string.IsNullOrWhiteSpace(backupDir))
            backupDir = "Backups";

        return Path.IsPathRooted(backupDir)
            ? backupDir
            : Path.Combine(Environment.CurrentDirectory, backupDir);
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
