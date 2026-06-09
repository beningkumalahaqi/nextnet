# Entity Framework Core Provider

The `NextNet.Data.EntityFramework` package provides a full featured Entity Framework Core provider for the NextNet data layer. It supports SQL Server, SQLite, PostgreSQL, and any other EF Core compatible database.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [DbContext Setup](#dbcontext-setup)
- [Entity Configuration](#entity-configuration)
- [Repository Pattern](#repository-pattern)
- [Migrations](#migrations)
- [Advanced Topics](#advanced-topics)
  - [Composite Keys](#composite-keys)
  - [Owned Types](#owned-types)
  - [Value Converters](#value-converters)
  - [Shadow Properties](#shadow-properties)
  - [Query Filters](#query-filters)
- [Health Checks](#health-checks)
- [Scaffolding](#scaffolding)

## Installation

```bash
# Via CLI
nextnet add data ef

# Or manually
dotnet add package NextNet.Data.EntityFramework
```

Add the EF Core provider for your actual database (if not using SQLite):

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
# or
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
# or
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

## Configuration

### Registration

```csharp
using NextNet.Data.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()
    .Build();
```

### nextnet.config.json

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "Server=(localdb)\\MSSQLLocalDB;Database=MyApp;Trusted_Connection=True;",
        "provider": "EntityFramework",
        "timeoutSeconds": 30,
        "poolSize": 100,
        "enabled": true
      }
    },
    "migration": {
      "autoApply": false,
      "directory": "Migrations",
      "historyTableName": "__NextNetMigrations",
      "timeoutSeconds": 60
    }
  }
}
```

### Programmatic Configuration

```csharp
builder.Services.AddNextNetData()
    .ConfigureEfCore(options =>
    {
        // Override DbContext options
        options.DbContextPoolSize = 64;
        options.EnableSensitiveDataLogging = false;
        options.EnableDetailedErrors = false;
    })
    .UseProvider<EfCoreDataProvider>()
    .Build();
```

## DbContext Setup

The provider manages `AppDbContext` internally. By default, it creates a `DbContext` that maps entities discovered via the provider registry. You can customize the `DbContext`:

```csharp
// Custom AppDbContext for advanced configuration
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

Register the custom `DbContext`:

```csharp
builder.Services.AddNextNetData()
    .UseDbContext<AppDbContext>()  // Register custom DbContext
    .UseProvider<EfCoreDataProvider>()
    .Build();
```

### Accessing the DbContext Directly

```csharp
public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Set<User>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetUserOrdersWithDetailsAsync(int userId)
    {
        return await _context.Set<Order>()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .AsSplitQuery()  // Avoid cartesian explosion
            .ToListAsync();
    }
}
```

## Entity Configuration

### Data Annotations

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("users")]
public sealed record User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Fluent API (IEntityTypeConfiguration)

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");
    }
}
```

Apply via `ApplyConfigurationsFromAssembly` in your `DbContext.OnModelCreating`.

## Repository Pattern

### IRepository<T>

```csharp
public class UserRepository
{
    private readonly IRepository<User> _repository;

    public UserRepository(IRepository<User> repository)
    {
        _repository = repository;
    }

    // Basic CRUD
    public async Task<User?> GetByIdAsync(int id)
        => await _repository.FindAsync(id);

    public async Task<PagedResult<User>> GetAllAsync(int page, int pageSize)
        => await _repository.GetAllAsync(page: page, pageSize: pageSize);

    public async Task<User> CreateAsync(User user)
        => await _repository.InsertAsync(user);

    public async Task<User> UpdateAsync(User user)
        => await _repository.UpdateAsync(user);

    public async Task DeleteAsync(int id)
        => await _repository.DeleteAsync(id);

    // Filtering and ordering
    public async Task<PagedResult<User>> SearchAsync(
        string searchTerm, int page, int pageSize)
    {
        return await _repository.GetAllAsync(
            filter: u => u.Name.Contains(searchTerm)
                      || u.Email.Contains(searchTerm),
            orderBy: u => u.Name,
            descending: false,
            page: page,
            pageSize: pageSize);
    }

    // Count
    public async Task<int> CountActiveAsync()
        => await _repository.CountAsync(u => u.IsActive);

    // Existence check
    public async Task<bool> EmailExistsAsync(string email)
        => await _repository.ExistsAsync(u => u.Email == email);
}
```

### Custom Repository with Query Methods

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersInDateRangeAsync(
        DateTime from, DateTime to);

    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
}

public sealed class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(
        IDataConnection connection,
        ILogger<OrderRepository> logger) : base(connection, logger) { }

    public async Task<IEnumerable<Order>> GetOrdersInDateRangeAsync(
        DateTime from, DateTime to)
    {
        // Uses IQueryable from the underlying EF Core DbContext
        return await FindAllAsync(o => o.CreatedAt >= from
                                    && o.CreatedAt <= to);
    }

    public async Task<decimal> GetTotalRevenueAsync(
        DateTime from, DateTime to)
    {
        var orders = await FindAllAsync(o => o.CreatedAt >= from
                                          && o.CreatedAt <= to);
        return orders.Sum(o => o.Total);
    }
}
```

## Migrations

The provider uses the EF Core migration engine under the hood.

```bash
# Create a migration
nextnet db migration add AddUserStatus

# Apply migrations
nextnet db migrate

# Rollback the last migration
nextnet db rollback
```

### Auto-Apply on Startup

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()
    .WithAutoMigration()  // Applies pending migrations on startup
    .Build();
```

See the full [Migrations guide](../migrations.md) for details.

## Advanced Topics

### Composite Keys

```csharp
public sealed record OrderItem
{
    public int OrderId { get; init; }
    public int ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
```

Configure composite key via Fluent API:

```csharp
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => new { oi.OrderId, oi.ProductId });

        builder.Property(oi => oi.UnitPrice)
            .HasColumnType("decimal(18,2)");
    }
}
```

Or via data annotations:

```csharp
public sealed record OrderItem
{
    [Key, Column(Order = 0)]
    public int OrderId { get; init; }

    [Key, Column(Order = 1)]
    public int ProductId { get; init; }

    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
```

### Owned Types

```csharp
public sealed record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public sealed record Customer
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Address BillingAddress { get; init; } = new();
    public Address ShippingAddress { get; init; } = new();
}
```

Fluent configuration:

```csharp
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.OwnsOne(c => c.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("billing_street");
            address.Property(a => a.City).HasColumnName("billing_city");
            address.Property(a => a.ZipCode).HasColumnName("billing_zip");
            address.Property(a => a.Country).HasColumnName("billing_country");
        });

        builder.OwnsOne(c => c.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("shipping_street");
            address.Property(a => a.City).HasColumnName("shipping_city");
            address.Property(a => a.ZipCode).HasColumnName("shipping_zip");
            address.Property(a => a.Country).HasColumnName("shipping_country");
        });
    }
}
```

### Value Converters

```csharp
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// Store enums as strings
public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled
}

public sealed record Order
{
    public int Id { get; init; }
    public OrderStatus Status { get; init; }
}
```

```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
```

Custom value converter for complex types:

```csharp
public sealed class StringArrayConverter : ValueConverter<string[], string>
{
    public StringArrayConverter()
        : base(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
    { }
}

// Usage
builder.Property(e => e.Tags)
    .HasConversion<StringArrayConverter>();
```

### Shadow Properties

```csharp
public sealed record BlogPost
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    // No CreatedAt property on the entity
}
```

```csharp
public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        // Shadow property — stored in DB but not on the entity
        builder.Property<DateTime>("CreatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property<DateTime?>("LastModifiedAt");
    }
}

// Access shadow properties
public class BlogService
{
    private readonly AppDbContext _context;

    public BlogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SetLastModifiedAsync(int postId)
    {
        var entry = _context.Entry(
            await _context.Set<BlogPost>().FindAsync(postId));
        entry.Property("LastModifiedAt").CurrentValue = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
```

### Query Filters

Global query filters for soft deletes or multi tenancy:

```csharp
public sealed record Product
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public int TenantId { get; init; }
}
```

```csharp
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

Tenant aware filter:

```csharp
public sealed class AppDbContext : DbContext
{
    private readonly int _currentTenantId;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _currentTenantId = tenantService.CurrentTenantId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasQueryFilter(
            p => p.TenantId == _currentTenantId && !p.IsDeleted);
    }
}
```

## Health Checks

The provider includes `EfCoreHealthCheckProvider` for database connectivity checks. It executes a simple `SELECT 1` query against the database.

```csharp
// Registered automatically when using UseProvider<EfCoreDataProvider>()
// See health-checks.md for endpoint configuration
```

See [Health Checks](../health-checks.md).

## Scaffolding

The provider includes `EfCoreScaffoldProvider` for generating model and repository code from an existing database schema.

```bash
# Scaffold all tables
nextnet db scaffold

# Scaffold specific tables
nextnet db scaffold Users,Orders
```

See [Scaffolding](../scaffolding.md).

## See Also

- [Getting Started](../getting-started.md)
- [Configuration Reference](../configuration.md)
- [Migrations Guide](../migrations.md)
- [Multi-Database Support](../multi-database.md)
- [Troubleshooting](../troubleshooting.md)
