namespace NextNet.Templates.Official.Dashboard;

using NextNet.Templates.Models;

/// <summary>
/// Static manifest factory for the official Admin Dashboard template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DashboardTemplateManifest"/> provides the static metadata for the Dashboard template,
/// including its name, version, NextNet compatibility requirement, and the list of
/// files and variables that drive code generation.
/// </para>
/// <para>
/// The manifest is created via <see cref="Create()"/> and returned by
/// <see cref="DashboardTemplateProvider.GetManifestAsync"/>.
/// </para>
/// </remarks>
public static class DashboardTemplateManifest
{
    /// <summary>
    /// The template name constant: <c>"dashboard"</c>.
    /// </summary>
    public const string TemplateName = "dashboard";

    /// <summary>
    /// The current template version: <c>"1.0.0"</c>.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// The minimum NextNet framework version required: <c>"&gt;=3.0.0"</c>.
    /// </summary>
    public const string NextNetVersion = ">=3.0.0";

    /// <summary>
    /// Creates the <see cref="TemplateManifest"/> for the official Dashboard template.
    /// </summary>
    /// <returns>A fully populated <see cref="TemplateManifest"/> instance.</returns>
    /// <example>
    /// <code>
    /// var manifest = DashboardTemplateManifest.Create();
    /// Console.WriteLine(manifest.Name); // "dashboard"
    /// </code>
    /// </example>
    public static TemplateManifest Create()
    {
        return new TemplateManifest(
            Name: TemplateName,
            Version: Version,
            NextNetVersion: NextNetVersion,
            Author: "NextNet Team",
            Description: "Admin dashboard with auth, navigation, and layout",
            Tags: new[] { "dashboard", "admin", "auth", "ui" },
            Variables: new List<TemplateVariable>
            {
                new(Name: "projectName", Type: "string", Required: true,
                    Description: "The project name"),
                new(Name: "namespaceName", Type: "string", Required: false,
                    Description: "PascalCase namespace for C# code (auto-derived from projectName)"),
                new(Name: "appTitle", Type: "string", Required: false,
                    Description: "Display title for the dashboard"),
                new(Name: "primaryColor", Type: "enum", Required: false,
                    AllowedValues: new[] { "blue", "green", "purple", "red" },
                    Description: "Primary theme color")
            },
            Files: new List<TemplateFile>
            {
                new(SourcePath: "template.json", TargetPath: "template.json"),
                new(SourcePath: "app/Program.cs", TargetPath: "{{projectName}}.App/Program.cs"),
                new(SourcePath: "app/appsettings.json", TargetPath: "{{projectName}}.App/appsettings.json"),
                new(SourcePath: "app/dashboard/page.cs", TargetPath: "{{projectName}}.App/dashboard/page.cs"),
                new(SourcePath: "app/login/page.cs", TargetPath: "{{projectName}}.App/login/page.cs"),
                new(SourcePath: "app/logout/page.cs", TargetPath: "{{projectName}}.App/logout/page.cs"),
                new(SourcePath: "app/layouts/dashboard.cs", TargetPath: "{{projectName}}.App/layouts/dashboard.cs"),
                new(SourcePath: "app/Services/AuthService.cs", TargetPath: "{{projectName}}.App/Services/AuthService.cs"),
                new(SourcePath: "app/Services/IAuthService.cs", TargetPath: "{{projectName}}.App/Services/IAuthService.cs"),
                new(SourcePath: "app/Models/User.cs", TargetPath: "{{projectName}}.App/Models/User.cs"),
                new(SourcePath: "app/wwwroot/css/dashboard.css", TargetPath: "{{projectName}}.App/wwwroot/css/dashboard.css")
            }
        );
    }
}
