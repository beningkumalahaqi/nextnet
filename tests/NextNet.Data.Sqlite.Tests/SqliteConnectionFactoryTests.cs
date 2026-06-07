namespace NextNet.Data.Sqlite.Tests;

/// <summary>
/// Tests for <see cref="SqliteConnectionFactory"/>.
/// </summary>
public sealed class SqliteConnectionFactoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture;

    public SqliteConnectionFactoryTests()
    {
        _fixture = new SqliteTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public void CreateConnection_Should_ReturnSqliteConnection()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        using var connection = factory.CreateConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.IsType<SqliteConnection>(connection);
    }

    [Fact]
    public void CreateConnection_Should_HaveCorrectConnectionString()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        using var connection = factory.CreateConnection();

        // Assert
        var expectedPath = Path.GetFullPath(_fixture.DefaultDbPath);
        Assert.Contains(expectedPath, connection.ConnectionString);
    }

    [Fact]
    public async Task CreateConnectionAsync_Should_ReturnOpenedConnection()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        await using var connection = await factory.CreateConnectionAsync();

        // Assert
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }

    [Fact]
    public void CreateConnection_WithName_Should_UseDefault_When_Default()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        using var connection = factory.CreateConnection("Default");

        // Assert
        Assert.NotNull(connection);
        var expectedPath = Path.GetFullPath(_fixture.DefaultDbPath);
        Assert.Contains(expectedPath, connection.ConnectionString);
    }

    [Fact]
    public void CreateConnection_WithUnknownName_Should_Throw()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => factory.CreateConnection("Analytics"));
    }

    [Fact]
    public void DatabaseExists_Should_ReturnTrue_ForInMemory()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            InMemory = true
        };
        var factory = CreateFactory(options);

        // Act
        var exists = factory.DatabaseExists();

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void DatabaseExists_Should_ReturnFalse_When_FileMissing()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        var exists = factory.DatabaseExists();

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task EnsureDatabaseAsync_Should_CreateFile_When_Missing()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        await factory.EnsureDatabaseAsync();

        // Assert
        Assert.True(File.Exists(_fixture.DefaultDbPath));
    }

    [Fact]
    public async Task EnsureDatabaseAsync_Should_BeNoop_ForInMemory()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            InMemory = true
        };
        var factory = CreateFactory(options);

        // Act
        await factory.EnsureDatabaseAsync();

        // Assert
        // No file should be created; just verify no exception was thrown
        Assert.True(factory.DatabaseExists());
    }

    [Fact]
    public async Task EnsureDatabaseAsync_Should_CreateDirectory_When_Missing()
    {
        // Arrange
        var nestedPath = Path.Combine(_fixture.TempDir, "subdir", "nested", "app.db");
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = nestedPath
        };
        var factory = CreateFactory(options);

        // Act
        await factory.EnsureDatabaseAsync();

        // Assert
        Assert.True(Directory.Exists(Path.GetDirectoryName(nestedPath)));
        Assert.True(File.Exists(nestedPath));
    }

    [Fact]
    public void ConnectionString_Should_ReturnResolvedValue()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions
        {
            DataSource = _fixture.DefaultDbPath
        };
        var factory = CreateFactory(options);

        // Act
        var connString = factory.ConnectionString;

        // Assert
        Assert.NotNull(connString);
        Assert.Contains(Path.GetFullPath(_fixture.DefaultDbPath), connString);
    }

    /// <summary>
    /// Creates a <see cref="SqliteConnectionFactory"/> with the specified options.
    /// </summary>
    private static SqliteConnectionFactory CreateFactory(SqliteConnectionFactoryOptions options)
    {
        var optionsWrapper = new OptionsWrapper<SqliteConnectionFactoryOptions>(options);
        return new SqliteConnectionFactory(optionsWrapper);
    }
}
