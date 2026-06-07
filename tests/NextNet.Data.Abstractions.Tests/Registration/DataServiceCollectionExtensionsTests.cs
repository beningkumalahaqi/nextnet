using Microsoft.Extensions.DependencyInjection;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.MultiDb;
using NextNet.Data.Abstractions.Registration;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Registration;

public class DataServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetData_Should_ReturnBuilder_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddNextNetData();

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<NextNetDataBuilder>(builder);
    }

    [Fact]
    public void AddNextNetData_Should_Throw_When_ServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetData());
    }

    [Fact]
    public void AddNextNetData_WithConfiguration_Should_ApplyConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetData(builder =>
        {
            builder.UseProvider<NextNetDataBuilderTests.TestProvider>()
                   .WithConnection("Default", "Server=.;Database=Test;")
                   .WithMigrationOptions(m => m with { AutoApply = true });
        });

        // Assert
        Assert.Same(services, result);

        var provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<DataConfig>();
        Assert.NotNull(config);
        Assert.NotNull(config.Connections);
        Assert.True(config.Connections.ContainsKey("Default"));
        Assert.NotNull(config.Migration);
        Assert.True(config.Migration.AutoApply);

        var selector = provider.GetRequiredService<IDatabaseSelector>();
        Assert.NotNull(selector);
    }

    [Fact]
    public void AddNextNetData_WithConfiguration_Should_Throw_When_ServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetData(b => { }));
    }

    [Fact]
    public void AddNextNetData_WithConfiguration_Should_Throw_When_ConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddNextNetData((Action<NextNetDataBuilder>)null!));
    }

    [Fact]
    public void AddNextNetData_Should_IntegrateWithDatabaseSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetData(builder =>
        {
            builder.UseProvider<NextNetDataBuilderTests.TestProvider>()
                   .WithConnection("Default", "Server=.;Database=Test;")
                   .WithConnection("Analytics", "Server=.;Database=Analytics;");
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var selector = provider.GetRequiredService<IDatabaseSelector>();

        var connectionNames = selector.ConnectionNames;
        Assert.Contains("Default", connectionNames);
        Assert.Contains("Analytics", connectionNames);

        var defaultCtx = selector.Default;
        Assert.NotNull(defaultCtx);
        Assert.Equal("Default", defaultCtx.Connection.Name);
    }
}
