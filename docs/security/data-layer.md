# Data Layer Security Audit

This document provides the security audit checklist and verification procedures for the NextNet V2 Data Layer.

## 1. SQL Injection Prevention

### 1.1 Parameterized Queries

All raw SQL in provider packages **must** use parameterized queries. String concatenation or interpolation of user input into SQL strings is strictly forbidden.

| # | Check | Scope | Status |
|---|-------|-------|--------|
| 1 | All raw SQL uses parameterized queries | Dapper, PostgreSQL, SQLite | ✅ |
| 2 | No string concatenation in SQL building | All provider packages | ✅ |
| 3 | Dapper queries use `new { ... }` anonymous parameters | `NextNet.Data.Dapper` | ✅ |
| 4 | Migration SQL scripts reviewed for injection vectors | All migration engines | ✅ |
| 5 | Scaffolding templates escape user provided identifiers | All scaffold providers | ✅ |

### 1.2 Code Pattern (Safe)

```csharp
// ✅ SAFE — parameterized query
var users = await connection.QueryAsync<User>(
    "SELECT * FROM Users WHERE Email = @Email",
    new { Email = userInput });

// ✅ SAFE — parameterized with Dapper
var result = await connection.ExecuteAsync(
    "UPDATE Products SET Price = @Price WHERE Id = @Id",
    new { Price = model.Price, Id = model.Id });
```

### 1.3 Code Pattern (Unsafe — Blocked)

```csharp
// ❌ UNSAFE — string interpolation
var users = await connection.QueryAsync<User>(
    $"SELECT * FROM Users WHERE Email = '{userInput}'");

// ❌ UNSAFE — concatenation
var sql = "SELECT * FROM Users WHERE Email = '" + userInput + "'";
```

## 2. Connection String Handling

### 2.1 Principles

Connection strings contain sensitive credentials and must be protected at all times.

| # | Check | Scope | Status |
|---|-------|-------|--------|
| 1 | Connection strings never logged | All packages | ✅ |
| 2 | Connection strings never appear in exception messages | All packages | ✅ |
| 3 | Connection strings not exposed in health check responses | `NextNet.Data.HealthChecks` | ✅ |
| 4 | Connection strings should be encrypted at rest | Documentation guidance | 📝 |
| 5 | Default connection strings in templates use placeholders | Scaffolding templates | ✅ |
| 6 | `ConnectionString` setter not exposed publicly on config records | `NextNet.Data.Abstractions` | ✅ |

### 2.2 Guidelines

- **Never log connection strings**: The `ConnectionString` property must never be passed to `ILogger.Log()` or any logging framework.
- **Sanitize exceptions**: If a connection error occurs, replace the connection string value with `[REDACTED]` before including it in exception messages.
- **Health check safety**: Health check results must strip connection string values from error descriptions.
- **Configuration storage**: In production, use Azure Key Vault, AWS Secrets Manager, or environment variables rather than storing plain text connection strings in configuration files.

## 3. Secrets Management

| # | Check | Scope | Status |
|---|-------|-------|--------|
| 1 | Documentation recommends User Secrets for local dev | Documentation site | ✅ |
| 2 | Documentation recommends Azure Key Vault / AWS Secrets Manager for production | Documentation site | ✅ |
| 3 | No hardcoded secrets in source code or templates | All packages | ✅ |
| 4 | CLI commands accept connection strings from environment variable | `NextNet.Cli` | ✅ |
| 5 | Scaffolding templates use placeholders, not real credentials | Scaffolding templates | ✅ |

### 3.1 Recommended Practices

**Local Development:**
```bash
dotnet user-secrets init
dotnet user-secrets set "data:connections:Default:connectionString" "Server=localhost;Database=MyApp;Trusted_Connection=True;"
```

**Production:**
```bash
# Environment variable
export NextNet_Data_Connections_Default_ConnectionString="Server=prod.db;..."

# Azure Key Vault (via configuration)
builder.Configuration.AddAzureKeyVault(
    new Uri("https://myvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

## 4. XSS Prevention

| # | Check | Scope | Status |
|---|-------|-------|--------|
| 1 | All user supplied data HTML-encoded before rendering | Admin dashboard | ✅ |
| 2 | No `Html.Raw()` with user controlled input | Admin dashboard | ✅ |
| 3 | SQL preview renders use `HttpUtility.HtmlEncode` | Admin dashboard | ✅ |
| 4 | Content-Type headers set to `text/html; charset=utf-8` | Admin dashboard | ✅ |
| 5 | CSP headers present on admin dashboard responses | Admin dashboard | ✅ |

### 4.1 Safe Rendering

```csharp
// ✅ SAFE — automatically HTML-encoded
@Model.UserName

// ✅ SAFE — explicit encoding
@Html.Encode(userProvidedValue)

// ❌ UNSAFE — only with string literals
@Html.Raw("<strong>Safe Static Text</strong>")
```

## 5. Dependency Vulnerability Scanning

| # | Check | Tool | Status |
|---|-------|------|--------|
| 1 | All NuGet dependencies scanned for known vulnerabilities | `dotnet list package --vulnerable` | ✅ |
| 2 | No packages with "High" or "Critical" severity vulnerabilities | `dotnet list package --vulnerable --severity high` | ✅ |
| 3 | Dependencies are within supported versions | Manual review | ✅ |
| 4 | Transitive dependencies reviewed for supply chain risk | `dotnet nuget why` | ✅ |

### 5.1 Running the Vulnerability Scan

```bash
# Scan all projects for vulnerable packages
dotnet list package --vulnerable --include-transitive

# Check for high/critical severity only
dotnet list package --vulnerable --include-transitive --severity high
```

### 5.2 CI Integration

The vulnerability scan runs as a required CI step. Builds fail if High or Critical vulnerabilities are detected.

## 6. Provider Isolation Boundaries

| # | Check | Scope | Status |
|---|-------|-------|--------|
| 1 | Abstractions package has no provider dependencies | `NextNet.Data.Abstractions` | ✅ |
| 2 | Providers package has no implementation dependencies | `NextNet.Data.Providers` | ✅ |
| 3 | Provider implementations isolated from each other | All provider packages | ✅ |
| 4 | Health checks package isolated from routing/rendering | `NextNet.Data.HealthChecks` | ✅ |

### 6.1 Dependency Rules

```
NextNet.Data.Abstractions
  └── Microsoft.Extensions.DependencyInjection.Abstractions

NextNet.Data.Providers
  └── NextNet.Data.Abstractions
  └── Microsoft.Extensions.DependencyInjection

NextNet.Data.EntityFramework
  └── NextNet.Data.Abstractions
  └── NextNet.Data.Providers
  └── Microsoft.EntityFrameworkCore

NextNet.Data.Dapper
  └── NextNet.Data.Abstractions
  └── NextNet.Data.Providers
  └── Dapper

(Each provider package follows the same pattern — references Abstractions + Providers + its specific database package)
```

## 7. Audit Automation

The following automated checks run in CI:

- **Architecture Tests**: Enforce dependency isolation rules
- **Vulnerability Scan**: `dotnet list package --vulnerable`
- **CodeQL Analysis**: GitHub CodeQL for C# security patterns
- **Secret Scanning**: Git secrets scan for committed credentials
- **XML Doc Completeness**: Build fails on missing XML docs (CS1591 → error)

## 8. Security Incident Response

If a security issue is discovered:

1. **Immediately**: Open a confidential issue in the repository
2. **Assess**: Determine severity (Low/Medium/High/Critical)
3. **Fix**: Create a private fork, develop and test the fix
4. **Release**: Publish a patched version, update the security advisory
5. **Disclose**: After patch release, publish the advisory publicly

---

*Last updated: 2026-06-06*
*Audit frequency: Every release candidate*
