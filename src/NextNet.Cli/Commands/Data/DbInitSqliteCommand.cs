using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db init sqlite</c> command — creates a local SQLite
/// database file (<c>app.db</c>) in the project root and updates
/// <c>nextnet.config.json</c> with the connection string.
/// </summary>
public static class DbInitSqliteCommand
{
    /// <summary>
    /// Creates the <c>sqlite</c> subcommand with optional database file path.
    /// </summary>
    public static Command Create()
    {
        var fileOption = new Option<string>("--file", () => "app.db", "SQLite database file name or path");
        var outputOption = new Option<string>("--output", "Output directory for the database file (default: project root)");

        var command = new Command("sqlite", "Initialize a local SQLite database")
        {
            fileOption,
            outputOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var file = context.ParseResult.GetValueForOption(fileOption) ?? "app.db";
            var output = context.ParseResult.GetValueForOption(outputOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(file, output, plain, noColor, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the SQLite database initialization — creates the <c>.db</c> file
    /// and updates <c>nextnet.config.json</c> with the connection string.
    /// </summary>
    /// <param name="fileName">The database file name (default: app.db).</param>
    /// <param name="outputDir">Optional output directory for the database file.</param>
    /// <param name="plain">Plain text output (no Unicode, no colors).</param>
    /// <param name="noColor">Disable color output.</param>
    /// <param name="verbose">Enable verbose output.</param>
    /// <returns>Exit code (0 = success, 2 = input error, 4 = execution error).</returns>
    public static async Task<int> ExecuteAsync(string? fileName = "app.db", string? outputDir = null, bool plain = false, bool noColor = false, bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Determine the project root from config
            var config = ConfigLoader.Load();
            var projectRoot = Environment.CurrentDirectory;

            // If config has a data section with databaseType, verify it's compatible
            if (config?.Data?.DatabaseType is not null &&
                !string.Equals(config.Data.DatabaseType, "sqlite", StringComparison.OrdinalIgnoreCase))
            {
                console.WriteWarning(
                    $"Project is configured for '{config.Data.DatabaseType}', not SQLite. " +
                    "The database type will be updated.");
            }

            // Resolve the effective file name
            var effectiveFileName = string.IsNullOrWhiteSpace(fileName) ? "app.db" : fileName;
            if (!effectiveFileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                effectiveFileName += ".db";

            // Resolve the output directory
            var effectiveOutputDir = outputDir ?? projectRoot;
            var dbFilePath = Path.GetFullPath(Path.Combine(effectiveOutputDir, effectiveFileName));

            // Check if the database already exists
            var exists = File.Exists(dbFilePath);
            if (exists)
            {
                console.WriteInfo($"Database file already exists at {dbFilePath}");
            }
            else
            {
                // Create the .db file (empty SQLite database)
                console.WriteLine($"Creating SQLite database at {dbFilePath}...");
                await CreateSqliteDatabaseAsync(dbFilePath);
                console.WriteSuccess($"SQLite database created: {effectiveFileName}");
            }

            // Build connection string
            var connectionString = $"Data Source={dbFilePath}";

            // Update config
            console.WriteLine("Updating nextnet.config.json...");
            try
            {
                UpdateConfigWithSqlite(connectionString, effectiveFileName);
                console.WriteSuccess("Configuration updated with SQLite connection string");
            }
            catch (Exception ex)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigUpdateFailed,
                    $"Failed to update configuration: {ex.Message}");
                return 4;
            }

            console.WriteLine();
            console.WriteInfo($"Connection string: Data Source={effectiveFileName}");
            console.WriteLine();
            console.WriteSuccess("SQLite database initialized successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return 4;
        }
    }

    /// <summary>
    /// Creates an empty SQLite database file by writing the standard SQLite header bytes.
    /// </summary>
    private static async Task CreateSqliteDatabaseAsync(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // Create a valid empty SQLite database by writing the minimal header
        // SQLite database header: https://www.sqlite.org/fileformat.html#the_database_header
        var header = new byte[100];
        header[0] = (byte)'S';  // Magic string "SQLite format 3\0"
        header[1] = (byte)'Q';
        header[2] = (byte)'L';
        header[3] = (byte)'i';
        header[4] = (byte)'t';
        header[5] = (byte)'e';
        header[6] = (byte)' ';
        header[7] = (byte)'f';
        header[8] = (byte)'o';
        header[9] = (byte)'r';
        header[10] = (byte)'m';
        header[11] = (byte)'a';
        header[12] = (byte)'t';
        header[13] = (byte)' ';
        header[14] = (byte)'3';
        header[15] = 0;        // Null terminator
        header[16] = 0;        // Page size (0 means 65536)
        header[17] = 1;        // Write version (1 = legacy, 2 = WAL)
        header[18] = 1;        // Read version
        header[19] = 0;        // Reserved space
        header[20] = 0;        // Max embedded payload fraction
        header[21] = 64;       // Min embedded payload fraction
        header[22] = 32;       // Leaf payload fraction
        header[23] = 0;        // File change counter
        // header[24..27] are reserved (already zero-initialized)
        header[28] = 0;        // Page count (0 = empty database)

        await File.WriteAllBytesAsync(filePath, header);
    }

    /// <summary>
    /// Updates <c>nextnet.config.json</c> with the SQLite connection string and provider info.
    /// </summary>
    private static void UpdateConfigWithSqlite(string connectionString, string fileName)
    {
        var config = ConfigLoader.LoadRequired();

        // Ensure data section exists, defaulting to EF Core for SQLite
        config.Data ??= new DataConfig();
        config.Data.Provider ??= "ef";
        config.Data.DatabaseType = "sqlite";
        config.Data.ConnectionString = connectionString;

        // Track packages if not already set
        config.Data.Packages ??= new[] { "Microsoft.EntityFrameworkCore.Sqlite" };

        ConfigLoader.Save(config);
    }
}
