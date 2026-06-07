---
uid: guides/templates
title: Templates
description: Project templates for scaffolding NextNet applications
---

# Templates `v1.0` `stable`

NextNet provides project templates for quickly scaffolding new applications. Use the `nextnet new` command with different templates to match your project type.

## Available Templates

| Template | Description | When to Use |
|----------|-------------|-------------|
| `default` | Full-featured app with layouts, pages, and API | Most projects |
| `empty` | Minimal project with one page | Starting from scratch |
| `blog` | Blog with posts, tags, and RSS | Content sites |
| `api` | API-only project without pages | Backend services |

## Template: `default`

The default template includes a complete project structure with navigation, layouts, and example pages.

```bash
nextnet new my-app --template default
```

Creates:

```text
my-app/
├── app/
│   ├── layout.cs              # Root layout with navigation
│   ├── page.cs                # Homepage
│   ├── about/
│   │   └── page.cs            # About page
│   └── api/
│       └── health/
│           └── route.cs       # Health check endpoint
├── public/
│   └── styles.css
├── nextnet.config.json
├── Program.cs
└── my-app.csproj
```

## Template: `empty`

The empty template provides the minimum files to start.

```bash
nextnet new my-api --template empty
```

Creates:

```text
my-api/
├── app/
│   └── page.cs                # Single page
├── nextnet.config.json
├── Program.cs
└── my-api.csproj
```

## Template: `blog`

The blog template includes blog-specific features.

```bash
nextnet new my-blog --template blog
```

Creates:

```text
my-blog/
├── app/
│   ├── layout.cs              # Blog layout
│   ├── page.cs                # Homepage with post list
│   ├── about/
│   │   └── page.cs            # About page
│   ├── blog/
│   │   ├── layout.cs          # Blog section layout
│   │   ├── page.cs            # Blog index
│   │   ├── [slug]/
│   │   │   └── page.cs        # Blog post
│   │   └── tags/
│   │       └── [tag]/
│   │           └── page.cs    # Posts by tag
│   └── api/
│       ├── subscribe/
│       │   └── route.cs       # Newsletter subscription
│       └── search/
│           └── route.cs       # Blog search
├── public/
│   ├── styles.css
│   └── images/
├── nextnet.config.json
├── Program.cs
└── my-blog.csproj
```

## Template: `api`

The API template creates a project with only API routes, no pages.

```bash
nextnet new my-api --template api
```

Creates:

```text
my-api/
├── app/
│   └── api/
│       ├── health/
│       │   └── route.cs       # Health check
│       ├── users/
│       │   └── route.cs       # Users CRUD
│       └── v1/
│           └── products/
│               └── route.cs   # Products API
├── nextnet.config.json
├── Program.cs
└── my-api.csproj
```

## Configuration

Set the default template in `nextnet.config.json`:

```json
{
  "templates": {
    "defaultTemplate": "default"
  }
}
```

## Custom Templates

You can create custom templates for your organization:

```bash
# Create a template from an existing project
nextnet new my-template --save-template

# Use it later
nextnet new my-app --template my-template
```

> [!TIP]
> Custom templates are stored in `~/.nextnet/templates/` and can be shared via Git or NuGet packages.

## Template Structure

Templates follow this structure:

```text
~/.nextnet/templates/
└── my-template/
    ├── template.json           # Template metadata
    ├── app/
    │   └── page.cs
    ├── nextnet.config.json
    ├── Program.cs
    └── __ProjectName__.csproj  # Placeholder for project name
```

### `template.json`

```json
{
  "name": "My Custom Template",
  "description": "A template for my organization",
  "shortName": "my-template",
  "tags": ["custom", "organization"],
  "parameters": {
    "includeAuth": {
      "type": "bool",
      "default": true,
      "description": "Include authentication"
    }
  }
}
```

> [!NOTE]
> The placeholder `__ProjectName__` is automatically replaced with the project name provided via `nextnet new`.

## Related

- **Getting Started**: [Quickstart](../getting-started/quickstart.md)
- **Getting Started**: [Project Structure](../getting-started/project-structure.md)
- **Reference**: [CLI Reference](../reference/cli-reference.md)
