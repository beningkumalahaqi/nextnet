# Configuration Reference

The NextNet data layer is configured via the `"data"` section in `nextnet.config.json` (or any `IConfiguration` source).

## Table of Contents

- [Full Schema](#full-schema)
- [DataConfig Section](#dataconfig-section)
- [ConnectionConfig](#connectionconfig)
- [MigrationConfig](#migrationconfig)
- [ScaffoldingConfig](#scaffoldingconfig)
- [HealthChecksConfig](#healthchecksconfig)
- [Provider-Specific Configuration](#provider-specific-configuration)
- [Environment Variables](#environment-variables)
- [Secrets Management](#secrets-management)
- [Example Configurations](#example-configurations)

## Full Schema

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "<name>": {
        "connectionString": "<connection-string>",
        "provider": "<provider-name>",
        "timeoutSeconds": 30,
        "enabled": true,
        "poolSize": null,
        "tags": null,
        "options": {}
      }
    },
    "migration": {
      "autoApply": false,
      "directory": "Migrations",
      "historyTableName": "__NextNetMigrations",
      "timeoutSeconds": 60,
      "enableConfirmations": false,
      "backupEnabled": true,
      "backupDirectory": "Migrations/Backups"
    },
    "scaffolding": {
      "modelsNamespace": "Models",
      "repositoriesNamespace": "Repositories",
      "actionsNamespace": "Actions",
      "modelsDirectory": "Models",
      "repositoriesDirectory": "Repositories",
      "actionsDirectory": "app/api",
      "adminDirectory": "app/admin",
      "crudStyle": "minimal-api",
      "overwriteExisting": false,
      "generatePaging": true,
      "generateValidation": true,
      "generateAuditFields": true,
      "useRecords": true,
      "nullableEnabled": true,
      "templateDirectory": null
    },
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

## DataConfig Section

### Root `data` Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `defaultConnection` | string | `"Default"` | The named connection to use when none is explicitly specified |
| `connections` | object | `{}` | Map of named connection configurations (see [ConnectionConfig](#connectionconfig)) |
| `migration` | object | (see below) | Migration system configuration |
| `scaffolding` | object | (see below) | Code generation configuration |
| `healthChecks` | object | (see below) | Health check system configuration |

### TypeScript Interface

```csharp
// NextNet.Data.Abstractions.Configuration.DataConfig
public sealed record DataConfig
{
    public string DefaultConnection { get; init; } = "Default";
    public Dictionary<string, ConnectionConfig> Connections { get; init; } = new();
    public MigrationConfig Migration { get; init; } = new();
    public ScaffoldingConfig Scaffolding { get; init; } = new();
    public HealthChecksConfig HealthChecks { get; init; } = new();
}
```

## ConnectionConfig

Each entry in the `connections` map configures a single database connection.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `connectionString` | string | *(required)* | Database connection string |
| `provider` | string | `"EntityFramework"` | Provider identifier. One of: `EntityFramework`, `Dapper`, `Sqlite`, `PostgreSQL`, `MongoDB` |
| `timeoutSeconds` | int | `30` | Command timeout in seconds (valid range: 1-3600) |
| `enabled` | bool | `true` | Whether this connection is active at startup |
| `poolSize` | int? | `null` | Connection pool size (provider dependent; uses provider default when null) |
| `tags` | string[]? | `null` | Categorization tags for dynamic connection resolution |
| `options` | object | `{}` | Provider specific options (see [provider specific config](#provider-specific-configuration)) |

### TypeScript Interface

```csharp
public sealed record ConnectionConfig
{
    public string ConnectionString { get; init; } = string.Empty;
    public string Provider { get; init; } = "EntityFramework";
    public int TimeoutSeconds { get; init; } = 30;
    public bool Enabled { get; init; } = true;
    public int? PoolSize { get; init; }
    public string[]? Tags { get; init; }
    public Dictionary<string, object> Options { get; init; } = new();
}
```

### Provider Identifiers

| Identifier String | Provider Class |
|------------------|----------------|
| `EntityFramework` | `EfCoreDataProvider` |
| `Dapper` | `DapperDataProvider` |
| `Sqlite` | `SqliteDataProvider` |
| `PostgreSQL` | `PostgresDataProvider` |
| `MongoDB` | `MongoDbProvider` |

## MigrationConfig

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `autoApply` | bool | `false` | Apply pending migrations on application startup (not recommended for production) |
| `directory` | string | `"Migrations"` | Directory path for migration files (relative to project root) |
| `historyTableName` | string | `"__NextNetMigrations"` | Name of the migration history table (Dapper provider only) |
| `timeoutSeconds` | int | `60` | Per migration timeout in seconds |
| `enableConfirmations` | bool | `false` | Require confirmation before applying destructive changes |
| `backupEnabled` | bool | `true` | Create a database backup before applying migrations |
| `backupDirectory` | string | `"Migrations/Backups"` | Directory for migration backups |

```csharp
public sealed record MigrationConfig
{
    public bool AutoApply { get; init; } = false;
    public string Directory { get; init; } = "Migrations";
    public string HistoryTableName { get; init; } = "__NextNetMigrations";
    public int TimeoutSeconds { get; init; } = 60;
    public bool EnableConfirmations { get; init; } = false;
    public bool BackupEnabled { get; init; } = true;
    public string BackupDirectory { get; init; } = "Migrations/Backups";
}
```

## ScaffoldingConfig

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `modelsNamespace` | string | `"Models"` | Namespace for generated model classes |
| `repositoriesNamespace` | string | `"Repositories"` | Namespace for generated repository classes |
| `actionsNamespace` | string | `"Actions"` | Namespace for generated CRUD actions |
| `modelsDirectory` | string | `"Models"` | Output directory for model files |
| `repositoriesDirectory` | string | `"Repositories"` | Output directory for repository files |
| `actionsDirectory` | string | `"app/api"` | Output directory for CRUD action files |
| `adminDirectory` | string | `"app/admin"` | Output directory for admin pages |
| `crudStyle` | string | `"minimal-api"` | CRUD generation style: `"minimal-api"` or `"controller"` |
| `overwriteExisting` | bool | `false` | Whether to overwrite existing files on regeneration |
| `generatePaging` | bool | `true` | Add pagination parameters to list endpoints |
| `generateValidation` | bool | `true` | Add data annotation validation attributes |
| `generateAuditFields` | bool | `true` | Add `CreatedAt` / `UpdatedAt` fields to models |
| `useRecords` | bool | `true` | Generate models as C# `record` types |
| `nullableEnabled` | bool | `true` | Use nullable reference type annotations |
| `templateDirectory` | string? | `null` | Path to custom Scriban template directory |

```csharp
public sealed record ScaffoldingConfig
{
    public string ModelsNamespace { get; init; } = "Models";
    public string RepositoriesNamespace { get; init; } = "Repositories";
    public string ActionsNamespace { get; init; } = "Actions";
    public string ModelsDirectory { get; init; } = "Models";
    public string RepositoriesDirectory { get; init; } = "Repositories";
    public string ActionsDirectory { get; init; } = "app/api";
    public string AdminDirectory { get; init; } = "app/admin";
    public string CrudStyle { get; init; } = "minimal-api";
    public bool OverwriteExisting { get; init; } = false;
    public bool GeneratePaging { get; init; } = true;
    public bool GenerateValidation { get; init; } = true;
    public bool GenerateAuditFields { get; init; } = true;
    public bool UseRecords { get; init; } = true;
    public bool NullableEnabled { get; init; } = true;
    public string? TemplateDirectory { get; init; }
}
```

## HealthChecksConfig

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `path` | string | `"/health"` | URL path for the health check endpoint |
| `tags` | string[] | `["nextnet", "data"]` | Tags assigned to health check results |
| `showDetails` | bool | `false` | Show detailed per connection results (set to `false` in production) |
| `cacheDurationSeconds` | int | `5` | How long to cache health check results (seconds) |
| `includeMigrationStatus` | bool | `true` | Include pending migration info in responses |
| `timeoutSeconds` | int | `10` | Per health check timeout (seconds) |
| `degradedThresholdMs` | int | `100` | Response time threshold for Degraded status (milliseconds) |
| `unhealthyThresholdMs` | int | `5000` | Response time threshold for Unhealthy status (milliseconds) |

```csharp
public sealed record HealthChecksConfig
{
    public string Path { get; init; } = "/health";
    public string[] Tags { get; init; } = ["nextnet", "data"];
    public bool ShowDetails { get; init; } = false;
    public int CacheDurationSeconds { get; init; } = 5;
    public bool IncludeMigrationStatus { get; init; } = true;
    public int TimeoutSeconds { get; init; } = 10;
    public int DegradedThresholdMs { get; init; } = 100;
    public int UnhealthyThresholdMs { get; init; } = 5000;
}
```

## Provider-Specific Configuration

Provider specific options are configured in the `options` property of a connection entry or via `services.Configure<T>()`.

### EF Core

```json
{
  "connections": {
    "Default": {
      "connectionString": "...",
      "provider": "EntityFramework",
      "options": {
        "dbContextPoolSize": 64,
        "enableSensitiveDataLogging": false,
        "enableDetailedErrors": false,
        "autoDetectChanges": true,
        "queryTrackingBehavior": "NoTracking"
      }
    }
  }
}
```

Or programmatically:

```csharp
services.Configure<EfCoreProviderOptions>(options =>
{
    options.DbContextPoolSize = 64;
    options.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
});
```

### Dapper

```json
{
  "options": {
    "commandType": "Text",
    "buffered": true,
    "defaultSchema": "dbo"
  }
}
```

```csharp
services.Configure<DapperProviderOptions>(options =>
{
    options.DefaultCommandTimeout = 30;
    options.Buffered = true;
});
```

### MongoDB

```json
{
  "options": {
    "databaseName": "MyApp",
    "collectionNamingConvention": "Pluralized",
    "useCamelCaseElementNames": true,
    "ignoreExtraElements": true
  }
}
```

```csharp
services.Configure<MongoDbProviderOptions>(options =>
{
    options.DatabaseName = "MyApp";
    options.CollectionNamingConvention = CollectionNamingConvention.Pluralized;
});
```

### PostgreSQL

```json
{
  "options": {
    "sslMode": "Require",
    "trustServerCertificate": false,
    "maxPoolSize": 100,
    "minPoolSize": 10,
    "connectionIdleLifetime": 300,
    "useNetTopologySuite": false
  }
}
```

```csharp
services.Configure<PostgresProviderOptions>(options =>
{
    options.SslMode = PostgresSslMode.VerifyFull;
    options.UseNetTopologySuite = true;
});
```

### SQLite

```json
{
  "options": {
    "poolSize": 10,
    "defaultTimeoutSeconds": 30,
    "journalMode": "Wal"
  }
}
```

```csharp
services.Configure<SqliteProviderOptions>(options =>
{
    options.PoolSize = 10;
    options.JournalMode = SqliteJournalMode.Wal;
});
```

## Environment Variables

Connection strings and other settings can be configured via environment variables using the standard .NET configuration hierarchy.

### Naming Convention

Use double underscores (`__`) as the separator between hierarchy levels:

```bash
# Pattern: NextNet_Data__{Section}__{Property}
# Pattern: NextNet_Data__Connections__{Name}__{Property}

# Connection string for Default
NextNet_Data__Connections__Default__ConnectionString="Host=prod.db;Database=myapp;..."

# Override provider
NextNet_Data__Connections__Default__Provider="PostgreSQL"

# Disable a connection
NextNet_Data__Connections__Reports__Enabled=false

# Migration settings
NextNet_Data__Migration__AutoApply=false

# Health check settings
NextNet_Data__HealthChecks__ShowDetails=false

# Default connection name
NextNet_Data__DefaultConnection="Production"
```

### In nextnet.config.json via environment variables

NextNet also supports direct environment variable references in connection strings using the `{ENV_NAME}` syntax:

```json
{
  "connections": {
    "Default": {
      "connectionString": "Host={DB_HOST};Database=myapp;Username={DB_USER};Password={DB_PASSWORD}",
      "provider": "PostgreSQL"
    }
  }
}
```

### Container / Docker

```bash
docker run -e NextNet_Data__Connections__Default__ConnectionString="Host=..." myapp
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
spec:
  template:
    spec:
      containers:
        - env:
          - name: NextNet_Data__Connections__Default__ConnectionString
            valueFrom:
              secretKeyRef:
                name: db-secret
                key: connection-string
```

## Secrets Management

### .NET User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "data:connections:Default:connectionString" \
    "Host=localhost;Database=myapp_dev;Username=dev;Password=devpass"

# Set provider option
dotnet user-secrets set "data:connections:Default:options:sslMode" "Disable"
```

### Azure Key Vault

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://myvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

Access secrets as:

```bash
# Key Vault secret name
data--connections--Default--connectionString

# Maps to configuration key
data:connections:Default:connectionString
```

### AWS Secrets Manager

```csharp
builder.Configuration.AddSecretsManager(configurator: options =>
{
    options.SecretFilter = entry => entry.Name.StartsWith("NextNet/");
});
```

## Example Configurations

### Development (SQLite)

```json
{
  "data": {
    "defaultConnection": "Default",
    "connections": {
      "Default": {
        "connectionString": "Data Source=app.dev.db;Cache=Shared",
        "provider": "Sqlite",
        "enabled": true
      }
    },
    "migration": {
      "autoApply": true
    },
    "healthChecks": {
      "showDetails": true
    }
  }
}
```

### Production (PostgreSQL with Read Replica)

```json
{
  "data": {
    "defaultConnection": "Primary",
    "connections": {
      "Primary": {
        "connectionString": "Host=primary.internal;Database=myapp;Username=app;Password=secret;SSL Mode=VerifyFull",
        "provider": "PostgreSQL",
        "timeoutSeconds": 30,
        "poolSize": 50,
        "enabled": true,
        "tags": ["write"]
      },
      "ReadReplica": {
        "connectionString": "Host=replica1.internal;Database=myapp;Username=app_read;Password=secret;SSL Mode=VerifyFull",
        "provider": "PostgreSQL",
        "timeoutSeconds": 15,
        "poolSize": 100,
        "enabled": true,
        "tags": ["readonly", "replica"]
      }
    },
    "migration": {
      "autoApply": false,
      "enableConfirmations": true,
      "backupEnabled": true
    },
    "healthChecks": {
      "showDetails": false,
      "cacheDurationSeconds": 10,
      "degradedThresholdMs": 200
    }
  }
}
```

### Polyglot (PostgreSQL + MongoDB)

```json
{
  "data": {
    "defaultConnection": "PostgresMain",
    "connections": {
      "PostgresMain": {
        "connectionString": "Host=localhost;Database=myapp;Username=app;Password=secret",
        "provider": "PostgreSQL",
        "enabled": true,
        "tags": ["relational", "transactions"]
      },
      "MongoAnalytics": {
        "connectionString": "mongodb://localhost:27017/Analytics",
        "provider": "MongoDB",
        "enabled": true,
        "tags": ["nosql", "analytics"]
      },
      "LegacySqlite": {
        "connectionString": "Data Source=legacy.db",
        "provider": "EntityFramework",
        "enabled": false,
        "tags": ["legacy", "migration-pending"]
      }
    }
  }
}
```

### Minimal (Single SQLite)

```json
{
  "data": {
    "connections": {
      "Default": {
        "connectionString": "Data Source=app.db",
        "provider": "Sqlite"
      }
    }
  }
}
```

## See Also

- [Getting Started](getting-started.md)
- [CLI Reference](cli-reference.md)
- [Migrations Guide](migrations.md)
- [Health Checks](health-checks.md)
- [Multi-Database Support](multi-database.md)
- [Troubleshooting](troubleshooting.md)
