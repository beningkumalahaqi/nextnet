# MongoDB Provider

The `NextNet.Data.MongoDB` package provides a document database provider for the NextNet data layer, built on the official `MongoDB.Driver`.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [BSON Conventions](#bson-conventions)
- [Collection Naming](#collection-naming)
- [Repository Pattern](#repository-pattern)
- [Indexes (Migration)](#indexes-migration)
- [Query Patterns](#query-patterns)
- [Aggregation Pipeline](#aggregation-pipeline)
- [Health Checks](#health-checks)

## Installation

```bash
# Via CLI
nextnet add data mongodb

# Or manually
dotnet add package NextNet.Data.MongoDB
```

## Configuration

### Registration

```csharp
using NextNet.Data.MongoDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<MongoDbProvider>()
    .Build();
```

### nextnet.config.json

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "mongodb://localhost:27017/MyApp",
        "provider": "MongoDB",
        "timeoutSeconds": 30,
        "enabled": true
      }
    },
    "migration": {
      "autoApply": false
    }
  }
}
```

### Connection String Options

```
# Standalone
mongodb://localhost:27017/MyApp

# Replica set
mongodb://host1:27017,host2:27017,host3:27017/MyApp?replicaSet=rs0

# With authentication
mongodb://user:password@localhost:27017/MyApp?authSource=admin

# Atlas (cloud)
mongodb+srv://user:password@cluster0.abcde.mongodb.net/MyApp?retryWrites=true&w=majority

# Connection pool settings
mongodb://localhost:27017/MyApp?maxPoolSize=100&minPoolSize=10
```

## BSON Conventions

The provider configures BSON serialization conventions automatically. You can customize them:

```csharp
// Global conventions (applied once at startup)
builder.Services.Configure<MongoDbConventionOptions>(options =>
{
    // Use camelCase for all BSON elements
    options.ConventionPack = new ConventionPack
    {
        new CamelCaseElementNameConvention(),
        new IgnoreExtraElementsConvention(true),
        new EnumRepresentationConvention(BsonType.String)
    };
});
```

### Common Conventions

| Convention | Description |
|------------|-------------|
| `CamelCaseElementNameConvention` | Maps C# PascalCase to BSON camelCase |
| `IgnoreExtraElementsConvention(true)` | Ignores unknown BSON fields during deserialization |
| `EnumRepresentationConvention(BsonType.String)` | Stores enums as strings instead of integers |
| `IgnoreIfDefaultConvention` | Omits default valued fields during serialization |
| `NamedIdMemberConvention` | Maps `Id` property to `_id` in MongoDB |

### Custom Class Maps

```csharp
[BsonIgnoreExtraElements]
public sealed record Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("name")]
    public string Name { get; init; } = string.Empty;

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; init; }

    [BsonElement("tags")]
    [BsonIgnoreIfNull]
    public List<string>? Tags { get; init; }
}
```

## Collection Naming

By default, the provider pluralizes entity type names for collection names:

| Entity Type | Collection Name |
|------------|----------------|
| `User` | `users` |
| `Order` | `orders` |
| `ProductCategory` | `productCategories` |
| `BlogPost` | `blogPosts` |

### Custom Collection Names

```csharp
[BsonCollection("products")]
public sealed record Product
{
    // ...
}
```

Or via configuration:

```csharp
builder.Services.Configure<MongoDbNamingOptions>(options =>
{
    options.CollectionNamingConvention = CollectionNamingConvention.SnakeCase;
    // Options: Pluralized (default), SnakeCase, LowerCase, Custom
});
```

## Repository Pattern

### IRepository<T>

```csharp
public class OrderRepository
{
    private readonly IRepository<Order> _repository;

    public OrderRepository(IRepository<Order> repository)
    {
        _repository = repository;
    }

    // Standard CRUD
    public async Task<Order?> GetByIdAsync(string id)
        => await _repository.FindAsync(id);

    public async Task<PagedResult<Order>> GetAllAsync(int page, int pageSize)
        => await _repository.GetAllAsync(page: page, pageSize: pageSize);

    public async Task<Order> CreateAsync(Order order)
        => await _repository.InsertAsync(order);

    public async Task<Order> UpdateAsync(Order order)
        => await _repository.UpdateAsync(order);

    public async Task DeleteAsync(string id)
        => await _repository.DeleteAsync(id);

    // Filtered queries
    public async Task<PagedResult<Order>> GetByCustomerAsync(
        string customerId, int page, int pageSize)
    {
        return await _repository.GetAllAsync(
            filter: o => o.CustomerId == customerId,
            orderBy: o => o.CreatedAt,
            descending: true,
            page: page,
            pageSize: pageSize);
    }
}
```

### Custom Repository with Aggregations

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<decimal> GetCustomerTotalSpentAsync(string customerId);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int minutes);
}

public sealed class OrderRepository : MongoRepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(
        IDataConnection connection,
        ILogger<OrderRepository> logger) : base(connection, logger) { }

    public async Task<decimal> GetCustomerTotalSpentAsync(string customerId)
    {
        var pipeline = new EmptyPipelineDefinition<Order>()
            .Match(o => o.CustomerId == customerId)
            .Group(o => o.CustomerId, g => new
            {
                Total = g.Sum(o => o.Total)
            });

        var result = await AggregateAsync(pipeline);
        return result?.FirstOrDefault()?.Total ?? 0;
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int minutes)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-minutes);
        return await FindAllAsync(o => o.CreatedAt >= cutoff);
    }
}
```

## Indexes (Migration)

MongoDB indexes are managed through the provider's migration engine.

### Defining Indexes on Entities

```csharp
public sealed record Product
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
```

### Index Configuration

```csharp
[BsonCollection("products")]
public sealed class ProductIndexes : IMongoIndexConfiguration<Product>
{
    public IEnumerable<CreateIndexModel<Product>> GetIndexes()
    {
        yield return new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Name),
            new CreateIndexOptions { Name = "idx_name", Unique = false });

        yield return new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys
                .Ascending(p => p.Category)
                .Descending(p => p.CreatedAt),
            new CreateIndexOptions { Name = "idx_category_created" });

        yield return new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Text(p => p.Name),
            new CreateIndexOptions { Name = "idx_name_text" });
    }
}
```

### CLI

```bash
# Create index migration
nextnet db migration add AddProductIndexes

# Apply indexes
nextnet db migrate

# List current indexes
nextnet db explore
```

## Query Patterns

### Basic Queries

```csharp
// Equality
var user = await collection.Find(u => u.Email == "test@example.com")
    .FirstOrDefaultAsync();

// Comparison
var expensive = await collection.Find(p => p.Price > 100)
    .SortByDescending(p => p.Price)
    .ToListAsync();

// Text search (requires text index)
var search = await collection.Find(
    Builders<Product>.Filter.Text("widget"))
    .ToListAsync();

// Date range
var recent = await collection.Find(o =>
    o.CreatedAt >= DateTime.UtcNow.AddDays(-7))
    .ToListAsync();
```

### Projections

```csharp
var summary = await collection.Find(
    FilterDefinition<Product>.Empty)
    .Project(p => new { p.Name, p.Price })
    .ToListAsync();
```

### Pagination

```csharp
var page = 1;
var pageSize = 20;

var results = await collection.Find(filter)
    .SortByDescending(o => o.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Limit(pageSize)
    .ToListAsync();

var totalCount = await collection.CountDocumentsAsync(filter);
```

## Aggregation Pipeline

```csharp
var pipeline = collection.Aggregate()
    .Match(o => o.Status == "shipped")
    .Group(o => o.CustomerId, g => new
    {
        CustomerId = g.Key,
        TotalSpent = g.Sum(o => o.Total),
        OrderCount = g.Count(),
        AvgOrder = g.Average(o => o.Total)
    })
    .SortByDescending(r => r.TotalSpent)
    .Limit(10);

var topCustomers = await pipeline.ToListAsync();
```

### Lookup (Join)

```csharp
var pipeline = collection.Aggregate()
    .Lookup<Order, Customer, OrderWithCustomer>(
        foreignCollection: customerCollection,
        localKey: o => o.CustomerId,
        foreignKey: c => c.Id,
        resultKey: o => o.Customer)
    .Match(owc => owc.Customer.IsActive);

var results = await pipeline.ToListAsync();
```

## Document Modeling

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonCollection("orders")]
public sealed record Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; init; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    [BsonElement("items")]
    public List<OrderItem> Items { get; init; } = [];

    [BsonElement("total")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Total { get; init; }

    [BsonElement("status")]
    public OrderStatus Status { get; init; } = OrderStatus.Pending;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public sealed record OrderItem
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public enum OrderStatus { Pending, Shipped, Delivered, Cancelled }
```

## Health Checks

The provider includes `MongoDbHealthCheck` that verifies MongoDB connectivity by running a `ping` command against the admin database.

```csharp
// Registered automatically when using UseProvider<MongoDbProvider>()
```

See [Health Checks](../health-checks.md).

## See Also

- [Getting Started](../getting-started.md)
- [Configuration Reference](../configuration.md)
- [Migrations Guide](../migrations.md)
- [Scaffolding Guide](../scaffolding.md)
- [Multi-Database Support](../multi-database.md)
- [Troubleshooting](../troubleshooting.md)
