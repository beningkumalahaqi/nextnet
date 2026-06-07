using NextNet.Data.Extensions;
using Xunit;

namespace NextNet.Data.Providers.Tests;

public class ProviderMetadataAttributeTests
{
    [Fact]
    public void Attribute_Should_SetRequiredProperties()
    {
        // Act
        var attr = new ProviderMetadataAttribute("ef", "Entity Framework", "ORM provider");

        // Assert
        Assert.Equal("ef", attr.Id);
        Assert.Equal("Entity Framework", attr.DisplayName);
        Assert.Equal("ORM provider", attr.Description);
    }

    [Fact]
    public void Attribute_Should_HaveDefaultOptionalProperties()
    {
        // Arrange
        var attr = new ProviderMetadataAttribute("test", "Test", "A test provider");

        // Assert
        Assert.Equal(string.Empty, attr.PackageName);
        Assert.Equal(string.Empty, attr.CliCommand);
        Assert.Empty(attr.SupportedDatabases);
        Assert.False(attr.SupportsMigrations);
        Assert.False(attr.SupportsRepositories);
    }

    [Fact]
    public void Attribute_Should_SetOptionalProperties()
    {
        // Arrange
        var attr = new ProviderMetadataAttribute("mongo", "MongoDB", "NoSQL provider")
        {
            PackageName = "NextNet.Data.MongoDB",
            CliCommand = "nextnet add data mongo",
            SupportedDatabases = new[] { "MongoDB" },
            SupportsMigrations = false,
            SupportsRepositories = true
        };

        // Assert
        Assert.Equal("NextNet.Data.MongoDB", attr.PackageName);
        Assert.Equal("nextnet add data mongo", attr.CliCommand);
        Assert.Equal(new[] { "MongoDB" }, attr.SupportedDatabases);
        Assert.False(attr.SupportsMigrations);
        Assert.True(attr.SupportsRepositories);
    }

    [Fact]
    public void GetMetadata_Should_ReturnNull_When_NoAttribute()
    {
        // Arrange
        var type = typeof(NoAttributeProvider);

        // Act
        var metadata = type.GetMetadata();

        // Assert
        Assert.Null(metadata);
    }

    [Fact]
    public void GetMetadata_Should_ReturnMetadata_When_AttributePresent()
    {
        // Arrange
        var type = typeof(TestAttributedProvider);

        // Act
        var metadata = type.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("TestProvider", metadata.Id);
        Assert.Equal("Test Provider v1", metadata.DisplayName);
        Assert.Equal("A test provider for unit tests", metadata.Description);
    }

    [Fact]
    public void GetMetadata_Should_IncludeOptionalProperties()
    {
        // Arrange
        var type = typeof(FullAttributedProvider);

        // Act
        var metadata = type.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("FullProvider", metadata.Id);
        Assert.Equal("Full Provider", metadata.DisplayName);
        Assert.Equal("NextNet.Data.Full", metadata.PackageName);
        Assert.Equal("nextnet add data full", metadata.CliCommand);
        Assert.Contains("SQL Server", metadata.SupportedDatabases);
        Assert.Contains("PostgreSQL", metadata.SupportedDatabases);
        Assert.True(metadata.SupportsMigrations);
        Assert.True(metadata.SupportsRepositories);
    }

    [Fact]
    public void GetMetadata_Should_ReturnVersion_FromAssembly()
    {
        // Arrange
        var type = typeof(TestAttributedProvider);

        // Act
        var metadata = type.GetMetadata();

        // Assert
        Assert.NotNull(metadata);
        Assert.NotNull(metadata.Version);
    }

    [Fact]
    public void GetMetadataAttribute_Should_ReturnAttribute_When_Present()
    {
        // Arrange
        var type = typeof(TestAttributedProvider);

        // Act
        var attr = type.GetMetadataAttribute();

        // Assert
        Assert.NotNull(attr);
        Assert.Equal("TestProvider", attr.Id);
    }

    [Fact]
    public void GetMetadataAttribute_Should_ReturnNull_When_NotPresent()
    {
        // Arrange
        var type = typeof(NoAttributeProvider);

        // Act
        var attr = type.GetMetadataAttribute();

        // Assert
        Assert.Null(attr);
    }

    [Fact]
    public void GetMetadata_Should_Throw_When_TypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((Type)null!).GetMetadata());
    }

    [Fact]
    public void GetMetadataAttribute_Should_Throw_When_TypeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ((Type)null!).GetMetadataAttribute());
    }

    [Fact]
    public void Attribute_Should_HaveCorrectUsage()
    {
        // Assert
        var attrUsage = (AttributeUsageAttribute)typeof(ProviderMetadataAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.Equal(AttributeTargets.Class, attrUsage.ValidOn);
        Assert.False(attrUsage.AllowMultiple);
        Assert.False(attrUsage.Inherited);
    }

    [ProviderMetadata("TestProvider", "Test Provider v1", "A test provider for unit tests")]
    private sealed class TestAttributedProvider : IDataProvider
    {
        public string Name => "TestProvider";
        public string DisplayName => "Test Provider v1";
        public Version Version => new(1, 0, 0);
        public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken ct = default)
            => Task.FromResult(DataProviderHealthResult.Healthy());
    }

    [ProviderMetadata(
        "FullProvider",
        "Full Provider",
        "A fully attributed provider",
        PackageName = "NextNet.Data.Full",
        CliCommand = "nextnet add data full",
        SupportedDatabases = new[] { "SQL Server", "PostgreSQL" },
        SupportsMigrations = true,
        SupportsRepositories = true)]
    private sealed class FullAttributedProvider : IDataProvider
    {
        public string Name => "FullProvider";
        public string DisplayName => "Full Provider";
        public Version Version => new(2, 0, 0);
        public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken ct = default)
            => Task.FromResult(DataProviderHealthResult.Healthy());
    }

    private sealed class NoAttributeProvider : IDataProvider
    {
        public string Name => "NoAttr";
        public string DisplayName => "No Attribute";
        public Version Version => new(1, 0, 0);
        public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task<DataProviderHealthResult> IsHealthyAsync(CancellationToken ct = default)
            => Task.FromResult(DataProviderHealthResult.Healthy());
    }
}
