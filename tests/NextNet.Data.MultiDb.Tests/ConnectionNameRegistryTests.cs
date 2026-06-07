using NextNet.Data.MultiDb.Internal;

namespace NextNet.Data.MultiDb.Tests;

public class ConnectionNameRegistryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_Store_When_NewName()
    {
        var registry = new ConnectionNameRegistry();
        var registration = new ConnectionRegistration(
            ConnectionName: "Analytics",
            ProviderName: "Dapper",
            ConnectionString: "Host=...",
            ProviderType: typeof(FakeDataProvider));

        registry.Register("Analytics", registration);

        Assert.True(registry.Exists("Analytics"));
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_Overwrite_When_DuplicateName()
    {
        var registry = new ConnectionNameRegistry();
        var reg1 = new ConnectionRegistration(
            ConnectionName: "Test", ProviderName: "ProviderA",
            ConnectionString: "cs1", ProviderType: typeof(FakeDataProvider));
        var reg2 = new ConnectionRegistration(
            ConnectionName: "Test", ProviderName: "ProviderB",
            ConnectionString: "cs2", ProviderType: typeof(FakeDataProvider));

        registry.Register("Test", reg1);
        registry.Register("Test", reg2);

        Assert.True(registry.TryGet("Test", out var result));
        Assert.Equal("ProviderB", result.ProviderName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TryGet_Should_ReturnTrue_When_Exists()
    {
        var registry = new ConnectionNameRegistry();
        registry.Register("Primary", new ConnectionRegistration(
            ConnectionName: "Primary", ProviderName: "EF",
            ConnectionString: "cs", ProviderType: typeof(FakeDataProvider)));

        var found = registry.TryGet("Primary", out var registration);

        Assert.True(found);
        Assert.NotNull(registration);
        Assert.Equal("Primary", registration.ConnectionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TryGet_Should_ReturnFalse_When_Missing()
    {
        var registry = new ConnectionNameRegistry();

        var found = registry.TryGet("NonExistent", out var registration);

        Assert.False(found);
        Assert.Null(registration);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Exists_Should_ReturnTrue_ForRegisteredName()
    {
        var registry = new ConnectionNameRegistry();
        registry.Register("Logging", new ConnectionRegistration(
            ConnectionName: "Logging", ProviderName: "EF",
            ConnectionString: "cs", ProviderType: typeof(FakeDataProvider)));

        Assert.True(registry.Exists("Logging"));
        Assert.False(registry.Exists("Unknown"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Names_Should_ReturnAllKeys()
    {
        var registry = new ConnectionNameRegistry();
        registry.Register("A", new ConnectionRegistration("A", "P1", "cs1", typeof(FakeDataProvider)));
        registry.Register("B", new ConnectionRegistration("B", "P2", "cs2", typeof(FakeDataProvider)));

        var names = registry.Names;

        Assert.Equal(2, names.Count);
        Assert.Contains("A", names);
        Assert.Contains("B", names);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Names_Should_BeCaseInsensitive()
    {
        var registry = new ConnectionNameRegistry();
        registry.Register("analytics", new ConnectionRegistration(
            "analytics", "Dapper", "cs", typeof(FakeDataProvider)));

        Assert.True(registry.Exists("Analytics"));
        Assert.True(registry.Exists("ANALYTICS"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Count_Should_ReturnZero_When_Empty()
    {
        var registry = new ConnectionNameRegistry();
        Assert.Equal(0, registry.Count);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_Throw_When_NameIsNull()
    {
        var registry = new ConnectionNameRegistry();
        var reg = new ConnectionRegistration(
            "Test", "P", "cs", typeof(FakeDataProvider));

        Assert.Throws<ArgumentNullException>(() => registry.Register(null!, reg));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Register_Should_Throw_When_RegistrationIsNull()
    {
        var registry = new ConnectionNameRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.Register("Test", null!));
    }
}
