---
uid: reference/api-reference
title: API Reference
description: Complete API documentation for NextNet types and members
---

# API Reference `v1.0` `stable`

Core API documentation for NextNet types and members. This reference covers the most commonly used public APIs.

> [!NOTE]
> This reference is a curated overview. The full auto-generated API documentation (produced by DocFX from XML comments) covers every public type, method, and property in detail.

## `IPage` (interface)

`NextNet.Components.IPage`

**Assembly:** `NextNet.Core`

The interface for all NextNet page components. Pages are the top-level routeable components that render HTML for a given URL.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Props` | `IReadOnlyDictionary<string, object>` | Page properties/parameters |

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Render()` | `Task<IHtmlContent>` | Render the page content (implement this) |

### Example

```csharp
public class AboutPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Element("h1", content: HtmlHelper.Text("About Us"));
    }
}
```

---

## `ILayout` (interface)

`NextNet.Components.ILayout`

**Assembly:** `NextNet.Core`

The interface for all NextNet layout components. Layouts wrap page content with a shared shell.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Render(IHtmlContent children)` | `Task<IHtmlContent>` | Render the layout wrapping child content (implement this) |
| `RenderShell()` | `Task<IHtmlContent>` | Render the opening shell for streaming (optional override) |
| `RenderFooter()` | `Task<IHtmlContent>` | Render the closing portion for streaming (optional override) |

### Example

```csharp
public class RootLayout : ILayout
{
    public async Task<IHtmlContent> Render(IHtmlContent children)
    {
        await Task.CompletedTask;

        return HtmlHelper.Fragment(
            HtmlHelper.Element("nav",
                content: HtmlHelper.Element("a",
                    new Dictionary<string, string> { ["href"] = "/" },
                    content: HtmlHelper.Text("Home"))),
            HtmlHelper.Element("main", content: children),
            HtmlHelper.Element("footer", content: HtmlHelper.Text("© 2026"))
        );
    }
}
```

---

## `IErrorPage` (interface)

`NextNet.Components.IErrorPage`

**Assembly:** `NextNet.Core`

Interface for error page components that render when an exception occurs.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Render(Exception exception)` | `Task<IHtmlContent>` | Render the error page with exception details |

### Example

```csharp
public class ErrorPage : IErrorPage
{
    public async Task<IHtmlContent> Render(Exception exception)
    {
        await Task.CompletedTask;

        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text("Something went wrong")),
            HtmlHelper.Element("p", content: HtmlHelper.Text(exception.Message))
        );
    }
}
```

---

## `IHtmlContent` (interface)

`NextNet.Components.IHtmlContent`

**Assembly:** `NextNet.Core`

Represents HTML content that can be rendered to a string or written to a `TextWriter`.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `WriteToAsync(TextWriter writer)` | `Task` | Writes the HTML content to the specified writer |
| `ToHtml()` | `string` | Returns the HTML content as a string |

---

## `HtmlHelper` (static class)

`NextNet.Components.HtmlHelper`

**Assembly:** `NextNet.Core`

Provides static helper methods for building HTML content programmatically.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Element(tagName, attributes, content)` | `IHtmlContent` | Creates any HTML element with optional attributes and content |
| `Text(text)` | `IHtmlContent` | Creates HTML-encoded text content |
| `Raw(html)` | `IHtmlContent` | Creates raw (unencoded) HTML content |
| `Fragment(contents)` | `IHtmlContent` | Combines multiple content items into a single fragment |
| `DocType(type)` | `IHtmlContent` | Creates a DOCTYPE declaration (defaults to `"html"`) |
| `Stylesheet(href)` | `IHtmlContent` | Creates a `<link>` stylesheet element |
| `Script(src)` | `IHtmlContent` | Creates a `<script>` element with source URL |

### Examples

```csharp
// Element with content only
HtmlHelper.Element("h1", content: HtmlHelper.Text("Title"));

// Element with attributes and content
HtmlHelper.Element("a",
    new Dictionary<string, string> { ["href"] = "/about" },
    content: HtmlHelper.Text("About"));

// Self-closing element
HtmlHelper.Element("img", new Dictionary<string, string>
{
    ["src"] = "/logo.png",
    ["alt"] = "Logo"
});

// Multiple children
HtmlHelper.Fragment(
    HtmlHelper.Element("h1", content: HtmlHelper.Text("Hello")),
    HtmlHelper.Element("p", content: HtmlHelper.Text("World"))
);
```

> [!CAUTION]
> `HtmlHelper.Raw()` bypasses HTML encoding. Only use it with trusted content.
> User-provided HTML must be sanitized to prevent XSS attacks. For user content,
> use `HtmlHelper.Text()` which automatically encodes.

---

## `ComponentContext`

`NextNet.Components.ComponentContext`

**Assembly:** `NextNet.Core`

Provides context for the currently executing component, including access to the HTTP context and parsed route/query parameters.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `HttpContext` | `HttpContext` | The underlying ASP.NET Core HTTP context |
| `RouteParams` | `IReadOnlyDictionary<string, string>` | Route parameters extracted from the URL |
| `QueryParams` | `IReadOnlyDictionary<string, string>` | Query string parameters from the URL |

### Example

```csharp
public class BlogPostPage : IPage
{
    private readonly ComponentContext _context;
    private readonly IBlogService _blogService;

    public BlogPostPage(ComponentContext context, IBlogService blogService)
    {
        _context = context;
        _blogService = blogService;
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        var slug = _context.RouteParams["slug"];
        var post = await _blogService.GetBySlug(slug);
        return HtmlHelper.Element("h1", content: HtmlHelper.Text(post.Title));
    }
}
```

---

## `IMiddleware` (interface)

`NextNet.Middleware.IMiddleware`

**Assembly:** `NextNet.Middleware`

Interface for implementing custom middleware components in the NextNet pipeline.

### Methods

| Method | Description |
|--------|-------------|
| `InvokeAsync(MiddlewareContext context, RequestDelegate next)` | Execute middleware logic |

### Example

```csharp
public class RequestLoggerMiddleware : IMiddleware
{
    private readonly ILogger _logger;

    public RequestLoggerMiddleware(ILogger<RequestLoggerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        var start = DateTime.UtcNow;
        _logger.LogInformation("Request: {Method} {Path}",
            context.HttpContext.Request.Method, context.HttpContext.Request.Path);

        await next(context.HttpContext);

        var elapsed = DateTime.UtcNow - start;
        _logger.LogInformation("Response: {StatusCode} ({Elapsed}ms)",
            context.HttpContext.Response.StatusCode, elapsed.TotalMilliseconds);
    }
}
```

---

## `MiddlewareContext`

`NextNet.Middleware.MiddlewareContext`

**Assembly:** `NextNet.Middleware`

Provides context for middleware execution.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `HttpContext` | `HttpContext` | The current HTTP context |
| `Items` | `IDictionary<string, object?>` | Per-request dictionary for sharing data between middleware |
| `Pipeline` | `MiddlewarePipeline` | The owning middleware pipeline |

---

## `INextNetPlugin` (interface)

`NextNet.Plugins.INextNetPlugin`

**Assembly:** `NextNet.Plugins`

Interface for implementing NextNet plugins.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Plugin name |
| `Description` | `string` | Plugin description |
| `Version` | `Version` | Plugin version |

### Methods

| Method | Description |
|--------|-------------|
| `OnInitializeAsync(PluginContext context)` | Called once when the plugin is loaded and initialized |

### Example

```csharp
public class SeoPlugin : INextNetPlugin
{
    public string Name => "NextNet.Seo";
    public string Description => "SEO metadata plugin";
    public Version Version => new(1, 0, 0);

    public async Task OnInitializeAsync(PluginContext context)
    {
        await Task.CompletedTask;
        context.Services.AddScoped<ISeoService, SeoService>();
    }
}
```

---

## `IServerAction` (interface)

`NextNet.ServerActions.ServerActions.IServerAction`

**Assembly:** `NextNet.ServerActions`

Optional interface for server action classes that require dependency injection.

### Methods

| Method | Description |
|--------|-------------|
| `SetServices(IServiceProvider services)` | Called by the action executor to set resolved DI services |

---

## `IRouteComponentResolver` (interface)

`NextNet.Routing.IRouteComponentResolver`

**Assembly:** `NextNet.Routing`

Resolves route components (pages, layouts, error pages) from file system paths.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ResolvePage(string routePath)` | `Type?` | Resolves the page type for a given route path |
| `ResolveLayoutChain(string routePath)` | `IReadOnlyList<Type>` | Resolves the layout chain for a given route path |

---

## `SsrRenderer`

`NextNet.Rendering.SsrRenderer`

**Assembly:** `NextNet.Rendering`

The server-side rendering engine that produces complete HTML pages.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `RenderPageAsync(string routePath)` | `Task<IHtmlContent>` | Renders a complete page with its layout chain |
| `RenderFragmentAsync(IHtmlContent content)` | `Task<string>` | Renders an IHtmlContent fragment to a string |

---

## `LayoutRenderer`

`NextNet.Layouts.LayoutRenderer`

**Assembly:** `NextNet.Layouts`

Resolves and renders layout chains for page components.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ResolveChain(string pagePath)` | `IReadOnlyList<Type>` | Resolves the layout chain for a page path |
| `RenderWithLayoutsAsync(Type pageType, IHtmlContent pageContent)` | `Task<IHtmlContent>` | Wraps page content through the layout chain |

---

## `RouteScanner`

`NextNet.Routing.RouteScanner`

**Assembly:** `NextNet.Routing`

Discovers routes from the file system.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Scan(string appDir)` | `RouteManifest` | Scans a directory for route files |
| `Watch(string appDir)` | `IObservable<RouteChange>` | Watches a directory for file changes |

---

## `BuildPipeline`

`NextNet.Build.BuildPipeline`

**Assembly:** `NextNet.Build`

Build pipeline for production output.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `BuildAsync(BuildOptions options)` | `BuildResult` | Execute the build pipeline |
| `BuildStaticAsync()` | `BuildResult` | Build with static generation |

---

## `IsrRevalidationManager`

`NextNet.Isr.Revalidation.IsrRevalidationManager`

**Assembly:** `NextNet.Isr`

Manages Incremental Static Regeneration revalidation.

### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `RevalidateAsync(string path)` | `Task` | Revalidate a single path |
| `RevalidateManyAsync(params string[] paths)` | `Task` | Revalidate multiple paths |
| `RevalidateByTagAsync(string tag)` | `Task` | Revalidate all pages with a specific cache tag |

---

## Related

- **Concept**: [Components](../core-concepts/components.md)
- **Reference**: [Configuration Reference](configuration-reference.md)
- **Contributing**: [Architecture](../contributing/architecture.md)
