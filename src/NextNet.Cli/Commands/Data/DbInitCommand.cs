using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db init</c> command — initializes a database for
/// the configured data provider. Supports subcommands <c>sqlite</c> and <c>postgresql</c>.
/// </summary>
public static class DbInitCommand
{
    /// <summary>
    /// Creates the <c>init</c> subcommand with nested database type commands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("init", "Initialize a database for the configured data provider")
        {
            DbInitSqliteCommand.Create(),
            CreatePostgreSqlCommand()
        };

        // Default handler when no subcommand is specified
        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);
            var exitCode = ExecuteAsync(plain, noColor, verbose).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Default execution when no subcommand is specified — checks config and guides the user.
    /// </summary>
    public static Task<int> ExecuteAsync(bool plain = false, bool noColor = false, bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            var config = ConfigLoader.Load();

            if (config?.Data?.Provider is null)
            {
                ErrorMessage.Write(console, ErrorCodes.NoProviderConfigured,
                    "No data provider is configured. Add one first with 'nextnet add data <provider>'.");
                return Task.FromResult(2);
            }

            // If database type is already configured, suggest the appropriate subcommand
            if (!string.IsNullOrWhiteSpace(config.Data.DatabaseType))
            {
                console.WriteInfo(
                    $"Database type '{config.Data.DatabaseType}' is already configured. " +
                    $"Run 'nextnet db init {config.Data.DatabaseType}' to re-initialize.");
                return Task.FromResult(0);
            }

            console.WriteLine("No database type configured. Choose one:");
            console.WriteLine();
            console.WriteLine("  sqlite      Local SQLite database (app.db)");
            console.WriteLine("  postgresql  PostgreSQL database (connection string or Docker)");
            console.WriteLine();
            console.WriteInfo("  nextnet db init sqlite");
            console.WriteInfo("  nextnet db init postgresql --connection-string \"...\"");
            console.WriteLine();
            console.WriteMuted("Run one of the commands above to initialize your database.");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return Task.FromResult(4);
        }
    }

    /// <summary>
    /// Creates the <c>postgresql</c> subcommand (stub for future implementation).
    /// </summary>
    private static Command CreatePostgreSqlCommand()
    {
        var connectionStringOption = new Option<string>("--connection-string",
            "PostgreSQL connection string (e.g., Host=localhost;Database=mydb;Username=user;Password=pass)");
        var useDockerOption = new Option<bool>("--docker",
            "Start a local PostgreSQL instance via Docker");
        var dockerPortOption = new Option<int>("--docker-port", () => 5432,
            "Host port for Docker PostgreSQL instance");

        var command = new Command("postgresql", "Initialize a PostgreSQL database")
        {
            connectionStringOption,
            useDockerOption,
            dockerPortOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var connectionString = context.ParseResult.GetValueForOption(connectionStringOption);
            var useDocker = context.ParseResult.GetValueForOption(useDockerOption);
            var dockerPort = context.ParseResult.GetValueForOption(dockerPortOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecutePostgreSqlAsync(connectionString, useDocker, dockerPort, plain, noColor, verbose)
                .GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes PostgreSQL database initialization (stub for future implementation).
    /// </summary>
    private static Task<int> ExecutePostgreSqlAsync(
        string? connectionString,
        bool useDocker,
        int dockerPort,
        bool plain = false,
        bool noColor = false,
        bool verbose = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            if (useDocker)
            {
                console.WriteInfo("PostgreSQL Docker setup is not yet implemented.");
                console.WriteLine();
                console.WriteMuted("In the meantime, you can:");
                console.WriteCode("  docker run --name nextnet-postgres -e POSTGRES_PASSWORD=nextnet -p 5432:5432 -d postgres:16");
                console.WriteLine();
                console.WriteInfo("Then run this command again with a connection string.");
                return Task.FromResult(1);
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                ErrorMessage.Write(console, ErrorCodes.ConnectionStringInvalid,
                    "Provide a connection string with --connection-string or use --docker for Docker setup.");
                console.WriteLine();
                console.WriteInfo("Example:");
                console.WriteCode("  nextnet db init postgresql --connection-string \"Host=localhost;Database=nextnet;Username=postgres;Password=nextnet\"");
                return Task.FromResult(2);
            }

            console.WriteLine("Connecting to PostgreSQL...");
            console.WriteLine();

            // TODO: Implement actual PostgreSQL connection validation in V2
            // For now, just save the connection string to config
            console.WriteMuted("Note: PostgreSQL connection validation will be added in a future update.");
            console.WriteLine();

            console.WriteLine("Updating nextnet.config.json...");
            try
            {
                UpdateConfigWithPostgreSql(connectionString);
                console.WriteSuccess("Configuration updated with PostgreSQL connection string");
            }
            catch (Exception ex)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigUpdateFailed,
                    $"Failed to update configuration: {ex.Message}");
                return Task.FromResult(4);
            }

            console.WriteLine();
            console.WriteSuccess("PostgreSQL database configured successfully!");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return Task.FromResult(4);
        }
    }

    /// <summary>
    /// Updates <c>nextnet.config.json</c> with the PostgreSQL connection string.
    /// </summary>
    private static void UpdateConfigWithPostgreSql(string connectionString)
    {
        var config = ConfigLoader.LoadRequired();

        config.Data ??= new DataConfig();
        config.Data.Provider ??= "ef";
        config.Data.DatabaseType = "postgresql";
        config.Data.ConnectionString = connectionString;

        // Add Npgsql package if not already tracked
        config.Data.Packages ??= new[] { "Npgsql.EntityFrameworkCore.PostgreSQL" };

        ConfigLoader.Save(config);
    }
}
