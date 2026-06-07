---
uid: guides/file-system-conventions
title: File System Conventions
description: Special files, directories, and naming conventions
---

# File System Conventions `v1.0` `stable`

NextNet uses file system conventions to define routes, layouts, and components. Understanding these conventions is key to building applications efficiently.

## Overview

The `app/` directory is the heart of your application. Every file follows naming conventions that determine its behavior.

```text
app/
├── page.cs              # → / (homepage)
├── layout.cs            # Root layout
├── loading.cs           # Root loading state
├── error.cs             # Error boundary
├── not-found.cs         # 404 page
├── about/
│   └── page.cs          # → /about
├── blog/
│   ├── layout.cs        # Blog section layout
│   ├── page.cs          # → /blog
│   ├── [slug]/
│   │   └── page.cs      # → /blog/{slug}
│   └── loading.cs       # Blog loading state
├── api/
│   └── users/
│       └── route.cs     # → /api/users (API)
├── (marketing)/
│   └── page.cs          # → / (route group)
├── _components/
│   ├── _Card.cs         # Private component
│   └── _Header.cs       # Private component
└── _actions/
    └── UserActions.cs   # Server actions
```

## File Naming Conventions

### Page Files (`page.cs`)

Files named `page.cs` create page routes:

| Location | Route |
|----------|-------|
| `app/page.cs` | `/` |
| `app/about/page.cs` | `/about` |
| `app/blog/[slug]/page.cs` | `/blog/{slug}` |

### Route Files (`route.cs`)

Files named `route.cs` create API endpoints:

| Location | Route |
|----------|-------|
| `app/api/users/route.cs` | `/api/users` |
| `app/api/health/route.cs` | `/api/health` |
| `app/api/users/[id]/route.cs` | `/api/users/{id}` |

### Layout Files (`layout.cs`)

Files named `layout.cs` define layouts that wrap pages in their directory:

| Location | Scope |
|----------|-------|
| `app/layout.cs` | All pages (root layout) |
| `app/blog/layout.cs` | All pages under `app/blog/` |

### Loading Files (`loading.cs`)

Files named `loading.cs` define loading UI for streaming SSR:

| Location | Scope |
|----------|-------|
| `app/loading.cs` | Loading UI for all routes |
| `app/blog/loading.cs` | Loading UI for blog routes |
| `app/blog/[slug]/loading.cs` | Loading UI for individual blog posts |

### Error Files (`error.cs`)

Files named `error.cs` define error boundaries:

| Location | Scope |
|----------|-------|
| `app/error.cs` | Error UI for all routes |
| `app/blog/error.cs` | Error UI for blog routes |

### Not Found (`not-found.cs`)

Files named `not-found.cs` define custom 404 pages:

| Location | Scope |
|----------|-------|
| `app/not-found.cs` | Global 404 page |

## Special Directories

### Private Directory (`_components/`)

Directories prefixed with `_` contain private components that are not routes:

```text
app/
├── _components/
│   ├── _Card.cs
│   ├── _Header.cs
│   └── _Footer.cs
└── page.cs
```

Use these in your pages:

```csharp
public class HomePage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Fragment(
            HtmlHelper.Element("header", content: HtmlHelper.Text("Site Header")),
            HtmlHelper.Element("h1", content: HtmlHelper.Text("Home")),
            HtmlHelper.Element("footer", content: HtmlHelper.Text("Site Footer"))
        );
    }
}
```

> [!NOTE]
> The `_` prefix tells NextNet to skip these files during route discovery.
> They are reusable through:
> - **Composition** via `HtmlHelper.Fragment()` to combine multiple elements into one render output
> - **Component classes** implementing `IPage`/`ILayout` that can be instantiated and rendered independently
> - **Layout slots** via `RenderShell()`/`RenderFooter()` in layout files to inject content into named regions

### Route Groups (`(name)/`)

Parentheses in directory names create route groups — they organize files without affecting the URL:

```text
app/
├── (marketing)/
│   ├── page.cs         # → /
│   └── about/
│       └── page.cs     # → /about
├── (blog)/
│   ├── layout.cs
│   └── [slug]/
│       └── page.cs     # → /{slug}
└── (admin)/
    └── dashboard/
        └── page.cs     # → /dashboard
```

### Server Actions (`_actions/`)

The `_actions/` directory contains server action classes:

```text
app/
└── _actions/
    ├── UserActions.cs
    ├── ProductActions.cs
    └── AuthActions.cs
```

These are not routes but generate API endpoints automatically.

## Static Directory (`public/`)

The `public/` directory serves static files at the root URL:

```text
public/
├── images/
│   └── logo.png       # → /images/logo.png
├── fonts/
│   └── inter.woff2    # → /fonts/inter.woff2
├── favicon.ico         # → /favicon.ico
└── robots.txt          # → /robots.txt
```

## Directory Resolution Rules

1. **Route matching** uses the longest prefix match
2. **Layout resolution** walks up the directory tree from the page file
3. **Route groups `(name)`** are transparent to routing and layout inheritance
4. **Private `_` directories** are excluded from route discovery

## Configuration

Customize file naming conventions in `nextnet.config.json`:

```json
{
  "routing": {
    "pageFileName": "page.cs",
    "routeFileName": "route.cs",
    "layoutFileName": "layout.cs",
    "loadingFileName": "loading.cs",
    "errorFileName": "error.cs",
    "notFoundFileName": "not-found.cs",
    "ignorePatterns": [
      "**/_*",
      "**/node_modules/**"
    ]
  }
}
```

## Related

- **Concept**: [Routing](../core-concepts/routing.md)
- **Concept**: [Components](../core-concepts/components.md)
- **Concept**: [Layouts](../core-concepts/layouts.md)
- **Reference**: [Configuration Reference](../reference/configuration-reference.md)
