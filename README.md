<p align="center">
  <img src="images/NextNet-logo.png" alt="NextNet" width="200" />
</p>

# NextNet

> A modern full-stack web framework for .NET — inspired by Next.js, powered by ASP.NET Core.
> **V5 is live.** Design system, streaming SSR, server actions, plugin isolation, edge runtime, multi-database support — all at version 0.2.0.
>
> [![NuGet](https://img.shields.io/nuget/v/NextNet.Cli)](https://nuget.org/packages/NextNet.Cli)
> ![License](https://img.shields.io/github/license/beningkumalahaqi/nextnet)
> ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
> ![Version](https://img.shields.io/badge/NextNet-0.2.0-8A2BE2)

---

## Why NextNet?

ASP.NET Core is powerful but requires significant boilerplate — controllers, route mappings, DI registration, and middleware configuration. NextNet brings the **convention-over-configuration** philosophy of Next.js to the .NET ecosystem.

Create a file, get a route. No controllers. No manual routing. Just C# and a `page.cs` file.

## 🚀 V5 Upgrade — What's New

NextNet V5 is a major release that brings 27 upgraded packages to version 0.2.0, a unified Design System, and a complete UI component framework. **3,005 tests pass** across the entire codebase.

### V5 Highlights

| Area | What's New |
|------|-----------|
| **Design System** | Token-based theming engine, design tokens (DS-000 → DS-929, 258+ error codes), CSS variable generation |
| **UI Components** | `NextNet.UI.Abstractions`, `NextNet.UI.Rendering`, `NextNet.UI.DesignSystem` — declarative component model with Tailwind CSS integration |
| **Tailwind Integration** | First-class Tailwind CSS support, utility-first styling pipeline, automatic purge configuration |
| **Theming** | `NextNet.UI.Theming` — dark/light mode, custom theme definitions, runtime theme switching |
| **Data Providers** | MongoDB, PostgreSQL, SQLite, Dapper, EF Core — all at `NextNet.Data.*`, with health checks and multi-db support |
| **Template System** | Template engine, SDK, registry, marketplace, and security (SHA-256 + RSA-2048) — complete authoring pipeline |
| **Edge Runtime** | `NextNet.Edge` — deploy to edge environments with minimal overhead |
| **ISR** | Incremental Static Regeneration — stale-while-revalidate, on-demand revalidation |
| **Plugin System** | `AssemblyLoadContext` isolation — load plugins without assembly version conflicts |
| **Middleware Pipeline** | Route-level middleware with a clean pipeline API |
| **SSR Streaming** | Partial HTML streaming for faster time-to-first-byte |
| **Error Codes** | 258+ structured error codes (DS-000 through DS-929) for precise diagnostics |

## Packages

NextNet is distributed as **27 NuGet packages** at version 0.2.0. The only one you install directly is the CLI:

```bash
dotnet tool install -g NextNet.Cli
```

The CLI pulls in core framework packages as dependencies automatically. When you scaffold a project with `nextnet new`, the generated project references only the library packages it needs.

| What you install | What it gets you |
|---|---|
| `NextNet.Cli` (dotnet tool) | `nextnet new`, `nextnet dev`, `nextnet build`, templates, scaffolding |
| — auto-installs → | Core engine: Routing, Rendering, Layouts, Server Actions, Source Generators, ISR, Edge, Middleware, Plugins, Build, DevTools |
| — scaffold adds → | Data providers, UI packages, design system, template SDK as needed |

### All 27 Packages

| Category | Packages |
|---|---|
| **Core** | `NextNet.Core`, `NextNet.Routing`, `NextNet.Rendering`, `NextNet.Layouts`, `NextNet.ServerActions`, `NextNet.SourceGenerators` |
| **Runtime** | `NextNet.Isr`, `NextNet.Middleware`, `NextNet.Edge`, `NextNet.Plugins`, `NextNet.DevTools` |
| **Build** | `NextNet.Build`, `NextNet.Cli` |
| **UI & Design** | `NextNet.UI.Abstractions`, `NextNet.UI.Rendering`, `NextNet.UI.DesignSystem`, `NextNet.UI.Theming`, `NextNet.UI.Tailwind`, `NextNet.DesignSystem` |
| **Data** | `NextNet.Data.Abstractions`, `NextNet.Data.Providers`, `NextNet.Data.Dapper`, `NextNet.Data.EntityFramework`, `NextNet.Data.PostgreSQL`, `NextNet.Data.Sqlite`, `NextNet.Data.MongoDB`, `NextNet.Data.MultiDb`, `NextNet.Data.HealthChecks`, `NextNet.Data.Sdk` |

## Quick Start

Install NextNet and create a new project in seconds:

```bash
# 1. Install the NextNet CLI
dotnet tool install -g NextNet.Cli

# 2. Create a new project
nextnet new MyApp
cd MyApp

# 3. Run the dev server
nextnet dev
```

Open `http://localhost:5000` — your app is running.

### From source (for contributors)

```bash
git clone https://github.com/beningkumalahaqi/nextnet.git
cd nextnet
dotnet build
dotnet run --project src/NextNet.Cli -- new MyApp
```

## Features

| Feature | Description |
|---|---|
| **File-based Routing** | Create a file in `app/`, get a route — no configuration needed |
| **Design System** | Token-based theming, 258+ design tokens (DS-000 → DS-929), CSS variable generation |
| **UI Components** | Declarative component model with `NextNet.UI.*` packages and Tailwind CSS integration |
| **Theming** | Dark/light mode, custom theme definitions, runtime theme switching |
| **SSR by Default** | Server-Side Rendering for fast first paint and great SEO |
| **Streaming SSR** | Partial HTML streaming for faster time-to-first-byte |
| **Static Generation** | Pre-render pages at build time with `nextnet build` |
| **Nested Layouts** | Composable page shells with automatic inheritance |
| **API Routes** | REST endpoints alongside your pages — `app/api/users/route.cs` |
| **Server Actions** | Call server functions directly from the client — no manual API wiring |
| **Middleware Pipeline** | Route-level middleware with a clean pipeline API |
| **Plugin System** | Extend NextNet with `AssemblyLoadContext`-isolated plugins |
| **ISR** | Incremental Static Regeneration for stale-while-revalidate patterns |
| **Edge Runtime** | `NextNet.Edge` — deploy to edge environments |
| **Data Providers** | MongoDB, PostgreSQL, SQLite, Dapper, EF Core — with health checks and multi-db support |
| **Template System** | Engine, SDK, registry, marketplace, security — complete authoring pipeline |
| **Package Security** | SHA-256 checksums, RSA-2048 signatures, trusted publishers |
| **Structured Errors** | 258+ error codes (DS-000 → DS-929) for precise diagnostics |

## Example

```csharp
// app/page.cs — That's your homepage!
public class HomePage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Element("h1", content: HtmlHelper.Text("Welcome to NextNet"));
    }
}
```

```csharp
// app/about/page.cs — Creates /about automatically
public class AboutPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text("About NextNet")),
            HtmlHelper.Element("p", content: HtmlHelper.Text("A modern .NET web framework."))
        );
    }
}
```

```csharp
// app/blog/[slug]/page.cs — Dynamic route for blog posts
public class SlugPage : IPage
{
    private readonly IBlogService _blog;
    public SlugPage(IBlogService blog) => _blog = blog;

    public async Task<IHtmlContent?> RenderAsync(string slug)
    {
        var post = await _blog.GetPostAsync(slug);
        if (post is null) return null; // 404
        return HtmlHelper.Element("article", content: post.HtmlContent);
    }
}
```

## Documentation

| Resource | Link |
|----------|------|
| 📖 **Getting Started** | [Installation](docs/getting-started/installation.md) |
| 🚀 **Quick Start** | [Quickstart Guide](docs/getting-started/quickstart.md) |
| 🎨 **Design System** | [docs/design-system/overview.md](docs/design-system/overview.md) |
| 🧩 **UI Components** | [docs/ui-components/overview.md](docs/ui-components/overview.md) |
| 📦 **Templates Guide** | [docs/getting-started/templates.md](docs/getting-started/templates.md) |
| 🛠️ **Template Authoring** | [docs/template-authoring/manifest.md](docs/template-authoring/manifest.md) |
| 🔧 **CLI Reference** | [docs/reference/cli/nextnet-new.md](docs/reference/cli/nextnet-new.md) |
| 📦 **NuGet Packages** | [Browse all 27 packages](https://www.nuget.org/packages?q=NextNet&prerel=false) |
| 💬 **Discord Community** | [Join Discord](https://discord.gg/nextnet) |
| 📝 **Changelog** | [CHANGELOG.md](CHANGELOG.md) |

## V5 Status

| Area | Status |
|------|--------|
| Core Engine (Routing, SSR, Source Gen) | ✅ Complete |
| Layouts, CLI, SSG | ✅ Complete |
| Server Actions, Middleware, Plugins | ✅ Complete |
| ISR, Edge Runtime, Optimizations | ✅ Complete |
| **Design System** (Tokens, Theming, UI) | ✅ **Complete** |
| **Tailwind Integration** | ✅ **Complete** |
| **Data Providers** (MongoDB, PG, SQLite, Dapper, EF) | ✅ **Complete** |
| **Template System** (Engine, SDK, Registry, Marketplace, Security) | ✅ **Complete** |
| **Error Code System** (DS-000 → DS-929) | ✅ **Complete** |

### V5 Highlights

- **3,005 unit tests** passing across 32 test projects
- **27 NuGet packages** at version **0.2.0**
- **258+ structured error codes** (DS-000 → DS-929)
- Design System with token-based theming and CSS variable generation
- Tailwind CSS integration with UI component library
- Full data provider suite: MongoDB, PostgreSQL, SQLite, Dapper, EF Core
- Edge runtime support for low-latency deployments
- Plugin system with `AssemblyLoadContext` isolation
- ISR with stale-while-revalidate and on-demand revalidation
- Streaming SSR for faster time-to-first-byte
- See [CHANGELOG.md](CHANGELOG.md) for the full V5.0.0 release notes

## Contributing

We welcome contributions! See the [architecture overview](docs/contributing/architecture.md) and [development setup](docs/contributing/development-setup.md) to get started.

## License

MIT © NextNet Contributors
