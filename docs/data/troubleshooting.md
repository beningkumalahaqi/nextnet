# Troubleshooting

Common issues, error messages, and solutions for the NextNet V2 Data Layer.

## Table of Contents

- [Installation and Setup](#installation-and-setup)
- [Configuration Issues](#configuration-issues)
- [Connection Problems](#connection-problems)
- [Migration Problems](#migration-problems)
- [Provider-Specific Issues](#provider-specific-issues)
- [Scaffolding Issues](#scaffolding-issues)
- [Performance Tips](#performance-tips)
- [Getting Help](#getting-help)

## Installation and Setup

### "The type or namespace 'NextNet' could not be found"

**Cause:** Missing or incorrect NuGet package reference.

**Solution:**

```bash
# Ensure the necessary packages are installed
dotnet add package NextNet.Data.Abstractions
dotnet add package NextNet.Data.Providers
dotnet add package NextNet.Data.EntityFramework  # (or your provider)

# Verify the packages are restored
dotnet restore
```

### "Cannot resolve 'AddNextNetData' method"

**Cause:** Missing `using` directive or outdated package version.

**Solution:**

```csharp
// Add the required using directive
using NextNet.Data.Providers;

// Ensure you are using version 2.0.0 or later
// Check your .csproj for the package version
```

### "'nextnet' is not recognized as a command"

**Cause:** NextNet CLI is not installed or not on PATH.

**Solution:**

```bash
# Install the CLI globally
dotnet tool install --global NextNet.Cli

# Or update if already installed
dotnet tool update --global NextNet.Cli

# Verify installation
nextnet --version
```

## Configuration Issues

### "Connection 'Default' not found in configuration"

**Cause:** The `connections` section is missing or the connection name is incorrect.

**Solution:**

```json
{
  "data": {
    "connections": {
      "Default": {
        "connectionString": "Data Source=app.db;Cache=Shared",
        "provider": "Sqlite"
      }
    }
  }
}
```

Also verify the `defaultConnection` property matches a defined connection name:

```json
{
  "data": {
    "defaultConnection": "Default",
    ...
  }
}
```

### "Provider 'X' is not registered"

**Cause:** The provider package is installed but not registered in the DI setup.

**Solution:**

```csharp
// Register the provider in Program.cs
builder.Services.AddNextNetData()
    .FromConfig(builder.Configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()  // Register your provider here
    .Build();
```

### "Configuration section 'data' not found"

**Cause:** The `nextnet.config.json` file is missing or not being loaded.

**Solution:**

```bash
# Verify the file exists
ls nextnet.config.json

# Ensure it's configured in Program.cs
builder.Configuration.AddJsonFile("nextnet.config.json", optional: false);
```

### Connection string contains sensitive information in logs

**Cause:** Default logging configuration may expose connection strings in error messages.

**Solution:**

```json
{
  "data": {
    "connections": {
      "Default": {
        "connectionString": "Host=...;Username=...;Password=...",
        "provider": "PostgreSQL"
      }
    },
    "logging": {
      "maskConnectionStrings": true
    }
  }
}
```

Connection strings are automatically masked in health check responses. If you see them in other logs, configure:

```csharp
builder.Services.Configure<NextNetDataLoggingOptions>(options =>
{
    options.MaskConnectionStrings = true;
});
```

## Connection Problems

### "Unable to connect to database" / "Connection refused"

**Common causes and solutions:**

| Cause | Symptom | Solution |
|-------|---------|----------|
| Database server not running | `Connection refused` | Start the database service |
| Wrong host/port | `No connection could be made` | Verify connection string |
| Firewall blocking | `Connection timed out` | Check firewall rules |
| Authentication failed | `Login failed` / `Authentication failed` | Verify credentials |
| SSL required | `SSL connection required` | Add `SSL Mode=Require` to connection string |

**Diagnostic steps:**

```bash
# 1. Check if the database is running
# PostgreSQL:
pg_isready -h localhost -p 5432

# SQLite (check file exists):
ls -la app.db

# MongoDB:
mongosh --eval "db.runCommand({ ping: 1 })"

# 2. Use NextNet health check
nextnet db health --verbose

# 3. Test connection from CLI
nextnet db info
```

### "Timeout expired" / "Connection timed out"

**Solutions:**

1. Increase the timeout:
   ```json
   {
     "connections": {
       "Default": {
         "connectionString": "...",
         "timeoutSeconds": 60
       }
     }
   }
   ```

2. Check network connectivity:
   ```bash
   ping <database-host>
   telnet <database-host> <port>
   ```

3. Verify connection pool settings are appropriate.

4. For SQLite, ensure WAL mode is enabled for better concurrency:
   ```
   Data Source=app.db;Cache=Shared
   ```

### "Too many connections" (PostgreSQL/MySQL)

**Cause:** Connection pool exhaustion or database connection limit reached.

**Solutions:**

1. Reduce pool size:
   ```json
   {
     "connections": {
       "Default": {
         "connectionString": "...",
         "poolSize": 20
       }
     }
   }
   ```

2. Ensure connections are properly disposed (use `using` statements).

3. Check database max connections:
   ```sql
   -- PostgreSQL
   SHOW max_connections;
   SELECT count(*) FROM pg_stat_activity;
   ```

4. Increase database max connections if needed.

## Migration Problems

### "Migration 'X' has already been applied"

**Cause:** Attempting to apply a migration that is already recorded in the history table.

**Solutions:**

```bash
# Check current status
nextnet db migration status

# If the migration was partially applied, manually fix the history table
nextnet db execute "DELETE FROM __NextNetMigrations WHERE MigrationId = '20260606_AddUserTable'"

# Then re-apply
nextnet db migrate
```

### "Migration failed: ... column already exists"

**Cause:** Migration is trying to create a column that already exists.

**Solutions:**

```bash
# 1. Check the actual database state
nextnet db explore --table Users

# 2. Mark the migration as applied (skip it)
nextnet db migration add --fake AddUserTable
nextnet db migration status
```

For EF Core, you can generate a no-op migration:

```bash
nextnet db migration add AddUserTable --no-op
```

### "Cannot roll back migration 'X'"

**Cause:** The migration does not have a DOWN script (Dapper provider) or the DOWN method throws.

**Solutions:**

- For Dapper: Manually write the reverse SQL and run it:
  ```bash
  nextnet db execute "DROP TABLE IF EXISTS Users;"
  nextnet db execute "DELETE FROM __NextNetMigrations WHERE MigrationId = '20260606_AddUserTable'"
  ```

- For EF Core: Ensure the `Down()` method is properly implemented.

### "Pending migrations in production — how to apply safely?"

**Production safety checklist:**

```bash
# 1. Review pending migrations
nextnet db migration status

# 2. Backup the database
pg_dump myapp > backup_$(date +%Y%m%d_%H%M%S).sql

# 3. Review the SQL that will be executed
nextnet db migrate --dry-run

# 4. Apply with verbose output
nextnet db migrate --verbose

# 5. Verify
nextnet db migration status
nextnet db health
```

### SQLite: "ALTER TABLE ... ADD COLUMN ... failed"

**Cause:** SQLite does not support all ALTER TABLE operations (e.g., dropping columns, adding constraints).

**Solution:** Use the "recreate table" workaround:

```sql
-- 1. Create new table with desired schema
CREATE TABLE Users_New (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL
);

-- 2. Copy data from old table
INSERT INTO Users_New (Id, Name, Email)
SELECT Id, Name, Email FROM Users;

-- 3. Drop old table
DROP TABLE Users;

-- 4. Rename new table
ALTER TABLE Users_New RENAME TO Users;
```

## Provider-Specific Issues

### EF Core

| Issue | Solution |
|-------|----------|
| `The LINQ expression could not be translated` | Use `AsEnumerable()` or switch to client-side evaluation for unsupported expressions. Or rewrite the query with supported constructs. |
| `A second operation was started on this context instance` | EF Core DbContext is not thread-safe. Use `AddDbContextPool` or ensure each operation creates a new scope. NextNet manages this automatically via `DbContextPool`. |
| `Cannot use table 'X' for entity type 'Y' since it is being used for entity type 'Z'` | Entities are mapped to the same table. Use `[Table("different_name")]` or configure separate table names via Fluent API. |
| `Invalid column name 'Discriminator'` | TPH inheritance mapping is active. Add `[NotMapped]` to base class properties or configure TPH explicitly. |
| `Npgsql.PostgresException: 42P01: relation "X" does not exist` | Migration has not been applied or the table name is incorrect. Check for case sensitivity (use lowercase table names with PostgreSQL). |

### Dapper

| Issue | Solution |
|-------|----------|
| `A column named 'X' does not match any property` | The SQL result column does not map to any property on the target type. Use column aliases: `SELECT column AS PropertyName FROM ...`. |
| `Object reference not set to an instance of an object` (in multi-mapping) | The `splitOn` parameter is incorrect. Ensure the split column name matches exactly. |
| `The connection was not closed. The connection's current state is open` | Ensure `using` statements or `await using` are used for connections. |
| Slow query performance | Check if query plan caching is working. Use `SqlMapper.PurgeQueryCache()` only during development. |

### MongoDB

| Issue | Solution |
|-------|----------|
| `MongoDB.Driver.MongoConnectionException: Unable to connect to server` | MongoDB server is not running. Start with `mongod` or check service status. |
| `Command insert requires authentication` | Add credentials to the connection string: `mongodb://user:pass@localhost:27017/db`. |
| `BSON serialization exception` | Add `[BsonIgnoreExtraElements]` to your model or configure global conventions. |
| `WriteConcern error` | Check replica set status if using write concern majority. |
| `OperationCanceledException` | Query timeout. Increase timeout or optimize the query. |
| Index build is slow | Use `Background = true` for index creation options. |

### SQLite

| Issue | Solution |
|-------|----------|
| `SQLite Error 5: 'database is locked'` | Concurrent write contention. Enable WAL mode: `Data Source=app.db;Cache=Shared` and set `PRAGMA journal_mode=WAL;`. |
| `SQLite Error 1: 'no such table'` | Migration has not been applied or database file is not in the expected location. Check the connection string path. |
| `SQLite Error 8: 'attempt to write a readonly database'` | File permissions issue. Ensure the application has write access to the database file and its directory. |
| Corrupt database: `database disk image is malformed` | Restore from backup or use `PRAGMA integrity_check;` to assess damage. Enable WAL mode to reduce corruption risk. |

### PostgreSQL

| Issue | Solution |
|-------|----------|
| `Npgsql.PostgresException: 3D000: database "X" does not exist` | Create the database first: `CREATE DATABASE myapp;` or use `nextnet db init postgres`. |
| `Npgsql.PostgresException: 28000: no pg_hba.conf entry` | Update `pg_hba.conf` to allow connections from the application host. |
| `Npgsql.PostgresException: 08P01: unsupported startup parameter` | Outdated Npgsql version. Update to the latest: `dotnet add package Npgsql`. |
| SSL connection error | Ensure SSL is properly configured. For local dev, set `SSL Mode=Disable` or `Prefer`. |
| `Timeout expired` | Check `statement_timeout` and `connection_timeout` PostgreSQL settings. Increase `timeoutSeconds` in config. |
| High memory usage | Reduce `MaxPoolSize`. Monitor with `SELECT * FROM pg_stat_activity`. |

## Scaffolding Issues

### "Model 'X' not found"

**Cause:** The specified model class does not exist or is not in a namespace scanned by the scaffolding system.

**Solutions:**

```bash
# Verify the model file exists
ls Models/User.cs

# Ensure the model is in the configured namespace
# Check scaffolding config:
# "modelsNamespace": "Models"
```

### "Template 'model.sbn' not found"

**Cause:** Custom template directory is configured but the template file is missing.

**Solutions:**

```json
// Remove or correct the template directory
{
  "scaffolding": {
    "templateDirectory": null  // Uses built-in templates
  }
}
```

Or ensure the template file exists in the configured directory:

```
Scaffolding/Templates/
├── model.sbn
├── repository-interface.sbn
├── repository.sbn
└── crud-*.sbn
```

### Generated code has compilation errors

**Cause:** Outdated templates or property definitions with invalid types.

**Solutions:**

```bash
# Regenerate with correct properties
nextnet generate model User --properties "Id:int:pk,Name:string,Email:string"

# Check for template syntax errors in custom templates
# Use --dry-run to preview
nextnet generate model User --dry-run
```

## Performance Tips

### General

| Tip | Description |
|-----|-------------|
| **Use `AsNoTracking()` for read-only queries** | (EF Core) Disable change tracking for queries that don't need it. |
| **Enable query logging only in development** | `options.EnableSensitiveDataLogging = false` in production. |
| **Use projection (`Select`) instead of loading entire entities** | Reduces data transferred from the database. |
| **Batch operations in transactions** | Reduces round-trips for related operations. |
| **Use `CountAsync()` instead of loading all records** | More efficient than `(await GetAllAsync()).TotalCount`. |
| **Configure pool size appropriately** | Too large = wasted resources, too small = contention. |
| **Monitor slow queries** | Use `nextnet db explore` and database-specific query analyzers. |

### Connection Pooling

```json
{
  "connections": {
    "Default": {
      "connectionString": "...",
      "poolSize": 50,
      "timeoutSeconds": 30
    }
  }
}
```

General guideline for pool size:

```
Pool Size = (Max Concurrent Requests) × (Average Query Duration in seconds)
```

Example: 100 concurrent requests × 0.2s average query = 20 connections.

### Query Optimization

| Scenario | Recommendation |
|----------|---------------|
| Slow SELECT on large table | Add appropriate indexes. Use `nextnet db explore --table X` to review. |
| N+1 query problem | Use `Include()` (EF Core) or batch queries (Dapper). |
| High CPU on database server | Profile queries. Check for missing indexes or inefficient joins. |
| High memory usage | Reduce page size, use streaming for large result sets. |

### Caching

For read-heavy workloads, consider caching:

```csharp
public class CachedUserRepository
{
    private readonly IRepository<User> _inner;
    private readonly IMemoryCache _cache;

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _cache.GetOrCreateAsync($"user_{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _inner.FindAsync(id);
        });
    }
}
```

## Getting Help

### Diagnostic Commands

```bash
# Check CLI version
nextnet --version

# Show configuration info
nextnet db info

# List all connections
nextnet db list

# Check migration status
nextnet db migration status

# Run health check
nextnet db health --verbose

# Explore database schema
nextnet db explore

# Enable verbose output for any command
nextnet <command> --verbose
```

### Logging

Enable detailed logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "NextNet": "Debug",
      "NextNet.Data": "Debug"
    }
  }
}
```

### Reporting Issues

When reporting issues on GitHub, include:

1. **NextNet version**: `nextnet --version`
2. **Provider and version**: e.g., `NextNet.Data.EntityFramework 2.0.0`
3. **Database type and version**: e.g., `PostgreSQL 16`
4. **.NET version**: `dotnet --version`
5. **Full error message and stack trace**
6. **Configuration** (with connection string masked)
7. **Steps to reproduce**
8. **Minimal repro project** (if possible)

### Useful Links

- [GitHub Issues](https://github.com/nextnet/nextnet/issues)
- [Documentation Home](../index.md)
- [API Reference](../api/index.md)
- [Discord Community](https://discord.gg/nextnet)
- [Stack Overflow (tag: nextnet)](https://stackoverflow.com/questions/tagged/nextnet)

## See Also

- [Configuration Reference](configuration.md)
- [CLI Reference](cli-reference.md)
- [Migrations Guide](migrations.md)
- [Scaffolding Guide](scaffolding.md)
- [Multi-Database Support](multi-database.md)
- [Health Checks](health-checks.md)
