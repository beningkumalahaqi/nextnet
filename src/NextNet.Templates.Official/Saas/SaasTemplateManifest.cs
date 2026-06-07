namespace NextNet.Templates.Official.Saas;

using NextNet.Templates.Models;

/// <summary>
/// Static manifest factory for the official SaaS template.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SaasTemplateManifest"/> provides the static metadata for the SaaS template,
/// including its name, version, NextNet compatibility requirement, and the list of
/// files and variables that drive code generation.
/// </para>
/// <para>
/// The manifest is created via <see cref="Create()"/> and returned by
/// <see cref="SaasTemplateProvider.GetManifestAsync"/>.
/// </para>
/// </remarks>
public static class SaasTemplateManifest
{
    /// <summary>
    /// The template name constant: <c>"saas"</c>.
    /// </summary>
    public const string TemplateName = "saas";

    /// <summary>
    /// The current template version: <c>"1.0.0"</c>.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// The minimum NextNet framework version required: <c>"&gt;=3.0.0"</c>.
    /// </summary>
    public const string NextNetVersion = ">=3.0.0";

    /// <summary>
    /// Creates the <see cref="TemplateManifest"/> for the official SaaS template.
    /// </summary>
    /// <returns>A fully populated <see cref="TemplateManifest"/> instance.</returns>
    /// <example>
    /// <code>
    /// var manifest = SaasTemplateManifest.Create();
    /// Console.WriteLine(manifest.Name); // "saas"
    /// </code>
    /// </example>
    public static TemplateManifest Create()
    {
        return new TemplateManifest(
            Name: TemplateName,
            Version: Version,
            NextNetVersion: NextNetVersion,
            Author: "NextNet Team",
            Description: "Multi-tenant SaaS starter with users, organizations, and auth",
            Tags: new[] { "saas", "multi-tenant", "auth", "billing" },
            Variables: new List<TemplateVariable>
            {
                new(Name: "projectName", Type: "string", Required: true,
                    Description: "The project name"),
                new(Name: "namespaceName", Type: "string", Required: false,
                    Description: "PascalCase namespace for C# code (auto-derived from projectName)"),
                new(Name: "database", Type: "enum", Required: false,
                    AllowedValues: new[] { "sqlite", "postgres" },
                    Description: "Database provider"),
                new(Name: "includeBilling", Type: "bool", Required: false,
                    Description: "Include billing scaffold (Stripe placeholder)")
            },
            Features: new List<TemplateFeature>
            {
                new(Name: "billing", Description: "Stripe billing integration scaffold")
            },
            Files: new List<TemplateFile>
            {
                new(SourcePath: "template.json", TargetPath: "template.json"),
                new(SourcePath: "app/Program.cs", TargetPath: "{{projectName}}.App/Program.cs"),
                new(SourcePath: "app/appsettings.json", TargetPath: "{{projectName}}.App/appsettings.json"),
                new(SourcePath: "app/Models/Organization.cs", TargetPath: "{{projectName}}.App/Models/Organization.cs"),
                new(SourcePath: "app/Models/Membership.cs", TargetPath: "{{projectName}}.App/Models/Membership.cs"),
                new(SourcePath: "app/Models/User.cs", TargetPath: "{{projectName}}.App/Models/User.cs"),
                new(SourcePath: "app/Services/ITenantService.cs", TargetPath: "{{projectName}}.App/Services/ITenantService.cs"),
                new(SourcePath: "app/Services/TenantService.cs", TargetPath: "{{projectName}}.App/Services/TenantService.cs"),
                new(SourcePath: "app/Services/IUserService.cs", TargetPath: "{{projectName}}.App/Services/IUserService.cs"),
                new(SourcePath: "app/Services/UserService.cs", TargetPath: "{{projectName}}.App/Services/UserService.cs"),
                new(SourcePath: "app/Auth/AuthController.cs", TargetPath: "{{projectName}}.App/Auth/AuthController.cs"),
                new(SourcePath: "app/Billing/BillingService.cs", TargetPath: "{{projectName}}.App/Billing/BillingService.cs", Condition: "includeBilling == true"),
                new(SourcePath: "app/Data/AppDbContext.cs", TargetPath: "{{projectName}}.App/Data/AppDbContext.cs"),
                new(SourcePath: "app/page.cs", TargetPath: "{{projectName}}.App/page.cs"),
                new(SourcePath: "app/signup/page.cs", TargetPath: "{{projectName}}.App/signup/page.cs"),
                new(SourcePath: "app/login/page.cs", TargetPath: "{{projectName}}.App/login/page.cs"),
                new(SourcePath: "app/dashboard/page.cs", TargetPath: "{{projectName}}.App/dashboard/page.cs")
            }
        );
    }
}
