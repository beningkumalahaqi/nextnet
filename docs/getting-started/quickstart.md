---
uid: getting-started/quickstart
title: Quickstart
description: Build your first NextNet application in under 2 minutes
---

# Quickstart `v1.0` `stable`

Build your first NextNet application in under 2 minutes. By the end of this guide, you'll have a working web app with multiple pages and a shared layout.

## Prerequisites

- [NextNet installed](installation.md) (CLI tool and templates)
- .NET 10 SDK or later

## Step 1: Create a new project

```bash
nextnet new my-app
cd my-app
```

This creates a new NextNet project with the following structure:

```text
my-app/
├── app/
│   ├── layout.cs        # Root layout
│   └── page.cs          # Homepage
├── public/
├── nextnet.config.json # Configuration
└── my-app.csproj        # Project file
```

## Step 2: View your homepage

Open `app/page.cs`:

```csharp
// File: app/page.cs
public class HomePage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Element("h1", content: HtmlHelper.Text("Welcome to NextNet"));
    }
}
```

> [!NOTE]
> The file name `page.cs` in the `app/` directory maps to the root route `/`.
> Any file named `page.cs` inside a folder creates a page route at that path.

## Step 3: Start the dev server

```bash
nextnet dev
```

Open `http://localhost:3000` — you'll see your homepage with the "Welcome to NextNet" heading.

## Step 4: Add a second page

Create `app/about/page.cs`:

```csharp
// File: app/about/page.cs
public class AboutPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text("About Us")),
            HtmlHelper.Element("p", content: HtmlHelper.Text("This page was created by adding a file."))
        );
    }
}
```

The dev server automatically detects new files. Navigate to `http://localhost:3000/about` — your new page is live.

> [!TIP]
> NextNet uses **hot reload**. Save any file and see changes instantly without restarting the server.

## Step 5: Add a dynamic route

Create `app/blog/[slug]/page.cs`:

```csharp
// File: app/blog/[slug]/page.cs
public class BlogPostPage : IPage
{
    private readonly ComponentContext _context;

    public BlogPostPage(ComponentContext context)
    {
        _context = context;
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        var slug = _context.RouteParams["slug"];
        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text($"Blog Post: {slug}")),
            HtmlHelper.Element("p", content: HtmlHelper.Text($"You are viewing the post with slug: {slug}"))
        );
    }
}
```

Visit `http://localhost:3000/blog/hello-world` — the `[slug]` parameter captures the URL segment.

## Step 6: Customize the layout

Edit `app/layout.cs`:

```csharp
// File: app/layout.cs
public class RootLayout : ILayout
{
    public async Task<IHtmlContent> Render(IHtmlContent children)
    {
        await Task.CompletedTask;

        return HtmlHelper.Fragment(
            HtmlHelper.Element("nav",
                content: HtmlHelper.Fragment(
                    HtmlHelper.Element("a", new Dictionary<string, string> { ["href"] = "/" },
                        content: HtmlHelper.Text("Home")),
                    HtmlHelper.Raw(" "),
                    HtmlHelper.Element("a", new Dictionary<string, string> { ["href"] = "/about" },
                        content: HtmlHelper.Text("About")),
                    HtmlHelper.Raw(" "),
                    HtmlHelper.Element("a", new Dictionary<string, string> { ["href"] = "/blog" },
                        content: HtmlHelper.Text("Blog"))
                )),
            HtmlHelper.Element("main",
                content: children  // Page content renders here
            ),
            HtmlHelper.Element("footer",
                content: HtmlHelper.Text("© 2026 NextNet")
            )
        );
    }
}
```

The layout wraps every page automatically. Changes to `layout.cs` apply to all routes.

## What you built

- ✅ A homepage at `/`
- ✅ An about page at `/about`
- ✅ A dynamic blog route at `/blog/{slug}`
- ✅ A shared layout with navigation

```text
my-app/
├── app/
│   ├── layout.cs         # Shared layout (nav + footer)
│   ├── page.cs           # → /
│   ├── about/
│   │   └── page.cs       # → /about
│   └── blog/
│       └── [slug]/
│           └── page.cs   # → /blog/{slug}
└── nextnet.config.json
```

## Next Steps

- [Project Structure](project-structure.md) — Deep dive into the project layout
- [File-Based Routing](../core-concepts/routing.md) — Understand how routing works
- [Configuration](configuration.md) — Customize NextNet's behavior
- [API Routes](../features/api-routes.md) — Add REST endpoints to your app
