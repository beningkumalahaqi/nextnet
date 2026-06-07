# Scaffolding

Scaffolding generates model classes, repository interfaces/implementations, CRUD endpoints, and admin pages from your data models or database schema.

## Table of Contents

- [Overview](#overview)
- [Model Generation](#model-generation)
- [Repository Generation](#repository-generation)
- [CRUD Generation](#crud-generation)
- [Admin Generation](#admin-generation)
- [Configuration](#configuration)
- [Custom Templates](#custom-templates)
- [Property Definitions](#property-definitions)
- [Generated Artifacts](#generated-artifacts)

## Overview

NextNet's scaffolding system uses `IScaffoldProvider` to generate code. The scaffolding can work in two directions:

1. **Model-first**: Generate repositories and CRUD from your model classes.
2. **Database-first**: Generate models, repositories, and CRUD from an existing database schema.

| Approach | Direction | Use Case |
|----------|-----------|----------|
| Model-first | Model → Repository → CRUD | Greenfield projects, domain-driven design |
| Database-first | Schema → Model → Repository → CRUD | Existing databases, legacy systems |

### Provider Support

| Provider | Scaffold Provider | Model-first | Database-first |
|----------|-------------------|-------------|----------------|
| EF Core | `EfCoreScaffoldProvider` | ✅ | ✅ |
| Dapper | `DapperScaffoldProvider` | ✅ | ✅ |
| SQLite | EF Core-based | ✅ | ✅ |
| PostgreSQL | EF Core-based | ✅ | ✅ |
| MongoDB | `MongoDbScaffoldProvider` | ✅ | Limited |

## Model Generation

### Basic Usage

```bash
# Generate a model with default properties
nextnet generate model User

# Generate with custom properties
nextnet generate model User --properties "Id:int:pk,Name:string,Email:string,Age:int,IsActive:bool"

# Generate in a specific namespace
nextnet generate model User --namespace "MyApp.Domain.Models"

# Generate and add to specific directory
nextnet generate model User --directory "src/MyApp/Models"

# Preview without writing
nextnet generate model User --dry-run

# Force overwrite existing file
nextnet generate model User --force
```

### Generated Output

```csharp
// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApp.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
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
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public int Age { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### Model Generation from Database Schema

```bash
# Scaffold all tables as models
nextnet db scaffold

# Scaffold specific tables
nextnet db scaffold Users,Orders,Products

# Preview tables available for scaffolding
nextnet db scaffold list

# Scaffold with custom namespace
nextnet db scaffold --models-namespace "MyApp.Domain.Entities"
```

## Repository Generation

### Basic Usage

```bash
# Generate repository for User model
nextnet generate repository User

# Generate with custom interface name
nextnet generate repository User --interface-name "IUserRepository"

# Generate in a specific directory
nextnet generate repository User --directory "src/MyApp/Data/Repositories"

# Generate with custom base class
nextnet generate repository User --base-class "CustomRepositoryBase<User>"

# Preview
nextnet generate repository User --dry-run
```

### Generated Output

```csharp
// Repositories/IUserRepository.cs
using NextNet.Data.Abstractions;

namespace MyApp.Repositories;

/// <summary>
/// Repository interface for User entities.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all active users.
    /// </summary>
    Task<PagedResult<User>> GetActiveUsersAsync(int page, int pageSize);
}

// Repositories/UserRepository.cs
using NextNet.Data.Abstractions;
using NextNet.Data.Providers;

namespace MyApp.Repositories;

/// <summary>
/// Repository implementation for User entities.
/// </summary>
public sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(
        IDataConnection connection,
        ILogger<UserRepository> logger)
        : base(connection, logger) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await FindAsync(u => u.Email == email);
    }

    public async Task<PagedResult<User>> GetActiveUsersAsync(
        int page, int pageSize)
    {
        return await GetAllAsync(
            filter: u => u.IsActive,
            orderBy: u => u.Name,
            descending: false,
            page: page,
            pageSize: pageSize);
    }
}
```

## CRUD Generation

### Basic Usage

```bash
# Generate CRUD endpoints for User model
nextnet generate crud User

# Generate with custom base path
nextnet generate crud User --path "api/v2/users"

# Generate without specific endpoints
nextnet generate crud User --no-delete --no-update

# Generate as controller instead of minimal API
nextnet generate crud User --style controller

# Generate with authorization
nextnet generate crud User --authorize --policy "AdminOnly"

# Preview
nextnet generate crud User --dry-run
```

### Generated Output (Minimal API)

```
app/api/Users/
├── List.cs          # GET /api/users
├── Get.cs           # GET /api/users/{id}
├── Create.cs        # POST /api/users
├── Update.cs        # PUT /api/users/{id}
└── Delete.cs        # DELETE /api/users/{id}
```

**List.cs**:

```csharp
using NextNet.Core.Routing;
using NextNet.Data.Abstractions;

namespace MyApp.Api.Users;

/// <summary>
/// GET /api/users — List all users with pagination.
/// </summary>
public sealed class ListUsersEndpoint : IEndpoint
{
    public async Task<IResult> HandleAsync(
        [FromServices] IRepository<User> users,
        int page = 1,
        int pageSize = 20)
    {
        var result = await users.GetAllAsync(
            page: page,
            pageSize: pageSize);

        return Results.Ok(result);
    }
}
```

### Generated Output (Controller)

```csharp
// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers;

/// <summary>
/// CRUD controller for User entities.
/// </summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IRepository<User> _users;

    public UsersController(IRepository<User> users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<User>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _users.GetAllAsync(
            page: page, pageSize: pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById(int id)
    {
        var user = await _users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create(User user)
    {
        var created = await _users.InsertAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> Update(int id, User user)
    {
        if (id != user.Id) return BadRequest();
        return Ok(await _users.UpdateAsync(user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _users.DeleteAsync(id);
        return NoContent();
    }
}
```

## Admin Generation

```bash
# Generate admin pages for an entity
nextnet generate admin User

# Generate admin for multiple entities
nextnet generate admin User,Order,Product

# Generate with custom admin path
nextnet generate admin User --path "admin/users"

# Generate with custom layout
nextnet generate admin User --layout "AdminLayout"
```

Generates Blazor admin pages for managing entities:

```
app/admin/
├── Users/
│   ├── Index.razor       # List
│   ├── Detail.razor      # View/Edit
│   └── Create.razor      # Create
├── Orders/
└── Products/
```

## Configuration

```json
{
  "data": {
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
    }
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `modelsNamespace` | string | `"Models"` | Namespace for generated models |
| `repositoriesNamespace` | string | `"Repositories"` | Namespace for generated repositories |
| `actionsNamespace` | string | `"Actions"` | Namespace for generated CRUD actions |
| `modelsDirectory` | string | `"Models"` | Output directory for models |
| `repositoriesDirectory` | string | `"Repositories"` | Output directory for repositories |
| `actionsDirectory` | string | `"app/api"` | Output directory for CRUD actions |
| `adminDirectory` | string | `"app/admin"` | Output directory for admin pages |
| `crudStyle` | string | `"minimal-api"` | `"minimal-api"` or `"controller"` |
| `overwriteExisting` | bool | `false` | Overwrite existing files |
| `generatePaging` | bool | `true` | Add pagination to list endpoints |
| `generateValidation` | bool | `true` | Add validation attributes |
| `generateAuditFields` | bool | `true` | Add CreatedAt/UpdatedAt |
| `useRecords` | bool | `true` | Generate as `record` types |
| `nullableEnabled` | bool | `true` | Use nullable reference types |
| `templateDirectory` | string? | `null` | Custom template directory |

## Custom Templates

Scaffolding uses [Scriban](https://github.com/scriban/scriban) templates embedded in the provider packages. You can override them with custom templates.

### Default Template Locations

| Template | Purpose | Default Embedded Path |
|----------|---------|----------------------|
| `model.sbn` | Entity model | `NextNet.Data.Scaffolding/Templates/model.sbn` |
| `repository-interface.sbn` | Repository interface | `NextNet.Data.Scaffolding/Templates/repository-interface.sbn` |
| `repository.sbn` | Repository implementation | `NextNet.Data.Scaffolding/Templates/repository.sbn` |
| `crud-list.sbn` | List endpoint | `NextNet.Data.Scaffolding/Templates/crud-list.sbn` |
| `crud-get.sbn` | Get endpoint | `NextNet.Data.Scaffolding/Templates/crud-get.sbn` |
| `crud-create.sbn` | Create endpoint | `NextNet.Data.Scaffolding/Templates/crud-create.sbn` |
| `crud-update.sbn` | Update endpoint | `NextNet.Data.Scaffolding/Templates/crud-update.sbn` |
| `crud-delete.sbn` | Delete endpoint | `NextNet.Data.Scaffolding/Templates/crud-delete.sbn` |

### Custom Template Directory

```json
{
  "data": {
    "scaffolding": {
      "templateDirectory": "Scaffolding/Templates"
    }
  }
}
```

### Example Custom Template

Create `Scaffolding/Templates/model.sbn`:

```scriban
{{~ if has_namespace ~}}
namespace {{ namespace }};

{{~ end ~}}
/// <summary>
/// Represents a {{ name }} in the system.
/// </summary>
public sealed record {{ name }}
{
    public int Id { get; set; }
{{~ for property in properties ~}}
    public {{ property.type }} {{ property.name }} { get; set; }{{ if property.default_value ~}} = {{ property.default_value }};{{ end }}
{{~ end ~}}
}
```

### Template Variables

| Variable | Type | Description |
|----------|------|-------------|
| `name` | string | Entity name (e.g., `User`) |
| `name_plural` | string | Pluralized name (e.g., `Users`) |
| `name_lower` | string | Lowercase name (e.g., `user`) |
| `namespace` | string | Target namespace |
| `properties` | array | List of property definitions |
| `has_namespace` | bool | Whether namespace is set |
| `connection_name` | string | Connection name |
| `provider_type` | string | Provider type name |
| `generate_audit` | bool | Whether to generate audit fields |
| `use_records` | bool | Whether to use record types |

## Property Definitions

When generating models, you can specify custom properties with the `--properties` flag:

### Syntax

```
--properties "Name:Type:Constraints,Name:Type:Constraints"
```

Each property uses the format: `Name:Type:Constraint1,Constraint2`

| Part | Description | Examples |
|------|-------------|---------|
| `Name` | Property name (PascalCase) | `Id`, `FirstName`, `CreatedAt` |
| `Type` | C# type name | `int`, `string`, `decimal`, `DateTime`, `bool` |
| `Constraints` | Comma-separated modifiers | `pk`, `required`, `max(100)`, `email`, `default(0)` |

### Supported Constraints

| Constraint | Description | Example Usage |
|------------|-------------|---------------|
| `pk` | Primary key | `Id:int:pk` |
| `required` | `[Required]` attribute | `Email:string:required` |
| `max(N)` | `[MaxLength(N)]` | `Name:string:max(200)` |
| `min(N)` | `[MinLength(N)]` | `Password:string:min(8)` |
| `range(L,H)` | `[Range(L, H)]` | `Age:int:range(0,150)` |
| `email` | `[EmailAddress]` | `Email:string:email` |
| `phone` | `[Phone]` | `Phone:string:phone` |
| `url` | `[Url]` | `Website:string:url` |
| `creditcard` | `[CreditCard]` | `CardNumber:string:creditcard` |
| `default(V)` | Default value | `IsActive:bool:default(true)` |
| `nullable` | Nullable type | `DeletedAt:DateTime:nullable` |
| `index` | Create database index | `Email:string:index` |
| `unique` | Unique index | `Email:string:unique` |

### Examples

```bash
# Basic model
nextnet generate model Product \
  --properties "Id:int:pk,Name:string:required:max(200),Price:decimal,Category:string"

# Complex model with validation
nextnet generate model Customer \
  --properties "Id:int:pk,Name:string:required:max(100),Email:string:required:email:unique,\
Phone:string:phone,BirthDate:DateTime:nullable,IsVip:bool:default(false)"

# Audit model
nextnet generate model AuditLog \
  --properties "Id:long:pk,EntityName:string:required:max(100),Action:string:required:max(50),\
PerformedBy:string:required:max(100),PerformedAt:DateTime,Details:string:max(4000)"
```

## Generated Artifacts

### Full Scaffold Output

```bash
nextnet generate crud User --style minimal-api
```

```
Models/
├── User.cs

Repositories/
├── IUserRepository.cs
└── UserRepository.cs

app/api/Users/
├── List.cs
├── Get.cs
├── Create.cs
├── Update.cs
└── Delete.cs
```

### Database-First Scaffold Output

```bash
nextnet db scaffold Users,Orders
```

```
Models/
├── User.cs
├── Order.cs
├── UserConfiguration.cs     # EF Core Fluent API config
└── OrderConfiguration.cs

Repositories/
├── IUserRepository.cs
├── UserRepository.cs
├── IOrderRepository.cs
└── OrderRepository.cs

app/api/Users/
└── ...

app/api/Orders/
└── ...
```

## See Also

- [Getting Started](../getting-started.md)
- [Configuration Reference](configuration.md)
- [CLI Reference](cli-reference.md)
- [Multi-Database Support](multi-database.md)
- [Troubleshooting](troubleshooting.md)
