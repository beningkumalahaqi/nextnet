# Dapper Provider

The `NextNet.Data.Dapper` package provides a lightweight Dapper-based provider for the NextNet data layer. It is ideal for high-performance scenarios where full ORM features are not needed and you want maximum control over SQL.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [Connection Management](#connection-management)
- [Repository Pattern](#repository-pattern)
- [Raw SQL Queries](#raw-sql-queries)
- [Migrations (SQL Scripts)](#migrations-sql-scripts)
- [Stored Procedures](#stored-procedures)
- [Performance Tips](#performance-tips)
- [Health Checks](#health-checks)

## Installation

```bash
# Via CLI
nextnet add data dapper

# Or manually
dotnet add package NextNet.Data.Dapper

# Also add your ADO.NET provider
dotnet add package Microsoft.Data.SqlClient
# or
dotnet add package Npgsql
# or
dotnet add package Microsoft.Data.Sqlite
```

## Configuration

### Registration

```csharp
using NextNet.Data.Dapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<DapperDataProvider>()
    .Build();
```

### nextnet.config.json

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "Server=.;Database=MyApp;Trusted_Connection=True;TrustServerCertificate=True;",
        "provider": "Dapper",
        "timeoutSeconds": 30,
        "poolSize": 50,
        "enabled": true
      }
    },
    "migration": {
      "directory": "Migrations",
      "historyTableName": "__NextNetMigrations"
    }
  }
}
```

## Connection Management

The provider uses the `DapperConnectionFactory` to create and manage `IDbConnection` instances.

### Basic Connection Access

```csharp
public class ProductService
{
    private readonly IDataProvider _provider;

    public ProductService(IDataProvider provider)
    {
        _provider = provider;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        using var connection = _provider.CreateConnection();
        await connection.OpenAsync();

        return await connection.QueryAsync<Product>(
            "SELECT * FROM Products WHERE IsActive = @Active",
            new { Active = true });
    }
}
```

### Connection Lifetime Options

| Approach | When to Use |
|----------|------------|
| `CreateConnection()` — create/open/close per operation | Short-lived operations, simple queries |
| `IConnectionFactory` with pooling | High-throughput scenarios |
| `DbConnection` directly via DI | When you need full ADO.NET control |

### Transaction Support

```csharp
public async Task TransferFundsAsync(int fromId, int toId, decimal amount)
{
    using var connection = _provider.CreateConnection();
    await connection.OpenAsync();
    using var transaction = connection.BeginTransaction();

    try
    {
        await connection.ExecuteAsync(
            "UPDATE Accounts SET Balance = Balance - @Amount WHERE Id = @Id",
            new { Amount = amount, Id = fromId },
            transaction: transaction);

        await connection.ExecuteAsync(
            "UPDATE Accounts SET Balance = Balance + @Amount WHERE Id = @Id",
            new { Amount = amount, Id = toId },
            transaction: transaction);

        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

## Repository Pattern

### IRepository<T>

```csharp
public class CustomerRepository
{
    private readonly IRepository<Customer> _repository;

    public CustomerRepository(IRepository<Customer> repository)
    {
        _repository = repository;
    }

    // Standard CRUD
    public async Task<Customer?> GetByIdAsync(int id)
        => await _repository.FindAsync(id);

    public async Task<PagedResult<Customer>> GetAllAsync(int page, int pageSize)
        => await _repository.GetAllAsync(page: page, pageSize: pageSize);

    public async Task<Customer> CreateAsync(Customer customer)
        => await _repository.InsertAsync(customer);

    public async Task<Customer> UpdateAsync(Customer customer)
        => await _repository.UpdateAsync(customer);

    public async Task DeleteAsync(int id)
        => await _repository.DeleteAsync(id);

    // Filtered queries
    public async Task<PagedResult<Customer>> SearchAsync(
        string term, int page, int pageSize)
    {
        return await _repository.GetAllAsync(
            filter: c => c.Name.Contains(term)
                      || c.Email.Contains(term),
            orderBy: c => c.Name,
            descending: false,
            page: page,
            pageSize: pageSize);
    }
}
```

### Custom Repository with SQL

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<OrderSummary?> GetOrderSummaryAsync(int orderId);
    Task<IEnumerable<Order>> GetHighValueOrdersAsync(decimal minTotal);
}

public sealed class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(
        IDataConnection connection,
        ILogger<OrderRepository> logger) : base(connection, logger) { }

    public async Task<OrderSummary?> GetOrderSummaryAsync(int orderId)
    {
        var sql = @"
            SELECT o.Id, o.OrderDate, o.Total,
                   c.Name AS CustomerName, c.Email
            FROM Orders o
            INNER JOIN Customers c ON c.Id = o.CustomerId
            WHERE o.Id = @OrderId";

        return await ExecuteSingleAsync<OrderSummary>(
            sql, new { OrderId = orderId });
    }

    public async Task<IEnumerable<Order>> GetHighValueOrdersAsync(decimal minTotal)
    {
        var sql = "SELECT * FROM Orders WHERE Total >= @MinTotal ORDER BY Total DESC";
        return await ExecuteQueryAsync<Order>(
            sql, new { MinTotal = minTotal });
    }
}
```

## Raw SQL Queries

### Querying

```csharp
using var connection = _provider.CreateConnection();
await connection.OpenAsync();

// Multi-mapping (one-to-one)
var sql = @"
    SELECT o.*, c.*
    FROM Orders o
    INNER JOIN Customers c ON c.Id = o.CustomerId
    WHERE o.Id = @Id";

var order = await connection.QueryAsync<Order, Customer, Order>(
    sql,
    (order, customer) =>
    {
        order.Customer = customer;
        return order;
    },
    new { Id = orderId },
    splitOn: "Id");

// Multi-result sets
var multi = await connection.QueryMultipleAsync(
    "SELECT * FROM Products; SELECT COUNT(*) FROM Products;");

var products = await multi.ReadAsync<Product>();
var count = await multi.ReadFirstAsync<int>();
```

### Executing

```csharp
// Insert with identity return
var id = await connection.ExecuteScalarAsync<int>(@"
    INSERT INTO Products (Name, Price)
    OUTPUT INSERTED.Id
    VALUES (@Name, @Price)",
    new { Name = "Widget", Price = 9.99m });

// Bulk insert
await connection.ExecuteAsync(
    "INSERT INTO Products (Name, Price) VALUES (@Name, @Price)",
    new[]
    {
        new { Name = "Widget", Price = 9.99m },
        new { Name = "Gadget", Price = 14.99m }
    });
```

## Migrations (SQL Scripts)

Dapper uses plain SQL script files for migrations. Place numbered scripts in the `Migrations/` directory:

```
Migrations/
├── 001_CreateUsersTable.sql
├── 002_AddEmailToUsers.sql
├── 003_CreateOrdersTable.sql
└── 004_SeedData.sql
```

### Example Migration Script

```sql
-- 001_CreateUsersTable.sql
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);

-- Record this migration in the history table
INSERT INTO __NextNetMigrations (MigrationId, AppliedOn, Script)
VALUES ('001_CreateUsersTable', datetime('now'), '001_CreateUsersTable.sql');
```

> [!TIP]
> NextNet automatically manages the `__NextNetMigrations` history table for you. Include the INSERT statement in your scripts for manual control, or let the CLI handle it by using `nextnet db migration add <name>`.

### CLI Usage

```bash
# Create a new migration script
nextnet db migration add AddUserAge

# Apply pending migrations
nextnet db migrate

# Rollback the last migration
nextnet db rollback
```

## Stored Procedures

```csharp
using var connection = _provider.CreateConnection();
await connection.OpenAsync();

// Execute stored procedure
var result = await connection.QueryAsync<Product>(
    "usp_GetProductsByCategory",
    new { CategoryId = 1 },
    commandType: CommandType.StoredProcedure);

// With output parameters
var parameters = new DynamicParameters();
parameters.Add("@CategoryId", 1);
parameters.Add("@Count", dbType: DbType.Int32, direction: ParameterDirection.Output);

await connection.ExecuteAsync("usp_CountProductsInCategory",
    parameters,
    commandType: CommandType.StoredProcedure);

var count = parameters.Get<int>("@Count");
```

## Performance Tips

| Tip | Explanation |
|-----|-------------|
| **Always use parameterized queries** | Prevents SQL injection and enables query plan caching. Never concatenate user input. |
| **Use `AsList()` or `AsArray()`** | Avoids extra allocation when reusing parameter collections. |
| **Prefer `ExecuteScalarAsync` for single values** | More efficient than `QueryAsync` + `FirstOrDefault`. |
| **Use buffered = false for large result sets** | Reduces memory pressure by streaming results. |
| **Batch inserts with `ExecuteAsync` and arrays** | One round-trip for multiple rows. |
| **Enable query plan caching** | Same SQL text reuses cached plans — use consistent formatting. |
| **Use `ColumnMapping` for non-matching names** | Avoids runtime reflection overhead. |
| **Disable tracking (not applicable — Dapper has none)** | Dapper is already a read-only micro-ORM with no change tracking. |
| **Use `DynamicParameters` for complex params** | Better performance than anonymous objects for large parameter sets. |
| **Consider `SqlMapper.AddTypeHandler` for custom types** | Registers efficient type mappings for domain-specific types. |

### Benchmark Guidance

| Operation | Dapper | EF Core | Winner |
|-----------|--------|---------|--------|
| Single row by PK | ~2 µs | ~10 µs | Dapper |
| 1000 row read | ~50 µs | ~200 µs | Dapper |
| Simple insert | ~3 µs | ~15 µs | Dapper |
| Complex query with joins | SQL-only | LINQ | Use case dependent |

## Health Checks

The provider includes `DapperHealthCheck` that verifies database connectivity by executing `SELECT 1`.

```csharp
// Registered automatically when using UseProvider<DapperDataProvider>()
```

See [Health Checks](../health-checks.md).

## Security

```csharp
// ✅ SAFE — parameterized query
var user = await connection.QueryAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = userId });

// ❌ UNSAFE — SQL injection risk
var user = await connection.QueryAsync<User>(
    $"SELECT * FROM Users WHERE Id = {userId}");

// ✅ SAFE — DynamicParameters
var parameters = new DynamicParameters();
parameters.Add("@Id", userId);
var user = await connection.QueryAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id", parameters);
```

## See Also

- [Getting Started](../getting-started.md)
- [Configuration Reference](../configuration.md)
- [Migrations Guide](../migrations.md)
- [Scaffolding Guide](../scaffolding.md)
- [Performance Troubleshooting](../troubleshooting.md#performance-tips)
