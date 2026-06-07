using NextNet.Cli.Commands.Data;
using System.CommandLine;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet db</c> parent command — groups subcommands
/// for database operations (init, migrate, rollback, migration, explore).
/// </summary>
public static class DbCommand
{
    /// <summary>
    /// Creates the <c>db</c> command with all database subcommands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("db", "Manage databases for your NextNet project")
        {
            DbInitCommand.Create(),
            DbMigrationCommand.Create(),
            DbMigrateCommand.Create(),
            DbRollbackCommand.Create(),
            DbExploreCommand.Create()
        };

        // When no subcommand is specified, show help
        command.SetHandler(() => { });

        return command;
    }
}
