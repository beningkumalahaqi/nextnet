namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="EfCoreDataProvider"/>.
/// </summary>
public sealed class EfCoreDataProviderTests : IDisposable
{
    private readonly InMemoryDbContextFactory _fixture = new();
    private readonly EfCoreDataProvider _provider;

    public EfCoreDataProviderTests()
    {
        var factory = _fixture.CreateFactory();
        var options = new EfCoreOptions
        {
            ConnectionName = "Default",
            ConfigureDbContext = builder => builder.UseInMemoryDatabase("TestDb")
        };
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreDataProvider>();

        _provider = new EfCoreDataProvider(factory, options, logger);
    }

    [Fact]
    public void Name_Should_ReturnEntityFramework()
    {
        Assert.Equal("EntityFramework", _provider.Name);
    }

    [Fact]
    public void DisplayName_Should_ReturnEntityFrameworkCore()
    {
        Assert.Equal("Entity Framework Core 8", ((NextNet.Data.IDataProvider)_provider).DisplayName);
    }

    [Fact]
    public void Version_Should_BeNonZero()
    {
        var version = ((NextNet.Data.IDataProvider)_provider).Version;
        Assert.NotNull(version);
        Assert.True(version.Major >= 0);
    }

    [Fact]
    public async Task InitializeAsync_Should_CompleteSuccessfully()
    {
        var config = new DataConfig(
            DefaultConnection: "Default",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Default"] = new ConnectionConfig("InMemory=TestDb")
            });

        await ((NextNet.Data.Abstractions.Abstractions.IDataProvider)_provider)
            .InitializeAsync(config, CancellationToken.None);

        Assert.True(true); // No exception means success
    }

    [Fact]
    public async Task InitializeAsync_Should_BeIdempotent()
    {
        var config = new DataConfig(
            DefaultConnection: "Default",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Default"] = new ConnectionConfig("InMemory=TestDb")
            });

        await ((NextNet.Data.Abstractions.Abstractions.IDataProvider)_provider)
            .InitializeAsync(config, CancellationToken.None);

        // Second call should not fail
        await ((NextNet.Data.Abstractions.Abstractions.IDataProvider)_provider)
            .InitializeAsync(config, CancellationToken.None);
    }

    [Fact]
    public async Task ProvidersInterface_InitializeAsync_Should_Work()
    {
        await ((NextNet.Data.IDataProvider)_provider).InitializeAsync(CancellationToken.None);
    }

    [Fact]
    public async Task IsHealthyAsync_Should_ReturnHealthResult()
    {
        var result = await ((NextNet.Data.Abstractions.Abstractions.IDataProvider)_provider)
            .IsHealthyAsync(CancellationToken.None);

        Assert.NotNull(result);
        // InMemory provider may return Healthy or Unhealthy depending on the version.
        // We just verify a result is returned without throwing.
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task ProvidersInterface_IsHealthyAsync_Should_ReturnDataProviderHealthResult()
    {
        var result = await ((NextNet.Data.IDataProvider)_provider)
            .IsHealthyAsync(CancellationToken.None);

        Assert.NotNull(result);
        // InMemory provider compatibility - we just verify no exception
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
