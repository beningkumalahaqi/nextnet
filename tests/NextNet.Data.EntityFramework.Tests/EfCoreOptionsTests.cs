namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="EfCoreOptions"/> record.
/// </summary>
public sealed class EfCoreOptionsTests
{
    [Fact]
    public void DefaultValues_Should_BeSetCorrectly()
    {
        var options = new EfCoreOptions();

        Assert.Equal("Default", options.ConnectionName);
        Assert.True(options.RegisterHealthChecks);
        Assert.Equal(ServiceLifetime.Scoped, options.RepositoryLifetime);
        Assert.Null(options.AutoApplyMigrations);
        Assert.Equal(3, options.MaxRetryCount);
        Assert.False(options.EnableSensitiveDataLogging);
        Assert.Null(options.ConfigureDbContext);
        Assert.Null(options.EntityConfigurationAssemblies);
    }

    [Fact]
    public void Properties_Should_BeSettable()
    {
        var options = new EfCoreOptions
        {
            ConnectionName = "Analytics",
            RegisterHealthChecks = false,
            RepositoryLifetime = ServiceLifetime.Singleton,
            AutoApplyMigrations = true,
            MaxRetryCount = 5,
            EnableSensitiveDataLogging = true,
            ConfigureDbContext = _ => { }
        };

        Assert.Equal("Analytics", options.ConnectionName);
        Assert.False(options.RegisterHealthChecks);
        Assert.Equal(ServiceLifetime.Singleton, options.RepositoryLifetime);
        Assert.True(options.AutoApplyMigrations);
        Assert.Equal(5, options.MaxRetryCount);
        Assert.True(options.EnableSensitiveDataLogging);
        Assert.NotNull(options.ConfigureDbContext);
    }

    [Fact]
    public void Equality_Should_WorkAsRecord()
    {
        var options1 = new EfCoreOptions { ConnectionName = "Primary", AutoApplyMigrations = true };
        var options2 = new EfCoreOptions { ConnectionName = "Primary", AutoApplyMigrations = true };

        Assert.Equal(options1, options2);
        Assert.Equal(options1.GetHashCode(), options2.GetHashCode());
    }

    [Fact]
    public void Inequality_Should_DetectDifference()
    {
        var options1 = new EfCoreOptions { ConnectionName = "Primary" };
        var options2 = new EfCoreOptions { ConnectionName = "Secondary" };

        Assert.NotEqual(options1, options2);
    }
}
