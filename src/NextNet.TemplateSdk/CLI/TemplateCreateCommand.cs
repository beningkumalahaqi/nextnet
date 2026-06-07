using System.CommandLine;

namespace NextNet.TemplateSdk.CLI;

/// <summary>
/// Implements the <c>template create</c> command — scaffolds a new template project.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template create</c> command uses <see cref="TemplateScaffolder"/> to generate
/// the initial directory structure for a new NextNet template. It creates a
/// <c>template.json</c> manifest, a README, and a sample file with placeholder variables.
/// </para>
/// </remarks>
public sealed class TemplateCreateCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateCreateCommand"/> class.
    /// </summary>
    public TemplateCreateCommand() : base("create", "Create a new template project")
    {
        var nameArg = new Argument<string>("name", "The template name");
        var dirArg = new Argument<string>("output-dir", () => ".", "Output directory");
        var authorOption = new Option<string?>("--author", "Author of the template");
        var descriptionOption = new Option<string?>("--description", "Description of the template");

        AddArgument(nameArg);
        AddArgument(dirArg);
        AddOption(authorOption);
        AddOption(descriptionOption);

        this.SetHandler(HandleAsync, nameArg, dirArg, authorOption, descriptionOption);
    }

    private static async Task<int> HandleAsync(string name, string outputDir, string? author, string? description)
    {
        try
        {
            var targetDir = Path.Combine(outputDir, name);
            var scaffolder = new TemplateScaffolder();
            await scaffolder.ScaffoldAsync(targetDir, new ScaffoldOptions
            {
                Name = name,
                Author = author,
                Description = description
            });

            Console.WriteLine($"Template '{name}' created at {targetDir}");
            Console.WriteLine($"  {Path.Combine(targetDir, "template.json")}");
            Console.WriteLine($"  {Path.Combine(targetDir, "README.md")}");
            Console.WriteLine($"  {Path.Combine(targetDir, "files", "hello.txt")}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
