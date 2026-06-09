# Provider SDK

The NextNet Provider SDK (`NextNet.Data.Sdk`) allows third party developers to create custom database providers that integrate seamlessly with the NextNet data layer.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Base Classes](#base-classes)
- [Attributes](#attributes)
- [Required Interfaces](#required-interfaces)
- [Registration](#registration)
- [Testing](#testing)
- [Publishing to NuGet](#publishing-to-nuget)
- [Provider Contract Checklist](#provider-contract-checklist)

## Overview

The SDK provides base classes, attributes, and analyzers that:

- **Simplify development** — Reduce boilerplate with `DataProviderBase`, `RepositoryBase<T>`, etc.
- **Ensure consistency** — Enforce NextNet's conventions and contracts.
- **Validate at compile time** — Roslyn analyzers catch common mistakes before runtime.
- **Streamline registration** — The `[DataProvider]` attribute enables auto discovery.

### Package

```bash
dotnet add package NextNet.Data.Sdk
```

### What You Can Build

- Custom database providers (e.g., Cosmos DB, DynamoDB, Couchbase)
- Custom migration engines
- Custom health check implementations
- Custom scaffold providers

## Getting Started

### Minimal Provider

```csharp
using NextNet.Data.Abstractions;
using NextNet.Data.Providers;
using NextNet.Data.Sdk;
using NextNet.Data.Sdk.Attributes;

[DataProvider("CosmosDB")]
public sealed class CosmosDbProvider : DataProviderBase
{
    private readonly string _connectionString;

    public CosmosDbProvider(
        string connectionString,
        ILogger<CosmosDbProvider> logger) : base(logger)
    {
        _connectionString = connectionString;
    }

    public override string Name => "CosmosDB";
    public override string DisplayName => "Azure Cosmos DB";

    public override async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        ValidateNotInitialized();
        // Initialize Cosmos DB client
        await Task.CompletedTask;
        MarkInitialized();
    }

    public override IDataConnection? CreateConnection()
    {
        return new CosmosDbConnection(_connectionString);
    }
}
```

### Project Structure

```
MyNextNetProvider/
├── src/
│   └── MyNextNet.Provider.CosmosDB/
│       ├── MyNextNet.Provider.CosmosDB.csproj
│       ├── CosmosDbProvider.cs            # Main provider class
│       ├── CosmosDbConnection.cs          # Connection implementation
│       ├── CosmosDbRepository.cs          # Repository implementation
│       ├── CosmosDbHealthCheck.cs         # Health check implementation
│       ├── CosmosDbMigrationEngine.cs     # Migration engine (optional)
│       └── CosmosDbScaffoldProvider.cs    # Scaffold provider (optional)
├── tests/
│   └── MyNextNet.Provider.CosmosDB.Tests/
│       ├── CosmosDbProviderTests.cs
│       └── CosmosDbRepositoryTests.cs
└── README.md
```

## Base Classes

### DataProviderBase

Base implementation of `IDataProvider` with initialization guard and logging.

```csharp
public abstract class DataProviderBase : IDataProvider
{
    // Properties you must override
    public abstract string Name { get; }
    public abstract string DisplayName { get; }

    // Properties inherited
    public string Version => GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0";
    public bool IsInitialized { get; private set; }

    // Methods you must implement
    public abstract Task InitializeAsync(CancellationToken cancellationToken = default);
    public abstract IDataConnection? CreateConnection();

    // Protected helpers
    protected void ValidateNotInitialized();
    protected void MarkInitialized();
    protected ILogger Logger { get; }
}
```

### RepositoryBase<T>

Base implementation of `IRepository<T>` with common CRUD patterns.

```csharp
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected RepositoryBase(
        IDataConnection connection,
        ILogger<RepositoryBase<T>> logger);

    // IRepository<T> implementation
    public abstract Task<T?> FindAsync(object id);
    public abstract Task<PagedResult<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        int page = 1,
        int pageSize = 20);
    public abstract Task<T> InsertAsync(T entity);
    public abstract Task<T> UpdateAsync(T entity);
    public abstract Task DeleteAsync(object id);

    // Additional helpers
    protected abstract Task<IEnumerable<T>> FindAllAsync(
        Expression<Func<T, bool>> predicate);
    protected abstract Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate);
    protected abstract Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null);
    protected abstract Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate);
    protected abstract Task<IEnumerable<T>> ExecuteQueryAsync(
        string sql, object? parameters = null);
    protected abstract Task<T?> ExecuteSingleAsync(
        string sql, object? parameters = null);

    protected IDataConnection Connection { get; }
    protected ILogger Logger { get; }
}
```

### HealthCheckProviderBase

Base implementation of `IHealthCheckProvider` with error handling.

```csharp
public abstract class HealthCheckProviderBase : IHealthCheckProvider
{
    protected HealthCheckProviderBase();

    public abstract Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default);

    // Factory methods
    protected HealthCheckResult Healthy(string description);
    protected HealthCheckResult Degraded(string description);
    protected HealthCheckResult Unhealthy(
        string description, Exception? exception = null);
}
```

### MigrationEngineBase

Base implementation of `IMigrationEngine` (optional, for providers with migration support).

```csharp
public abstract class MigrationEngineBase : IMigrationEngine
{
    protected MigrationEngineBase(ILogger<MigrationEngineBase> logger);

    public abstract Task<MigrationResult> ApplyAsync(
        MigrationOptions options, CancellationToken ct = default);
    public abstract Task<MigrationResult> RollbackAsync(
        int steps = 1, CancellationToken ct = default);
    public abstract Task<MigrationStatus> GetStatusAsync(
        CancellationToken ct = default);
    public abstract Task<Migration> CreateMigrationAsync(
        string name, CancellationToken ct = default);
}
```

## Attributes

### [DataProvider]

Marks a class as a data provider and specifies its unique identifier.

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DataProviderAttribute : Attribute
{
    /// <summary>
    /// Unique provider identifier (used in configuration's "provider" field).
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Optional description of the provider.
    /// </summary>
    public string? Description { get; set; }

    public DataProviderAttribute(string identifier)
    {
        Identifier = identifier;
    }
}
```

Usage:

```csharp
[DataProvider("CosmosDB", Description = "Azure Cosmos DB NoSQL provider")]
public sealed class CosmosDbProvider : DataProviderBase { ... }
```

The identifier maps to the `"provider"` field in configuration:

```json
{
  "connections": {
    "Default": {
      "connectionString": "AccountEndpoint=...;AccountKey=...",
      "provider": "CosmosDB"   // Matches [DataProvider("CosmosDB")]
    }
  }
}
```

### [ProviderOption]

Defines a configurable option for the provider (used for code generation and documentation).

```csharp
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ProviderOptionAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; set; }
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; }

    public ProviderOptionAttribute(string name)
    {
        Name = name;
    }
}
```

Usage:

```csharp
public sealed class CosmosDbProviderOptions
{
    [ProviderOption("MaxThroughput",
        Description = "Maximum RU/s for the Cosmos DB container",
        DefaultValue = 400)]
    public int MaxThroughput { get; set; } = 400;

    [ProviderOption("DatabaseName",
        Description = "Name of the Cosmos DB database",
        IsRequired = true)]
    public string DatabaseName { get; set; } = string.Empty;
}
```

## Required Interfaces

Your provider must implement `IDataProvider`. Additional interfaces are optional but recommended.

### IDataProvider (Required)

```csharp
public interface IDataProvider
{
    string Name { get; }
    string DisplayName { get; }
    string Version { get; }
    bool IsInitialized { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
    IDataConnection? CreateConnection();
}
```

### IRepository<T> (Recommended)

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> FindAsync(object id);
    Task<PagedResult<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        int page = 1,
        int pageSize = 20);
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
```

### IHealthCheckProvider (Recommended)

```csharp
public interface IHealthCheckProvider
{
    string Name { get; }
    Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default);
}
```

### IMigrationEngine (Optional)

```csharp
public interface IMigrationEngine
{
    Task<Migration> CreateMigrationAsync(
        string name, CancellationToken cancellationToken = default);
    Task<MigrationResult> ApplyAsync(
        MigrationOptions options, CancellationToken cancellationToken = default);
    Task<MigrationResult> RollbackAsync(
        int steps = 1, CancellationToken cancellationToken = default);
    Task<MigrationStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);
}
```

### IScaffoldProvider (Optional)

```csharp
public interface IScaffoldProvider
{
    Task<IEnumerable<TableInfo>> GetTablesAsync(
        CancellationToken cancellationToken = default);
    Task<ScaffoldedModel> ScaffoldModelAsync(
        string tableName, ScaffoldOptions options,
        CancellationToken cancellationToken = default);
    Task<ScaffoldedRepository> ScaffoldRepositoryAsync(
        string modelName, ScaffoldOptions options,
        CancellationToken cancellationToken = default);
}
```

## Registration

### Auto-Discovery via Assembly Scanning

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .ScanProviders(typeof(CosmosDbProvider).Assembly)  // Auto-discover providers
    .Build();
```

### Manual Registration

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseProvider<CosmosDbProvider>()
    .Build();
```

### Custom Registration Extensions

Create extension methods for a clean developer experience:

```csharp
using NextNet.Data.Providers;

public static class NextNetDataBuilderExtensions
{
    public static INextNetDataBuilder UseCosmosDb(
        this INextNetDataBuilder builder)
    {
        return builder.UseProvider<CosmosDbProvider>();
    }
}
```

Usage:

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseCosmosDb()
    .Build();
```

## Testing

### Unit Tests

Test your provider against the NextNet contract tests:

```bash
dotnet add package NextNet.Data.Abstractions.TestHelpers
```

```csharp
using NextNet.Data.Abstractions.TestHelpers;

public class CosmosDbProviderTests
{
    [Fact]
    public void Provider_Should_Implement_IDataProvider()
    {
        var provider = new CosmosDbProvider(
            "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDj...",
            NullLogger<CosmosDbProvider>.Instance);

        Assert.IsAssignableFrom<IDataProvider>(provider);
        Assert.Equal("CosmosDB", provider.Name);
        Assert.False(provider.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_Should_Throw_When_CalledTwice()
    {
        var provider = new CosmosDbProvider(
            "AccountEndpoint=https://localhost:8081;AccountKey=C2y6yDj...",
            NullLogger<CosmosDbProvider>.Instance);

        await provider.InitializeAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.InitializeAsync());
    }

    [Fact]
    public async Task Repository_Should_Implement_IRepository()
    {
        var provider = new CosmosDbProvider(...);
        await provider.InitializeAsync();

        var connection = provider.CreateConnection();
        var repository = new CosmosDbRepository<User>(connection, ...);

        Assert.IsAssignableFrom<IRepository<User>>(repository);
    }
}
```

### Integration Tests

Use the NextNet test infrastructure:

```csharp
[CollectionDefinition("CosmosDB")]
public class CosmosDbTestCollection : ICollectionFixture<CosmosDbTestFixture> { }

[Collection("CosmosDB")]
public class CosmosDbRepositoryTests
{
    private readonly CosmosDbTestFixture _fixture;

    public CosmosDbRepositoryTests(CosmosDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InsertAsync_Should_PersistEntity()
    {
        var repo = _fixture.CreateRepository<User>();
        var user = new User { Name = "Test User", Email = "test@example.com" };

        var created = await repo.InsertAsync(user);

        Assert.NotNull(created);
        Assert.NotEqual(0, created.Id);
    }
}
```

### Contract Validation Tests

```csharp
[Fact]
public void Provider_Should_Meet_NextNet_Contracts()
{
    var validator = new ProviderContractValidator();
    var result = validator.Validate(typeof(CosmosDbProvider));

    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}
```

## Publishing to NuGet

### Package Metadata

```xml
<!-- MyNextNet.Provider.CosmosDB.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>NextNet.Data.CosmosDB</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Azure Cosmos DB provider for NextNet Data Layer.</Description>
    <PackageTags>nextnet;data;cosmosdb;nosql</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/yourorg/NextNet.Data.CosmosDB</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourorg/NextNet.Data.CosmosDB</RepositoryUrl>
    <PackageIcon>icon.png</PackageIcon>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NextNet.Data.Sdk" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.*" />
  </ItemGroup>

</Project>
```

### Publishing Steps

```bash
# 1. Build
dotnet build -c Release

# 2. Run tests
dotnet test

# 3. Create NuGet package
dotnet pack -c Release -o ./nupkg

# 4. Publish to NuGet.org
dotnet nuget push ./nupkg/NextNet.Data.CosmosDB.1.0.0.nupkg \
    --api-key <your-api-key> \
    --source https://api.nuget.org/v3/index.json

# Or publish to GitHub Packages
dotnet nuget push ./nupkg/NextNet.Data.CosmosDB.1.0.0.nupkg \
    --source https://nuget.pkg.github.com/yourorg/index.json
```

### Naming Conventions

| Package Type | Naming Pattern | Example |
|-------------|----------------|---------|
| Provider | `NextNet.Data.<ProviderName>` | `NextNet.Data.CosmosDB` |
| Extension | `NextNet.Data.<Feature>` | `NextNet.Data.HealthChecks` |
| Test Helpers | `NextNet.Data.Abstractions.TestHelpers` | `NextNet.Data.Abstractions.TestHelpers` |

## Provider Contract Checklist

Before releasing a custom provider, verify:

### Required

- [ ] Implements `IDataProvider`
- [ ] Decorated with `[DataProvider("identifier")]`
- [ ] `InitializeAsync` validates it is called only once (throws on double call)
- [ ] `CreateConnection()` returns `null` if not initialized
- [ ] Connection strings are never logged or exposed in error messages
- [ ] Health check wraps all exceptions (never throws)
- [ ] XML documentation on all public types and members
- [ ] Package follows NextNet naming conventions

### Recommended

- [ ] Implements `IRepository<T>` (via `RepositoryBase<T>`)
- [ ] Implements `IHealthCheckProvider` (via `HealthCheckProviderBase`)
- [ ] Implements `IMigrationEngine` (if applicable)
- [ ] Implements `IScaffoldProvider` (if applicable)
- [ ] Unit tests for provider initialization
- [ ] Unit tests for repository CRUD operations
- [ ] Integration tests against a real/test database
- [ ] README with installation and usage instructions
- [ ] License file included in package

### Analyzer Checks

The SDK includes Roslyn analyzers that verify at compile time:

| Rule ID | Description |
|---------|-------------|
| SK1001 | Provider must implement `IDataProvider` |
| SK1002 | Provider must have a parameterless or DI-compatible constructor |
| SK1003 | Provider must not expose connection strings in exceptions |
| SK1004 | Provider must guard against double initialization |
| SK1005 | `[DataProvider]` identifier must be unique |
| SK1006 | Provider package must include XML documentation |

## See Also

- [Configuration Reference](configuration.md)
- [EF Core Provider](providers/ef-core.md) -- reference implementation
- [Dapper Provider](providers/dapper.md) -- reference implementation
- [Troubleshooting](troubleshooting.md)
