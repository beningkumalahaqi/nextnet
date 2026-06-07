using NextNet.Data.Internal;
using Xunit;

namespace NextNet.Data.Providers.Tests;

public class DataProviderRegistryTests
{
    [Fact]
    public void GetAll_Should_ReturnEmpty_When_NoProvidersRegistered()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act
        var providers = registry.GetAll();

        // Assert
        Assert.Empty(providers);
    }

    [Fact]
    public void GetAll_Should_ReturnAllProviders_When_ProvidersAreTracked()
    {
        // Arrange
        var registry = CreateRegistry();
        var provider1 = new FakeProvider("Provider1");
        var provider2 = new FakeProvider("Provider2");

        registry.TrackInstance("Provider1", provider1);
        registry.TrackInstance("Provider2", provider2);

        // Act
        var providers = registry.GetAll();

        // Assert
        Assert.Equal(2, providers.Count);
        Assert.Contains(provider1, providers);
        Assert.Contains(provider2, providers);
    }

    [Fact]
    public void GetByName_Should_ReturnCorrectProvider_When_Exists()
    {
        // Arrange
        var registry = CreateRegistry();
        var provider = new FakeProvider("MyProvider");
        registry.TrackInstance("MyProvider", provider);

        // Act
        var result = registry.GetByName("MyProvider");

        // Assert
        Assert.Same(provider, result);
    }

    [Fact]
    public void GetByName_Should_ReturnNull_When_NotExists()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act
        var result = registry.GetByName("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetByName_Should_BeCaseInsensitive()
    {
        // Arrange
        var registry = CreateRegistry();
        var provider = new FakeProvider("TestProvider");
        registry.TrackInstance("TestProvider", provider);

        // Act
        var result = registry.GetByName("testprovider");

        // Assert
        Assert.Same(provider, result);
    }

    [Fact]
    public void GetDefault_Should_ReturnFirstProvider_When_ProvidersExist()
    {
        // Arrange
        var registry = CreateRegistry();
        var provider1 = new FakeProvider("First");
        var provider2 = new FakeProvider("Second");

        registry.TrackInstance("First", provider1);
        registry.TrackInstance("Second", provider2);

        // Act
        var result = registry.GetDefault();

        // Assert
        Assert.Same(provider1, result);
    }

    [Fact]
    public void GetDefault_Should_ReturnNull_When_NoProviders()
    {
        // Arrange
        var registry = CreateRegistry();

        // Act
        var result = registry.GetDefault();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TrackInstance_Should_Throw_When_DuplicateName()
    {
        // Arrange
        var registry = CreateRegistry();
        registry.TrackInstance("Dup", new FakeProvider("Dup"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => registry.TrackInstance("Dup", new FakeProvider("Dup2")));
    }

    private static DataProviderRegistryImpl CreateRegistry()
    {
        return new DataProviderRegistryImpl();
    }
}
