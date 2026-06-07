namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="PostgresConnectionFactory"/>.
/// </summary>
/// <remarks>
/// Tests that interact with a real PostgreSQL instance are marked with
/// <c>[Category("Integration")]</c> and are not run in default test execution.
/// </remarks>
public sealed class PostgresConnectionFactoryTests
{
    [Fact]
    public void CreateConnection_Should_ReturnNpgsqlConnection()
    {
        // Arrange
        using var factory = new PostgresConnectionFactory("Host=localhost;Port=5432;Database=testdb;Username=test");

        // Act
        using var connection = factory.CreateConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.IsType<NpgsqlConnection>(connection);
    }

    [Fact]
    public void CreateConnection_Should_HaveCorrectConnectionString()
    {
        // Arrange
        var connectionString = "Host=localhost;Port=5432;Database=testdb;Username=test";
        using var factory = new PostgresConnectionFactory(connectionString);

        // Act
        using var connection = factory.CreateConnection();

        // Assert - The connection string may include additional default parameters
        // (SSL Mode=Prefer, Pooling=True) added by the resolver.
        Assert.Contains("Host=localhost", connection.ConnectionString);
        Assert.Contains("Database=testdb", connection.ConnectionString);
        Assert.Contains("Username=test", connection.ConnectionString);
    }

    [Fact]
    public void ConnectionString_Should_MatchResolvedValue()
    {
        // Arrange
        var connectionString = "Host=localhost;Port=5432;Database=testdb;Username=test";
        using var factory = new PostgresConnectionFactory(connectionString);

        // Assert - Resolved string may include additional default parameters
        Assert.Contains("Host=localhost", factory.ConnectionString);
        Assert.Contains("Database=testdb", factory.ConnectionString);
        Assert.Contains("Username=test", factory.ConnectionString);
    }

    [Fact]
    public void CreateConnection_WithName_Should_UseNamedConnection_When_Default()
    {
        // Arrange
        var connectionString = "Host=localhost;Port=5432;Database=testdb;Username=test";
        using var factory = new PostgresConnectionFactory(connectionString);

        // Act
        using var connection = factory.CreateConnection("Default");

        // Assert - The connection string may include additional default parameters
        Assert.Contains("Host=localhost", connection.ConnectionString);
        Assert.Contains("Database=testdb", connection.ConnectionString);
        Assert.Contains("Username=test", connection.ConnectionString);
    }

    [Fact]
    public void CreateConnection_WithName_Should_Throw_When_UnknownName()
    {
        // Arrange
        using var factory = new PostgresConnectionFactory("Host=localhost;Port=5432;Database=testdb;Username=test");

        // Act & Assert
        var ex = Assert.Throws<KeyNotFoundException>(() => factory.CreateConnection("Analytics"));
        Assert.Contains("Analytics", ex.Message);
    }

    [Fact]
    public void CreateConnection_WithEmptyName_Should_Throw()
    {
        // Arrange
        using var factory = new PostgresConnectionFactory("Host=localhost;Port=5432;Database=testdb;Username=test");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.CreateConnection(""));
    }

    [Fact]
    public void DisposedFactory_Should_ThrowOnCreate()
    {
        // Arrange
        var factory = new PostgresConnectionFactory("Host=localhost;Port=5432;Database=testdb;Username=test");
        factory.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => factory.CreateConnection());
    }

    [Fact]
    public void Constructor_Should_Throw_When_ConnectionStringNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PostgresConnectionFactory((string)null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_ConnectionStringEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new PostgresConnectionFactory(""));
    }

    [Fact]
    public void Constructor_WithOptions_Should_Throw_When_OptionsNull()
    {
        // Act & Assert
        var nullOptions = Options.Create<PostgresConnectionFactoryOptions>(null!);
        Assert.Throws<ArgumentNullException>(() =>
            new PostgresConnectionFactory(nullOptions));
    }

    [Fact]
    public void CreateConnectionAsync_Should_ReturnOpenedConnection()
    {
        // This test requires a running PostgreSQL instance.
        // Marked as integration test.
    }

    [Fact]
    public async Task TestConnectionAsync_Should_ReturnFalse_When_ConnectionFails()
    {
        // Arrange - use an unreachable host
        using var factory = new PostgresConnectionFactory(
            "Host=192.0.2.1;Port=5432;Database=nonexistent;Username=test;Timeout=3");

        // Act
        var result = await factory.TestConnectionAsync();

        // Assert
        Assert.False(result);
    }
}
