---
uid: reference/migration-guide
title: Migration Guide
description: Migrate existing ASP.NET Core applications to NextNet
---

# Migration Guide `v1.0` `stable`

Migrate your existing ASP.NET Core application to NextNet. This guide covers common migration patterns from controllers, Razor Pages, and Minimal APIs.

## Migration Overview

| From | Difficulty | Effort | Recommended For |
|------|-----------|--------|-----------------|
| ASP.NET Core MVC (Controllers) | Medium | Depends on app size | Large applications |
| ASP.NET Core Razor Pages | Low | Small | Small to medium apps |
| ASP.NET Core Minimal APIs | Low | Small | API-focused apps |
| Other frameworks (Next.js, Astro) | Medium | Significant | New projects recommended |

## From Controllers to Pages

### Before (ASP.NET Core Controller)

```csharp
// File: Controllers/HomeController.cs
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repo.GetAll();
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var user = await _repo.Create(request);
        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
    }
}
```

### After (NextNet API Route)

```csharp
// File: app/api/users/route.cs
public class UsersRoute
{
    private readonly IUserRepository _repo;

    public UsersRoute(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<IResult> Get()
    {
        var users = await _repo.GetAll();
        return Results.Ok(users);
    }

    public async Task<IResult> Post(CreateUserRequest request)
    {
        var user = await _repo.Create(request);
        return Results.Created($"/api/users/{user.Id}", user);
    }
}
```

> [!TIP]
| Benefit | Description |
|---------|-------------|
| No `[Route]` attributes | Path determined by file location |
| No `[ApiController]` | Convention based, not attribute based |
| Reduced boilerplate | No controller base class needed |

## From Razor Pages to NextNet Pages

### Before (Razor Page)

```csharp
// File: Pages/About.cshtml.cs
public class AboutModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "About";
    }
}
```

```html
<!-- File: Pages/About.cshtml -->
@page
@model AboutModel
<h1>About Us</h1>
```

### After (NextNet Page)

```csharp
// File: app/about/page.cs
public class AboutPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Element("h1", content: HtmlHelper.Text("About Us"));
    }
}
```

> [!NOTE]
> NextNet Pages use pure C# instead of Razor syntax. The `HtmlHelper` static class provides
> methods for building HTML programmatically. This eliminates the `.cshtml` file split.

## From View Components to Partials

### Before (View Component)

```csharp
public class NewsLetterViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}
```

```html
<!-- Views/Shared/Components/NewsLetter/Default.cshtml -->
<div class="newsletter">
    <h3>Subscribe to our newsletter</h3>
    <form method="post">
        <input type="email" name="email" placeholder="Your email">
        <button type="submit">Subscribe</button>
    </form>
</div>
```

### After (NextNet Partial)

```csharp
// File: app/_components/_Newsletter.cs
public static class Newsletter
{
    public static IHtmlContent Render()
    {
        return HtmlHelper.Element("div",
            content: HtmlHelper.Fragment(
                HtmlHelper.Element("h3", content: HtmlHelper.Text("Subscribe to our newsletter")),
                HtmlHelper.Element("form",
                    new Dictionary<string, string> { ["method"] = "post", ["action"] = "/api/newsletter/subscribe" },
                    content: HtmlHelper.Fragment(
                        HtmlHelper.Element("input",
                            new Dictionary<string, string> { ["type"] = "email", ["name"] = "email", ["placeholder"] = "Your email" }),
                        HtmlHelper.Element("button", content: HtmlHelper.Text("Subscribe"))
                    ))
            ));
    }
}
```

## From Minimal APIs

### Before (Minimal API)

```csharp
// File: Program.cs
var app = WebApplication.Create(args);

app.MapGet("/api/products", async (IProductService service) =>
{
    var products = await service.GetAll();
    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", async (int id, IProductService service) =>
{
    var product = await service.GetById(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.Run();
```

### After (NextNet API Route)

```csharp
// File: app/api/products/route.cs
public class ProductsRoute
{
    private readonly IProductService _service;

    public ProductsRoute(IProductService service)
    {
        _service = service;
    }

    public async Task<IResult> Get()
    {
        var products = await _service.GetAll();
        return Results.Ok(products);
    }
}
```

```csharp
// File: app/api/products/[id]/route.cs
public class ProductByIdRoute
{
    private readonly IProductService _service;

    public ProductByIdRoute(IProductService service)
    {
        _service = service;
    }

    public async Task<IResult> Get(int id)
    {
        var product = await _service.GetById(id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }
}
```

## From _ViewStart.cshtml to Layouts

### Before

```html
<!-- File: Views/_ViewStart.cshtml -->
@{
    Layout = "_Layout";
}
```

### After

```csharp
// File: app/layout.cs
public class RootLayout : ILayout
{
    public async Task<IHtmlContent> Render(IHtmlContent children)
    {
        await Task.CompletedTask;

        return HtmlHelper.Fragment(
            HtmlHelper.Element("nav",
                content: HtmlHelper.Element("a", new Dictionary<string, string> { ["href"] = "/" },
                    content: HtmlHelper.Text("Home"))),
            HtmlHelper.Element("main", content: children),
            HtmlHelper.Element("footer", content: HtmlHelper.Text("© 2026"))
        );
    }
}
```

> [!NOTE]
> NextNet layouts automatically apply to all pages within their directory hierarchy.
> No explicit `Layout = ...` assignment needed.

## Startup Configuration

### Before (ASP.NET Core)

```csharp
// File: Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
```

### After (NextNet)

```csharp
// File: Program.cs
using NextNet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNet();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseNextNet();

app.Run();
```

## Step by Step Migration Strategy

### Step 1: Create a new NextNet project

```bash
nextnet new migration-target
```

### Step 2: Copy existing services and DI registration

```csharp
// File: Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
// Copy all existing service registrations
```

### Step 3: Migrate pages

1. Create corresponding folders in `app/`
2. Convert `.cshtml` + `.cshtml.cs` to a single `page.cs`
3. Replace Razor syntax with `Html` helper calls

### Step 4: Migrate API routes

1. Create `app/api/` folder structure
2. Convert controller actions to `route.cs` methods
3. Remove `[Route]`, `[ApiController]`, and base class references

### Step 5: Migrate layouts

1. Create `app/layout.cs` from `_Layout.cshtml`
2. Move nested layouts to their corresponding `app/` subdirectories
3. Replace `@RenderBody()` with `ILayout.Render(children)` — the layout receives page content as the `children` parameter

### Step 6: Migrate static files

```bash
cp -r wwwroot/* public/
```

### Step 7: Update configuration

```bash
# Copy relevant settings
cp appsettings.json nextnet.config.json
```

## Migration Checklist

- [ ] Create new NextNet project
- [ ] Copy DI service registrations to `Program.cs`
- [ ] Convert pages from `.cshtml` to `page.cs`
- [ ] Convert API endpoints to `route.cs`
- [ ] Migrate layouts to `layout.cs`
- [ ] Move static files to `public/`
- [ ] Update configuration to `nextnet.config.json`
- [ ] Test all routes match old URLs
- [ ] Verify API responses match format
- [ ] Run `nextnet build` to verify compilation

## Common Migration Issues

| Issue | Solution |
|-------|----------|
| Razor syntax used with `HtmlHelper` | Replace `@` expressions with `HtmlHelper.Element()` calls |
| `ViewData` / `ViewBag` references | Use `IPage.Props` dictionary or shared services |
| `TempData` usage | Use `HttpContext.Session` or cookies |
| `IActionResult` returns | Use `IResult` and `Results.*` helpers |
| `[Authorize]` attributes | Use middleware in the pipeline |
| `_ViewImports.cshtml` | Not needed — NextNet uses global usings |

## Related

- **Concept**: [Components](../core-concepts/components.md)
- **Getting Started**: [Quickstart](../getting-started/quickstart.md)
- **Reference**: [API Reference](api-reference.md)
