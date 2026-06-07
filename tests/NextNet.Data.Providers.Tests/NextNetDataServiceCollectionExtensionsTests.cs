using NextNet.Data;
using Xunit;

namespace NextNet.Data.Providers.Tests;

public class NextNetDataServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetData_Should_ReturnBuilder_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = NextNetDataServiceCollectionExtensions.AddNextNetData(services);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<NextNetDataBuilder>(builder);
    }

    [Fact]
    public void AddNextNetData_Should_Throw_When_ServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            NextNetDataServiceCollectionExtensions.AddNextNetData(null!));
    }

    [Fact]
    public void AddNextNetData_WithConfigureAction_Should_ApplyOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var optionsApplied = false;

        // Act
        services.AddNextNetData(options =>
        {
            options.FailOnInitializationError = false;
            optionsApplied = true;
        });

        // Assert
        Assert.True(optionsApplied);
    }

    [Fact]
    public void AddNextNetData_WithConfigureAction_Should_SetFailOnInitializationError()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddNextNetData(options =>
        {
            options.FailOnInitializationError = false;
        });

        // Build and verify the option was passed through
        builder.Build();
        var serviceProvider = services.BuildServiceProvider();
        var resolvedOptions = serviceProvider.GetRequiredService<DataAbstractionsOptions>();

        Assert.False(resolvedOptions.FailOnInitializationError);
    }

    [Fact]
    public void AddNextNetData_WithBuilderAction_Should_ReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = NextNetDataServiceCollectionExtensions.AddNextNetData(
            services,
            builder =>
            {
                builder.AddProvider<FakeProvider>("Test");
            });

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddNextNetData_WithBuilderAction_Should_RegisterProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        NextNetDataServiceCollectionExtensions.AddNextNetData(
            services,
            builder =>
            {
                builder.AddProvider<FakeProvider>("Test");
            });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IDataProvider>().ToList();
        Assert.Single(providers);
    }

    [Fact]
    public void AddNextNetData_WithBuilderAction_Should_Throw_When_ServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            NextNetDataServiceCollectionExtensions.AddNextNetData(
                null!,
                builder => { });
        });
    }

    [Fact]
    public void AddNextNetData_WithBuilderAction_Should_Throw_When_BuilderActionIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            NextNetDataServiceCollectionExtensions.AddNextNetData(
                services,
                (Action<NextNetDataBuilder>)null!);
        });
    }

    [Fact]
    public void AddNextNetData_WithBothActions_Should_ApplyOptionsAndBuild()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetData(
            builder =>
            {
                builder.AddProvider<FakeProvider>("Test");
            },
            options =>
            {
                options.FailOnInitializationError = false;
            });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<DataAbstractionsOptions>();
        Assert.False(options.FailOnInitializationError);

        var providers = serviceProvider.GetServices<IDataProvider>().ToList();
        Assert.Single(providers);
    }
}
