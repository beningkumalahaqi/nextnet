namespace NextNet.Data.MongoDB.Tests;

/// <summary>
/// Tests for <see cref="MongoDbOptions"/> and <see cref="MongoDbRepositoryOptions"/>.
/// </summary>
public sealed class MongoDbOptionsTests
{
    [Fact]
    public void MongoDbOptions_ShouldHaveDefaults()
    {
        var options = new MongoDbOptions();
        Assert.Equal("Default", options.ConnectionName);
        Assert.Null(options.DefaultDatabaseName);
        Assert.Equal(100, options.MaxConnectionPoolSize);
        Assert.Equal(0, options.MinConnectionPoolSize);
        Assert.Equal(10, options.ConnectTimeoutSeconds);
        Assert.Equal(30, options.ServerSelectionTimeoutSeconds);
        Assert.Equal(30, options.SocketTimeoutSeconds);
        Assert.True(options.RetryWrites);
        Assert.True(options.RetryReads);
        Assert.True(options.RegisterHealthChecks);
        Assert.Equal(ServiceLifetime.Scoped, options.RepositoryLifetime);
        Assert.False(options.EnableQueryLogging);
    }

    [Fact]
    public void MongoDbOptions_ShouldAllowPropertyAssignment()
    {
        var options = new MongoDbOptions
        {
            ConnectionName = "Analytics",
            DefaultDatabaseName = "myapp",
            MaxConnectionPoolSize = 50,
            ConnectTimeoutSeconds = 15,
            RetryWrites = false,
            RegisterHealthChecks = false,
        };

        Assert.Equal("Analytics", options.ConnectionName);
        Assert.Equal("myapp", options.DefaultDatabaseName);
        Assert.Equal(50, options.MaxConnectionPoolSize);
        Assert.Equal(15, options.ConnectTimeoutSeconds);
        Assert.False(options.RetryWrites);
        Assert.False(options.RegisterHealthChecks);
    }

    [Fact]
    public void MongoDbOptions_ShouldSupportConfigureClientSettings()
    {
        var called = false;
        var options = new MongoDbOptions
        {
            ConfigureClientSettings = settings => called = true,
        };

        Assert.NotNull(options.ConfigureClientSettings);
        options.ConfigureClientSettings(null!);
        Assert.True(called);
    }

    [Fact]
    public void MongoDbRepositoryOptions_ShouldHaveDefaults()
    {
        var options = new MongoDbRepositoryOptions();
        Assert.Null(options.CollectionName);
        Assert.Null(options.ConnectionName);
        Assert.Null(options.IdFieldName);
        Assert.True(options.TreatIdAsObjectId);
    }

    [Fact]
    public void MongoDbRepositoryOptions_ShouldAllowPropertyAssignment()
    {
        var options = new MongoDbRepositoryOptions
        {
            CollectionName = "myCollection",
            ConnectionName = "Analytics",
            IdFieldName = "EntityId",
            TreatIdAsObjectId = false,
        };

        Assert.Equal("myCollection", options.CollectionName);
        Assert.Equal("Analytics", options.ConnectionName);
        Assert.Equal("EntityId", options.IdFieldName);
        Assert.False(options.TreatIdAsObjectId);
    }

    [Fact]
    public void MongoDbConventionOptions_ShouldHaveDefaults()
    {
        var options = new MongoDbConventionOptions();
        Assert.True(options.UseCamelCaseElementNames);
        Assert.True(options.IgnoreExtraElements);
        Assert.True(options.AutoMapIdToUnderscoreId);
        Assert.Null(options.AdditionalConventionPacks);
    }

    [Fact]
    public void MongoDbConventionOptions_ShouldAllowPropertyAssignment()
    {
        var options = new MongoDbConventionOptions
        {
            UseCamelCaseElementNames = false,
            IgnoreExtraElements = false,
            AutoMapIdToUnderscoreId = false,
        };

        Assert.False(options.UseCamelCaseElementNames);
        Assert.False(options.IgnoreExtraElements);
        Assert.False(options.AutoMapIdToUnderscoreId);
    }
}
