namespace NextNet.Templates.Official.Api;

using NextNet.Templates.Models;

/// <summary>
/// Static manifest factory for the official REST API template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ApiTemplateManifest"/> provides the static metadata for the API template,
/// including its name, version, NextNet compatibility requirement, variables, features,
/// and the list of files that drive code generation.
/// </para>
/// <para>
/// The manifest is created via <see cref="Create()"/> and returned by
/// <see cref="ApiTemplateProvider.GetManifestAsync"/>.
/// </para>
/// </remarks>
public static class ApiTemplateManifest
{
    /// <summary>
    /// The template name constant: <c>"api"</c>.
    /// </summary>
    public const string TemplateName = "api";

    /// <summary>
    /// The current template version: <c>"1.0.0"</c>.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// The minimum NextNet framework version required: <c>"&gt;=3.0.0"</c>.
    /// </summary>
    public const string NextNetVersion = ">=3.0.0";

    /// <summary>
    /// Creates the <see cref="TemplateManifest"/> for the official API template.
    /// </summary>
    /// <returns>A fully populated <see cref="TemplateManifest"/> instance.</returns>
    /// <example>
    /// <code>
    /// var manifest = ApiTemplateManifest.Create();
    /// Console.WriteLine(manifest.Name); // "api"
    /// </code>
    /// </example>
    public static TemplateManifest Create()
    {
        return new TemplateManifest(
            Name: TemplateName,
            Version: Version,
            NextNetVersion: NextNetVersion,
            Author: "NextNet Team",
            Description: "Production-ready REST API with OpenAPI, Swagger, health checks, and optional auth",
            Tags: new[] { "api", "rest", "openapi", "swagger" },
            Variables: new List<TemplateVariable>
            {
                new(Name: "projectName", Type: "string", Required: true,
                    Description: "The project name (e.g., 'MyApi')"),
                new(Name: "includeAuth", Type: "bool", Required: false,
                    Description: "Include JWT authentication scaffold"),
                new(Name: "database", Type: "enum", Required: false, AllowedValues: new[] { "sqlite", "postgres", "none" },
                    Description: "Database provider")
            },
            Features: new List<TemplateFeature>
            {
                new(Name: "auth", Description: "JWT authentication",
                    Conflicts: new[] { "noAuth" }),
                new(Name: "noAuth", Description: "No authentication")
            },
            Files: new List<TemplateFile>
            {
                new(SourcePath: "template.json", TargetPath: "template.json"),
                new(SourcePath: "app/Program.cs", TargetPath: "{{projectName}}.App/Program.cs"),
                new(SourcePath: "app/appsettings.json", TargetPath: "{{projectName}}.App/appsettings.json"),
                new(SourcePath: "app/Controllers/TodosController.cs", TargetPath: "{{projectName}}.App/Controllers/TodosController.cs"),
                new(SourcePath: "app/Controllers/HealthController.cs", TargetPath: "{{projectName}}.App/Controllers/HealthController.cs"),
                new(SourcePath: "app/Models/Todo.cs", TargetPath: "{{projectName}}.App/Models/Todo.cs"),
                new(SourcePath: "app/Models/Dto/CreateTodoRequest.cs", TargetPath: "{{projectName}}.App/Models/Dto/CreateTodoRequest.cs"),
                new(SourcePath: "app/Models/Dto/UpdateTodoRequest.cs", TargetPath: "{{projectName}}.App/Models/Dto/UpdateTodoRequest.cs"),
                new(SourcePath: "app/Services/ITodoService.cs", TargetPath: "{{projectName}}.App/Services/ITodoService.cs"),
                new(SourcePath: "app/Services/TodoService.cs", TargetPath: "{{projectName}}.App/Services/TodoService.cs"),
                new(SourcePath: "app/Auth/AuthController.cs", TargetPath: "{{projectName}}.App/Auth/AuthController.cs"),
                new(SourcePath: "app/Auth/JwtTokenGenerator.cs", TargetPath: "{{projectName}}.App/Auth/JwtTokenGenerator.cs"),
                new(SourcePath: "app/Data/AppDbContext.cs", TargetPath: "{{projectName}}.App/Data/AppDbContext.cs"),
                new(SourcePath: "app/Middleware/ErrorHandlingMiddleware.cs", TargetPath: "{{projectName}}.App/Middleware/ErrorHandlingMiddleware.cs")
            }
        );
    }
}
