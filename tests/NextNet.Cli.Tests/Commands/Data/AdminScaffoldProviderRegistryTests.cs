using NextNet.Cli.Commands.Data;
using NextNet.Data.Abstractions.Abstractions;
using Xunit;

namespace NextNet.Cli.Tests.Commands.Data;

/// <summary>
/// Tests for the <see cref="AdminScaffoldProviderRegistry"/> class.
/// </summary>
public class AdminScaffoldProviderRegistryTests
{
    [Fact]
    public void GetProvider_WithEfKey_ReturnsEfProvider()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider("ef");
        Assert.NotNull(provider);
        Assert.Equal("EntityFramework", provider.ProviderName);
    }

    [Fact]
    public void GetProvider_WithEntityFrameworkKey_ReturnsEfProvider()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider("entityframework");
        Assert.NotNull(provider);
        Assert.Equal("EntityFramework", provider.ProviderName);
    }

    [Fact]
    public void GetProvider_WithDapperKey_ReturnsDapperProvider()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider("dapper");
        Assert.NotNull(provider);
    }

    [Fact]
    public void GetProvider_WithNullKey_ReturnsNull()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider(null);
        Assert.Null(provider);
    }

    [Fact]
    public void GetProvider_WithUnknownKey_ReturnsNull()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider("unknown");
        Assert.Null(provider);
    }

    [Fact]
    public void GetProvider_IsCaseInsensitive()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider("EF");
        Assert.NotNull(provider);
    }

    [Fact]
    public void GetSupportedProviderKeys_ReturnsKnownKeys()
    {
        var keys = AdminScaffoldProviderRegistry.GetSupportedProviderKeys().ToList();
        Assert.Contains("ef", keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("dapper", keys, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetProvider_EfProvider_ImplementsIAdminScaffoldProvider()
    {
        var provider = AdminScaffoldProviderRegistry.GetProvider("ef");
        Assert.IsAssignableFrom<IAdminScaffoldProvider>(provider);
    }
}
