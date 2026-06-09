---
uid: getting-started/configuration
title: Configuration
description: Configure NextNet through nextnet.config.json
---

# Configuration `v1.0` `stable`

Configure NextNet's behavior through `nextnet.config.json` at the root of your project.

## Configuration File

NextNet uses a single configuration file at the project root:

```text
my-app/
└── nextnet.config.json
```

> [!NOTE]
> If `nextnet.config.json` is not present, NextNet uses sensible defaults.
> Running `nextnet new` generates this file automatically.

## All Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `appDir` | `string` | `"app"` | Root directory for application pages and routes |
| `outputDir` | `string` | `"dist"` | Build output directory |
| `devPort` | `number` | `3000` | Development server port |
| `ssr` | `boolean` | `true` | Enable server side rendering |
| `ssg` | `boolean` | `false` | Enable static site generation at build time |
| `streaming` | `boolean` | `true` | Enable streaming SSR |
| `serverActions` | `boolean` | `true` | Enable server actions |
| `rendering` | `object` | `{}` | Rendering engine options |

### `rendering` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `rendering.prettyPrint` | `boolean` | `false` | Pretty print HTML output (development only) |
| `rendering.maxRecursionDepth` | `number` | `128` | Maximum layout nesting depth |
| `rendering.minify` | `boolean` | `true` | Minify HTML output (production only) |

## Complete Example

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

## Configuration Profiles

You can manage different configurations using environment variables or CLI flags.

### Environment Variables

NextNet respects the `ASPNETCORE_ENVIRONMENT` environment variable:

```bash
ASPNETCORE_ENVIRONMENT=Development nextnet dev
ASPNETCORE_ENVIRONMENT=Production nextnet build
```

In **Development** mode:
- Pretty print is enabled by default
- Source maps are generated
- Hot reload is active

In **Production** mode:
- HTML is minified
- Static assets are compressed
- Detailed errors are suppressed

### CLI Flag Overrides

CLI flags can override `nextnet.config.json` settings:

```bash
# Override port
nextnet dev --port 4000

# Override app directory
nextnet dev --app-dir src/app

# Enable SSG for this build
nextnet build --ssg
```

> [!TIP]
> CLI flags take precedence over `nextnet.config.json` settings.
> Use flags for one off overrides and the config file for persistent settings.

## Configuration Precedence

Settings are resolved in this order (highest priority first):

1. CLI flags (e.g., `--port 4000`)
2. Environment variables (e.g., `NEXTNET_DEV_PORT=4000`)
3. `nextnet.config.json`
4. Default values

## Programmatic Configuration

You can also configure NextNet programmatically in `Program.cs`:

```csharp
// File: Program.cs
using NextNet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNextNet(options =>
{
    options.AppDir = "app";
    options.DevPort = 3000;
    options.Ssr = true;
    options.Streaming = true;
});

var app = builder.Build();
app.UseNextNet();

await app.RunAsync();
```

> [!WARNING]
> Programmatic configuration overrides `nextnet.config.json` settings.
> Use one approach consistently to avoid confusion.

## Next Steps

- [Project Structure](project-structure.md) — Understand the project layout
- [CLI Reference](../reference/cli-reference.md) — All CLI commands and options
- [Configuration Reference](../reference/configuration-reference.md) — Complete configuration reference
