using System.CommandLine;
using System.CommandLine.Invocation;
using NextNet.Cli.Errors;
using NextNet.Cli.Services;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db explore</c> command — explores the connected
/// database schema, listing tables/collections and their detailed structure.
/// </summary>
/// <remarks>
/// <para>
/// The explore command uses <c>IAdminSchemaProvider</c> implementations to
/// introspect the database schema without requiring a running application.
/// It supports both list mode (all tables) and detail mode (specific table).
/// </para>
/// <para>
/// Output can be displayed as a tree, table, or JSON format.
/// </para>
/// </remarks>
public static class DbExploreCommand
{
    /// <summary>
    /// Creates the <c>explore</c> subcommand with optional table argument and options.
    /// </summary>
    public static Command Create()
    {
        var tableArg = new Argument<string?>("table", () => null, "Table or collection name to inspect in detail (omit to list all)");
        var connectionOption = new Option<string>("--connection", () => "Default", "Named connection to explore");
        var formatOption = new Option<string>("--format", () => "tree", "Output format: tree, table, json");
        var includeViewsOption = new Option<bool>("--include-views", "Include database views in the listing");
        var verboseOption = new Option<bool>("--verbose", "Enable verbose output");

        var command = new Command("explore", "Explore the connected database schema")
        {
            tableArg,
            connectionOption,
            formatOption,
            includeViewsOption,
            verboseOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var table = context.ParseResult.GetValueForArgument(tableArg);
            var connection = context.ParseResult.GetValueForOption(connectionOption) ?? "Default";
            var format = context.ParseResult.GetValueForOption(formatOption) ?? "tree";
            var includeViews = context.ParseResult.GetValueForOption(includeViewsOption);
            var verbose = context.ParseResult.GetValueForOption(verboseOption);
            // Can't directly access global options from here; parse result has them
            var exitCode = ExecuteAsync(table, connection, format, includeViews, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        // Alias for when it's used as a standalone command
        command.AddAlias("inspect");

        return command;
    }

    /// <summary>
    /// Executes the <c>db explore</c> command logic.
    /// </summary>
    /// <param name="tableName">Optional table name for detail view. Null to list all tables.</param>
    /// <param name="connectionName">The named connection to explore.</param>
    /// <param name="format">Output format: "tree", "table", "json".</param>
    /// <param name="includeViews">Whether to include views in the listing.</param>
    /// <param name="verbose">Enable verbose output.</param>
    /// <returns>Exit code (0 = success).</returns>
    public static async Task<int> ExecuteAsync(
        string? tableName = null,
        string? connectionName = null,
        string format = "tree",
        bool includeViews = false,
        bool verbose = false)
    {
        var console = NextNetConsole.Create(false, false);

        try
        {
            // Validate format
            var validFormats = new[] { "tree", "table", "json" };
            if (!validFormats.Contains(format, StringComparer.OrdinalIgnoreCase))
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreSchemaFailed,
                    $"Invalid format '{format}'. Valid options: {string.Join(", ", validFormats)}.");
                return 2;
            }

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                // Detail mode: explore a specific table
                if (verbose)
                    console.WriteInfo($"Exploring table '{tableName}'...");

                return await DbExploreService.ShowTableDetailAsync(
                    console, tableName, connectionName, format, verbose);
            }
            else
            {
                // List mode: show all tables
                if (verbose)
                    console.WriteInfo("Listing all database tables...");

                return await DbExploreService.ListTablesAsync(
                    console, connectionName, includeViews, format, verbose);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }
}
