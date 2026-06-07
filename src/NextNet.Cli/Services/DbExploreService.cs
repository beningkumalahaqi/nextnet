using NextNet.Cli.Commands.Data;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Cli.Services;

/// <summary>
/// Orchestrates database exploration operations, bridging the CLI
/// <c>db explore</c> command with <see cref="IAdminSchemaProvider"/>
/// implementations.
/// </summary>
/// <remarks>
/// <para>
/// The DbExploreService handles:
/// <list type="bullet">
///   <item><description>Loading project configuration and resolving the connection string</description></item>
///   <item><description>Resolving the appropriate <see cref="IAdminSchemaProvider"/></description></item>
///   <item><description>Testing database connectivity</description></item>
///   <item><description>Formatting and displaying schema information</description></item>
/// </list>
/// </para>
/// </remarks>
public static class DbExploreService
{
    /// <summary>
    /// Explores the database and lists all tables/collections.
    /// </summary>
    /// <param name="console">The console for output.</param>
    /// <param name="connectionName">The named connection to use (default: "Default").</param>
    /// <param name="includeViews">Whether to include views.</param>
    /// <param name="format">Output format: "tree", "table", or "json".</param>
    /// <param name="verbose">Whether to enable verbose output.</param>
    /// <returns>Exit code (0 = success).</returns>
    public static async Task<int> ListTablesAsync(
        NextNetConsole console,
        string? connectionName = null,
        bool includeViews = false,
        string format = "tree",
        bool verbose = false)
    {
        try
        {
            // Step 1: Load config
            var config = ConfigLoader.Load();
            if (config is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound);
                return 2;
            }

            // Step 2: Resolve connection string and provider
            var (connectionString, providerKey, errorCode) = ResolveConnection(config, connectionName);
            if (connectionString is null || providerKey is null)
                return errorCode ?? 2;

            // Step 3: Resolve schema provider
            var schemaProvider = AdminSchemaProviderRegistry.GetProvider(providerKey);
            if (schemaProvider is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreSchemaProviderNotFound,
                    $"No schema provider available for provider '{providerKey}'.");
                return 2;
            }

            // Step 4: Test connection
            if (verbose)
                console.WriteInfo("Testing database connection...");

            var connected = await schemaProvider.TestConnectionAsync(connectionString);
            if (!connected)
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreConnectionFailed,
                    $"Could not connect to the database using connection '{connectionName ?? "Default"}'.");
                return 4;
            }

            // Step 5: List tables
            if (verbose)
                console.WriteInfo("Retrieving table listing...");

            var tables = await schemaProvider.ListTablesAsync(connectionString, includeViews);

            if (tables.Count == 0)
            {
                console.WriteInfo("No tables or collections found in the database.");
                return 0;
            }

            // Step 6: Format and display
            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var json = AdminOutputFormatter.FormatJson(new
                {
                    connection = new
                    {
                        name = connectionName ?? "Default",
                        provider = providerKey,
                        database = config.Data?.ConnectionString
                    },
                    tables
                });
                console.WriteLine(json);
            }
            else if (string.Equals(format, "table", StringComparison.OrdinalIgnoreCase) || console.IsPlain)
            {
                var tableListing = AdminOutputFormatter.FormatTableList(tables, connectionName ?? "Default",
                    config.Data?.ConnectionString);
                console.WriteLine(tableListing);
            }
            else
            {
                var tree = AdminOutputFormatter.BuildTreeView(tables, connectionName ?? "Default");
                tree.Render(console.SpectreConsole);
            }

            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }

    /// <summary>
    /// Explores a specific table and shows its detailed schema.
    /// </summary>
    /// <param name="console">The console for output.</param>
    /// <param name="tableName">The table name to inspect.</param>
    /// <param name="connectionName">The named connection to use.</param>
    /// <param name="format">Output format: "tree", "table", or "json".</param>
    /// <param name="verbose">Whether to enable verbose output.</param>
    /// <returns>Exit code (0 = success).</returns>
    public static async Task<int> ShowTableDetailAsync(
        NextNetConsole console,
        string tableName,
        string? connectionName = null,
        string format = "tree",
        bool verbose = false)
    {
        try
        {
            // Step 1: Load config
            var config = ConfigLoader.Load();
            if (config is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound);
                return 2;
            }

            // Step 2: Resolve connection string and provider
            var (connectionString, providerKey, errorCode) = ResolveConnection(config, connectionName);
            if (connectionString is null || providerKey is null)
                return errorCode ?? 2;

            // Step 3: Resolve schema provider
            var schemaProvider = AdminSchemaProviderRegistry.GetProvider(providerKey);
            if (schemaProvider is null)
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreSchemaProviderNotFound,
                    $"No schema provider available for provider '{providerKey}'.");
                return 2;
            }

            // Step 4: Test connection
            if (verbose)
                console.WriteInfo("Testing database connection...");

            var connected = await schemaProvider.TestConnectionAsync(connectionString);
            if (!connected)
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreConnectionFailed,
                    $"Could not connect to the database using connection '{connectionName ?? "Default"}'.");
                return 4;
            }

            // Step 5: Get table detail
            if (verbose)
                console.WriteInfo($"Retrieving schema for table '{tableName}'...");

            SchemaTableDetail detail;
            try
            {
                detail = await schemaProvider.GetTableDetailAsync(connectionString, tableName);
            }
            catch
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreTableNotFound,
                    $"Table or collection '{tableName}' was not found in the database.");
                return 2;
            }

            if (detail.Columns.Count == 0)
            {
                ErrorMessage.Write(console, ErrorCodes.ExploreTableNotFound,
                    $"Table or collection '{tableName}' was not found or has no accessible columns.");
                return 2;
            }

            // Step 6: Format and display
            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var json = AdminOutputFormatter.FormatJson(new
                {
                    connection = new
                    {
                        name = connectionName ?? "Default",
                        provider = providerKey
                    },
                    table = detail
                });
                console.WriteLine(json);
            }
            else
            {
                var formatted = AdminOutputFormatter.FormatTableDetail(detail);
                console.WriteLine(formatted);
            }

            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }

    private static (string? connectionString, string? providerKey, int? errorCode) ResolveConnection(
        NextNetProjectConfig config,
        string? connectionName)
    {
        if (config.Data?.Provider is null)
        {
            return (null, null, null);
        }

        var providerKey = config.Data.Provider.ToLowerInvariant();
        var connectionString = config.Data.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (null, null, null);
        }

        return (connectionString, providerKey, null);
    }
}
