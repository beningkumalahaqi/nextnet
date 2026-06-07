using NextNet.Cli.Commands.Data;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

/// <summary>
/// Tests for the <see cref="AdminSchemaProviderRegistry"/> class.
/// </summary>
public class AdminSchemaProviderRegistryTests
{
    [Fact]
    public void GetProvider_WithEfKey_ReturnsEfSchemaProvider()
    {
        var provider = AdminSchemaProviderRegistry.GetProvider("ef");
        Assert.NotNull(provider);
        Assert.Equal("EntityFramework", provider.ProviderName);
    }

    [Fact]
    public void GetProvider_WithEntityFrameworkKey_ReturnsEfSchemaProvider()
    {
        var provider = AdminSchemaProviderRegistry.GetProvider("entityframework");
        Assert.NotNull(provider);
        Assert.Equal("EntityFramework", provider.ProviderName);
    }

    [Fact]
    public void GetProvider_WithDapperKey_ReturnsDapperSchemaProvider()
    {
        var provider = AdminSchemaProviderRegistry.GetProvider("dapper");
        Assert.NotNull(provider);
    }

    [Fact]
    public void GetProvider_WithNullKey_ReturnsNull()
    {
        var provider = AdminSchemaProviderRegistry.GetProvider(null);
        Assert.Null(provider);
    }

    [Fact]
    public void GetProvider_WithUnknownKey_ReturnsNull()
    {
        var provider = AdminSchemaProviderRegistry.GetProvider("unknown");
        Assert.Null(provider);
    }

    [Fact]
    public void GetProvider_IsCaseInsensitive()
    {
        var provider = AdminSchemaProviderRegistry.GetProvider("EF");
        Assert.NotNull(provider);
    }

    [Fact]
    public void GetSupportedProviderKeys_ReturnsKnownKeys()
    {
        var keys = AdminSchemaProviderRegistry.GetSupportedProviderKeys().ToList();
        Assert.Contains("ef", keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("dapper", keys, StringComparer.OrdinalIgnoreCase);
    }
}
