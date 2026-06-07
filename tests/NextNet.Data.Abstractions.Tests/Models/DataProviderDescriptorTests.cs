using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Models;

public class DataProviderDescriptorTests
{
    [Fact]
    public void Constructor_Should_SetAllProperties()
    {
        // Arrange
        var providerType = typeof(IDataProvider);
        var connections = new[] { "Default", "Analytics" };
        var lastCheck = new DateTime(2026, 6, 6, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var descriptor = new DataProviderDescriptor(
            "EntityFramework",
            providerType,
            connections,
            true,
            lastCheck);

        // Assert
        Assert.Equal("EntityFramework", descriptor.Name);
        Assert.Same(providerType, descriptor.ProviderType);
        Assert.Equal(connections, descriptor.Connections);
        Assert.True(descriptor.IsInitialized);
        Assert.Equal(lastCheck, descriptor.LastHealthCheck);
    }

    [Fact]
    public void Constructor_Should_UseDefaultValues()
    {
        // Arrange & Act
        var descriptor = new DataProviderDescriptor(
            "Dapper",
            typeof(IDataProvider),
            Array.Empty<string>());

        // Assert
        Assert.Equal("Dapper", descriptor.Name);
        Assert.False(descriptor.IsInitialized);
        Assert.Null(descriptor.LastHealthCheck);
    }
}
