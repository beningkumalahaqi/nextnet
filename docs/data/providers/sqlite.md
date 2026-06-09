# SQLite Provider

The `NextNet.Data.Sqlite` package provides a lightweight SQLite provider for the NextNet data layer, ideal for development, testing, CI pipelines, and single user desktop scenarios. It requires no database server installation.

## Table of Contents

- [Installation](#installation)
- [Configuration](#configuration)
- [Connection Strings](#connection-strings)
- [In-Memory Mode for Testing](#in-memory-mode-for-testing)
- [Local Development Setup](#local-development-setup)
- [Repository Pattern](#repository-pattern)
- [Migrations](#migrations)
- [Connection Management](#connection-management)
- [Limitations](#limitations)
- [Health Checks](#health-checks)

## Installation

```bash
# Via CLI
nextnet add data sqlite

# Or manually
dotnet add package NextNet.Data.Sqlite
```

## Configuration

### Registration

```csharp
using NextNet.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<SqliteDataProvider>()
    .Build();
```

### nextnet.config.json

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "Data Source=app.db;Cache=Shared",
        "provider": "Sqlite",
        "timeoutSeconds": 30,
        "enabled": true
      }
    },
    "migration": {
      "autoApply": false,
      "directory": "Migrations"
    }
  }
}
```

## Connection Strings

### File-Based Database

```ini
# Basic -- creates app.db in the current directory
Data Source=app.db

# With shared cache (recommended for most scenarios)
Data Source=app.db;Cache=Shared

# With password encryption (SQLite SEE extension required)
Data Source=app.db;Password=mysecret

# Custom file path
Data Source=/data/myapp.db;Cache=Shared

# Read-only mode
Data Source=app.db;Mode=ReadOnly
```

### Connection String Builder

```csharp
using Microsoft.Data.Sqlite;

var builder = new SqliteConnectionStringBuilder
{
    DataSource = "app.db",
    Cache = SqliteCacheMode.Shared,
    Pooling = true,
    Mode = SqliteOpenMode.ReadWriteCreate
};
var connectionString = builder.ToString();
```

## In-Memory Mode for Testing

SQLite in memory databases are perfect for unit and integration tests -- they are fast, isolated, and require no cleanup.

```ini
# In-memory, named (shared across connections)
Data Source=TestDb;Mode=Memory;Cache=Shared

# In-memory, unnamed (each connection gets a new DB)
# Not useful for EF Core or Dapper since connections are separate
```

### Test Fixture Example

```csharp
public class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public UserRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
        _connection.Open();

        var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL
            );
            INSERT INTO Users (Name, Email) VALUES ('Alice', 'alice@test.com');
            INSERT INTO Users (Name, Email) VALUES ('Bob', 'bob@test.com');
        ";
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task FindAsync_Should_ReturnUser_When_IdExists()
    {
        var repo = new UserRepository(_connection, ...);
        var user = await repo.FindAsync(1);
        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllUsers()
    {
        var repo = new UserRepository(_connection, ...);
        var users = await repo.GetAllAsync();
        Assert.Equal(2, users.TotalCount);
    }

    public void Dispose()
    {
        _connection.Close();
    }
}
```

### Integration Test with WebApplicationFactory

```csharp
public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetUsers_Should_ReturnOk()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["data:connections:Default:connectionString"] =
                            "Data Source=TestDb_Integration;Mode=Memory;Cache=Shared"
                    });
                });
            });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Local Development Setup

### Typical Workflow

```bash
# 1. Create database file
nextnet db init sqlite

# 2. Create migration
nextnet db migration add InitialCreate

# 3. Apply migration
nextnet db migrate

# 4. Run the app
dotnet run
```

### Multiple Environments

Use `appsettings.Development.json`:

```json
{
  "data": {
    "connections": {
      "Default": {
        "connectionString": "Data Source=app.development.db;Cache=Shared"
      }
    }
  }
}
```

## Repository Pattern

```csharp
public class NoteRepository
{
    private readonly IRepository<Note> _repository;

    public NoteRepository(IRepository<Note> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<Note>> GetRecentNotesAsync(int page, int pageSize)
        => await _repository.GetAllAsync(
            filter: n => n.CreatedAt > DateTime.UtcNow.AddDays(-7),
            orderBy: n => n.CreatedAt,
            descending: true,
            page: page,
            pageSize: pageSize);

    public async Task<IEnumerable<Note>> SearchByTitleAsync(string term)
        => await _repository.FindAllAsync(n => n.Title.Contains(term));
}
```

## Migrations

SQLite migrations can use either the EF Core migration engine (when using the EF Core provider with SQLite) or raw SQL scripts (when using the Dapper provider).

### Via EF Core

```bash
nextnet db migration add AddUserAge
nextnet db migrate
```

### Via SQL Scripts (Dapper)

Place scripts in `Migrations/`:

```sql
-- 001_InitialCreate.sql
CREATE TABLE IF NOT EXISTS Notes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Body TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);
```

> [!WARNING]
> SQLite has **limited ALTER TABLE support**. It supports `ALTER TABLE ... ADD COLUMN` but not `DROP COLUMN` or `ALTER COLUMN`. For schema changes beyond adding columns, you must recreate the table. See [SQLite ALTER TABLE documentation](https://www.sqlite.org/lang_altertable.html).

## Connection Management

The provider uses `SqliteConnectionFactory` for creating and managing `SqliteConnection` instances.

```csharp
// Configure SQLite-specific options
services.Configure<SqliteConnectionFactoryOptions>(options =>
{
    options.PoolSize = 10;
    options.DefaultTimeoutSeconds = 30;
});
```

### Pooling

SQLite connection pooling is enabled by default with `Cache=Shared`. Pooling benefits read heavy workloads but offers less benefit for write heavy scenarios.

| Cache Mode | Behavior |
|------------|----------|
| `Default` | No shared cache, limited pooling |
| `Shared` | Shared cache across connections, enables pooling |
| `Private` | Each connection has its own cache |

## Limitations

| Limitation | Details | Workaround |
|------------|---------|------------|
| **Concurrent Writes** | SQLite uses a database-level lock. Concurrent write heavy workloads can cause `SQLITE_BUSY` errors. | Use WAL mode: `Data Source=app.db;Cache=Shared;Mode=ReadWriteCreate` + set PRAGMA journal_mode=WAL. |
| **ALTER TABLE** | Cannot drop columns, change column types, or add constraints. | Recreate the table for complex schema changes. |
| **No Stored Procedures** | SQLite does not support stored procedures or functions. | Use application side logic. |
| **No User Management** | No built in user/role system. | Use application level authentication and authorization. |
| **Max Database Size** | Default max is ~140TB, but practical limit is much lower for performance. | Consider PostgreSQL for databases exceeding 50GB. |
| **No Replication** | No built in replication. | Use SQLite backups or consider a client server database for production. |
| **No UNIQUE INDEX on NULL** | Multiple NULLs are allowed in unique columns. | Use application validation. |
| **Concurrency Limit** | Best suited for single server, low concurrency scenarios. | Not recommended for high traffic web applications (use PostgreSQL/MongoDB instead). |

### Enabling WAL Mode

```csharp
// Connection string with WAL mode
"Data Source=app.db;Cache=Shared"

// Set WAL mode at connection open
using var connection = new SqliteConnection("Data Source=app.db;Cache=Shared");
connection.Open();
var command = connection.CreateCommand();
command.CommandText = "PRAGMA journal_mode=WAL;";
command.ExecuteNonQuery();
```

## Health Checks

The provider includes `SqliteHealthCheckProvider` that verifies database connectivity by executing a `SELECT 1` query.

```csharp
// Registered automatically when using UseProvider<SqliteDataProvider>()
```

See [Health Checks](../health-checks.md).

## See Also

- [Getting Started](../getting-started.md)
- [Configuration Reference](../configuration.md)
- [Migrations Guide](../migrations.md)
- [PostgreSQL Provider](postgresql.md) -- for production workloads
- [Troubleshooting](../troubleshooting.md)
