using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Abstractions;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db migration status</c> command — displays the status of
/// all database migrations (applied, pending) for the configured provider.
/// </summary>
public static class DbMigrationStatusCommand
{
    /// <summary>
    /// Creates the <c>status</c> subcommand with options for connection, verbose, and JSON output.
    /// </summary>
    public static Command Create()
    {
        var connectionOption = new Option<string>("--connection", "Named connection string to use");
        var verboseOption = new Option<bool>("--verbose", "Show full migration file paths");
        var jsonOption = new Option<bool>("--json", "Output in JSON format for tooling");

        var command = new Command("status", "Show the status of all migrations (pending, applied)")
        {
            connectionOption,
            verboseOption,
            jsonOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var connection = context.ParseResult.GetValueForOption(connectionOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            var json = context.ParseResult.GetValueForOption(jsonOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);

            var exitCode = ExecuteAsync(connection, verbose, json, plain, noColor).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the migration status command — resolves the migration engine and displays
    /// the current migration status in table or JSON format.
    /// </summary>
    /// <param name="connection">Optional named connection string.</param>
    /// <param name="verbose">Show full migration file paths.</param>
    /// <param name="json">Output in JSON format.</param>
    /// <param name="plain">Plain text output (no Unicode, no colors).</param>
    /// <param name="noColor">Disable color output.</param>
    /// <returns>Exit code (0 = success, 2 = input error, 4 = execution error).</returns>
    public static Task<int> ExecuteAsync(
        string? connection = null,
        bool verbose = false,
        bool json = false,
        bool plain = false,
        bool noColor = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Load config to verify this is a NextNet project
            var config = ConfigLoader.Load();
            if (config is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound,
                    "Not a NextNet project. Run 'nextnet new' or initialize a project first.");
                return Task.FromResult(2);
            }

            // Resolve the migration engine
            var engine = ResolveEngine(connection);
            if (engine is null)
            {
                ErrorMessage.Write(console, ErrorCodes.MigrationStatusFailed,
                    "No migration engine registered for the configured provider. " +
                    "Ensure a data provider with migration support is installed.");
                return Task.FromResult(4);
            }

            // Attempt to get migration status via the engine
            // Since IMigrationEngine doesn't have a status/discovery method,
            // we provide status based on what we can infer
            if (json)
            {
                var statusInfo = new
                {
                    Provider = config.Data?.Provider ?? "unknown",
                    DatabaseType = config.Data?.DatabaseType ?? "unknown",
                    Status = "No migration engine available for status discovery",
                    Migrations = new object[] { }
                };

                var jsonOutput = JsonSerializer.Serialize(statusInfo, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                console.WriteLine(jsonOutput);
            }
            else
            {
                // Table output
                console.WriteHeading("Migration Status");

                var table = console.CreateTable("Status", "Migration", "Details");

                table.AddRow("—", "Migration discovery", "Not yet implemented");
                table.AddSeparator();
                table.AddRow("ℹ", "Provider", config.Data?.Provider ?? "N/A");
                table.AddRow("ℹ", "Database type", config.Data?.DatabaseType ?? "N/A");

                console.WriteLine();
                table.Render(console.SpectreConsole);
                console.WriteLine();

                console.WriteMuted("Run this command in a project with a configured data provider");
                console.WriteMuted("and an active database connection to see migration details.");
            }

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return Task.FromResult(4);
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
