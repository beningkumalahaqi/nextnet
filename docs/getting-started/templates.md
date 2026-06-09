---
uid: getting-started/templates
title: Templates
description: Get started with NextNet V3's first class template system
---

# Getting Started with Templates `v3.0` `stable`

NextNet V3 introduces a first class template system. Generate a new project in seconds with one of four official templates, or create your own.

## Quick Start

```bash
# Create a blog
nextnet new blog MyAwesomeBlog
cd MyAwesomeBlog
dotnet run

# Create an API
nextnet new api MyApi
cd MyApi
dotnet run
```

## Available Templates

Run `nextnet template list` to see all available templates:

```
blog        v1.0.0  Production-ready blog with Markdown, RSS, sitemap
api         v1.0.0  Production-ready REST API with OpenAPI, Swagger, health checks
dashboard   v1.0.0  Admin dashboard with auth, navigation, layout
saas        v1.0.0  Multi-tenant SaaS starter with users, organizations, and auth
```

### Blog Template

Create a content focused blog with built in Markdown support, RSS feeds, and SEO:

```bash
nextnet new blog MyBlog
cd MyBlog
dotnet run
```

The blog template includes:
- Markdown based blog posts
- RSS/Atom feed generation
- Tag based categorization
- Newsletter subscription API
- Blog search functionality
- SEO friendly URLs

### API Template

Create a REST API with OpenAPI/Swagger documentation, health checks, and versioning:

```bash
nextnet new api MyApi
cd MyApi
dotnet run
```

The API template includes:
- OpenAPI / Swagger UI
- Health check endpoints
- API versioning structure
- CORS configuration
- Structured error responses
- Request rate limiting stubs

### Dashboard Template

Create an admin dashboard with authentication, navigation, and responsive layout:

```bash
nextnet new dashboard MyDashboard
cd MyDashboard
dotnet run
```

The dashboard template includes:
- Authentication pages (login, register)
- Sidebar navigation
- Dashboard widgets
- User profile pages
- Settings pages
- Responsive design

### SaaS Template

Create a multi tenant SaaS starter with organizations, user management, and billing stubs:

```bash
nextnet new saas MySaaS
cd MySaaS
dotnet run
```

The SaaS template includes:
- Multi tenant architecture
- Organization CRUD
- Team/role management
- Billing plan stubs
- Usage tracking
- Tenant isolation

## Interactive Mode

For custom projects, use interactive mode by omitting the template argument:

```bash
nextnet new myapp
```

This will prompt you for:
- **Project name** — Your project's name
- **Template choice** — Select from available templates
- **Database** — SQLite, PostgreSQL, or none
- **Authentication** — Include auth (yes/no)
- **Output directory** — Where to create the project

Each choice includes a default value — press Enter to accept defaults.

## Variables

Templates use `{{variable}}` syntax for placeholders. Values can be provided in several ways:

### Via CLI flags

```bash
nextnet new blog MyBlog --base-url http://example.com --author "John Doe"
```

### Via interactive prompts

When a variable has no default value and is not provided via CLI, the interactive prompt asks for it.

### Via manifest defaults

Each variable in the template's `template.json` can specify a `default` value:

```json
{
  "variables": {
    "includeAuth": {
      "type": "bool",
      "default": true,
      "description": "Include authentication"
    }
  }
}
```

### Variable Naming

Use **camelCase** for all variable names:
- `{{projectName}}` ✅
- `{{ProjectName}}` ❌
- `{{connectionString}}` ✅
- `{{ConnectionString}}` ❌

### Nested Variables

Access nested properties using dot notation:

```
{{project.database.provider}}
{{project.database.connectionString}}
{{author.name}}
```

## Conditional Generation

Files and directories can be conditionally included based on variable values:

```json
{
  "files": [
    {
      "source": "auth/login.cs",
      "target": "Auth/Login.cs",
      "condition": "includeAuth == true"
    },
    {
      "source": "database/sqlite.cs",
      "target": "Data/Database.cs",
      "condition": "databaseProvider == 'sqlite'"
    }
  ]
}
```

### Supported Operators

| Operator | Example | Description |
|----------|---------|-------------|
| `==` | `includeAuth == true` | Equality |
| `!=` | `includeAuth != true` | Inequality |
| `&&` | `a && b` | Logical AND |
| `\|\|` | `a \|\| b` | Logical OR |
| `!` | `!includeAuth` | Logical NOT |
| `>` | `version > 2` | Greater than |
| `>=` | `version >= 2` | Greater than or equal |
| `<` | `version < 5` | Less than |
| `<=` | `version <= 5` | Less than or equal |
| `in` | `'sqlite' in databases` | Contains |

### Feature Resolution

For complex templates, features can have dependencies:

```json
{
  "features": {
    "auth": {},
    "database": {},
    "api": { "dependsOn": ["auth"] }
  }
}
```

Features are resolved in dependency order using topological sorting. If `api` depends on `auth`, and `auth` is included, then `api`'s files are included after `auth`'s files.

## Learn More

- **Guides**: [Authoring Templates](../guides/templates.md)
- **CLI Reference**: [CLI Reference](../reference/cli-reference.md)
- **Migration**: [V2 to V3 Migration Guide](../../.plan/V3/launch/migration-guide-v2-to-v3.md)

## Related

- **Getting Started**: [Quickstart](quickstart.md)
- **Getting Started**: [Installation](installation.md)
- **Guides**: [Testing](../guides/testing.md)
