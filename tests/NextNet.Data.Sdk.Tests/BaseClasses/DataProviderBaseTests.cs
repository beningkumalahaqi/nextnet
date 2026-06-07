using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.Sdk.Base;

namespace NextNet.Data.Sdk.Tests.BaseClasses;

/// <summary>
/// Tests for <see cref="DataProviderBase"/>.
/// </summary>
public class DataProviderBaseTests
{
    [Fact]
    public void Constructor_Should_SetName_FromClassName()
    {
        var provider = new TestDataProvider();
        Assert.Equal("Test", provider.Name);
    }

    [Fact]
    public void Constructor_Should_SetDefaultVersion()
    {
        var provider = new TestDataProvider();
        Assert.Equal(new Version(0, 1, 0), provider.Version);
    }

    [Fact]
    public void Constructor_Should_SetStatusToCreated()
    {
        var provider = new TestDataProvider();
        Assert.Equal(ProviderStatus.Created, provider.Status);
    }

    [Fact]
    public async Task InitializeAsync_Should_SetStatusToInitialized()
    {
        var provider = new TestDataProvider();
        var config = CreateValidConfig();

        await provider.InitializeAsync(config);

        Assert.True(provider.IsInitialized);
        Assert.Equal(ProviderStatus.Initialized, provider.Status);
        Assert.NotNull(provider.LastInitializedAt);
    }

    [Fact]
    public async Task InitializeAsync_Should_CallInitializeCore()
    {
        var provider = new TestDataProvider();
        var config = CreateValidConfig();

        await provider.InitializeAsync(config);

        Assert.True(provider.InitializeCoreCalled);
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_WhenConfigIsNull()
    {
        var provider = new TestDataProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.InitializeAsync(null!));
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_WhenValidationFails()
    {
        var provider = new TestDataProvider();
        var config = new DataConfig(DefaultConnection: string.Empty, Connections: null);

        var ex = await Assert.ThrowsAsync<ProviderInitializationException>(() => provider.InitializeAsync(config));
        Assert.Equal(ProviderStatus.InitializationFailed, provider.Status);
    }

    [Fact]
    public async Task IsHealthyAsync_Should_ReturnHealthy_WhenNoHealthCheckProvider()
    {
        var provider = new TestDataProvider();
        var config = CreateValidConfig();
        await provider.InitializeAsync(config);

        var result = await provider.IsHealthyAsync();

        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Status);
    }

    [Fact]
    public async Task IsHealthyAsync_Should_DelegateToHealthCheckProvider()
    {
        var provider = new TestHealthCheckDataProvider();
        var config = CreateValidConfig();
        await provider.InitializeAsync(config);

        var result = await provider.IsHealthyAsync();

        Assert.True(result.IsHealthy);
    }

    [Fact]
    public void Dispose_Should_SetStatusToDisposed()
    {
        var provider = new TestDataProvider();
        provider.Dispose();

        Assert.Equal(ProviderStatus.Disposed, provider.Status);
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_WhenDisposed()
    {
        var provider = new TestDataProvider();
        provider.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => provider.InitializeAsync(CreateValidConfig()));
    }

    private static DataConfig CreateValidConfig()
    {
        return new DataConfig(
            DefaultConnection: "Default",
            Connections: new Dictionary<string, ConnectionConfig>
            {
                ["Default"] = new ConnectionConfig("Server=localhost;Database=Test;")
            });
    }

    /// <summary>
    /// Minimal DataProviderBase implementation for testing.
    /// </summary>
    private sealed class TestDataProvider : DataProviderBase
    {
        public bool InitializeCoreCalled { get; private set; }

        public TestDataProvider() : base(null) { }

        protected override Task InitializeCoreAsync(DataConfig config, CancellationToken cancellationToken)
        {
            InitializeCoreCalled = true;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// DataProviderBase implementation with custom health check for testing.
    /// </summary>
    private sealed class TestHealthCheckDataProvider : DataProviderBase
    {
        public TestHealthCheckDataProvider() : base(null) { }

        protected override Task InitializeCoreAsync(DataConfig config, CancellationToken cancellationToken)
            => Task.CompletedTask;

        protected override IHealthCheckProvider? CreateHealthCheckProvider()
            => new TestHealthCheckProvider();
    }

    /// <summary>
    /// Simple health check provider for testing.
    /// </summary>
    private sealed class TestHealthCheckProvider : IHealthCheckProvider
    {
        public Task<HealthCheckResult> GetHealthCheckAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new HealthCheckResult(true, "Healthy", TimeSpan.Zero));
    }
}
