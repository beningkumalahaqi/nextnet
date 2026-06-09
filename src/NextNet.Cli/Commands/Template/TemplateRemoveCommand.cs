using NextNet.Cli.Community;
using System.CommandLine;

namespace NextNet.Cli.Commands.Template;

/// <summary>
/// Implements the <c>nextnet template remove</c> command — removes an installed
/// community template from disk and the local registry.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template remove</c> command deletes the template directory from
/// <c>~/.nextnet/templates/</c> and removes the entry from the local manifest.
/// This operation cannot be undone — the template must be reinstalled to be used again.
/// </para>
/// </remarks>
public sealed class TemplateRemoveCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRemoveCommand"/> class.
    /// </summary>
    public TemplateRemoveCommand() : base("remove", "Remove an installed community template")
    {
        var nameArg = new Argument<string>("name", "Template name to remove");

        AddArgument(nameArg);

        this.SetHandler(HandleAsync, nameArg);
    }

    private static async Task<int> HandleAsync(string name)
    {
        try
        {
            // The manager itself is synchronous for Remove, but we create it asynchronously
            // to match the common pattern used by other commands.
            var manager = TemplateInstallCommand.CreateManager();

            var result = await Task.Run(() => manager.Remove(name));

            if (result.Success)
            {
                Console.WriteLine(result.Message);
                return 0;
            }

            Console.Error.WriteLine(result.Message);
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
