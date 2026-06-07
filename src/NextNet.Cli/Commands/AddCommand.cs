using NextNet.Cli.Commands.Data;
using System.CommandLine;

namespace NextNet.Cli.Commands;

/// <summary>
/// Implements the <c>nextnet add</c> parent command — groups subcommands
/// that add functionality to a NextNet project (e.g., data providers).
/// </summary>
public static class AddCommand
{
    /// <summary>
    /// Creates the <c>add</c> command with data and future subcommands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("add", "Add functionality to your NextNet project")
        {
            AddDataCommand.Create()
        };

        // When no subcommand is specified, show help
        command.SetHandler(() => { });

        return command;
    }
}
