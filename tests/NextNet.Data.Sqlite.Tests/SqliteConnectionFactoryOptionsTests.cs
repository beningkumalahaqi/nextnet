namespace NextNet.Data.Sqlite.Tests;

/// <summary>
/// Tests for <see cref="SqliteConnectionFactoryOptions"/> default values and property setting.
/// </summary>
public sealed class SqliteConnectionFactoryOptionsTests
{
    [Fact]
    public void Defaults_Should_BeReadWriteCreate_When_NotSet()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions();

        // Assert
        Assert.Equal(SqliteOpenMode.ReadWriteCreate, options.Mode);
    }

    [Fact]
    public void Defaults_Should_EnsureDatabaseCreated_When_NotSet()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions();

        // Assert
        Assert.True(options.EnsureDatabaseCreated);
    }

    [Fact]
    public void Defaults_Should_CacheBeDefault_When_NotSet()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions();

        // Assert
        Assert.Equal(SqliteCacheMode.Default, options.Cache);
    }

    [Fact]
    public void Defaults_Should_InMemoryBeFalse_When_NotSet()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions();

        // Assert
        Assert.False(options.InMemory);
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var options = new SqliteConnectionFactoryOptions();

        // Act
        options.ConnectionString = "Data Source=custom.db";
        options.DataSource = "custom.db";
        options.Mode = SqliteOpenMode.ReadOnly;
        options.InMemory = true;
        options.Cache = SqliteCacheMode.Shared;
        options.EnsureDatabaseCreated = false;

        // Assert
        Assert.Equal("Data Source=custom.db", options.ConnectionString);
        Assert.Equal("custom.db", options.DataSource);
        Assert.Equal(SqliteOpenMode.ReadOnly, options.Mode);
        Assert.True(options.InMemory);
        Assert.Equal(SqliteCacheMode.Shared, options.Cache);
        Assert.False(options.EnsureDatabaseCreated);
    }
}
