using System.CommandLine;
using NextNet.Cli.Commands.Data;

namespace NextNet.Cli.Commands.Generate;

/// <summary>
/// Implements the <c>nextnet generate</c> parent command — groups subcommands
/// for code generation (model, repository, CRUD actions, admin pages).
/// </summary>
public static class GenerateCommand
{
    /// <summary>
    /// Creates the <c>generate</c> parent command with model, repository, crud, and admin subcommands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("generate", "Generate code for data entities (models, repositories, CRUD actions, admin pages)")
        {
            GenerateModelCommand.Create(),
            GenerateRepositoryCommand.Create(),
            GenerateCrudCommand.Create(),
            GenerateAdminCommand.Create()
        };

        // When no subcommand is specified, show help
        command.SetHandler(() => { });

        return command;
    }
}
