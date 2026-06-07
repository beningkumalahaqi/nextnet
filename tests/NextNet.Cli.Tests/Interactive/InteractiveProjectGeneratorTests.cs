using NextNet.Cli.Interactive;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;
using NextNet.Templates.Official.Blog;
using Xunit;

namespace NextNet.Cli.Tests.Interactive;

/// <summary>
/// Tests for the <see cref="InteractiveProjectGenerator"/> class and related
/// interactive project generation components.
/// </summary>
public class InteractiveProjectGeneratorTests
{
    /// <summary>
    /// Verifies that <see cref="InteractiveProjectGenerator.GenerateAsync"/> returns
    /// the expected result when all options are provided (non-interactive mode).
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_UseProvidedOptions_When_NonInteractive()
    {
        var providers = new[] { new BlogTemplateProvider() };
        var gen = new InteractiveProjectGenerator(providers);

        var options = new InteractiveOptions
        {
            ProjectName = "TestBlog",
            TemplateName = "blog",
            AuthorName = "Jane",
            BaseUrl = "http://test.com",
            NonInteractive = true
        };

        var result = await gen.GenerateAsync(options);
        Assert.Equal("blog", result.TemplateName);
        Assert.Equal("TestBlog", result.VariableContext.Get("projectName"));
        Assert.Equal("Jane", result.VariableContext.Get("authorName"));
        Assert.Equal("http://test.com", result.VariableContext.Get("baseUrl"));
    }

    /// <summary>
    /// Verifies that <see cref="InteractiveProjectGenerator.GenerateAsync"/> throws
    /// <see cref="InvalidOperationException"/> when the specified template does not exist.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_Throw_When_TemplateNotFound()
    {
        var providers = new[] { new BlogTemplateProvider() };
        var gen = new InteractiveProjectGenerator(providers);

        var options = new InteractiveOptions
        {
            ProjectName = "Test",
            TemplateName = "nonexistent",
            NonInteractive = true
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => gen.GenerateAsync(options));
    }

    /// <summary>
    /// Verifies that <see cref="InteractiveProjectGenerator"/> accepts multiple providers
    /// and selects the correct one based on the requested template.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_SelectCorrectProvider_When_MultipleProviders()
    {
        var providers = new ITemplateProvider[]
        {
            new BlogTemplateProvider(),
            new ApiTemplateProviderStub()
        };
        var gen = new InteractiveProjectGenerator(providers);

        var options = new InteractiveOptions
        {
            ProjectName = "MyApi",
            TemplateName = "api",
            Database = "postgres",
            IncludeAuth = true,
            NonInteractive = true
        };

        var result = await gen.GenerateAsync(options);
        Assert.Equal("api", result.TemplateName);
        Assert.Equal("MyApi", result.VariableContext.Get("projectName"));
        Assert.Equal("postgres", result.VariableContext.Get("database"));
        Assert.True(result.VariableContext.Get<bool>("includeAuth"));
    }

    /// <summary>
    /// Verifies that the prompt definition for project name validates correctly.
    /// </summary>
    [Fact]
    public void ProjectNameDefinition_Should_ValidateCorrectly()
    {
        var def = PromptDefinitions.ProjectName();

        // Valid names
        Assert.Null(def.Validator!("MyApp"));
        Assert.Null(def.Validator!("blog-2024"));
        Assert.Null(def.Validator!("A"));

        // Invalid names
        Assert.NotNull(def.Validator!(""));
        Assert.NotNull(def.Validator!("123app")); // starts with digit
        Assert.NotNull(def.Validator!("my app")); // contains space
        Assert.NotNull(def.Validator!("special!chars")); // contains invalid char
    }

    /// <summary>
    /// Verifies that the BaseUrl prompt definition validates URLs correctly.
    /// </summary>
    [Fact]
    public void BaseUrlDefinition_Should_ValidateCorrectly()
    {
        var def = PromptDefinitions.BaseUrl();

        // Valid URLs
        Assert.Null(def.Validator!("http://localhost:5000"));
        Assert.Null(def.Validator!("https://example.com"));
        Assert.Null(def.Validator!("http://127.0.0.1"));

        // Invalid URLs
        Assert.NotNull(def.Validator!(""));
        Assert.NotNull(def.Validator!("not-a-url"));

        // Valid but unusual URIs
        Assert.Null(def.Validator!("ftp://invalid")); // ftp://invalid is a valid absolute URI
    }

    /// <summary>
    /// Verifies that <see cref="InteractiveVariableContextBuilder"/> correctly
    /// sets and retrieves typed values.
    /// </summary>
    [Fact]
    public void InteractiveVariableContextBuilder_Should_ProduceContext_WithTypedValues()
    {
        var builder = new InteractiveVariableContextBuilder();
        builder.SetString("projectName", "MyApp");
        builder.SetBool("includeAuth", true);
        builder.Set("port", 8080);

        var ctx = builder.Build();
        Assert.Equal("MyApp", ctx.Get<string>("projectName"));
        Assert.True(ctx.Get<bool>("includeAuth"));
        Assert.Equal(8080, ctx.Get<int>("port"));
    }

    /// <summary>
    /// Verifies that <see cref="InteractiveProjectGenerator"/> throws when no providers are available.
    /// </summary>
    [Fact]
    public void Constructor_Should_Throw_When_ProvidersNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InteractiveProjectGenerator(null!));
    }

    /// <summary>
    /// Verifies that <see cref="InteractiveProjectGenerator"/> handles empty provider collection.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_Should_Throw_When_NoProviders()
    {
        var gen = new InteractiveProjectGenerator(Array.Empty<ITemplateProvider>());

        var options = new InteractiveOptions
        {
            ProjectName = "Test",
            TemplateName = "blog",
            NonInteractive = true
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => gen.GenerateAsync(options));
    }

    /// <summary>
    /// A stub <see cref="ITemplateProvider"/> for testing the API template selection.
    /// </summary>
    private sealed class ApiTemplateProviderStub : ITemplateProvider
    {
        public string Name => "api-official";

        public Task<TemplateManifest> GetManifestAsync(
            string templateName,
            string? version = null,
            CancellationToken cancellationToken = default)
        {
            var manifest = new TemplateManifest(
                Name: "api",
                Version: "1.0.0",
                NextNetVersion: ">=3.0.0",
                Author: "NextNet Team",
                Description: "Test API template",
                Variables: new List<TemplateVariable>
                {
                    new(Name: "projectName", Type: "string", Required: true,
                        Description: "The project name"),
                    new(Name: "includeAuth", Type: "bool",
                        Description: "Include authentication scaffold"),
                    new(Name: "database", Type: "enum",
                        AllowedValues: new[] { "sqlite", "postgres", "none" },
                        Description: "Database provider")
                }
            );
            return Task.FromResult(manifest);
        }

        public Task<IReadOnlyDictionary<string, byte[]>> GetFilesAsync(
            TemplateManifest manifest,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyDictionary<string, byte[]>>(
                new Dictionary<string, byte[]>()
            );
        }

        public Task<bool> ExistsAsync(string templateName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(templateName == "api");
        }
    }
}
