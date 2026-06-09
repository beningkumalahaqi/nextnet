using NextNet.Data.Exceptions;
using NextNet.Data.Internal;
using NextNet.Data.Providers;
using Xunit;

namespace NextNet.Data.Providers.Tests;

public class NextNetDataBuilderTests
{
    [Fact]
    public void Constructor_Should_Throw_When_ServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new NextNetDataBuilder(null!));
    }

    [Fact]
    public void Constructor_Should_Initialize_When_ServicesProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = new NextNetDataBuilder(services);

        // Assert
        Assert.NotNull(builder);
        Assert.Same(services, builder.Services);
    }

    [Fact]
    public void Services_Should_ReturnSameCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = new NextNetDataBuilder(services);

        // Assert
        Assert.Same(services, builder.Services);
    }

    [Fact]
    public void AddProvider_Should_RegisterDescriptor_When_Valid()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.AddProvider<FakeProvider>("TestProvider");

        // Assert
        // The actual registration happens during Build(), so we verify via build
        builder.Build();
        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IDataProvider>().ToList();
        Assert.Single(providers);
    }

    [Fact]
    public void AddProvider_Should_Throw_When_NameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddProvider<FakeProvider>(""));
    }

    [Fact]
    public void AddProvider_Should_Throw_When_NameIsWhitespace()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.AddProvider<FakeProvider>("   "));
    }

    [Fact]
    public void AddProvider_Should_ThrowProviderRegistrationException_When_DuplicateName()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("TestProvider");

        // Act & Assert
        var ex = Assert.Throws<ProviderRegistrationException>(
            () => builder.AddProvider<FakeProvider>("TestProvider"));
        Assert.Equal(DataProviderErrorCodes.ProviderAlreadyRegistered, ex.ErrorCode);
        Assert.Contains("TestProvider", ex.Message);
    }

    [Fact]
    public void AddProvider_WithOptions_Should_ApplyOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        var optionsApplied = false;

        // Act
        builder.AddProvider<FakeProvider>("TestProvider", opts =>
        {
            opts.ConnectionStringName = "MyConnection";
            opts.RegisterHealthChecks = false;
            optionsApplied = true;
        });

        // Assert
        Assert.True(optionsApplied);
    }

    [Fact]
    public void AddNamedProvider_Should_Register_When_Valid()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.AddNamedProvider<FakeProvider>("Analytics", "Server=.;Database=Analytics;");

        // Assert
        builder.Build();
        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IDataProvider>().ToList();
        Assert.Single(providers);
    }

    [Fact]
    public void AddNamedProvider_Should_Throw_When_NameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => builder.AddNamedProvider<FakeProvider>("", "connStr"));
    }

    [Fact]
    public void AddNamedProvider_Should_Throw_When_ConnectionStringIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => builder.AddNamedProvider<FakeProvider>("Test", ""));
    }

    [Fact]
    public void AddNamedProvider_Should_ThrowProviderRegistrationException_When_DuplicateName()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("TestProvider");

        // Act & Assert
        var ex = Assert.Throws<ProviderRegistrationException>(
            () => builder.AddNamedProvider<FakeProvider>("TestProvider", "connStr"));
        Assert.Equal(DataProviderErrorCodes.ProviderAlreadyRegistered, ex.ErrorCode);
    }

    [Fact]
    public void Build_Should_NotThrow_When_NoRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        builder.Build(); // Should not throw
    }

    [Fact]
    public void Build_Should_RegisterDataAbstractionsOptionsAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.Build();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options1 = serviceProvider.GetRequiredService<DataAbstractionsOptions>();
        var options2 = serviceProvider.GetRequiredService<DataAbstractionsOptions>();
        Assert.Same(options1, options2);
    }

    [Fact]
    public void Build_Should_RegisterHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("Test");

        // Act
        builder.Build();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().ToList();
        Assert.Single(hostedServices);
    }

    [Fact]
    public void Build_Should_RegisterProviderRegistryAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("Test");

        // Act
        builder.Build();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var registry1 = serviceProvider.GetRequiredService<IDataProviderRegistry>();
        var registry2 = serviceProvider.GetRequiredService<IDataProviderRegistry>();
        Assert.Same(registry1, registry2);
    }

    [Fact]
    public void Build_Should_Throw_When_CalledTwice()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("Test");
        builder.Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void ModifyAfterBuild_Should_Throw()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("Test");
        builder.Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => builder.AddProvider<FakeProvider>("Another"));
        Assert.Throws<InvalidOperationException>(
            () => builder.AddNamedProvider<FakeProvider>("Named", "connStr"));
    }

    [Fact]
    public void MultipleProviders_Should_RegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.AddProvider<FakeProvider>("Provider1");
        builder.AddProvider<FakeProvider>("Provider2");
        builder.AddNamedProvider<FakeProvider>("NamedProvider", "connStr");
        builder.Build();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<IDataProvider>().ToList();
        Assert.Equal(3, providers.Count);
    }

    [Fact]
    public void Dispose_Should_CallBuild_When_NotAlreadyBuilt()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("Test");

        // Act
        builder.Dispose();

        // Assert - Build was called, registry should be registered
        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetService<IDataProviderRegistry>();
        Assert.NotNull(registry);
    }

    [Fact]
    public void Dispose_Should_NotThrow_When_AlreadyBuilt()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.AddProvider<FakeProvider>("Test");
        builder.Build();

        // Act (should not throw)
        builder.Dispose();
    }

    [Fact]
    public void Build_Should_SupportDefaultLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.AddProvider<FakeProvider>("Test");
        builder.Build();

        // Assert - providers are registered as singleton by default
        var serviceProvider = services.BuildServiceProvider();
        var provider1 = serviceProvider.GetRequiredService<IDataProvider>();
        var provider2 = serviceProvider.GetRequiredService<IDataProvider>();
        Assert.Same(provider1, provider2);
    }
}
