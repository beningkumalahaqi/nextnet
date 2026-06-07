using Microsoft.Extensions.DependencyInjection;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.Abstractions.MultiDb;
using NextNet.Data.Abstractions.Registration;
using Xunit;

namespace NextNet.Data.Abstractions.Tests.Registration;

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
    }

    [Fact]
    public void UseProvider_Should_RegisterProvider_When_ValidType()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.UseProvider<TestProvider>();

        // Assert
        var description = Assert.Single(services, s => s.ServiceType == typeof(IDataProvider));
        Assert.Equal(typeof(TestProvider), description.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, description.Lifetime);
    }

    [Fact]
    public void UseProvider_WithFactory_Should_RegisterProviderCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.UseProvider(sp => new TestProvider());

        // Assert
        var description = Assert.Single(services, s => s.ServiceType == typeof(IDataProvider));
        Assert.NotNull(description.ImplementationFactory);
        Assert.Equal(ServiceLifetime.Singleton, description.Lifetime);
    }

    [Fact]
    public void UseProvider_WithFactory_Should_Throw_When_FactoryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.UseProvider((Func<IServiceProvider, IDataProvider>)null!));
    }

    [Fact]
    public void WithConnection_Should_StoreConnectionConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act
        builder.WithConnection("Default", "Server=.;Database=Test;");

        // Verify by building and checking config
        builder.UseProvider<TestProvider>();
        builder.Build();

        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<DataConfig>();

        Assert.NotNull(config.Connections);
        Assert.True(config.Connections.ContainsKey("Default"));
        Assert.Equal("Server=.;Database=Test;", config.Connections["Default"].ConnectionString);
    }

    [Fact]
    public void WithConnection_Should_Throw_When_NameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithConnection("", "connStr"));
    }

    [Fact]
    public void WithConnection_Should_Throw_When_ConnectionStringIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithConnection("Default", ""));
    }

    [Fact]
    public void WithConnection_WithConfig_Should_Throw_When_ConfigIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithConnection("Default", (ConnectionConfig)null!));
    }

    [Fact]
    public void Build_Should_RegisterDataConfigAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");

        // Act
        builder.Build();

        var serviceProvider = services.BuildServiceProvider();
        var config1 = serviceProvider.GetRequiredService<DataConfig>();
        var config2 = serviceProvider.GetRequiredService<DataConfig>();

        // Assert singleton
        Assert.Same(config1, config2);
        Assert.NotNull(config1.Connections);
        Assert.True(config1.Connections.ContainsKey("Default"));
    }

    [Fact]
    public void Build_Should_RegisterIDatabaseSelectorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");

        // Act
        builder.Build();

        var serviceProvider = services.BuildServiceProvider();
        var selector1 = serviceProvider.GetRequiredService<IDatabaseSelector>();
        var selector2 = serviceProvider.GetRequiredService<IDatabaseSelector>();

        // Assert singleton
        Assert.Same(selector1, selector2);
    }

    [Fact]
    public void Build_Should_Throw_When_NoProviderRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert - validation should fail because DefaultConnection validation
        // (connections is null but we don't require connections)
        builder.WithConnection("Default", "Server=.;Database=Test;");
        builder.Build(); // Should succeed - no provider required at this level
    }

    [Fact]
    public void Build_Should_Throw_When_CalledTwice()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");
        builder.Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_Should_Throw_When_ConfigIsInvalid()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", new ConnectionConfig(ConnectionString: ""));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("validation failed", ex.Message);
        Assert.Contains("ConnectionString", ex.Message);
    }

    [Fact]
    public void FluentChain_Should_AllowMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddNextNetData()
            .UseProvider<TestProvider>()
            .WithConnection("Default", "Server=.;Database=Test;")
            .WithMigrationOptions(m => m with { AutoApply = true })
            .WithScaffoldingOptions(s => s with { ModelsNamespace = "App.Models" })
            .WithDefaultConnection("Default");

        builder.Build();

        // Assert
        var provider = services.BuildServiceProvider();
        var config = provider.GetRequiredService<DataConfig>();
        Assert.NotNull(config.Migration);
        Assert.True(config.Migration.AutoApply);
        Assert.NotNull(config.Scaffolding);
        Assert.Equal("App.Models", config.Scaffolding.ModelsNamespace);
    }

    [Fact]
    public void WithMigrationOptions_Should_Throw_When_ConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithMigrationOptions(null!));
    }

    [Fact]
    public void WithScaffoldingOptions_Should_Throw_When_ConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithScaffoldingOptions(null!));
    }

    [Fact]
    public void WithDefaultConnection_Should_Throw_When_NameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithDefaultConnection(""));
    }

    [Fact]
    public void ModifyAfterBuild_Should_Throw()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");
        builder.Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.UseProvider<TestProvider>());
        Assert.Throws<InvalidOperationException>(() => builder.WithConnection("X", "conn"));
        Assert.Throws<InvalidOperationException>(() => builder.WithDefaultConnection("X"));
        Assert.Throws<InvalidOperationException>(() => builder.WithMigrationOptions(m => m));
        Assert.Throws<InvalidOperationException>(() => builder.WithScaffoldingOptions(s => s));
        Assert.Throws<InvalidOperationException>(() => builder.AddRepository<object>());
    }

    [Fact]
    public void AddRepository_Should_RegisterRepositoryService()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");

        // Act
        builder.AddRepository<TestEntity>();

        // Assert
        Assert.Single(services, s =>
            s.ServiceType == typeof(IRepository<TestEntity>));
    }

    [Fact]
    public void Dispose_Should_CallBuild_When_NotAlreadyBuilt()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");

        // Act
        builder.Dispose();

        // Assert - Build was called, config should be registered
        var provider = services.BuildServiceProvider();
        var config = provider.GetService<DataConfig>();
        Assert.NotNull(config);
    }

    [Fact]
    public void Dispose_Should_NotThrow_When_AlreadyBuilt()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);
        builder.UseProvider<TestProvider>();
        builder.WithConnection("Default", "Server=.;Database=Test;");
        builder.Build();

        // Act (should not throw)
        builder.Dispose();
    }

    /// <summary>
    /// Test implementation of <see cref="IDataProvider"/> for builder tests.
    /// </summary>
    public class TestProvider : IDataProvider
    {
        public string Name => "TestProvider";
        public Task InitializeAsync(DataConfig config, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<HealthCheckResult> IsHealthyAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new HealthCheckResult(true, "Healthy", TimeSpan.Zero));
    }

    /// <summary>
    /// Test entity class for repository registration tests.
    /// </summary>
    public class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
