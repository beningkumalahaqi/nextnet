using NextNet.Data.EntityFramework;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection.Tests;

/// <summary>
/// Tests for <see cref="NextNetDataBuilderExtensions"/> extension methods on <see cref="NextNetDataBuilder"/>.
/// </summary>
public sealed class NextNetDataBuilderExtensionsTests
{
    [Fact]
    public void UseSqlite_WithConnectionString_Should_RegisterFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework()
               .UseSqlite("Data Source=test.db");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<SqliteConnectionFactory>();
        Assert.NotNull(factory);
        Assert.Contains("test.db", factory.ConnectionString);
    }

    [Fact]
    public void UseSqlite_WithOptions_Should_ApplyConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework()
               .UseSqlite(options =>
               {
                   options.DataSource = "custom.db";
                   options.Cache = SqliteCacheMode.Shared;
               });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<SqliteConnectionFactory>();
        Assert.NotNull(factory);
        Assert.Contains("custom.db", factory.ConnectionString);
        Assert.Contains("Cache=Shared", factory.ConnectionString);
    }

    [Fact]
    public void UseInMemorySqlite_Should_SetInMemoryFlag()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework()
               .UseInMemorySqlite();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<SqliteConnectionFactory>();
        Assert.NotNull(factory);
        Assert.Contains(":memory:", factory.ConnectionString);
    }

    [Fact]
    public void UseSqlite_Should_ReturnBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        var result = builder.UseEntityFramework()
                            .UseSqlite();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NextNetDataBuilder>(result);
    }

    [Fact]
    public void UseSqlite_WithConnectionString_Should_ReturnBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        var result = builder.UseEntityFramework()
                            .UseSqlite("Data Source=test.db");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NextNetDataBuilder>(result);
    }

    [Fact]
    public void UseSqlite_Should_Throw_When_BuilderIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            NextNetDataBuilderExtensions.UseSqlite(null!));
    }

    [Fact]
    public void UseSqlite_WithOptions_Should_Throw_When_BuilderIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            NextNetDataBuilderExtensions.UseSqlite(null!, options => { }));
    }

    [Fact]
    public void UseSqlite_WithOptions_Should_Throw_When_ConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.UseSqlite((Action<SqliteConnectionFactoryOptions>)null!));
    }

    [Fact]
    public void UseSqlite_Should_Throw_When_UseEntityFrameworkNotCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.UseSqlite("Data Source=test.db"));
        Assert.Contains("UseEntityFramework", ex.Message);
    }

    [Fact]
    public void UseSqlite_Should_ConfigureDbContext_WithSqlite()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = CreateBuilder(services);

        // Act
        builder.UseEntityFramework()
               .UseSqlite("Data Source=:memory:");

        // Assert - The EfCoreOptions should have ConfigureDbContext set
        var serviceProvider = services.BuildServiceProvider();
        var efOptions = serviceProvider.GetService<EfCoreOptions>();
        Assert.NotNull(efOptions);
        Assert.NotNull(efOptions.ConfigureDbContext);
    }

    /// <summary>
    /// Creates a <see cref="NextNetDataBuilder"/> using the Providers AddNextNetData.
    /// </summary>
    private static NextNetDataBuilder CreateBuilder(IServiceCollection services)
        => NextNetDataServiceCollectionExtensions.AddNextNetData(services);
}
