namespace NextNet.Templates.Official.Blog;

using NextNet.Templates.Models;

/// <summary>
/// Static manifest factory for the official Blog template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BlogTemplateManifest"/> provides the static metadata for the Blog template,
/// including its name, version, NextNet compatibility requirement, and the list of
/// files and variables that drive code generation.
/// </para>
/// <para>
/// The manifest is created via <see cref="Create()"/> and returned by
/// <see cref="BlogTemplateProvider.GetManifestAsync"/>.
/// </para>
/// </remarks>
public static class BlogTemplateManifest
{
    /// <summary>
    /// The template name constant: <c>"blog"</c>.
    /// </summary>
    public const string TemplateName = "blog";

    /// <summary>
    /// The current template version: <c>"1.0.0"</c>.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// The minimum NextNet framework version required: <c>"&gt;=3.0.0"</c>.
    /// </summary>
    public const string NextNetVersion = ">=3.0.0";

    /// <summary>
    /// Creates the <see cref="TemplateManifest"/> for the official Blog template.
    /// </summary>
    /// <returns>A fully populated <see cref="TemplateManifest"/> instance.</returns>
    /// <example>
    /// <code>
    /// var manifest = BlogTemplateManifest.Create();
    /// Console.WriteLine(manifest.Name); // "blog"
    /// </code>
    /// </example>
    public static TemplateManifest Create()
    {
        var manifest = new TemplateManifest(
            Name: TemplateName,
            Version: Version,
            NextNetVersion: NextNetVersion,
            Author: "NextNet Team",
            Description: "Production-ready blog with Markdown, RSS, and sitemap",
            Tags: new[] { "blog", "content", "markdown", "rss" },
            Variables: new List<TemplateVariable>
            {
                new(Name: "projectName", Type: "string", Required: true,
                    Description: "The project name (e.g., 'MyBlog')"),
                new(Name: "authorName", Type: "string", Required: false,
                    Default: null,
                    Description: "Default author for posts"),
                new(Name: "baseUrl", Type: "string", Required: false,
                    Description: "Base URL for RSS and sitemap (default: http://localhost:5000)")
            },
            Features: null,
            Files: new List<TemplateFile>
            {
                new(SourcePath: "template.json", TargetPath: "template.json"),
                new(SourcePath: "app/Program.cs", TargetPath: "{{projectName}}.App/Program.cs"),
                new(SourcePath: "app/appsettings.json", TargetPath: "{{projectName}}.App/appsettings.json"),
                new(SourcePath: "app/pages/index.cs", TargetPath: "{{projectName}}.App/pages/index.cs"),
                new(SourcePath: "app/pages/blog/index.cs", TargetPath: "{{projectName}}.App/pages/blog/index.cs"),
                new(SourcePath: "app/pages/blog/[slug].cs", TargetPath: "{{projectName}}.App/pages/blog/[slug].cs"),
                new(SourcePath: "app/Services/BlogService.cs", TargetPath: "{{projectName}}.App/Services/BlogService.cs"),
                new(SourcePath: "app/Services/IBlogService.cs", TargetPath: "{{projectName}}.App/Services/IBlogService.cs"),
                new(SourcePath: "app/Models/Post.cs", TargetPath: "{{projectName}}.App/Models/Post.cs"),
                new(SourcePath: "app/Models/PostFrontmatter.cs", TargetPath: "{{projectName}}.App/Models/PostFrontmatter.cs"),
                new(SourcePath: "app/content/posts/welcome.md", TargetPath: "{{projectName}}.App/content/posts/welcome.md"),
                new(SourcePath: "app/content/posts/getting-started.md", TargetPath: "{{projectName}}.App/content/posts/getting-started.md")
            }
        );
        return manifest;
    }
}
