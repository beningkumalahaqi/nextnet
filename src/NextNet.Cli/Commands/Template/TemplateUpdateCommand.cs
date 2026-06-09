using NextNet.Cli.Community;
using System.CommandLine;

namespace NextNet.Cli.Commands.Template;

/// <summary>
/// Implements the <c>nextnet template update</c> command — updates installed
/// community templates to their latest version.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template update</c> command checks the registry for newer versions of
/// installed templates. When called without a name, all installed templates are
/// checked and updated. When a name is provided, only that template is updated.
/// </para>
/// <para>
/// Use <c>--pre-release</c> to include pre-release versions when checking for updates.
/// </para>
/// </remarks>
public sealed class TemplateUpdateCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateUpdateCommand"/> class.
    /// </summary>
    public TemplateUpdateCommand() : base("update", "Update installed community templates")
    {
        var nameArg = new Argument<string?>("name", () => null, "Template name to update (omit to update all)");
        var preReleaseOption = new Option<bool>("--pre-release", "Include pre-release versions");

        AddArgument(nameArg);
        AddOption(preReleaseOption);

        this.SetHandler(HandleAsync, nameArg, preReleaseOption);
    }

    private static async Task<int> HandleAsync(string? name, bool preRelease)
    {
        try
        {
            var manager = TemplateInstallCommand.CreateManager();

            var options = new UpdateOptions
            {
                PreRelease = preRelease
            };

            var result = await manager.UpdateAsync(name, options);

            if (result.Updated.Count > 0)
            {
                Console.WriteLine("Updated templates:");
                foreach (var t in result.Updated)
                    Console.WriteLine($"  - {t}");
            }

            if (result.Failed.Count > 0)
            {
                Console.Error.WriteLine("Failed updates:");
                foreach (var t in result.Failed)
                    Console.Error.WriteLine($"  - {t}");
            }

            if (result.Updated.Count == 0 && result.Failed.Count == 0)
            {
                Console.WriteLine("All templates are up to date.");
            }

            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
