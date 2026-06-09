# Database Migrations

NextNet provides a unified migration system through the `IMigrationEngine` interface, supporting all database providers with a consistent CLI and API.

## Table of Contents

- [Overview](#overview)
- [How It Works](#how-it-works)
- [Configuration](#configuration)
- [Creating Migrations](#creating-migrations)
- [Applying Migrations](#applying-migrations)
- [Rolling Back Migrations](#rolling-back-migrations)
- [Migration Status](#migration-status)
- [Auto-Apply on Startup](#auto-apply-on-startup)
- [Production Safety](#production-safety)
- [Provider-Specific Migration Notes](#provider-specific-migration-notes)
- [Manual Migration Scripts](#manual-migration-scripts)

## Overview

Migrations allow you to evolve your database schema over time in a controlled, repeatable way. Each provider implements its own migration engine:

| Provider | Migration Engine | Mechanism | History Table |
|----------|-----------------|-----------|---------------|
| EF Core | `EfCoreMigrationEngine` | EF Core migrations (C#) | `__EFMigrationsHistory` |
| Dapper | `DapperMigrationEngine` | SQL script files | `__NextNetMigrations` |
| SQLite | EF Core based | EF Core migrations (C#) | `__EFMigrationsHistory` |
| PostgreSQL | EF Core based | EF Core migrations (C#) | `__EFMigrationsHistory` |
| MongoDB | `MongoDbMigrationEngine` | Index management scripts | NextNet tracks in metadata |

## How It Works

1. **Migration files** are stored in a configurable `directory` (default: `Migrations/`).
2. Each migration has a unique name and timestamp.
3. The **history table** tracks which migrations have been applied.
4. When applying, the engine runs all unapplied migrations in order.
5. When rolling back, the engine reverses the last N migrations.

### Architecture

```
┌─────────────────────────────────────────────────┐
│              IMigrationEngine                    │
├─────────────────────────────────────────────────┤
│  CreateMigration(name) → Migration               │
│  ApplyAsync(connection, opts) → MigrationResult  │
│  RollbackAsync(connection, steps) → MigrationResult
│  GetStatusAsync() → MigrationStatus              │
└─────────────────────────────────────────────────┘
         ▲                    ▲                    ▲
         │                    │                    │
┌────────┴────────┐  ┌───────┴────────┐  ┌───────┴────────┐
│ EfCoreMigration │  │DapperMigration │  │MongoDbMigration│
│    Engine       │  │    Engine      │  │    Engine       │
└─────────────────┘  └────────────────┘  └────────────────┘
```

## Configuration

```json
{
  "data": {
    "migration": {
      "autoApply": false,
      "directory": "Migrations",
      "historyTableName": "__NextNetMigrations",
      "timeoutSeconds": 60,
      "enableConfirmations": false,
      "backupEnabled": true,
      "backupDirectory": "Migrations/Backups"
    }
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `autoApply` | bool | `false` | Apply pending migrations on startup |
| `directory` | string | `"Migrations"` | Directory for migration files |
| `historyTableName` | string | `"__NextNetMigrations"` | History table name (Dapper only) |
| `timeoutSeconds` | int | `60` | Per migration timeout |
| `enableConfirmations` | bool | `false` | Require confirmation before destructive changes |
| `backupEnabled` | bool | `true` | Create database backup before migration |
| `backupDirectory` | string | `"Migrations/Backups"` | Backup output directory |

## Creating Migrations

### Via CLI

```bash
# Create a new migration (auto-named with timestamp)
nextnet db migration add AddUserTable

# With a specific name
nextnet db migration add AddUserTable --name "Add user table with email"

# For a specific connection
nextnet db migration add AddUserTable --connection Reports
```

### EF Core (Generated C#)

For EF Core based providers, migrations are C# classes generated in the `Migrations` directory:

```bash
nextnet db migration add AddUserTable
```

Generates:

```csharp
// Migrations/20260606_120000_AddUserTable.cs
[Migration("20260606_120000")]
public sealed class AddUserTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 100, nullable: false),
                Email = table.Column<string>(maxLength: 255, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false,
                    defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Users");
    }
}
```

### Dapper (SQL Script)

For Dapper provider:

```bash
nextnet db migration add AddUserTable
```

Generates:

```sql
-- Migrations/20260606_120000_AddUserTable.sql
-- UP: Create the Users table
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

-- DOWN: Remove the Users table
-- DROP TABLE IF EXISTS Users;
```

> [!TIP]
> Always include the DOWN script as a comment for reference. The Dapper migration engine only supports rolling back via explicitly defined reverse scripts.

### MongoDB (Index Management)

```bash
nextnet db migration add AddUserIndexes
```

Generates index configuration class:

```csharp
// Migrations/AddUserIndexes.cs
public sealed class AddUserIndexes : IMongoIndexConfiguration<User>
{
    public IEnumerable<CreateIndexModel<User>> GetIndexes()
    {
        yield return new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions
            {
                Name = "idx_users_email",
                Unique = true
            });
    }
}
```

## Applying Migrations

```bash
# Apply all pending migrations
nextnet db migrate

# Apply for a specific connection
nextnet db migrate --connection Reports

# Dry-run (preview without applying)
nextnet db migrate --dry-run

# Apply with verbose output
nextnet db migrate --verbose
```

### Programmatic Apply

```csharp
public class MigrationService
{
    private readonly IMigrationEngine _engine;

    public MigrationService(IMigrationEngine engine)
    {
        _engine = engine;
    }

    public async Task<MigrationResult> RunMigrationsAsync()
    {
        return await _engine.ApplyAsync(new MigrationOptions
        {
            ConnectionName = "Default",
            DryRun = false,
            TimeoutSeconds = 60
        });
    }
}
```

## Rolling Back Migrations

```bash
# Rollback the last migration
nextnet db rollback

# Rollback 3 migrations
nextnet db rollback --steps 3

# Rollback to a specific migration
nextnet db rollback --target AddUserTable

# Rollback on a specific connection
nextnet db rollback --connection Reports
```

> [!CAUTION]
> Rollback is **destructive**. Always take a backup or use `--dry-run` first.

## Migration Status

```bash
# View status of all migrations
nextnet db migration status

# Status for a specific connection
nextnet db migration status --connection Reports

# Output as JSON
nextnet db migration status --output json
```

### Status Response

```
Connection: Default
Provider: PostgreSQL
────────────────────────────────────────────────────
  Status    │ Migration                    │ Applied
────────────┼──────────────────────────────┼────────────
  Applied   │ 20260601_100000_InitialCreate │ 2026-06-01 10:00:05
  Applied   │ 20260605_080000_AddUserTable  │ 2026-06-05 08:01:12
  Pending   │ 20260606_120000_AddOrderTable │ —
  Pending   │ 20260606_130000_AddIndexes    │ —
────────────────────────────────────────────────────
Pending: 2    Applied: 2    Total: 4
```

## Auto-Apply on Startup

```csharp
builder.Services.AddNextNetData()
    .FromConfig(configuration.GetSection("data"))
    .UseProvider<EfCoreDataProvider>()
    .WithAutoMigration()  // Apply migrations on app startup
    .Build();
```

> [!WARNING]
> Auto apply is convenient for development but **not recommended for production**. Use CI/CD pipelines or manual apply for production databases.

## Production Safety

| Safety Feature | Description | How to Enable |
|---------------|-------------|---------------|
| **Dry Run** | Preview SQL without executing | `nextnet db migrate --dry-run` |
| **Confirmation Prompts** | Require confirmation for destructive changes | Set `enableConfirmations: true` in config |
| **Automatic Backups** | Backup database before applying migrations | Set `backupEnabled: true` (default) |
| **Transaction Wrapping** | Wrap each migration in a transaction | Supported by EF Core (ensure provider supports DDL transactions) |
| **CI/CD Validation** | Fail the build if pending migrations | `nextnet db migration status --fail-if-pending` |
| **Read-Only Check** | Verify database is writable before migrate | Built into migration engine |

### Production Migration Workflow

```bash
# Step 1: Review pending migrations
nextnet db migration status

# Step 2: Backup the database
nextnet db migrate --dry-run
pg_dump myapp > backup_$(date +%Y%m%d).sql

# Step 3: Apply migrations
nextnet db migrate --verbose

# Step 4: Verify
nextnet db migration status
nextnet db health
```

### CI/CD Pipeline Integration

```yaml
# .github/workflows/deploy.yml
jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      
      - name: Check pending migrations
        run: nextnet db migration status --fail-if-pending
        
      - name: Run migrations
        run: nextnet db migrate --verbose
        env:
          NextNet_Data__Connections__Default__ConnectionString: ${{ secrets.DB_CONNECTION_STRING }}
```

## Provider-Specific Migration Notes

### EF Core

- Migrations are C# classes with `Up()` and `Down()` methods.
- Supports all DDL operations (create, alter, drop).
- Transactions supported for providers that support DDL transactions (SQL Server, PostgreSQL).
- SQLite has limited ALTER TABLE support.

### Dapper

- Migrations are SQL script files executed in order.
- Each script should be **idempotent** (use `IF NOT EXISTS`, `CREATE OR REPLACE`, etc.).
- Rollback requires explicit DOWN scripts.
- History is tracked in `__NextNetMigrations` table.

### SQLite

- Uses EF Core migration engine.
- **Limited ALTER TABLE**: only `ADD COLUMN` is supported.
- For column changes, use the "recreate table" pattern:
  1. Create new table
  2. Copy data
  3. Drop old table
  4. Rename new table

### PostgreSQL

- Uses EF Core migration engine.
- Full DDL transaction support.
- Supports enums, schemas, extensions (PostGIS, pgcrypto, uuid-ossp).
- Can generate idempotent SQL scripts with `nextnet db migrate --script`.

### MongoDB

- Migrations manage index creation/removal.
- No schema migration needed (schemaless document model).
- Index builds can be background or foreground.
- Use `background: true` for production index creation to avoid blocking reads.

```csharp
// Background index creation for production safety
yield return new CreateIndexModel<Product>(
    Builders<Product>.IndexKeys.Ascending(p => p.Name),
    new CreateIndexOptions
    {
        Name = "idx_name",
        Background = true  // Don't block reads during index build
    });
```

## Manual Migration Scripts

For scenarios where you need full control, place SQL scripts directly in the migrations directory:

```
Migrations/
├── Manual/
│   ├── 001_SeedLookupData.sql
│   └── 002_FixCorruptedRecords.sql
├── 20260601_InitialCreate.sql
└── 20260605_AddUserTable.sql
```

Manual scripts are not tracked in the history table. Run them separately:

```bash
nextnet db execute Migrations/Manual/001_SeedLookupData.sql
```

## See Also

- [Configuration Reference](configuration.md)
- [CLI Reference](cli-reference.md)
- [Scaffolding Guide](scaffolding.md)
- [EF Core Provider](providers/ef-core.md)
- [Dapper Provider](providers/dapper.md)
- [Troubleshooting](troubleshooting.md)
