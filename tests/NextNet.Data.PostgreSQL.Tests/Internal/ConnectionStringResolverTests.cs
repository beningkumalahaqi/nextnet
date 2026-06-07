using NextNet.Data.PostgreSQL.Internal;

namespace NextNet.Data.PostgreSQL.Tests.Internal;

/// <summary>
/// Tests for <see cref="ConnectionStringResolver"/> resolution priority chain.
/// </summary>
public sealed class ConnectionStringResolverTests : IDisposable
{
    private readonly EnvironmentVariableFixture _envFixture = new();

    [Fact]
    public void Resolve_Should_ReturnExplicitConnectionString_When_Provided()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options, "Host=explicit;Database=test");

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Host=explicit", result);
        Assert.Contains("Database=test", result);
    }

    [Fact]
    public void Resolve_Should_UseOptionsConnectionString_When_NoExplicit()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions
        {
            ConnectionString = "Host=options;Database=testdb;Username=admin"
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Host=options", result);
        Assert.Contains("Database=testdb", result);
    }

    [Fact]
    public void Resolve_Should_BuildFromComponents_When_NoConnectionString()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions
        {
            Database = "testdb",
            Username = "admin",
            Password = "secret"
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Host=localhost", result);
        Assert.Contains("Port=5432", result);
        Assert.Contains("Database=testdb", result);
        Assert.Contains("Username=admin", result);
        Assert.Contains("Password=secret", result);
    }

    [Fact]
    public void Resolve_Should_ReadEnvironmentVariables_When_NoOptions()
    {
        // Arrange
        _envFixture.SetVariable("PGHOST", "envhost");
        _envFixture.SetVariable("PGDATABASE", "envdb");
        _envFixture.SetVariable("PGUSER", "envuser");
        _envFixture.SetVariable("PGPASSWORD", "envpass");

        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);
        resolver.ResetCache();

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Host=envhost", result);
        Assert.Contains("Database=envdb", result);
        Assert.Contains("Username=envuser", result);
        Assert.Contains("Password=envpass", result);
    }

    [Fact]
    public void Resolve_Should_FallbackToDefault_When_NothingConfigured()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Host=localhost", result);
        Assert.Contains("Port=5432", result);
        Assert.Contains("Username=postgres", result);
    }

    [Fact]
    public void Resolve_Should_UsePghost_When_Set()
    {
        // Arrange
        _envFixture.SetVariable("PGHOST", "pg.example.com");

        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);
        resolver.ResetCache();

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Host=pg.example.com", result);
    }

    [Fact]
    public void Resolve_Should_UsePgpassword_When_Set()
    {
        // Arrange
        _envFixture.SetVariable("PGPASSWORD", "supersecret");

        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);
        resolver.ResetCache();

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Password=supersecret", result);
    }

    [Fact]
    public void Resolve_Should_UsePgport_When_Set()
    {
        // Arrange
        _envFixture.SetVariable("PGPORT", "5433");

        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);
        resolver.ResetCache();

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("Port=5433", result);
    }

    [Fact]
    public void Resolve_Should_UsePgsslmode_When_Set()
    {
        // Arrange
        _envFixture.SetVariable("PGSSLMODE", "Require");

        var options = new PostgresConnectionFactoryOptions();
        var resolver = new ConnectionStringResolver(options);
        resolver.ResetCache();

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("SSL Mode=Require", result) ;
    }

    [Fact]
    public void Resolve_Should_IncludeSslAndPooling_When_BuildingFromComponents()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions
        {
            Database = "testdb",
            Username = "admin",
            Ssl = { Mode = PostgresSslMode.Require },
            Pooling = new PostgresPoolingOptions { MaxPoolSize = 50 }
        };
        var resolver = new ConnectionStringResolver(options);

        // Act
        var result = resolver.Resolve();

        // Assert
        Assert.Contains("SSL Mode=Require", result);
        Assert.Contains("Maximum Pool Size=50", result);
    }

    public void Dispose()
    {
        _envFixture.Dispose();
    }
}
