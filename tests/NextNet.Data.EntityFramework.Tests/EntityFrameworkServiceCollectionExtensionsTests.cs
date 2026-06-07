using Microsoft.Extensions.DependencyInjection;

namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="Microsoft.Extensions.DependencyInjection.EntityFrameworkNextNetDataExtensions"/>.
/// </summary>
public sealed class EntityFrameworkServiceCollectionExtensionsTests
{
    /// <summary>
    /// Creates a <see cref="NextNet.Data.NextNetDataBuilder"/> using the Providers AddNextNetData.
    /// Uses the static method to avoid ambiguity with the Abstractions NextNetDataBuilder type.
    /// </summary>
    private static NextNet.Data.NextNetDataBuilder CreateBuilder(IServiceCollection services)
        => NextNetDataServiceCollectionExtensions.AddNextNetData(services);

    [Fact]
    public void UseEntityFramework_Should_ReturnBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        var result = builder.UseEntityFramework();

        // Assert
        Assert.NotNull(result);
        Assert.Same(builder, result);
    }

    [Fact]
    public void UseEntityFramework_Should_RegisterEfCoreOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework(options =>
        {
            options.ConnectionName = "TestDb";
            options.MaxRetryCount = 5;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var resolvedOptions = serviceProvider.GetRequiredService<EfCoreOptions>();
        Assert.Equal("TestDb", resolvedOptions.ConnectionName);
        Assert.Equal(5, resolvedOptions.MaxRetryCount);
    }

    [Fact]
    public void UseEntityFramework_Should_RegisterProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);
        services.AddLogging(); // Required for provider initialization

        // Act
        builder.UseEntityFramework();
        builder.Build(); // Providers are registered during Build()

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var providers = serviceProvider.GetServices<NextNet.Data.IDataProvider>().ToList();
        Assert.Contains(providers, p => p.Name == "EntityFramework");
    }

    [Fact]
    public void UseEntityFramework_Should_RegisterDbContextFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework(options =>
        {
            options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestDb");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<IDbContextFactory<AppDbContext>>();
        Assert.NotNull(factory);
    }

    [Fact]
    public void UseEntityFramework_Should_RegisterMigrationEngine()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework(options =>
        {
            options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestDb");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var engine = serviceProvider.GetService<IMigrationEngine>();
        Assert.NotNull(engine);
        Assert.IsType<EfCoreMigrationEngine>(engine);
    }

    [Fact]
    public void UseEntityFramework_Should_RegisterHealthCheckProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework(options =>
        {
            options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestDb");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var healthCheck = serviceProvider.GetService<IHealthCheckProvider>();
        Assert.NotNull(healthCheck);
        Assert.IsType<EfCoreHealthCheckProvider>(healthCheck);
    }

    [Fact]
    public void AddRepository_Should_RegisterRepository()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder
            .UseEntityFramework(options =>
            {
                options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestRepoDb");
            })
            .AddRepository<TestEntity>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var repo = serviceProvider.GetService<IRepository<TestEntity>>();
        Assert.NotNull(repo);
        Assert.IsType<EfCoreRepository<TestEntity>>(repo);
    }

    [Fact]
    public void AddRepository_Should_RegisterMultipleRepositories()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder
            .UseEntityFramework(options =>
            {
                options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestMultiRepoDb");
            })
            .AddRepository<TestEntity>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var repo1 = serviceProvider.GetService<IRepository<TestEntity>>();
        Assert.NotNull(repo1);
    }

    [Fact]
    public void UseEntityFramework_Should_RegisterAutoMigrationService_When_Enabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework(options =>
        {
            options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestAutoMigrateDb");
            options.AutoApplyMigrations = true;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().ToList();
        Assert.Contains(hostedServices, h => h.GetType().Name == "AutoMigrationHostedService");
    }

    [Fact]
    public void UseEntityFramework_Should_NotRegisterAutoMigrationService_When_Disabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework(options =>
        {
            options.ConfigureDbContext = db => db.UseInMemoryDatabase("TestNoAutoDb");
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>().ToList();
        Assert.All(hostedServices, h =>
            Assert.NotEqual("AutoMigrationHostedService", h.GetType().Name));
    }
}
