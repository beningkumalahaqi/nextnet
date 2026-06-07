using System.Text;
using System.Text.Json;

namespace NextNet.TemplateSdk;

/// <summary>
/// Scaffolds a new NextNet template project directory structure with a manifest,
/// README, and sample files.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TemplateScaffolder"/> creates the initial file layout for template authors.
/// Given a target directory and <see cref="ScaffoldOptions"/>, it produces:
/// </para>
/// <list type="bullet">
///   <item><c>template.json</c> — the template manifest with name, version, and metadata.</item>
///   <item><c>README.md</c> — installation and usage documentation.</item>
///   <item><c>files/</c> — directory containing sample template files with placeholder variables.</item>
/// </list>
/// <para>
/// This is the starting point for creating a new template. After scaffolding, authors
/// can edit <c>template.json</c> and add their own files under <c>files/</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var scaffolder = new TemplateScaffolder();
/// await scaffolder.ScaffoldAsync("./my-template", new ScaffoldOptions
/// {
///     Name = "my-template",
///     Author = "Me",
///     Description = "A custom template"
/// });
/// </code>
/// </example>
public sealed class TemplateScaffolder
{
    /// <summary>
    /// Creates a complete template project scaffold at the specified directory.
    /// </summary>
    /// <param name="directory">The target directory where the template will be scaffolded. Must not be null or empty.</param>
    /// <param name="options">Configuration options including template name, author, and description.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="directory"/> is null or whitespace.</exception>
    public async Task ScaffoldAsync(string directory, ScaffoldOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(options);

        Directory.CreateDirectory(directory);
        Directory.CreateDirectory(Path.Combine(directory, "files"));

        // Write template.json
        var manifest = new
        {
            name = options.Name,
            version = "1.0.0",
            nextnetVersion = ">=3.0.0",
            author = options.Author ?? "Your Name",
            description = options.Description ?? $"A custom NextNet template: {options.Name}",
            tags = options.Tags ?? Array.Empty<string>()
        };

        var manifestPath = Path.Combine(directory, "template.json");
        var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, manifestJson);

        // Write README.md
        var readmePath = Path.Combine(directory, "README.md");
        var readme = $@"# {options.Name}

{options.Description ?? "A custom NextNet template."}

## Installation

```bash
nextnet template install {options.Author ?? "author"}/{options.Name}
```

## Variables

| Name | Type | Required | Description |
|------|------|----------|-------------|
| projectName | string | Yes | The project name |
";
        await File.WriteAllTextAsync(readmePath, readme);

        // Write a sample file
        var samplePath = Path.Combine(directory, "files", "hello.txt");
        var sample = "Hello, {{projectName}}!";
        await File.WriteAllTextAsync(samplePath, sample);
    }
}

/// <summary>
/// Configuration options for the <see cref="TemplateScaffolder.ScaffoldAsync"/> method.
/// </summary>
/// <remarks>
/// All properties are optional except <see cref="Name"/>.
/// </remarks>
public sealed record ScaffoldOptions
{
    /// <summary>
    /// The unique name of the template (e.g., "nextnet-webapi").
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// The author or organization that created the template.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// A human-readable description of the template.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional tags for searching and categorizing the template.
    /// </summary>
    public string[]? Tags { get; init; }
}
