using NextNet.Cli.Community;
using Xunit;

namespace NextNet.Cli.Tests.Community;

/// <summary>
/// Tests for the <see cref="InstalledTemplateRegistry"/> class.
/// </summary>
public class InstalledTemplateRegistryTests
{
    /// <summary>
    /// Registering a template and then retrieving it by name should return the
    /// same template with the expected version.
    /// </summary>
    [Fact]
    public void Register_Then_Get_Should_ReturnTemplate()
    {
        var registry = new InstalledTemplateRegistry();
        var template = new InstalledTemplate
        {
            Name = "test-template",
            Author = "tester",
            Version = "1.0.0",
            InstallPath = "/tmp/test",
            InstalledAt = DateTime.UtcNow
        };
        registry.Register(template);
        var retrieved = registry.Get("test-template");
        Assert.NotNull(retrieved);
        Assert.Equal("1.0.0", retrieved!.Version);
    }

    /// <summary>
    /// Retrieving a template that has not been registered should return <c>null</c>.
    /// </summary>
    [Fact]
    public void Get_Should_ReturnNull_When_NotFound()
    {
        var registry = new InstalledTemplateRegistry();
        Assert.Null(registry.Get("nonexistent"));
    }

    /// <summary>
    /// Unregistering an existing template should remove it and return <c>true</c>.
    /// </summary>
    [Fact]
    public void Unregister_Should_RemoveTemplate()
    {
        var registry = new InstalledTemplateRegistry();
        registry.Register(new InstalledTemplate { Name = "x", Author = "a", Version = "1.0.0", InstallPath = "/tmp" });
        Assert.True(registry.Unregister("x"));
        Assert.Null(registry.Get("x"));
    }

    /// <summary>
    /// Unregistering a non-existent template should return <c>false</c>.
    /// </summary>
    [Fact]
    public void Unregister_Should_ReturnFalse_When_TemplateNotFound()
    {
        var registry = new InstalledTemplateRegistry();
        Assert.False(registry.Unregister("nonexistent"));
    }

    /// <summary>
    /// Registering a template with a null reference should throw.
    /// </summary>
    [Fact]
    public void Register_Should_Throw_When_TemplateIsNull()
    {
        var registry = new InstalledTemplateRegistry();
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    /// <summary>
    /// The <c>All</c> property should contain any registered templates.
    /// </summary>
    [Fact]
    public void All_Should_ContainRegisteredTemplates()
    {
        var registry = new InstalledTemplateRegistry();
        registry.Register(new InstalledTemplate { Name = "unique-test-a", Author = "x", Version = "1.0.0", InstallPath = "/tmp" });
        registry.Register(new InstalledTemplate { Name = "unique-test-b", Author = "y", Version = "2.0.0", InstallPath = "/tmp" });
        Assert.True(registry.All.ContainsKey("unique-test-a"));
        Assert.True(registry.All.ContainsKey("unique-test-b"));
        // Clean up
        registry.Unregister("unique-test-a");
        registry.Unregister("unique-test-b");
    }
}
