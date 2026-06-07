namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="PostgresConnectionStringBuilder"/> static helper methods.
/// </summary>
public sealed class PostgresConnectionStringBuilderTests
{
    [Fact]
    public void FromComponents_Should_BuildBasicString_When_MinimalComponents()
    {
        // Act
        var result = PostgresConnectionStringBuilder.FromComponents(
            host: "localhost",
            port: 5432,
            database: "mydb",
            username: "user");

        // Assert
        Assert.Contains("Host=localhost", result);
        Assert.Contains("Port=5432", result);
        Assert.Contains("Database=mydb", result);
        Assert.Contains("Username=user", result);
        Assert.DoesNotContain("Password=", result);
    }

    [Fact]
    public void FromComponents_Should_IncludePassword_When_Provided()
    {
        // Act
        var result = PostgresConnectionStringBuilder.FromComponents(
            host: "localhost",
            port: 5432,
            database: "mydb",
            username: "user",
            password: "secret");

        // Assert
        Assert.Contains("Password=secret", result);
    }

    [Fact]
    public void FromComponents_Should_IncludeSsl_When_Provided()
    {
        // Arrange
        var ssl = new PostgresSslOptions
        {
            Mode = PostgresSslMode.Require
        };

        // Act
        var result = PostgresConnectionStringBuilder.FromComponents(
            host: "localhost",
            port: 5432,
            database: "mydb",
            username: "user",
            ssl: ssl);

        // Assert
        Assert.Contains("SSL Mode=Require", result) ;
    }

    [Fact]
    public void FromComponents_Should_IncludePooling_When_Provided()
    {
        // Arrange
        var pooling = new PostgresPoolingOptions
        {
            MinPoolSize = 5,
            MaxPoolSize = 50
        };

        // Act
        var result = PostgresConnectionStringBuilder.FromComponents(
            host: "localhost",
            port: 5432,
            database: "mydb",
            username: "user",
            pooling: pooling);

        // Assert
        Assert.Contains("Minimum Pool Size=5", result);
        Assert.Contains("Maximum Pool Size=50", result);
    }

    [Fact]
    public void FromComponents_Should_UseNpgsqlBuilder_ForProperEscaping()
    {
        // Arrange
        var password = "pass;word=test%value";

        // Act
        var result = PostgresConnectionStringBuilder.FromComponents(
            host: "localhost",
            port: 5432,
            database: "mydb",
            username: "user",
            password: password);

        // Assert - password should be properly encoded by NpgsqlConnectionStringBuilder
        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal(password, builder.Password);
    }

    [Fact]
    public void ForDocker_Should_UseDefaults()
    {
        // Act
        var result = PostgresConnectionStringBuilder.ForDocker("myapp");

        // Assert
        Assert.Contains("Host=localhost", result);
        Assert.Contains("Port=5432", result);
        Assert.Contains("Database=myapp", result);
        Assert.Contains("Username=postgres", result);
        Assert.Contains("Password=postgres", result);
    }

    [Fact]
    public void ForDocker_Should_UseCustomPort()
    {
        // Act
        var result = PostgresConnectionStringBuilder.ForDocker("myapp", port: 5433);

        // Assert
        Assert.Contains("Port=5433", result);
    }

    [Fact]
    public void ApplySsl_Should_AddSslMode()
    {
        // Arrange
        var baseConnectionString = "Host=localhost;Port=5432;Database=mydb;Username=user";
        var ssl = new PostgresSslOptions { Mode = PostgresSslMode.Require };

        // Act
        var result = PostgresConnectionStringBuilder.ApplySsl(baseConnectionString, ssl);

        // Assert
        Assert.Contains("SSL Mode=Require", result);
    }

    [Fact]
    public void ApplySsl_Should_AddSslCertificate_When_ClientCertificateProvided()
    {
        // Arrange
        var baseConnectionString = "Host=localhost;Port=5432;Database=mydb;Username=user";
        var ssl = new PostgresSslOptions
        {
            Mode = PostgresSslMode.Require,
            ClientCertificatePath = "/certs/client.pfx"
        };

        // Act
        var result = PostgresConnectionStringBuilder.ApplySsl(baseConnectionString, ssl);

        // Assert
        Assert.Contains("SSL Mode=Require", result);
        Assert.Contains("SSL Certificate=/certs/client.pfx", result);
    }

    [Fact]
    public void ApplySsl_Should_Throw_When_ConnectionStringEmpty()
    {
        // Arrange
        var ssl = new PostgresSslOptions();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            PostgresConnectionStringBuilder.ApplySsl("", ssl));
    }

    [Fact]
    public void ApplyPooling_Should_DisablePooling_When_EnabledFalse()
    {
        // Arrange
        var baseConnectionString = "Host=localhost;Port=5432;Database=mydb;Username=user";
        var pooling = new PostgresPoolingOptions { Enabled = false };

        // Act
        var result = PostgresConnectionStringBuilder.ApplyPooling(baseConnectionString, pooling);

        // Assert
        Assert.Contains("Pooling=False", result);
    }

    [Fact]
    public void ApplyPooling_Should_SetPoolSizes()
    {
        // Arrange
        var baseConnectionString = "Host=localhost;Port=5432;Database=mydb;Username=user";
        var pooling = new PostgresPoolingOptions
        {
            MinPoolSize = 5,
            MaxPoolSize = 50
        };

        // Act
        var result = PostgresConnectionStringBuilder.ApplyPooling(baseConnectionString, pooling);

        // Assert
        Assert.Contains("Minimum Pool Size=5", result);
        Assert.Contains("Maximum Pool Size=50", result);
    }

    [Fact]
    public void ApplyPooling_Should_Throw_When_ConnectionStringEmpty()
    {
        // Arrange
        var pooling = new PostgresPoolingOptions();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            PostgresConnectionStringBuilder.ApplyPooling("", pooling));
    }

    [Fact]
    public void ApplySsl_Should_Throw_When_SslNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PostgresConnectionStringBuilder.ApplySsl("Host=localhost", null!));
    }

    [Fact]
    public void ApplyPooling_Should_Throw_When_PoolingNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            PostgresConnectionStringBuilder.ApplyPooling("Host=localhost", null!));
    }
}
