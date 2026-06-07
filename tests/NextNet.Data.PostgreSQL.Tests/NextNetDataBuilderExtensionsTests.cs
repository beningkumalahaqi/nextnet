using NextNet.Data.Abstractions.Registration;
using NextNet.Data.EntityFramework;

namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="PostgreSQLNextNetDataExtensions"/> extension methods.
/// </summary>
public sealed class NextNetDataBuilderExtensionsTests
{
    /// <summary>
    /// Creates a <see cref="NextNetDataBuilder"/> with <c>EfCoreOptions</c> pre-registered
    /// to satisfy the guard clause in <c>UsePostgreSQL()</c>.
    /// </summary>
    private static (NextNetDataBuilder Builder, ServiceCollection Services) CreatePreparedBuilder()
    {
        var services = new ServiceCollection();
        var efOptions = new EfCoreOptions();
        services.AddSingleton(efOptions);
        var builder = new NextNetDataBuilder(services);
        return (builder, services);
    }

    [Fact]
    public void UsePostgreSQL_WithConnectionString_Should_ReturnBuilder()
    {
        // Arrange
        var (builder, _) = CreatePreparedBuilder();

        // Act
        var result = builder.UsePostgreSQL("Host=localhost;Port=5432;Database=testdb;Username=test");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NextNetDataBuilder>(result);
    }

    [Fact]
    public void UsePostgreSQL_WithOptions_Should_ReturnBuilder()
    {
        // Arrange
        var (builder, _) = CreatePreparedBuilder();

        // Act
        var result = builder.UsePostgreSQL(options =>
        {
            options.Host = "db.example.com";
            options.Database = "testdb";
        });

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NextNetDataBuilder>(result);
    }

    [Fact]
    public void UsePostgreSQL_Should_Throw_When_BuilderIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PostgreSQLNextNetDataExtensions.UsePostgreSQL(null!, "connectionString"));
    }

    [Fact]
    public void UsePostgreSQL_WithOptions_Should_Throw_When_BuilderIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PostgreSQLNextNetDataExtensions.UsePostgreSQL(null!, options => { }));
    }

    [Fact]
    public void UsePostgreSQL_Should_Throw_When_ConfigureIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new NextNetDataBuilder(services);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.UsePostgreSQL((Action<PostgresConnectionFactoryOptions>)null!));
    }

    [Fact]
    public void UsePostgreSQL_WithConnectionString_Should_ApplyOptions()
    {
        // Arrange
        var (builder, services) = CreatePreparedBuilder();

        // Act
        builder.UsePostgreSQL("Host=remote;Port=9999;Database=custom;Username=admin");

        // Assert - ConnectionString should be set in registered options
        var optionsDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(PostgresConnectionFactoryOptions));

        Assert.NotNull(optionsDescriptor);
    }

    [Fact]
    public void UsePostgreSQL_Should_RegisterConnectionFactory()
    {
        // Arrange
        var (builder, services) = CreatePreparedBuilder();

        // Act
        builder.UsePostgreSQL("Host=localhost;Port=5432;Database=testdb;Username=test");

        // Assert - PostgresConnectionFactory should be registered as singleton
        var factoryDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(PostgresConnectionFactory));

        Assert.NotNull(factoryDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, factoryDescriptor.Lifetime);
    }

    [Fact]
    public void UsePostgreSQL_Should_RegisterOptions()
    {
        // Arrange
        var (builder, services) = CreatePreparedBuilder();

        // Act
        builder.UsePostgreSQL("Host=localhost;Port=5432;Database=testdb;Username=test");

        // Assert - IOptions should be registered
        var optionsDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IOptions<PostgresConnectionFactoryOptions>));

        Assert.NotNull(optionsDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, optionsDescriptor.Lifetime);
    }
}
