# NextNet V2 Data Layer Overview

The NextNet V2 Data Layer provides a unified, multi-provider data access abstraction for .NET 8+ applications. It is designed to support relational databases (via Entity Framework Core, Dapper, SQLite, PostgreSQL) and document databases (via MongoDB) through a consistent, provider-based model.

## Table of Contents

- [What is NextNet Data](#what-is-nextnet-data)
- [Architecture](#architecture)
- [Which Provider Should I Use?](#which-provider-should-i-use)
- [Migration Path from V1](#migration-path-from-v1)
- [Quick Links](#quick-links)

## What is NextNet Data

NextNet Data is a **unified data access layer** that abstracts away the differences between database technologies behind a clean, consistent API. Whether you use EF Core for complex relational queries, Dapper for high-performance micro-ORM access, or MongoDB for document storage, you interact with the same `IRepository<T>`, `IMigrationEngine`, and `IHealthCheckProvider` interfaces.

**Core benefits:**

- **Provider-agnostic API** — Switch databases by changing a configuration value, not your code.
- **Built-in repository pattern** — Consistent CRUD operations across all providers.
- **Unified migration system** — Schema evolution managed through a single `IMigrationEngine` interface.
- **Integrated health checks** — Database connectivity monitoring out of the box.
- **Scaffolding** — Generate models, repositories, and CRUD endpoints from your schema.
- **Multi-database support** — Connect to multiple databases with different providers in one application.

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                       Your Application                            │
│   (Controllers, Services, Minimal APIs, Blazor, etc.)             │
├──────────────────────────────────────────────────────────────────┤
│   IRepository<T>    │   IMigrationEngine     │   IHealthCheck     │
│   IDataConnection   │   IScaffoldProvider    │   IDatabaseSelector │
├──────────────────────────────────────────────────────────────────┤
│                    NextNet.Data.Abstractions                       │
│         (Zero-dependency interfaces and configuration models)      │
├──────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                NextNet.Data.Providers                       │  │
│  │          (Provider registry, builder API, DI wiring)        │  │
│  └─────────────────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────────────────┤
│ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌─────────┐ │
│ │ EF Core  │ │  Dapper  │ │  SQLite  │ │PostgreSQL│ │ MongoDB │ │
│ │ Provider │ │ Provider │ │ Provider │ │ Provider │ │ Provider│ │
│ └──────────┘ └──────────┘ └──────────┘ └──────────┘ └─────────┘ │
├──────────────────────────────────────────────────────────────────┤
│        NextNet.Data.HealthChecks    │   NextNet.Data.MultiDb    │
│        NextNet.Data.Sdk            │   NextNet.Data.SourceGen   │
└──────────────────────────────────────────────────────────────────┘
```

### Layer Breakdown

| Layer | Package | Description |
|-------|---------|-------------|
| **Abstractions** | `NextNet.Data.Abstractions` | Core interfaces (`IRepository<T>`, `IDataProvider`, `IMigrationEngine`, etc.) and configuration models. Zero dependencies on any specific database. Depends only on `Microsoft.Extensions.DependencyInjection.Abstractions`. |
| **Providers** | `NextNet.Data.Providers` | `DataProviderRegistry`, builder API (`AddNextNetData()`), and DI registration. Orchestrates provider resolution at runtime. |
| **Provider Impl.** | `NextNet.Data.EntityFramework` | EF Core implementation: DbContext management, Fluent API config, EF migrations, scaffolding. |
| | `NextNet.Data.Dapper` | Dapper implementation: lightweight ORM, SQL script migrations, raw query support. |
| | `NextNet.Data.Sqlite` | SQLite implementation: local dev database, in-memory testing, cross-platform. |
| | `NextNet.Data.PostgreSQL` | PostgreSQL implementation: Npgsql integration, SSL/TLS, connection pooling. |
| | `NextNet.Data.MongoDB` | MongoDB implementation: document DB, BSON conventions, index management. |
| **Extras** | `NextNet.Data.HealthChecks` | Health check aggregation, caching, and ASP.NET Core endpoint. |
| | `NextNet.Data.MultiDb` | Named connection resolution via `IDatabaseSelector`. |
| | `NextNet.Data.Sdk` | Base classes, attributes, and analyzers for building custom providers. |

## Which Provider Should I Use?

| Scenario | Recommended Provider | Rationale |
|----------|---------------------|-----------|
| Complex relational queries, LINQ, rich domain models | **EF Core** | Full ORM with change tracking, navigation properties, and LINQ support. |
| High-performance reads, microservices, existing SQL | **Dapper** | Minimal overhead (~1 ns over raw ADO.NET), full SQL control. |
| Local development, unit testing, CI pipelines | **SQLite** | Zero-config, file-based or in-memory, cross-platform. |
| Production-grade relational, Geospatial, JSON | **PostgreSQL** | Rich feature set, excellent JSON support, strong ecosystem. |
| Document storage, flexible schema, high write throughput | **MongoDB** | Schema-less documents, horizontal scaling, aggregation pipeline. |
| Read/write splitting, multi-tenant, polyglot persistence | **Multi-DB** | Combine multiple providers under the same `IDatabaseSelector` API. |

## Migration Path from V1

### Breaking Changes in V2

| V1 | V2 | Migration Notes |
|----|----|-----------------|
| `NextNet.Data` monolithic package | Split into `Abstractions` + `Providers` + per-provider packages | Update your `.csproj` references. Add only the provider packages you need. |
| `IDataContext` | `IRepository<T>` + `IDataConnection` | Repositories are now per-entity. Direct connection access via `IDataConnection`. |
| Custom `DbContext` inheritance required | Provider manages DbContext internally | Remove custom `DbContext` base class. Configure via provider options. |
| `[FromServices]` DI in Razor Pages | Standard constructor injection | No change needed — DI continues to work the same way. |
| Manual health check registration | Automatic via provider | Remove manual `AddHealthChecks()` — providers register automatically. |
| `NextNet.Data.Migrations` | Integrated into `IMigrationEngine` per provider | Replace `using NextNet.Data.Migrations` with provider-specific migration engine. |

### Migration Steps

```bash
# 1. Update NextNet CLI
dotnet tool update --global NextNet.Cli

# 2. Replace monolithic package with targeted packages
dotnet remove package NextNet.Data
dotnet add package NextNet.Data.Abstractions
dotnet add package NextNet.Data.Providers
dotnet add package NextNet.Data.EntityFramework   # (or your provider)

# 3. Update nextnet.config.json (see configuration.md)

# 4. Replace IDataContext with IRepository<T> in your services

# 5. Run the V2 migration analyzer
nextnet db migrate v2-check
```

### Backward Compatibility

V2 maintains **source-level compatibility** for common patterns wherever possible. The V2 migration analyzer (`nextnet db migrate v2-check`) scans your codebase for V1 patterns and provides automated fix suggestions.

> [!NOTE]
> V1 packages will continue to receive security patches until **December 2026**. We recommend migrating to V2 to take advantage of new features and performance improvements.

## Quick Links

- [Getting Started](getting-started.md)
- [Configuration Reference](configuration.md)
- [EF Core Provider](providers/ef-core.md)
- [Dapper Provider](providers/dapper.md)
- [SQLite Provider](providers/sqlite.md)
- [PostgreSQL Provider](providers/postgresql.md)
- [MongoDB Provider](providers/mongodb.md)
- [Migrations](migrations.md)
- [Scaffolding](scaffolding.md)
- [Multi-Database](multi-database.md)
- [Health Checks](health-checks.md)
- [Provider SDK](provider-sdk.md)
- [CLI Reference](cli-reference.md)
- [Troubleshooting](troubleshooting.md)
