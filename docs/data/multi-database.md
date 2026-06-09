# Multi-Database Support

NextNet supports multiple database connections in a single application through the `IDatabaseSelector` abstraction. This allows you to connect to different databases — even with different providers — and resolve them dynamically at runtime.

## Table of Contents

- [Overview](#overview)
- [Configuration](#configuration)
- [Named Connections](#named-connections)
- [Connection Resolution](#connection-resolution)
- [Per-Connection Providers](#per-connection-providers)
- [Health Checks for Multiple Databases](#health-checks-for-multiple-databases)
- [Use Cases](#use-cases)
- [Best Practices](#best-practices)
- [Limitations](#limitations)

## Overview

The multi database feature enables:

- **Read/write splitting** — Primary for writes, replicas for reads.
- **Multi tenant applications** — Database per tenant with dynamic resolution.
- **Polyglot persistence** — Different database technologies for different concerns (e.g., PostgreSQL for transactions, MongoDB for analytics).
- **Gradual migration** — Run old and new databases side by side during migration.
- **Reporting databases** — Isolated read replicas for reporting workloads.

### Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Application                        │
│  ┌──────────────────────────────────────────────┐  │
│  │           IDatabaseSelector                   │  │
│  │  .For("Default")  .For("Reports")  .All()    │  │
│  └────────────┬─────────────────────┬───────────┘  │
│               │                     │               │
│               ▼                     ▼               │
│  ┌──────────────────┐   ┌──────────────────────┐   │
│  │  Default (EF)    │   │  Reports (Dapper)    │   │
│  │  PostgreSQL:5432 │   │  SQLite:reports.db   │   │
│  └──────────────────┘   └──────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## Configuration

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "Host=primary.db;Database=myapp;Username=app;Password=secret",
        "provider": "EntityFramework",
        "timeoutSeconds": 30,
        "poolSize": 100,
        "enabled": true,
        "tags": ["primary", "write"]
      },
      "ReadReplica": {
        "connectionString": "Host=replica1.db;Database=myapp;Username=app_read;Password=secret",
        "provider": "EntityFramework",
        "timeoutSeconds": 30,
        "poolSize": 50,
        "enabled": true,
        "tags": ["readonly", "replica"]
      },
      "Reports": {
        "connectionString": "Data Source=reports.db;Cache=Shared",
        "provider": "Dapper",
        "timeoutSeconds": 60,
        "poolSize": 10,
        "enabled": true,
        "tags": ["reporting", "analytics"]
      },
      "Analytics": {
        "connectionString": "mongodb://localhost:27017/Analytics",
        "provider": "MongoDB",
        "timeoutSeconds": 30,
        "enabled": true,
        "tags": ["analytics", "nosql"]
      },
      "Legacy": {
        "connectionString": "Server=legacy.db;Database=OldApp;...",
        "provider": "EntityFramework",
        "enabled": false,
        "tags": ["legacy", "migration-pending"]
      }
    }
  }
}
```

### Configuration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `defaultConnection` | string | `"Default"` | Connection used when none is specified |
| `connections` | object | `{}` | Map of named connection configurations |
| `.<name>.connectionString` | string | *(required)* | Database connection string |
| `.<name>.provider` | string | `"EntityFramework"` | Provider identifier |
| `.<name>.timeoutSeconds` | int | `30` | Command timeout |
| `.<name>.poolSize` | int? | `null` | Connection pool size |
| `.<name>.enabled` | bool | `true` | Whether this connection is active |
| `.<name>.tags` | string[]? | `null` | Categorization tags |

## Named Connections

### Defining Connections

Connections are defined in the `connections` map. Each key is a unique name used to resolve the connection at runtime:

```json
{
  "connections": {
    "OrdersDb": { ... },
    "InventoryDb": { ... },
    "LoggingDb": { ... }
  }
}
```

### Connection Metadata

Each connection carries metadata accessible via the `IDataConnection` interface:

```csharp
public interface IDataConnection
{
    string Name { get; }
    string ProviderType { get; }
    string ConnectionString { get; }  // May be masked in logs
    IReadOnlyList<string> Tags { get; }
    bool IsEnabled { get; }
    TimeSpan Timeout { get; }
}
```

## Connection Resolution

### Injecting IDatabaseSelector

```csharp
public class OrderService
{
    private readonly IDatabaseSelector _db;

    public OrderService(IDatabaseSelector db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        // Resolve by name
        var ordersDb = _db.For("OrdersDb");
        var repo = ordersDb.GetRepository<Order>();
        return await repo.GetAllAsync();
    }

    public async Task<Report> GenerateReportAsync()
    {
        // Different provider for reporting
        var reportsDb = _db.For("Reports");
        using var connection = reportsDb.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<Report>(
            "SELECT * FROM DailyReport WHERE Date = @Date",
            new { Date = DateTime.UtcNow.Date });
    }
}
```

### Using the Default Connection

```csharp
// Resolves the connection named in "defaultConnection"
var defaultDb = _db.For("Default");

// Or use the default without specifying
var defaultRepo = _db.Default.GetRepository<User>();
```

### Resolving by Tags

```csharp
// Get the first enabled connection with a matching tag
var readOnlyDb = _db.ForTag("readonly");

// Get all connections with a specific tag
var analyticsDbs = _db.AllWithTag("analytics");
foreach (var db in analyticsDbs)
{
    // Process each analytics database
}

// Check if a tag exists
bool hasReplica = _db.HasTag("replica");
```

### Listing All Connections

```csharp
var allConnections = _db.All();
foreach (var conn in allConnections)
{
    Console.WriteLine(
        $"{conn.Name}: {conn.ProviderType} " +
        $"[Enabled={conn.IsEnabled}, Tags={string.Join(", ", conn.Tags)}]");
}
```

### Dynamic Resolution

```csharp
public class TenantService
{
    private readonly IDatabaseSelector _db;

    public TenantService(IDatabaseSelector db)
    {
        _db = db;
    }

    public async Task<User> GetUserForTenantAsync(
        string tenantId, int userId)
    {
        // Resolve connection by tenant ID (tenant_db_<id>)
        var tenantDb = _db.For($"Tenant_{tenantId}");
        var repo = tenantDb.GetRepository<User>();
        return await repo.FindAsync(userId);
    }
}
```

## Per-Connection Providers

Each connection can use a different provider. Register all providers you need:

```csharp
builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()     // For Default, ReadReplica
    .UseProvider<DapperDataProvider>()     // For Reports
    .UseProvider<PostgresDataProvider>()   // For Analytics
    .UseProvider<MongoDbProvider>()        // For Metrics
    .Build();
```

### Provider Resolution

The system resolves the correct provider for each connection based on the `provider` field in configuration:

| `provider` Value | Provider Class |
|-----------------|----------------|
| `EntityFramework` | `EfCoreDataProvider` |
| `Dapper` | `DapperDataProvider` |
| `Sqlite` | `SqliteDataProvider` |
| `PostgreSQL` | `PostgresDataProvider` |
| `MongoDB` | `MongoDbProvider` |

## Health Checks for Multiple Databases

When using multiple databases, health checks run against **all enabled connections**:

```bash
# Check health of all connections
nextnet db health

# Check health of a specific connection
nextnet db health Reports

# Watch all connections with auto-refresh
nextnet db health watch
```

### Aggregated Response

```json
{
  "status": "Degraded",
  "timestamp": "2026-06-06T12:00:00Z",
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
      "status": "Healthy",
      "description": "Database is reachable and responding.",
      "duration": "00:00:00.0080000"
    },
    {
      "name": "Reports",
      "provider": "Dapper",
      "status": "Degraded",
      "description": "Response time exceeds threshold (150ms).",
      "duration": "00:00:00.1500000"
    },
    {
      "name": "Analytics",
      "provider": "MongoDB",
      "status": "Unhealthy",
      "description": "Connection refused at mongodb://localhost:27017.",
      "duration": "00:00:05.0000000"
    }
  ]
}
```

## Use Cases

### Read/Write Splitting

```csharp
public class BlogService
{
    private readonly IDatabaseSelector _db;

    public BlogService(IDatabaseSelector db)
    {
        _db = db;
    }

    // Writes go to the primary
    public async Task CreatePostAsync(BlogPost post)
    {
        var primary = _db.ForTag("write");
        var repo = primary.GetRepository<BlogPost>();
        await repo.InsertAsync(post);
    }

    // Reads go to the replica
    public async Task<PagedResult<BlogPost>> GetPostsAsync(int page, int pageSize)
    {
        var replica = _db.ForTag("readonly");
        var repo = replica.GetRepository<BlogPost>();
        return await repo.GetAllAsync(page: page, pageSize: pageSize);
    }
}
```

### Multi-Tenant Database-Per-Tenant

```json
{
  "connections": {
    "Tenant_acme": {
      "connectionString": "Host=acme.db;Database=myapp;Username=app;Password=secret",
      "provider": "EntityFramework",
      "tags": ["tenant"]
    },
    "Tenant_globex": {
      "connectionString": "Host=globex.db;Database=myapp;Username=app;Password=secret",
      "provider": "EntityFramework",
      "tags": ["tenant"]
    }
  }
}
```

```csharp
// Dynamic tenant resolution
var tenantDb = _db.For($"Tenant_{currentTenantId}");
```

## Best Practices

| Practice | Rationale |
|----------|-----------|
| **Use descriptive connection names** | Match their purpose (e.g., `Primary`, `ReadReplica`, `Reporting`) |
| **Tag connections for grouping** | Tags enable dynamic resolution by capability |
| **Set appropriate timeouts per connection** | Reporting queries may need longer timeouts |
| **Disable unused connections** | Set `"enabled": false` instead of removing config |
| **Register only the providers you need** | Avoid unnecessary provider initialization |
| **Use environment variables for connection strings** | Keep secrets out of config files |
| **Monitor all connections in health checks** | Configure alerts for unhealthy connections |
| **Test connection failover** | Verify replica promotion works when primary is down |

## Limitations

| Limitation | Description | Mitigation |
|------------|-------------|------------|
| **No distributed transactions** | Cross database transactions are not supported. Each connection operates independently. | Use the Saga pattern or Outbox pattern for distributed operations. |
| **No cross-DB queries** | You cannot JOIN across connections or providers. | Perform queries separately and combine in application code. |
| **No automatic failover** | NextNet does not detect or handle database failures automatically. | Use connection pooling health checks and implement application level retry logic. |
| **Different provider capabilities** | Not all `IRepository<T>` methods are equally efficient across providers. | Test specific query patterns on each provider. |
| **Connection metadata is static** | Connection configurations are read at startup and cached. | Restart the application to pick up configuration changes (or use config reload). |

### Saga Pattern for Multi-DB Operations

```csharp
public async Task PlaceOrderAsync(Order order)
{
    var ordersDb = _db.For("OrdersDb");
    var inventoryDb = _db.For("InventoryDb");

    var ordersRepo = ordersDb.GetRepository<Order>();
    var inventoryRepo = inventoryDb.GetRepository<Inventory>();

    try
    {
        await ordersRepo.InsertAsync(order);
        await inventoryRepo.UpdateAsync(inventory); // May fail
    }
    catch
    {
        // Compensating action: rollback the order
        await ordersRepo.DeleteAsync(order.Id);
        throw;
    }
}
```

## See Also

- [Configuration Reference](configuration.md)
- [Health Checks](health-checks.md)
- [EF Core Provider](providers/ef-core.md)
- [Dapper Provider](providers/dapper.md)
- [MongoDB Provider](providers/mongodb.md)
- [Troubleshooting](troubleshooting.md)
