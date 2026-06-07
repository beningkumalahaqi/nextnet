namespace NextNet.Data.PostgreSQL.Tests;

/// <summary>
/// Tests for <see cref="PostgresSslOptions"/> and <see cref="PostgresSslMode"/> enum.
/// </summary>
public sealed class PostgresSslOptionsTests
{
    [Fact]
    public void Defaults_Should_BePrefer_When_NotSet()
    {
        // Arrange
        var options = new PostgresSslOptions();

        // Assert
        Assert.Equal(PostgresSslMode.Prefer, options.Mode);
    }

    [Fact]
    public void Defaults_Should_NotTrustServerCertificate()
    {
        // Arrange
        var options = new PostgresSslOptions();

        // Assert
        Assert.False(options.TrustServerCertificate);
    }

    [Fact]
    public void SslMode_Values_Should_MatchPostgresConventions()
    {
        // Assert enum values follow PostgreSQL sslmode convention
        Assert.Equal(0, (int)PostgresSslMode.Disable);
        Assert.Equal(1, (int)PostgresSslMode.Allow);
        Assert.Equal(2, (int)PostgresSslMode.Prefer);
        Assert.Equal(3, (int)PostgresSslMode.Require);
        Assert.Equal(4, (int)PostgresSslMode.VerifyCa);
        Assert.Equal(5, (int)PostgresSslMode.VerifyFull);
    }

    [Fact]
    public void ClientCertificate_Should_BeSettable()
    {
        // Arrange
        var options = new PostgresSslOptions
        {
            ClientCertificatePath = "/certs/client.pfx",
            ClientCertificatePassword = "certpass"
        };

        // Assert
        Assert.Equal("/certs/client.pfx", options.ClientCertificatePath);
        Assert.Equal("certpass", options.ClientCertificatePassword);
    }

    [Fact]
    public void RootCertificate_Should_BeSettable()
    {
        // Arrange
        var options = new PostgresSslOptions
        {
            RootCertificatePath = "/certs/ca.pem"
        };

        // Assert
        Assert.Equal("/certs/ca.pem", options.RootCertificatePath);
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        // Arrange
        var options = new PostgresSslOptions
        {
            Mode = PostgresSslMode.VerifyFull,
            TrustServerCertificate = true,
            ClientCertificatePath = "/certs/client.p12",
            ClientCertificatePassword = "secret",
            RootCertificatePath = "/certs/root.pem"
        };

        // Assert
        Assert.Equal(PostgresSslMode.VerifyFull, options.Mode);
        Assert.True(options.TrustServerCertificate);
        Assert.Equal("/certs/client.p12", options.ClientCertificatePath);
        Assert.Equal("secret", options.ClientCertificatePassword);
        Assert.Equal("/certs/root.pem", options.RootCertificatePath);
    }

    [Fact]
    public void Mode_Should_MapFromNpgsqlSslMode()
    {
        // Verify that our SSL mode enum values match Npgsql's SslMode enum.
        // Note: Npgsql 8.x uses VerifyCA (capital CA), while our enum follows
        // a consistent PascalCase convention (VerifyCa).
        Assert.Equal((int)PostgresSslMode.Disable, (int)Npgsql.SslMode.Disable);
        Assert.Equal((int)PostgresSslMode.Allow, (int)Npgsql.SslMode.Allow);
        Assert.Equal((int)PostgresSslMode.Prefer, (int)Npgsql.SslMode.Prefer);
        Assert.Equal((int)PostgresSslMode.Require, (int)Npgsql.SslMode.Require);
        Assert.Equal((int)PostgresSslMode.VerifyCa, (int)Npgsql.SslMode.VerifyCA);
        Assert.Equal((int)PostgresSslMode.VerifyFull, (int)Npgsql.SslMode.VerifyFull);
    }
}
