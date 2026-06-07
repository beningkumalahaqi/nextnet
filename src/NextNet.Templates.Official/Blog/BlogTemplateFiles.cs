namespace NextNet.Templates.Official.Blog;

using System.Text;

/// <summary>
/// Static file content for the Blog template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="BlogTemplateFiles"/> provides the raw byte content for all files in the
/// official Blog template package. In a real production implementation, these would be
/// loaded from embedded resources or the file system. For simplicity and testability,
/// they are defined inline as static string constants.
/// </para>
/// <para>
/// The <see cref="GetAllFiles"/> method returns a dictionary mapping source paths
/// (relative to the template root) to UTF-8 encoded byte arrays, which are consumed
/// by <see cref="BlogTemplateProvider.GetFilesAsync"/>.
/// </para>
/// </remarks>
public static class BlogTemplateFiles
{
    /// <summary>
    /// Returns all template files as a dictionary of source path to byte content.
    /// </summary>
    /// <returns>A read-only dictionary mapping source paths to raw content bytes.</returns>
    /// <example>
    /// <code>
    /// var files = BlogTemplateFiles.GetAllFiles();
    /// var programCs = Encoding.UTF8.GetString(files["app/Program.cs"]);
    /// </code>
    /// </example>
    public static IReadOnlyDictionary<string, byte[]> GetAllFiles()
    {
        var files = new Dictionary<string, byte[]>
        {
            ["template.json"] = Encoding.UTF8.GetBytes(BlogTemplateManifestJson),
            ["app/Program.cs"] = Encoding.UTF8.GetBytes(ProgramCs),
            ["app/appsettings.json"] = Encoding.UTF8.GetBytes(AppSettingsJson),
            ["app/page.cs"] = Encoding.UTF8.GetBytes(HomePageCs),
            ["app/blog/page.cs"] = Encoding.UTF8.GetBytes(BlogIndexCs),
            ["app/blog/[slug]/page.cs"] = Encoding.UTF8.GetBytes(BlogPostCs),
            ["app/Services/BlogService.cs"] = Encoding.UTF8.GetBytes(BlogServiceCs),
            ["app/Services/IBlogService.cs"] = Encoding.UTF8.GetBytes(IBlogServiceCs),
            ["app/Models/Post.cs"] = Encoding.UTF8.GetBytes(PostModelCs),
            ["app/Models/PostFrontmatter.cs"] = Encoding.UTF8.GetBytes(PostFrontmatterCs),
            ["app/content/posts/welcome.md"] = Encoding.UTF8.GetBytes(WelcomePost),
            ["app/content/posts/getting-started.md"] = Encoding.UTF8.GetBytes(GettingStartedPost)
        };
        return files;
    }

    private const string BlogTemplateManifestJson = """
    {
      "name": "blog",
      "version": "1.0.0",
      "nextnetVersion": ">=3.0.0",
      "author": "NextNet Team",
      "description": "Production-ready blog with Markdown, RSS, and sitemap"
    }
    """;

    private const string ProgramCs = """
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using {{namespaceName}}.App.Services;

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSingleton<IBlogService, BlogService>();
    var app = builder.Build();
    app.Run();
    """;

    private const string AppSettingsJson = """
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information"
        }
      },
      "BaseUrl": "{{baseUrl}}",
      "Author": "{{authorName}}"
    }
    """;

    private const string HomePageCs = """"
    namespace {{namespaceName}}.App.Pages;

    public class IndexPage
    {
        public string Render() => """
            <!DOCTYPE html>
            <html>
            <head><title>{{projectName}}</title></head>
            <body>
                <h1>Welcome to {{projectName}}</h1>
                <p>A blog built with NextNet.</p>
                <a href="/blog">View blog &rarr;</a>
            </body>
            </html>
            """;
    }
    """";

    private const string BlogIndexCs = """
    namespace {{namespaceName}}.App.Pages.Blog;

    using {{namespaceName}}.App.Services;

    public class IndexPage
    {
        private readonly IBlogService _blog;

        public IndexPage(IBlogService blog) => _blog = blog;

        public async Task<string> RenderAsync()
        {
            var posts = await _blog.GetPostsAsync();
            var html = new System.Text.StringBuilder();
            html.AppendLine("<!DOCTYPE html><html><head><title>Blog</title></head><body>");
            html.AppendLine("<h1>Blog Posts</h1><ul>");
            foreach (var post in posts)
            {
                html.AppendLine($"<li><a href=\"/blog/{post.Slug}\">{post.Title}</a> &mdash; {post.PublishedDate:yyyy-MM-dd}</li>");
            }
            html.AppendLine("</ul></body></html>");
            return html.ToString();
        }
    }
    """;

    private const string BlogPostCs = """
    namespace {{namespaceName}}.App.Pages.Blog;

    using {{namespaceName}}.App.Services;

    public class SlugPage
    {
        private readonly IBlogService _blog;

        public SlugPage(IBlogService blog) => _blog = blog;

        public async Task<string?> RenderAsync(string slug)
        {
            var post = await _blog.GetPostAsync(slug);
            if (post is null) return null;
            return $"<!DOCTYPE html><html><head><title>{post.Title}</title></head><body><article><h1>{post.Title}</h1><div>{post.HtmlContent}</div></article></body></html>";
        }
    }
    """;

    private const string IBlogServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;

    public interface IBlogService
    {
        Task<IReadOnlyList<Post>> GetPostsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Post>> GetPostsByTagAsync(string tag, CancellationToken ct = default);
        Task<Post?> GetPostAsync(string slug, CancellationToken ct = default);
    }
    """;

    private const string BlogServiceCs = """
    namespace {{namespaceName}}.App.Services;

    using {{namespaceName}}.App.Models;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Markdig;

    public class BlogService : IBlogService
    {
        private readonly string _contentDir;
        private readonly MarkdownPipeline _pipeline;

        public BlogService(IWebHostEnvironment env)
        {
            _contentDir = Path.Combine(env.ContentRootPath, "content", "posts");
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public Task<IReadOnlyList<Post>> GetPostsAsync(CancellationToken ct = default)
        {
            var posts = new List<Post>();
            if (!Directory.Exists(_contentDir)) return Task.FromResult<IReadOnlyList<Post>>(posts);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            foreach (var file in Directory.EnumerateFiles(_contentDir, "*.md"))
            {
                var content = File.ReadAllText(file);
                var (frontmatter, body) = ParseFrontmatter(content);
                var post = new Post
                {
                    Slug = Path.GetFileNameWithoutExtension(file),
                    Title = frontmatter?.Title ?? Path.GetFileNameWithoutExtension(file),
                    PublishedDate = frontmatter?.Date ?? File.GetLastWriteTime(file),
                    Author = frontmatter?.Author ?? "Anonymous",
                    Tags = frontmatter?.Tags ?? new List<string>(),
                    IsDraft = frontmatter?.Draft ?? false,
                    Excerpt = frontmatter?.Excerpt ?? "",
                    MarkdownContent = body,
                    HtmlContent = Markdown.ToHtml(body, _pipeline)
                };
                posts.Add(post);
            }

            return Task.FromResult<IReadOnlyList<Post>>(posts.OrderByDescending(p => p.PublishedDate).ToList());
        }

        public Task<IReadOnlyList<Post>> GetPostsByTagAsync(string tag, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<Post>>(GetPostsAsync(ct).Result.Where(p => p.Tags.Contains(tag)).ToList());
        }

        public Task<Post?> GetPostAsync(string slug, CancellationToken ct = default)
        {
            return Task.FromResult<Post?>(GetPostsAsync(ct).Result.FirstOrDefault(p => p.Slug == slug));
        }

        private static (PostFrontmatter?, string) ParseFrontmatter(string content)
        {
            if (!content.StartsWith("---")) return (null, content);
            var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
            if (endIndex < 0) return (null, content);
            var yaml = content.Substring(3, endIndex - 3).Trim();
            var body = content.Substring(endIndex + 3).Trim();
            var deserializer = new Deserializer();
            try { return (deserializer.Deserialize<PostFrontmatter>(yaml), body); }
            catch { return (null, body); }
        }
    }
    """;

    private const string PostModelCs = """
    namespace {{namespaceName}}.App.Models;

    public sealed record Post
    {
        public string Slug { get; init; } = "";
        public string Title { get; init; } = "";
        public DateTime PublishedDate { get; init; }
        public string Author { get; init; } = "";
        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
        public bool IsDraft { get; init; }
        public string Excerpt { get; init; } = "";
        public string MarkdownContent { get; init; } = "";
        public string HtmlContent { get; init; } = "";
    }
    """;

    private const string PostFrontmatterCs = """
    namespace {{namespaceName}}.App.Models;

    using YamlDotNet.Serialization;

    public sealed class PostFrontmatter
    {
        [YamlMember(Alias = "title")]
        public string? Title { get; set; }

        [YamlMember(Alias = "date")]
        public DateTime? Date { get; set; }

        [YamlMember(Alias = "author")]
        public string? Author { get; set; }

        [YamlMember(Alias = "tags")]
        public List<string> Tags { get; set; } = new();

        [YamlMember(Alias = "draft")]
        public bool Draft { get; set; }

        [YamlMember(Alias = "excerpt")]
        public string? Excerpt { get; set; }
    }
    """;

    private const string WelcomePost = """
    ---
    title: "Welcome to {{projectName}}"
    date: 2026-01-15
    author: "{{authorName}}"
    tags: [introduction, meta]
    draft: false
    excerpt: "A brief welcome post to get things started."
    ---

    # Welcome

    This is your first blog post, generated by the NextNet Blog template.
    """;

    private const string GettingStartedPost = """
    ---
    title: "Getting Started"
    date: 2026-01-16
    author: "{{authorName}}"
    tags: [tutorial, guide]
    draft: false
    excerpt: "How to use and customize your new blog."
    ---

    # Getting Started

    Learn how to create, edit, and publish posts.
    """;
}
