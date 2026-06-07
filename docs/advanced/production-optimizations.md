---
uid: advanced/production-optimizations
title: Production Optimizations
description: Performance tuning, caching, and deployment best practices
---

# Production Optimizations `v1.0` `stable`

Optimize your NextNet application for production deployments. This guide covers performance tuning, caching strategies, build optimization, and deployment best practices.

## Build Optimization

### Minification

NextNet minifies HTML output by default in production:

```json
{
  "rendering": {
    "minify": true,
    "prettyPrint": false
  }
}
```

### Tree Shaking

The build pipeline removes unused code:

```bash
nextnet build --optimize
```

This enables:
- Dead code elimination
- Unused route removal
- Dependency pruning
- Asset compression

### Bundle Analysis

Generate a bundle report:

```bash
nextnet build --analyze
```

Output:

```text
Build Analysis Report
━━━━━━━━━━━━━━━━━━━━━

Routes:             12 pages, 4 API endpoints
Total Output Size:  2.4 MB
Static Assets:      1.1 MB (6 files)
Generated HTML:     1.3 MB (16 files)

Largest Routes:
  /dashboard/reports    420 KB
  /products             312 KB
  /search               280 KB

Slowest Routes (render time):
  /dashboard/reports    350ms
  /analytics            280ms
```

## Caching Strategies

### HTTP Caching

Set appropriate cache headers:

```csharp
public class HomePage : IPage
{
    private readonly ComponentContext _context;

    public HomePage(ComponentContext context)
    {
        _context = context;
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        // Static pages: cache aggressively
        _context.HttpContext.Response.Headers["Cache-Control"] = "public, max-age=3600, immutable";

        return HtmlHelper.Element("h1", content: HtmlHelper.Text("Welcome"));
    }
}
```

| Content Type | Cache Strategy | Example |
|-------------|----------------|---------|
| Static pages | `max-age=3600, immutable` | Blog posts, about page |
| API responses | `max-age=60, must-revalidate` | Product listings |
| User-specific | `private, no-cache` | Dashboard, settings |
| Dynamic data | `no-store` | Real-time data |

### Response Compression

Enable compression in production:

```json
{
  "middleware": {
    "compression": {
      "enabled": true,
      "level": "optimal",
      "minSize": 1024
    }
  }
}
```

### CDN Caching

Configure CDN caching for static assets:

```csharp
// File: Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public, max-age=31536000, immutable");
    }
});
```

## Performance Tuning

### Render Performance

| Optimization | Impact | Effort |
|-------------|--------|--------|
| Reduce layout nesting depth | High | Low |
| Cache database queries | High | Medium |
| Use streaming for slow data | Medium | Low |
| Pre-render static pages (SSG) | High | Medium |
| Enable response compression | Medium | Low |

### Database Queries

Use caching for frequently accessed data:

```csharp
public class BlogPostPage : IPage
{
    private readonly ComponentContext _context;
    private readonly IMemoryCache _cache;
    private readonly IBlogService _blogService;

    public BlogPostPage(ComponentContext context, IMemoryCache cache, IBlogService blogService)
    {
        _context = context;
        _cache = cache;
        _blogService = blogService;
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        var slug = _context.RouteParams["slug"];

        var post = await _cache.GetOrCreateAsync($"post:{slug}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _blogService.GetBySlug(slug);
        });

        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text(post.Title)),
            HtmlHelper.Raw(post.ContentHtml)
        );
    }
}
```

### Connection Pooling

Configure connection pooling for external services:

```csharp
// File: Program.cs
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    MaxConnectionsPerServer = 10,
    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
});
```

## Deployment Configuration

### Production Settings

Recommended `nextnet.config.json` for production:

```json
{
  "appDir": "app",
  "outputDir": "dist",
  "ssr": true,
  "ssg": true,
  "streaming": true,
  "rendering": {
    "prettyPrint": false,
    "minify": true
  },
  "isr": {
    "enabled": true,
    "revalidate": 60
  },
  "middleware": {
    "compression": {
      "enabled": true,
      "level": "optimal"
    },
    "security": {
      "hsts": true,
      "csp": "default-src 'self'"
    }
  }
}
```

### Environment Variables

```bash
# Production environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
NEXTNET_DEVTOOLS_ENABLED=false
NEXTNET_CACHE_PROVIDER=redis
NEXTNET_REDIS_CONNECTION=localhost:6379
```

### Health Checks

Add health check endpoints:

```csharp
// File: Program.cs
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration
            })
        });
    }
});
```

## Monitoring

### Metrics

Expose Prometheus-compatible metrics:

```csharp
// File: Program.cs
app.UseNextNetMetrics();
```

Available metrics:

| Metric | Type | Description |
|--------|------|-------------|
| `nextnet_requests_total` | Counter | Total requests |
| `nextnet_request_duration_ms` | Histogram | Request duration |
| `nextnet_routes_total` | Gauge | Active routes |
| `nextnet_render_duration_ms` | Histogram | Render duration |

### Logging

Structured logging in production:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "NextNet": "Information"
    }
  }
}
```

## Related

- **Concept**: [Rendering](../core-concepts/rendering.md)
- **Feature**: [ISR](../features/isr.md)
- **Feature**: [Static Generation](../features/static-generation.md)
- **Reference**: [Configuration Reference](../reference/configuration-reference.md)
