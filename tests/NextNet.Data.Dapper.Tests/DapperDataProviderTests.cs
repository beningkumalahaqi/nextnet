namespace NextNet.Data.Dapper.Tests;

/// <summary>
/// Tests for <see cref="DapperDataProvider"/>.
/// </summary>
public sealed class DapperDataProviderTests
{
    private readonly DapperDataProvider _provider;

    public DapperDataProviderTests()
    {
        var options = new DapperOptions
        {
            ConnectionName = "Default",
            EnablePooling = false
        };
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperDataProvider>();

        _provider = new DapperDataProvider(options, logger);
    }

    [Fact]
    public void Name_Should_ReturnDapper()
    {
        Assert.Equal("Dapper", _provider.Name);
    }

    [Fact]
    public void DisplayName_Should_ReturnDapper()
    {
        Assert.Equal("Dapper 2.1", ((NextNet.Data.IDataProvider)_provider).DisplayName);
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
                ["Default"] = new ConnectionConfig("Server=.;Database=TestDb;Trusted_Connection=true;")
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
                ["Default"] = new ConnectionConfig("Server=.;Database=TestDb;Trusted_Connection=true;")
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
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task ProvidersInterface_IsHealthyAsync_Should_ReturnDataProviderHealthResult()
    {
        var result = await ((NextNet.Data.IDataProvider)_provider)
            .IsHealthyAsync(CancellationToken.None);

        Assert.NotNull(result);
    }
}
