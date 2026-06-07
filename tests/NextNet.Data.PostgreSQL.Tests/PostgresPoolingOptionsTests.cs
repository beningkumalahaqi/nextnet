namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="PostgresPoolingOptions"/> default values and configuration.
/// </summary>
public sealed class PostgresPoolingOptionsTests
{
    [Fact]
    public void Defaults_Should_BeEnabled_When_NotSet()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.True(options.Enabled);
    }

    [Fact]
    public void Defaults_Should_HaveMinPoolSize1()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.Equal(1, options.MinPoolSize);
    }

    [Fact]
    public void Defaults_Should_HaveMaxPoolSize100()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.Equal(100, options.MaxPoolSize);
    }

    [Fact]
    public void Defaults_Should_HaveConnectionLifetime300()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.Equal(300, options.ConnectionLifetimeSeconds);
    }

    [Fact]
    public void Defaults_Should_HaveIdleLifetime60()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.Equal(60, options.ConnectionIdleLifetimeSeconds);
    }

    [Fact]
    public void Defaults_Should_HaveRetryOnFailure()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.True(options.RetryOnFailure);
    }

    [Fact]
    public void Defaults_Should_NotValidateOnRetrieve()
    {
        // Arrange
        var options = new PostgresPoolingOptions();

        // Assert
        Assert.False(options.ValidateOnRetrieve);
    }

    [Fact]
    public void Disabled_Should_SetEnabledFalse()
    {
        // Arrange
        var options = new PostgresPoolingOptions { Enabled = false };

        // Assert
        Assert.False(options.Enabled);
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var options = new PostgresPoolingOptions
        {
            MinPoolSize = 10,
            MaxPoolSize = 200,
            ConnectionLifetimeSeconds = 600,
            ConnectionIdleLifetimeSeconds = 120,
            RetryOnFailure = false,
            ValidateOnRetrieve = true
        };

        // Assert
        Assert.Equal(10, options.MinPoolSize);
        Assert.Equal(200, options.MaxPoolSize);
        Assert.Equal(600, options.ConnectionLifetimeSeconds);
        Assert.Equal(120, options.ConnectionIdleLifetimeSeconds);
        Assert.False(options.RetryOnFailure);
        Assert.True(options.ValidateOnRetrieve);
    }
}
