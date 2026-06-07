namespace NextNet.Templates.Official.Tests.Saas;

using NextNet.Templates.Exceptions;
using NextNet.Templates.Official.Saas;
using Xunit;

/// <summary>
/// Tests for the <see cref="SaasTemplateProvider"/> class and the SaaS template generation pipeline.
/// </summary>
public class SaasTemplateProviderTests
{
    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.Name"/> returns <c>"saas-official"</c>.
    /// </summary>
    [Fact]
    public void Name_Should_Return_SaasOfficial()
    {
        var provider = new SaasTemplateProvider();
        Assert.Equal("saas-official", provider.Name);
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.ExistsAsync"/> returns <c>true</c>
    /// when the template name is <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_When_NameIsSaas()
    {
        var provider = new SaasTemplateProvider();
        Assert.True(await provider.ExistsAsync("saas"));
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.ExistsAsync"/> returns <c>false</c>
    /// when the template name is not <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_When_NameIsNotSaas()
    {
        var provider = new SaasTemplateProvider();
        Assert.False(await provider.ExistsAsync("notsaas"));
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.GetManifestAsync"/> returns
    /// a valid SaaS manifest when the template name is <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_ReturnSaasManifest()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.Equal("saas", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.NotNull(manifest.Variables);
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.GetManifestAsync"/> throws
    /// <see cref="TemplateNotFoundException"/> when the template name is not <c>"saas"</c>.
    /// </summary>
    [Fact]
    public async Task GetManifestAsync_Should_Throw_When_NameIsNotSaas()
    {
        var provider = new SaasTemplateProvider();
        await Assert.ThrowsAsync<TemplateNotFoundException>(() => provider.GetManifestAsync("notsaas"));
    }

    /// <summary>
    /// Verifies that <see cref="SaasTemplateProvider.GetFilesAsync"/> returns a non-empty
    /// dictionary containing the expected template files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_ReturnNonEmptyFiles()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");
        var files = await provider.GetFilesAsync(manifest);

        Assert.NotEmpty(files);
        Assert.Contains("app/Program.cs", files.Keys);
    }

    /// <summary>
    /// Verifies that the BillingService file is included in the template files.
    /// </summary>
    [Fact]
    public async Task GetFilesAsync_Should_IncludeBillingService()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");
        var files = await provider.GetFilesAsync(manifest);
        Assert.Contains("app/Billing/BillingService.cs", files.Keys);
    }

    /// <summary>
    /// Verifies that manifests tags contain expected values.
    /// </summary>
    [Fact]
    public async Task Manifest_Should_HaveExpectedTags()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Tags);
        Assert.Contains("saas", manifest.Tags);
        Assert.Contains("multi-tenant", manifest.Tags);
        Assert.Contains("auth", manifest.Tags);
        Assert.Contains("billing", manifest.Tags);
    }

    /// <summary>
    /// Verifies manifest contains the expected template variables.
    /// </summary>
    [Fact]
    public async Task Manifest_Should_HaveExpectedVariables()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Variables);
        Assert.NotEmpty(manifest.Variables);
        Assert.Contains(manifest.Variables, v => v.Name == "projectName" && v.Required);
        Assert.Contains(manifest.Variables, v => v.Name == "database");
        Assert.Contains(manifest.Variables, v => v.Name == "includeBilling");
    }

    /// <summary>
    /// Verifies the billing feature is declared in the manifest.
    /// </summary>
    [Fact]
    public async Task Manifest_Should_HaveBillingFeature()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Features);
        Assert.Contains(manifest.Features, f => f.Name == "billing");
    }

    /// <summary>
    /// Verifies that billing file has a conditional and all core files are always present.
    /// </summary>
    [Fact]
    public async Task Manifest_CoreFiles_Should_BeAlwaysPresent()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Files);
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Program.cs" && string.IsNullOrEmpty(f.Condition));
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Models/User.cs" && string.IsNullOrEmpty(f.Condition));
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Models/Organization.cs" && string.IsNullOrEmpty(f.Condition));
        Assert.Contains(manifest.Files, f => f.SourcePath == "app/Models/Membership.cs" && string.IsNullOrEmpty(f.Condition));
    }

    /// <summary>
    /// Verifies that billing file has a condition.
    /// </summary>
    [Fact]
    public async Task Manifest_BillingService_Should_HaveCondition()
    {
        var provider = new SaasTemplateProvider();
        var manifest = await provider.GetManifestAsync("saas");

        Assert.NotNull(manifest.Files);
        var billingFile = manifest.Files.FirstOrDefault(f => f.SourcePath == "app/Billing/BillingService.cs");
        Assert.NotNull(billingFile);
        Assert.Equal("includeBilling == true", billingFile.Condition);
    }
}
