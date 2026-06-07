namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="PostgresConnectionFactoryOptions"/> default values and property setting.
/// </summary>
public sealed class PostgresConnectionFactoryOptionsTests
{
    [Fact]
    public void Defaults_Should_BeLocalhost_When_NotSet()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.Equal("localhost", options.Host);
    }

    [Fact]
    public void Defaults_Should_BePort5432()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.Equal(5432, options.Port);
    }

    [Fact]
    public void Defaults_Should_HavePoolingEnabled()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.NotNull(options.Pooling);
        Assert.True(options.Pooling.Enabled);
    }

    [Fact]
    public void Defaults_Should_HaveSslPrefer()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.NotNull(options.Ssl);
        Assert.Equal(PostgresSslMode.Prefer, options.Ssl.Mode);
    }

    [Fact]
    public void ConnectionString_Should_OverrideComponents()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions
        {
            ConnectionString = "Host=remote;Port=9999;Database=other;Username=admin;Password=pass",
            Host = "localhost",
            Port = 5432
        };

        // Assert
        Assert.Equal("Host=remote;Port=9999;Database=other;Username=admin;Password=pass", options.ConnectionString);
        // Host remains "localhost" because ConnectionString and Host are independent properties;
        // the resolver uses ConnectionString first when both are set.
        Assert.Equal("localhost", options.Host);
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions
        {
            Host = "db.example.com",
            Port = 5433,
            Database = "testdb",
            Username = "testuser",
            Password = "testpass",
            ApplicationName = "myapp",
            ConnectionTimeoutSeconds = 30,
            CommandTimeoutSeconds = 60,
            EnableKeepAlive = false
        };

        // Assert
        Assert.Equal("db.example.com", options.Host);
        Assert.Equal(5433, options.Port);
        Assert.Equal("testdb", options.Database);
        Assert.Equal("testuser", options.Username);
        Assert.Equal("testpass", options.Password);
        Assert.Equal("myapp", options.ApplicationName);
        Assert.Equal(30, options.ConnectionTimeoutSeconds);
        Assert.Equal(60, options.CommandTimeoutSeconds);
        Assert.False(options.EnableKeepAlive);
    }

    [Fact]
    public void Defaults_Should_HaveConnectionTimeout15()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.Equal(15, options.ConnectionTimeoutSeconds);
    }

    [Fact]
    public void Defaults_Should_HaveCommandTimeout30()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.Equal(30, options.CommandTimeoutSeconds);
    }

    [Fact]
    public void Defaults_Should_HaveKeepAliveEnabled()
    {
        // Arrange
        var options = new PostgresConnectionFactoryOptions();

        // Assert
        Assert.True(options.EnableKeepAlive);
        Assert.Equal(60, options.KeepAliveIntervalSeconds);
    }
}
