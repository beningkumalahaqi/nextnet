---
uid: getting-started/project-structure
title: Project Structure
description: Understand the NextNet project layout and file conventions
---

# Project Structure `v1.0` `stable`

Understand the files and directories in a NextNet project.

## Top-Level Layout

A typical NextNet project looks like this:

```text
my-app/
├── app/                  # Application source code
├── public/               # Static assets
├── bin/                  # Build output (auto-generated)
├── obj/                  # Intermediate build files (auto-generated)
├── nextnet.config.json  # Framework configuration
├── my-app.csproj         # .NET project file
├── Program.cs            # Application entry point
└── Properties/           # Launch settings
```

## The `app/` Directory

The `app/` directory is the heart of your NextNet application. Files here automatically become routes.

```text
app/
├── layout.cs             # Root layout (wraps all pages)
├── page.cs               # → /
├── loading.cs            # Loading UI (optional)
├── error.cs              # Error boundary (optional)
├── not-found.cs          # 404 page (optional)
├── about/
│   └── page.cs           # → /about
├── blog/
│   ├── layout.cs         # Blog section layout
│   ├── page.cs           # → /blog
│   └── [slug]/
│       ├── page.cs       # → /blog/{slug}
│       └── loading.cs    # Loading UI for blog posts
├── dashboard/
│   └── page.cs           # → /dashboard
└── api/
    ├── users/
    │   └── route.cs      # → /api/users (REST endpoint)
    └── health/
        └── route.cs      # → /api/health
```

### Special Files

| File | Purpose |
|------|---------|
| `page.cs` | Defines a page route. The file path determines the URL |
| `layout.cs` | Defines a layout that wraps pages in its directory subtree |
| `route.cs` | Defines an API route (REST endpoint) |
| `loading.cs` | Shows a loading UI during streaming SSR |
| `error.cs` | Error boundary for catching rendering errors |
| `not-found.cs` | Custom 404 page |

### Route Conventions

| File Path | URL Pattern | Type |
|-----------|-------------|------|
| `app/page.cs` | `/` | Static |
| `app/about/page.cs` | `/about` | Static |
| `app/blog/[slug]/page.cs` | `/blog/{slug}` | Dynamic |
| `app/docs/[...path]/page.cs` | `/docs/{*path}` | Catch all |
| `app/blog/[[slug]]/page.cs` | `/blog{/slug}?` | Optional |

## The `public/` Directory

Static files served at the root of your application:

```text
public/
├── images/
│   ├── logo.png
│   └── banner.jpg
├── fonts/
│   └── inter.woff2
├── favicon.ico
└── robots.txt
```

Files in `public/` are available at the root URL. For example, `public/images/logo.png` is served at `/images/logo.png`.

> [!WARNING]
> Do not place sensitive files in `public/`. All files in this directory are publicly accessible.

## Configuration Files

### `nextnet.config.json`

The main NextNet configuration file:

```json
{
  "appDir": "app",
  "outputDir": "dist",
  "devPort": 3000,
  "ssr": true,
  "ssg": false,
  "streaming": true,
  "serverActions": true,
  "rendering": {
    "prettyPrint": false,
    "maxRecursionDepth": 128,
    "minify": true
  }
}
```

See the [Configuration Reference](../reference/configuration-reference.md) for all available options.

### `my-app.csproj`

Standard .NET project file. NextNet uses target framework `net10.0`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NextNet.Core" Version="1.0.0" />
  </ItemGroup>
</Project>
```

## Entry Point: `Program.cs`

The minimal entry point for a NextNet app:

```csharp
// File: Program.cs
using NextNet;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNextNet();

var app = builder.Build();
app.UseNextNet();

await app.RunAsync();
```

## Build Output

When you run `nextnet build`, the output goes to the configured `outputDir` (default: `dist/`):

```text
dist/
├── index.html
├── about/
│   └── index.html
├── blog/
│   ├── index.html
│   └── hello-world/
│       └── index.html
└── _nextnet/
    └── manifest.json
```

> [!TIP]
> The `_nextnet/manifest.json` contains the route manifest used for client side navigation and ISR revalidation.

## Next Steps

- [Quickstart](quickstart.md) — Build your first page
- [File Based Routing](../core-concepts/routing.md) — Deep dive into routing
- [Configuration](configuration.md) — Configure NextNet behavior
