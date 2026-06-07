using System.CommandLine;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet db migration</c> parent command — groups subcommands
/// for managing database migrations (add, status).
/// </summary>
public static class DbMigrationCommand
{
    /// <summary>
    /// Creates the <c>migration</c> command with subcommands for migration operations.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("migration", "Manage database migrations")
        {
            DbMigrationAddCommand.Create(),
            DbMigrationStatusCommand.Create()
        };

        // When no subcommand is specified, show help
        command.SetHandler(() => { });

        return command;
    }
}
