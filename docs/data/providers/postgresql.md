# PostgreSQL Provider

The `NextNet.Data.PostgreSQL` package provides a production grade PostgreSQL provider for the NextNet data layer, built on Npgsql and EF Core. It is the recommended provider for production relational database workloads.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [SSL/TLS Setup](#ssltls-setup)
- [Connection Pooling](#connection-pooling)
- [Docker Compose for Local Dev](#docker-compose-for-local-dev)
- [Environment Variables (12-Factor)](#environment-variables-12-factor)
- [Repository Pattern](#repository-pattern)
- [Migrations](#migrations)
- [Advanced Features](#advanced-features)
- [Health Checks](#health-checks)

## Installation

```bash
# Via CLI
nextnet add data postgresql

# Or manually
dotnet add package NextNet.Data.PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## Configuration

### Registration

```csharp
using NextNet.Data.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<PostgresDataProvider>()
    .Build();
```

### nextnet.config.json

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "Host=localhost;Database=myapp;Username=app;Password=secret",
        "provider": "PostgreSQL",
        "timeoutSeconds": 30,
        "poolSize": 100,
        "enabled": true
      }
    },
    "migration": {
      "autoApply": false,
      "directory": "Migrations",
      "historyTableName": "__NextNetMigrations",
      "timeoutSeconds": 120
    }
  }
}
```

### Connection Strings

```ini
# Local development
Host=localhost;Database=myapp;Username=app;Password=secret

# With specific port
Host=localhost;Port=5433;Database=myapp;Username=app;Password=secret

# Docker/container
Host=postgres;Database=myapp;Username=app;Password=secret

# Azure Database for PostgreSQL
Host=mydb.postgres.database.azure.com;Database=myapp;Username=app@mydb;Password=secret;SSL Mode=Require;Trust Server Certificate=true

# AWS RDS
Host=mydb.abcdefghijk.us-east-1.rds.amazonaws.com;Database=myapp;Username=app;Password=secret;SSL Mode=Require

# With connection pooling settings
Host=localhost;Database=myapp;Username=app;Password=secret;Maximum Pool Size=100;Minimum Pool Size=10;Connection Idle Lifetime=300;Connection Pruning Interval=60
```

### Connection String Builder

```csharp
using NextNet.Data.PostgreSQL;

var builder = new PostgresConnectionStringBuilder
{
    Host = "db.example.com",
    Port = 5432,
    Database = "myapp",
    Username = "app_user",
    Password = "secure_password",
    MaxPoolSize = 50,
    SslMode = PostgresSslMode.Require
};
var connectionString = builder.ToString();
```

## SSL/TLS Setup

### Configuration

```csharp
services.Configure<PostgresSslOptions>(options =>
{
    options.Mode = PostgresSslMode.Require;
    options.TrustServerCertificate = false;  // true for self-signed certs only
    options.RootCertificatePath = "/etc/ssl/certs/ca-certificates.crt";
    options.ClientCertificatePath = "/etc/ssl/certs/client.crt";
    options.ClientKeyPath = "/etc/ssl/private/client.key";
});
```

### SSL Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| `Disable` | No SSL | Local development only |
| `Prefer` | SSL if available, fallback to plain | Development/Staging |
| `Require` | SSL required, no CA verification | Internal networks |
| `VerifyCA` | SSL + CA verification | Production |
| `VerifyFull` | SSL + CA + hostname verification | Production (recommended) |

### Connection String SSL Options

```ini
# Require SSL without CA check (development/staging)
Host=db.example.com;Database=myapp;Username=app;Password=secret;SSL Mode=Require;Trust Server Certificate=true

# Verify full SSL (production)
Host=db.example.com;Database=myapp;Username=app;Password=secret;SSL Mode=VerifyFull;Root Certificate=/etc/ssl/certs/ca.pem
```

## Connection Pooling

The provider uses Npgsql's built in connection pooling. Configure via options:

```csharp
services.Configure<PostgresPoolingOptions>(options =>
{
    options.MaxPoolSize = 100;          // Max connections in pool (default: 100)
    options.MinPoolSize = 10;           // Min connections maintained (default: 0)
    options.ConnectionIdleLifetime = 300;    // Seconds before idle connection is closed (default: 300)
    options.ConnectionPruningInterval = 60;  // Seconds between pool pruning (default: 60)
    options.ConnectionLifetime = 1800;       // Max connection lifetime in seconds (default: 0 = no limit)
});
```

### Pooling Best Practices

| Scenario | Recommended Pool Size | Rationale |
|----------|----------------------|-----------|
| Small API (< 100 req/s) | 10-20 | Minimal concurrency needs |
| Medium API (< 1000 req/s) | 20-50 | Balance between resource usage and throughput |
| High throughput API | 50-200 | Scale with concurrent requests |
| Background workers | 2-5 | Limited concurrency, long running queries |
| Serverless/Functions | 1-5 | Short lived, auto scaling instances |

> [!TIP]
> Set `MaxPoolSize` to roughly `(max concurrent requests) * (average query duration in seconds)`.

## Docker Compose for Local Dev

```yaml
# docker-compose.yml
version: "3.9"

services:
  postgres:
    image: postgres:16-alpine
    container_name: myapp-db
    environment:
      POSTGRES_USER: app
      POSTGRES_PASSWORD: secret
      POSTGRES_DB: myapp
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
      - ./docker/postgres/init:/docker-entrypoint-initdb.d
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U app -d myapp"]
      interval: 5s
      timeout: 5s
      retries: 5

  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: myapp-pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@example.com
      PGADMIN_DEFAULT_PASSWORD: admin
    ports:
      - "5050:80"
    depends_on:
      postgres:
        condition: service_healthy

volumes:
  pgdata:
```

```bash
# Start the database
docker compose up -d

# Verify connection
docker compose exec postgres psql -U app -d myapp -c "SELECT 1;"

# Stop and clean up
docker compose down -v
```

### Initialization Scripts

Place SQL scripts in `docker/postgres/init/` to run on first startup:

```sql
-- docker/postgres/init/001_create_extensions.sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "postgis";  -- if using PostGIS
```

## Environment Variables (12-Factor)

Follow the [12-factor app](https://12factor.net/config) methodology for database configuration:

```bash
# Production environment variables
export DATABASE_URL="Host=db.example.com;Database=myapp;Username=app;Password=${DB_PASSWORD};SSL Mode=VerifyFull"
export DB_PASSWORD="$(cat /run/secrets/db_password)"
export DB_POOL_SIZE=50
export DB_TIMEOUT_SECONDS=30
```

### Mapping to nextnet.config.json

```bash
# Override using standard .NET configuration environment variables

# Connection string
NextNet_Data__Connections__Default__ConnectionString="Host=prod.db;Database=myapp;..."

# Pool size
NextNet_Data__Connections__Default__PoolSize=100

# Timeout
NextNet_Data__Connections__Default__TimeoutSeconds=60

# Disable a connection
NextNet_Data__Connections__Reports__Enabled=false
```

### Using User Secrets (Development)

```bash
dotnet user-secrets set "data:connections:Default:connectionString" \
    "Host=localhost;Database=myapp_dev;Username=dev;Password=devpass"
```

## Repository Pattern

```csharp
public class CustomerRepository
{
    private readonly IRepository<Customer> _repository;

    public CustomerRepository(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        var results = await _repository.GetAllAsync(
            filter: c => c.Email == email,
            page: 1,
            pageSize: 1);
        return results.Items.FirstOrDefault();
    }

    public async Task<PagedResult<Customer>> SearchAsync(
        string searchTerm, int page, int pageSize)
    {
        return await _repository.GetAllAsync(
            filter: c => c.Name.Contains(searchTerm)
                      || c.Email.Contains(searchTerm),
            orderBy: c => c.CreatedAt,
            descending: true,
            page: page,
            pageSize: pageSize);
    }
}
```

### PostgreSQL-Specific Queries

PostgreSQL supports advanced query types that you can leverage via raw SQL or EF Core:

```csharp
public class SearchService
{
    private readonly IDataProvider _provider;

    public SearchService(IDataProvider provider)
    {
        _provider = provider;
    }

    // Full-text search
    public async Task<IEnumerable<Product>> FullTextSearchAsync(string query)
    {
        using var connection = _provider.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<Product>(@"
            SELECT *, ts_rank(to_tsvector('english', Name || ' ' || Description),
                             plainto_tsquery('english', @Query)) AS rank
            FROM Products
            WHERE to_tsvector('english', Name || ' ' || Description)
                  @@ plainto_tsquery('english', @Query)
            ORDER BY rank DESC
            LIMIT 20", new { Query = query });
    }

    // JSON query
    public async Task<IEnumerable<Order>> GetOrdersWithJsonDataAsync()
    {
        using var connection = _provider.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<Order>(@"
            SELECT *
            FROM Orders
            WHERE Metadata @> '{"priority": "high"}'::jsonb");
    }
}
```

## Migrations

PostgreSQL migrations use the EF Core migration engine.

```bash
# Create a migration
nextnet db migration add InitialCreate

# Apply
nextnet db migrate

# Rollback
nextnet db rollback
```

### Auto-Apply

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseProvider<PostgresDataProvider>()
    .WithAutoMigration()
    .Build();
```

See the full [Migrations guide](../migrations.md).

## Advanced Features

### PostGIS (Geospatial)

```csharp
// Add to project:
// dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite

// Configure
builder.Services.Configure<PostgresProviderOptions>(options =>
{
    options.UseNetTopologySuite = true;  // Enable PostGIS support
});

// Model
public sealed record Location
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Point Coordinates { get; init; } = null!;
}

// Query nearby locations
public async Task<IEnumerable<Location>> GetNearbyAsync(
    double latitude, double longitude, double radiusKm)
{
    var center = new Point(longitude, latitude) { SRID = 4326 };
    return await _repository.FindAllAsync(l =>
        l.Coordinates.Distance(center) < radiusKm * 1000);
}
```

### JSON/JSONB Columns

```csharp
public sealed record Order
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    // Stored as JSONB in PostgreSQL
    public Dictionary<string, object> Metadata { get; init; } = new();
}

// Configuration
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Metadata)
            .HasColumnType("jsonb");
    }
}
```

## Health Checks

PostgreSQL health checks verify database connectivity through the Npgsql connection.

```csharp
// Registered automatically when using UseProvider<PostgresDataProvider>()
```

See [Health Checks](../health-checks.md).

## See Also

- [Getting Started](../getting-started.md)
- [Configuration Reference](../configuration.md)
- [Migrations Guide](../migrations.md)
- [SQLite Provider](sqlite.md) -- for local development
- [Docker Compose Guide](../../guides/docker-compose.md)
- [Troubleshooting](../troubleshooting.md)
