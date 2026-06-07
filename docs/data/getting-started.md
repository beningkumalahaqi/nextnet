# Getting Started with NextNet Data Layer

This guide walks through setting up the NextNet V2 Data Layer in a .NET 8+ application using the NextNet CLI. By the end, you will have a full CRUD API running against a SQLite database.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Install the NextNet CLI](#install-the-nextnet-cli)
- [Create a New Project](#create-a-new-project)
- [Add a Data Provider](#add-a-data-provider)
- [Configure the Connection](#configure-the-connection)
- [Initialize the Database](#initialize-the-database)
- [Generate a Model](#generate-a-model)
- [Generate a Repository](#generate-a-repository)
- [Generate CRUD Endpoints](#generate-crud-endpoints)
- [Run Migrations](#run-migrations)
- [Full End-to-End Example](#full-end-to-end-example)
- [Next Steps](#next-steps)

## Prerequisites

- .NET 8.0 SDK or later
- A terminal (PowerShell, bash, zsh)
- (Optional) SQLite — included via provider, no separate install needed

Verify your environment:

```bash
dotnet --version
# Should be >= 8.0.100
```

## Install the NextNet CLI

```bash
dotnet tool install --global NextNet.Cli
```

Verify the installation:

```bash
nextnet --version
```

> If you already have an older version, update with:
> ```bash
> dotnet tool update --global NextNet.Cli
> ```

## Create a New Project

```bash
# Create a new ASP.NET Core Web API project
nextnet new app MyDataApp
cd MyDataApp

# Alternatively, use the .NET CLI directly:
# dotnet new webapi -n MyDataApp
# cd MyDataApp
```

## Add a Data Provider

Use the `nextnet add data` command to add a data provider to your project:

```bash
# Add EF Core provider (default)
nextnet add data ef

# Other options:
# nextnet add data dapper
# nextnet add data mongodb
# nextnet add data sqlite
# nextnet add data postgresql
```

This command:

1. Adds the required NuGet packages to your `.csproj`.
2. Creates or updates `nextnet.config.json` with a default data configuration.
3. Adds registration code to `Program.cs`.
4. Creates a `Migrations/` directory if applicable.

**Manual alternative:**

```bash
dotnet add package NextNet.Data.Abstractions
dotnet add package NextNet.Data.Providers
dotnet add package NextNet.Data.Sqlite
```

## Configure the Connection

After running `nextnet add data`, your `nextnet.config.json` will look similar to:

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

Adjust the `connectionString` and `provider` values for your environment.

## Initialize the Database

Create the initial database file and migration:

```bash
nextnet db init sqlite
```

This command:

1. Creates an empty SQLite database file (`app.db` by default).
2. Creates the initial migration directory structure.
3. Generates the first migration script.

> For PostgreSQL: `nextnet db init postgres`
> For MongoDB: No initialization needed — databases are created on first write.

## Generate a Model

Generate a `User` model class:

```bash
nextnet generate model User
```

This creates `Models/User.cs`:

```csharp
namespace MyDataApp.Models;

public sealed record User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

You can customize the generated properties. To generate a model with specific properties:

```bash
nextnet generate model User --properties "Id:int:pk,Name:string,Email:string,CreatedAt:datetime"
```

## Generate a Repository

Generate a repository for the `User` model:

```bash
nextnet generate repository User
```

This creates:

- `Repositories/IUserRepository.cs` — repository interface
- `Repositories/UserRepository.cs` — repository implementation

```csharp
// Repositories/IUserRepository.cs
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

// Repositories/UserRepository.cs
public sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(IDataConnection connection, ILogger<UserRepository> logger)
        : base(connection, logger) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await FindAsync(u => u.Email == email);
    }
}
```

## Generate CRUD Endpoints

Generate minimal API CRUD endpoints for the `User` model:

```bash
nextnet generate crud User
```

This creates `app/api/Users/` with the following files:

| File | HTTP Endpoint | Description |
|------|--------------|-------------|
| `List.cs` | `GET /api/users` | List users with pagination |
| `Get.cs` | `GET /api/users/{id}` | Get user by ID |
| `Create.cs` | `POST /api/users` | Create a new user |
| `Update.cs` | `PUT /api/users/{id}` | Update an existing user |
| `Delete.cs` | `DELETE /api/users/{id}` | Delete a user |

Example generated endpoint (`app/api/Users/List.cs`):

```csharp
using NextNet.Core.Routing;
using NextNet.Data.Abstractions;

namespace MyDataApp.Api.Users;

public sealed class ListEndpoint : IEndpoint
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

The endpoints are automatically registered via the NextNet source generator — no manual routing setup needed.

## Run Migrations

Apply pending migrations to your database:

```bash
nextnet db migrate
```

Or, for more control:

```bash
# Create a named migration
nextnet db migration add InitialCreate

# Apply pending migrations
nextnet db migrate

# Check migration status
nextnet db migration status
```

## Full End-to-End Example

Here is a complete walkthrough from scratch:

```bash
# 1. Install CLI
dotnet tool install --global NextNet.Cli

# 2. Create project (EF Core + SQLite)
nextnet new app BlogAPI
cd BlogAPI
nextnet add data sqlite

# 3. Generate domain
nextnet generate model Post --properties "Id:int:pk,Title:string,Body:string,Published:bool"
nextnet generate model Comment --properties "Id:int:pk,PostId:int,Text:string,Author:string"
nextnet generate repository Post
nextnet generate repository Comment

# 4. Generate CRUD
nextnet generate crud Post
nextnet generate crud Comment

# 5. Configure relationship in DbContext
# Edit Data/AppDbContext.cs to add navigation properties

# 6. Initialize and migrate
nextnet db init sqlite
nextnet db migration add InitialCreate
nextnet db migrate

# 7. Run the app
dotnet run

# 8. Test the API
curl http://localhost:5000/api/posts
curl -X POST http://localhost:5000/api/posts \
  -H "Content-Type: application/json" \
  -d '{"title":"Hello World","body":"My first post","published":true}'
```

### Project Structure After Setup

```
MyDataApp/
├── app/
│   └── api/
│       ├── Posts/
│       │   ├── List.cs
│       │   ├── Get.cs
│       │   ├── Create.cs
│       │   ├── Update.cs
│       │   └── Delete.cs
│       └── Comments/
│           └── ...
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── Post.cs
│   └── Comment.cs
├── Repositories/
│   ├── IPostRepository.cs
│   ├── PostRepository.cs
│   ├── ICommentRepository.cs
│   └── CommentRepository.cs
├── Migrations/
│   └── 001_InitialCreate.sql
├── nextnet.config.json
├── Program.cs
└── MyDataApp.csproj
```

## Next Steps

- Learn about [configuration options](configuration.md)
- Read the [EF Core provider guide](providers/ef-core.md) for relational databases
- Set up [database migrations](migrations.md) for production
- Enable [health checks](health-checks.md) for monitoring
- Use [multiple databases](multi-database.md) in one application
- Create [custom scaffolding templates](scaffolding.md#custom-templates)
- Review the [CLI reference](cli-reference.md) for all available commands
