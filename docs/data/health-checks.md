# Health Checks

NextNet provides a unified health check system for monitoring database connectivity across all registered providers and connections.

## Table of Contents

- [Overview](#overview)
- [Built-in Health Checks](#built-in-health-checks)
- [Installation](#installation)
- [Configuration](#configuration)
- [Endpoint](#endpoint)
- [Custom Health Checks](#custom-health-checks)
- [Aggregation](#aggregation)
- [Caching](#caching)
- [Security Considerations](#security-considerations)
- [Integration with ASP.NET Core](#integration-with-aspnet-core)

## Overview

The `NextNet.Data.HealthChecks` package automatically registers health checks for every enabled database connection. It aggregates results from all providers into a single health endpoint, providing real-time visibility into database connectivity.

### Architecture

```
┌─────────────────────────────────────────────────┐
│              Health Check Endpoint               │
│               GET /health                        │
├─────────────────────────────────────────────────┤
│            HealthCheckAggregator                 │
│     (Caches and aggregates results)              │
├─────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────┐ │
│ │  EF Core │ │  Dapper  │ │  SQLite  │ │Mongo │ │
│ │  Health  │ │  Health  │ │  Health  │ │Health│ │
│ │  Check   │ │  Check   │ │  Check   │ │Check │ │
│ └──────────┘ └──────────┘ └──────────┘ └──────┘ │
└─────────────────────────────────────────────────┘
```

## Built-in Health Checks

Each provider includes a built-in health check that registers automatically:

| Provider | Health Check Class | What It Does |
|----------|-------------------|--------------|
| EF Core | `EfCoreHealthCheckProvider` | Executes `SELECT 1` against the database |
| Dapper | `DapperHealthCheck` | Opens a connection and executes `SELECT 1` |
| SQLite | `SqliteHealthCheckProvider` | Opens a connection and verifies file access |
| PostgreSQL | `PostgresHealthCheckProvider` | Executes `SELECT 1` via Npgsql |
| MongoDB | `MongoDbHealthCheck` | Runs `ping` command against the admin database |

### Registration

Health checks are registered **automatically** when you call `UseProvider<T>()`:

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()  // Registers EfCoreHealthCheckProvider
    .UseProvider<DapperDataProvider>()  // Registers DapperHealthCheck
    .Build();
```

No additional registration code is needed.

## Installation

```bash
# Install the health checks package
dotnet add package NextNet.Data.HealthChecks

# This is auto-added when using `nextnet add data <provider>`
```

## Configuration

```json
{
  "data": {
    "healthChecks": {
      "path": "/health",
      "tags": ["nextnet", "data"],
      "showDetails": false,
      "cacheDurationSeconds": 5,
      "includeMigrationStatus": true,
      "timeoutSeconds": 10,
      "degradedThresholdMs": 100,
      "unhealthyThresholdMs": 5000
    }
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `path` | string | `"/health"` | Health check endpoint path |
| `tags` | string[] | `["nextnet", "data"]` | Health check tags for filtering |
| `showDetails` | bool | `false` | Show detailed results (includes connection names) |
| `cacheDurationSeconds` | int | `5` | Cache duration between checks |
| `includeMigrationStatus` | bool | `true` | Include pending migration count |
| `timeoutSeconds` | int | `10` | Per-check timeout |
| `degradedThresholdMs` | int | `100` | Response time threshold for Degraded status (ms) |
| `unhealthyThresholdMs` | int | `5000` | Response time threshold for Unhealthy status (ms) |

## Endpoint

### Map the Endpoint

```csharp
// In Program.cs
app.MapNextNetHealthChecks();
```

### Response Format

**Simple mode** (`showDetails: false`):

```json
{
  "status": "Healthy",
  "timestamp": "2026-06-06T12:00:00Z",
  "duration": "00:00:00.0050000"
}
```

**Detailed mode** (`showDetails: true`):

```json
{
  "status": "Degraded",
  "timestamp": "2026-06-06T12:00:00Z",
  "duration": "00:00:00.0500000",
  "results": [
    {
      "name": "Default",
      "provider": "EntityFramework",
      "status": "Healthy",
      "description": "Database is reachable and responding.",
      "duration": "00:00:00.0050000"
    },
    {
      "name": "ReadReplica",
      "provider": "EntityFramework",
      "status": "Degraded",
      "description": "Response time exceeds threshold (150ms).",
      "duration": "00:00:00.1500000",
      "error": null
    },
    {
      "name": "Analytics",
      "provider": "MongoDB",
      "status": "Unhealthy",
      "description": "Connection refused at [host]:27017.",
      "duration": "00:00:05.0000000",
      "error": "MongoDB.Driver.MongoConnectionException: ..."
    }
  ],
  "migrationStatus": {
    "hasPending": true,
    "pendingCount": 2,
    "lastApplied": "2026-06-01T10:00:00Z"
  }
}
```

### Status Values

| Status | Meaning |
|--------|---------|
| `Healthy` | Database is reachable and responding within threshold |
| `Degraded` | Database is reachable but slow (exceeds `degradedThresholdMs`) |
| `Unhealthy` | Database is unreachable or error occurred |

## Custom Health Checks

You can add custom health checks beyond the built-in database checks:

### Implementing IHealthCheckProvider

```csharp
using NextNet.Data.Abstractions.HealthChecks;
using NextNet.Data.Sdk;

public sealed class RedisHealthCheck : HealthCheckProviderBase
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public override async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();

            return Healthy("Redis cache is reachable and responding.");
        }
        catch (Exception ex)
        {
            return Unhealthy($"Redis cache is unreachable: {ex.Message}", ex);
        }
    }
}
```

### Registering Custom Health Checks

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()
    .AddHealthCheck<RedisHealthCheck>()  // Register custom health check
    .Build();
```

### HealthCheckResult Factory Methods

```csharp
// The base class provides factory methods:
return Healthy("description");           // Status: Healthy
return Degraded("description");          // Status: Degraded
return Unhealthy("description", ex);     // Status: Unhealthy
```

## Aggregation

The `HealthCheckAggregator` combines results from all registered health check providers:

```csharp
public class MonitoringService
{
    private readonly IHealthCheckAggregator _aggregator;

    public MonitoringService(IHealthCheckAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public async Task<AggregatedHealthResult> GetFullHealthAsync()
    {
        return await _aggregator.CheckAllAsync();
    }

    public async Task<bool> IsDatabaseHealthyAsync(string connectionName)
    {
        var result = await _aggregator.CheckAllAsync();
        return result.Results.Any(r =>
            r.Name == connectionName && r.Status == HealthStatus.Healthy);
    }
}
```

### Aggregation Logic

| If All Results Are | Aggregate Status |
|--------------------|------------------|
| All Healthy | `Healthy` |
| Any Degraded (none Unhealthy) | `Degraded` |
| Any Unhealthy | `Unhealthy` |

## Caching

Health check results are cached to avoid hammering databases on every request.

```csharp
// Configuration
"cacheDurationSeconds": 5  // Cache for 5 seconds (default)
```

### Cache Behavior

- **Cache hit** (within duration): Returns cached result instantly.
- **Cache miss** (expired): Executes all health checks, caches result.
- **Manual refresh**: `IHealthCheckAggregator.CheckAllAsync(true)` bypasses cache.

### Memory Cache

Caching uses `IMemoryCache` (registered automatically). For distributed caching scenarios:

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});
```

The `IHealthCheckAggregator` will use the distributed cache if available, falling back to memory cache.

## Security Considerations

| Concern | Recommendation |
|---------|---------------|
| **Connection string leakage** | Error messages automatically strip connection strings. Custom health checks must follow the same pattern. |
| **DoS via health endpoint** | Cache results (default 5s) to prevent rapid-fire health checks overwhelming the database. |
| **Information disclosure** | Set `showDetails: false` in production to hide connection names and provider details. |
| **Authentication** | Protect the health endpoint in production: |
| **Authorization** | Restrict health checks to monitoring systems. |

### Protecting the Endpoint

```csharp
// Require authentication for health endpoint
app.MapNextNetHealthChecks()
   .RequireAuthorization("Monitoring");

// Or restrict to specific IP addresses
app.MapNextNetHealthChecks()
   .RequireHost("*.internal.example.com");
```

### Production Configuration

```json
{
  "data": {
    "healthChecks": {
      "showDetails": false,
      "cacheDurationSeconds": 10,
      "includeMigrationStatus": false,
      "tags": ["nextnet"]
    }
  }
}
```

## Integration with ASP.NET Core

### Using with ASP.NET Core Health Checks

```csharp
// NextNet health checks integrate with ASP.NET Core's health check middleware
app.MapHealthChecks("/health/aspnet", new HealthCheckOptions
{
    Predicate = _ => true  // Include all registered health checks
});
```

### Using with Kubernetes

```csharp
// Liveness probe
app.MapNextNetHealthChecks("/health/live")
   .RequireHost("*:8080");

// Readiness probe (includes database checks)
app.MapNextNetHealthChecks("/health/ready")
   .RequireHost("*:8080");
```

Kubernetes deployment configuration:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 15
```

### Using with Azure Monitor / Application Insights

```csharp
builder.Services.Configure<HealthCheckOptions>(options =>
{
    // Automatically publishes health check results to Application Insights
    options.PublishToApplicationInsights = true;
});
```

## See Also

- [Configuration Reference](configuration.md)
- [Multi-Database Health Checks](multi-database.md#health-checks-for-multiple-databases)
- [CLI Reference](cli-reference.md)
- [Troubleshooting](troubleshooting.md)
