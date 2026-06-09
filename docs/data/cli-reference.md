# CLI Command Reference

The NextNet CLI provides commands for managing the data layer, including adding providers, running migrations, scaffolding code, and monitoring health.

## Table of Contents

- [Global Options](#global-options)
- [nextnet add data](#nextnet-add-data)
- [nextnet db init](#nextnet-db-init)
- [nextnet db migrate](#nextnet-db-migrate)
- [nextnet db migration](#nextnet-db-migration)
- [nextnet db rollback](#nextnet-db-rollback)
- [nextnet generate model](#nextnet-generate-model)
- [nextnet generate repository](#nextnet-generate-repository)
- [nextnet generate crud](#nextnet-generate-crud)
- [nextnet generate admin](#nextnet-generate-admin)
- [nextnet db scaffold](#nextnet-db-scaffold)
- [nextnet db health](#nextnet-db-health)
- [nextnet db list](#nextnet-db-list)
- [nextnet db info](#nextnet-db-info)
- [nextnet db explore](#nextnet-db-explore)
- [nextnet db execute](#nextnet-db-execute)

## Global Options

These options apply to all `nextnet` commands:

| Option | Alias | Description |
|--------|-------|-------------|
| `--config` | `-c` | Path to a custom `nextnet.config.json` file |
| `--environment` | `-e` | ASP.NET environment (`Development`, `Staging`, `Production`) |
| `--verbose` | `-v` | Enable verbose output |
| `--help` | `-h` | Show help for the command |
| `--version` | | Show the CLI version |

## nextnet add data

Add a data provider package to the current project.

```bash
nextnet add data <provider>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `provider` | Provider to add: `ef`, `dapper`, `sqlite`, `postgresql`, `mongodb` |

### Examples

```bash
# Add EF Core provider
nextnet add data ef

# Add Dapper provider
nextnet add data dapper

# Add MongoDB provider
nextnet add data mongodb

# Add multiple providers
nextnet add data ef
nextnet add data mongodb
```

### Effects

This command:

1. Adds the required NuGet package(s) to the project.
2. Creates or updates `nextnet.config.json` with a default connection configuration.
3. Wires up the provider registration in `Program.cs` (auto detection).
4. Creates the `Migrations/` directory (if applicable).
5. Adds `NextNet.Data.HealthChecks` package (for health check support).

## nextnet db init

Initialize a database for the specified database type.

```bash
nextnet db init [type]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `type` | Database type: `sqlite`, `postgres`, `mssql` (optional, auto detected from config if omitted) |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection name to initialize (default: default connection) |
| `--drop` | `-d` | Drop existing database if it exists |
| `--force` | `-f` | Force initialization without confirmation |

### Examples

```bash
# Initialize SQLite database
nextnet db init sqlite

# Initialize PostgreSQL database
nextnet db init postgres

# Initialize for a specific connection
nextnet db init --connection Reports

# Drop and recreate
nextnet db init postgres --drop --force
```

### Behavior by Type

| Type | Behavior |
|------|----------|
| `sqlite` | Creates the SQLite database file (`app.db` by default) |
| `postgres` | Creates the database if it does not exist |
| `mssql` | Creates the database if it does not exist |
| *(auto)* | Detects type from connection string provider |

## nextnet db migrate

Apply pending database migrations.

```bash
nextnet db migrate
```

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Target connection name (default: default connection) |
| `--dry-run` | | Preview the SQL that would be executed without applying |
| `--force` | `-f` | Apply even with warnings |
| `--verbose` | `-v` | Show detailed migration output |
| `--script` | `-s` | Generate SQL script instead of executing directly |
| `--output` | `-o` | Output file path (used with `--script`) |

### Examples

```bash
# Apply all pending migrations
nextnet db migrate

# Apply for a specific connection
nextnet db migrate --connection Reports

# Preview without applying
nextnet db migrate --dry-run

# Generate SQL script
nextnet db migrate --script --output migrate.sql

# Apply with verbose output
nextnet db migrate --verbose
```

## nextnet db migration

Manage individual migrations (create, list, status).

```bash
nextnet db migration <subcommand> [args]
```

### Subcommands

| Subcommand | Description |
|------------|-------------|
| `add <name>` | Create a new migration with the given name |
| `status` | Show the status of all migrations |
| `list` | List all migration files |
| `remove` | Remove the last migration (before it is applied) |

### nextnet db migration add

Create a new migration.

```bash
nextnet db migration add <name>
```

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection to create migration for |
| `--namespace` | | Namespace for the generated migration class |

**Examples:**

```bash
# Create a migration
nextnet db migration add AddUserTable

# Create for a specific connection
nextnet db migration add AddOrderTable --connection OrdersDb

# With custom namespace
nextnet db migration add AddIndexes --namespace "MyApp.Data.Migrations"
```

### nextnet db migration status

Show the current migration status.

```bash
nextnet db migration status
```

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection to check status for |
| `--output` | `-o` | Output format: `table` (default), `json` |
| `--fail-if-pending` | | Exit with non-zero code if there are pending migrations |

**Examples:**

```bash
# Show status
nextnet db migration status

# JSON output
nextnet db migration status --output json

# Fail CI build if migrations are pending
nextnet db migration status --fail-if-pending
```

### nextnet db migration list

List all migration files.

```bash
nextnet db migration list
```

## nextnet db rollback

Rollback applied migrations.

```bash
nextnet db rollback [steps]
```

### Arguments

| Argument | Default | Description |
|----------|---------|-------------|
| `steps` | `1` | Number of migrations to roll back |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection to roll back |
| `--target` | `-t` | Roll back to a specific migration name |
| `--dry-run` | | Preview without rolling back |
| `--force` | `-f` | Proceed without confirmation |

### Examples

```bash
# Rollback the last migration
nextnet db rollback

# Rollback 3 migrations
nextnet db rollback 3

# Rollback to a specific migration
nextnet db rollback --target AddUserTable

# Preview rollback
nextnet db rollback --dry-run

# Rollback on a specific connection
nextnet db rollback --connection Reports
```

## nextnet generate model

Generate a model class.

```bash
nextnet generate model <name>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the model (e.g., `User`, `Order`, `ProductCategory`) |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--properties` | `-p` | Property definitions: `"Name:Type:Constraints"` |
| `--namespace` | `-n` | Target namespace (overrides config) |
| `--directory` | `-d` | Output directory (overrides config) |
| `--dry-run` | | Preview output without writing |
| `--force` | `-f` | Overwrite existing file |

### Examples

```bash
# Generate with default properties
nextnet generate model User

# Generate with custom properties
nextnet generate model Product \
  --properties "Id:int:pk,Name:string:required:max(200),Price:decimal,Category:string"

# Generate in custom namespace
nextnet generate model User --namespace "MyApp.Domain"

# Preview only
nextnet generate model User --dry-run
```

See [Property Definitions](scaffolding.md#property-definitions) for the complete property syntax.

## nextnet generate repository

Generate a repository interface and implementation for a model.

```bash
nextnet generate repository <name>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the model (must match an existing model class) |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--interface-name` | | Custom interface name |
| `--namespace` | `-n` | Target namespace |
| `--directory` | `-d` | Output directory |
| `--base-class` | | Custom base repository class |
| `--dry-run` | | Preview output |
| `--force` | `-f` | Overwrite existing files |

### Examples

```bash
# Generate repository
nextnet generate repository User

# With custom interface
nextnet generate repository User --interface-name "IUserRepository"

# Custom base class
nextnet generate repository User --base-class "SoftDeleteRepository<User>"

# Preview
nextnet generate repository User --dry-run
```

## nextnet generate crud

Generate CRUD endpoints for a model.

```bash
nextnet generate crud <name>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Name of the model |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--path` | | Custom URL path (default: `/api/{plural_name}`) |
| `--style` | | Endpoint style: `minimal-api` (default) or `controller` |
| `--no-list` | | Skip generating list endpoint |
| `--no-get` | | Skip generating get endpoint |
| `--no-create` | | Skip generating create endpoint |
| `--no-update` | | Skip generating update endpoint |
| `--no-delete` | | Skip generating delete endpoint |
| `--authorize` | | Add `[Authorize]` attribute |
| `--policy` | | Authorization policy name |
| `--dry-run` | | Preview output |
| `--force` | `-f` | Overwrite existing files |

### Examples

```bash
# Generate all CRUD endpoints
nextnet generate crud User

# Generate as controller
nextnet generate crud User --style controller

# Read-only endpoints only
nextnet generate crud User --no-create --no-update --no-delete

# With authorization
nextnet generate crud User --authorize --policy "AdminOnly"

# Custom path
nextnet generate crud User --path "api/v2/users"
```

## nextnet generate admin

Generate admin pages (Blazor) for an entity.

```bash
nextnet generate admin <entity>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `entity` | Entity name (can be comma separated for multiple) |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--path` | | Custom admin path (default: `admin/{plural_name}`) |
| `--layout` | | Custom Blazor layout page |
| `--dry-run` | | Preview output |
| `--force` | `-f` | Overwrite existing files |

### Examples

```bash
# Generate admin for a single entity
nextnet generate admin User

# Generate for multiple entities
nextnet generate admin User,Order,Product

# Custom path and layout
nextnet generate admin User --path "admin/users" --layout "AdminLayout"
```

## nextnet db scaffold

Generate models, repositories, and CRUD endpoints from an existing database schema (database first).

```bash
nextnet db scaffold [tables]
```

### Arguments

| Argument | Default | Description |
|----------|---------|-------------|
| `tables` | *(all tables)* | Comma separated list of table names to scaffold |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection to scaffold from |
| `--dry-run` | | Preview generated output without writing files |
| `--force` | `-f` | Overwrite existing files |
| `--models-namespace` | | Override models namespace |
| `--repos-namespace` | | Override repositories namespace |
| `--actions-namespace` | | Override actions namespace |
| `--no-models` | | Skip model generation |
| `--no-repos` | | Skip repository generation |
| `--no-actions` | | Skip CRUD action generation |

### Subcommands

| Subcommand | Description |
|------------|-------------|
| `list` | List available tables for scaffolding |

### Examples

```bash
# Scaffold all tables
nextnet db scaffold

# Scaffold specific tables
nextnet db scaffold Users,Orders,Products

# Preview output
nextnet db scaffold --dry-run

# Scaffold only models
nextnet db scaffold --no-repos --no-actions

# List available tables
nextnet db scaffold list
```

## nextnet db health

Check database connectivity health.

```bash
nextnet db health [connection]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `connection` | Connection name to check (default: all connections) |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--output` | `-o` | Output format: `text` (default), `json` |
| `--verbose` | `-v` | Show detailed results including timing |
| `--watch` | `-w` | Watch mode — refresh every 5 seconds |

### Examples

```bash
# Check all connections
nextnet db health

# Check a specific connection
nextnet db health Reports

# JSON output
nextnet db health --output json

# Watch mode
nextnet db health --watch

# Verbose with details
nextnet db health --verbose
```

## nextnet db list

List all configured database connections.

```bash
nextnet db list
```

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--verbose` | `-v` | Show full connection details (hides connection strings) |

### Example Output

```
Configured Connections:
──────────────────────────────────────────────────
  Name       │ Provider       │ Enabled │ Tags
─────────────┼────────────────┼─────────┼──────────────
  Default    │ EntityFramework│ Yes     │ write
  ReadReplica│ EntityFramework│ Yes     │ readonly,replica
  Reports    │ Dapper         │ Yes     │ reporting
  Analytics  │ MongoDB        │ No      │ nosql
──────────────────────────────────────────────────
```

## nextnet db info

Show database provider and configuration information.

```bash
nextnet db info [connection]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `connection` | Connection name (default: default connection) |

### Example Output

```
Connection: Default
──────────────────────────────────
  Provider:     PostgreSQL
  Provider Ver: 2.0.0.0
  Host:         localhost
  Port:         5432
  Database:     myapp
  Pool Size:    100
  Timeout:      30s
  Initialized:  Yes
  Migrations:   5 applied, 0 pending
  Health:       Healthy (5ms)
──────────────────────────────────
```

## nextnet db explore

Explore the database schema (tables, columns, indexes).

```bash
nextnet db explore
```

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection to explore |
| `--table` | `-t` | Explore a specific table |
| `--output` | `-o` | Output format: `table` (default), `json`, `markdown` |

### Examples

```bash
# Explore entire database
nextnet db explore

# Explore a specific table
nextnet db explore --table Users

# Markdown output (useful for documentation)
nextnet db explore --output markdown

# JSON for programmatic use
nextnet db explore --output json
```

### Example Output

```
Database: myapp (PostgreSQL)
────────────────────────────────────────────────────
  Table           │ Columns │ Rows   │ Size
──────────────────┼─────────┼────────┼──────────────
  Users           │ 5       │ 1,234  │ 256 KB
  Orders          │ 8       │ 5,678  │ 1.2 MB
  Products        │ 6       │ 432    │ 128 KB
  OrderItems      │ 4       │ 12,345 │ 2.1 MB
────────────────────────────────────────────────────

Table: Users
────────────────────────────────────────────────────
  Column      │ Type        │ Nullable │ Key
──────────────┼─────────────┼──────────┼─────────
  Id          │ INTEGER     │ No       │ PK
  Name        │ TEXT        │ No       │
  Email       │ TEXT        │ No       │ UQ
  IsActive    │ INTEGER     │ No       │
  CreatedAt   │ TEXT        │ No       │
────────────────────────────────────────────────────
```

## nextnet db execute

Execute a raw SQL script against a database connection.

```bash
nextnet db execute <file>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `file` | Path to the SQL file to execute |

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--connection` | `-c` | Connection to execute against |
| `--dry-run` | | Validate SQL syntax without executing |

### Examples

```bash
# Execute a SQL script
nextnet db execute Migrations/Manual/SeedData.sql

# Validate without executing
nextnet db execute Migrations/Manual/SeedData.sql --dry-run
```

## See Also

- [Getting Started](getting-started.md)
- [Configuration Reference](configuration.md)
- [Migrations Guide](migrations.md)
- [Scaffolding Guide](scaffolding.md)
- [Multi-Database Support](multi-database.md)
- [Health Checks](health-checks.md)
- [Troubleshooting](troubleshooting.md)
