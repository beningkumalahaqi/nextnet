using NextNet.Cli.Commands;
using NextNet.Cli.Commands.Generate;
using NextNet.Cli.Commands.Template;
using System.CommandLine;

namespace NextNet.Cli;

/// <summary>
/// Factory and entry point for the NextNet CLI.
/// Creates and configures the root command with all subcommands and global options.
/// </summary>
public static class NextNetCli
{
    /// <summary>
    /// Global option: machine-readable output (no colors, no Unicode).
    /// </summary>
    public static Option<bool> PlainOption { get; } = new("--plain", "Machine-readable output (no colors, no Unicode)");

    /// <summary>
    /// Global option: disable color output.
    /// </summary>
    public static Option<bool> NoColorOption { get; } = new("--no-color", "Disable color output");

    /// <summary>
    /// Global option: enable verbose output.
    /// </summary>
    public static Option<bool> VerboseOption { get; } = new("--verbose", "Enable verbose output");

    /// <summary>
    /// Creates the root command-line application with all subcommands registered.
    /// </summary>
    /// <returns>A configured <see cref="RootCommand"/> ready for invocation.</returns>
    public static RootCommand Create()
    {
        var root = new RootCommand("NextNet CLI — full-stack web framework for .NET");

        VerboseOption.AddAlias("-v");

        root.AddGlobalOption(PlainOption);
        root.AddGlobalOption(NoColorOption);
        root.AddGlobalOption(VerboseOption);

        // Register commands
        root.AddCommand(new NewCommand());
        root.AddCommand(InfoCommand.Create());
        root.AddCommand(DoctorCommand.Create());
        root.AddCommand(BuildCommand.Create());
        root.AddCommand(DevCommand.Create());
        root.AddCommand(DevToolsCommand.Create());
        root.AddCommand(AddCommand.Create());
        root.AddCommand(DbCommand.Create());
        root.AddCommand(GenerateCommand.Create());
        root.AddCommand(new TemplateCommand());

        return root;
    }
}
