---
uid: reference/configuration-reference
title: Configuration Reference
description: Complete reference for all NextNet configuration options
---

# Configuration Reference `v5.0` `stable`

Complete reference for all NextNet configuration options in `nextnet.config.json`. V5 adds the `designSystem` configuration block for theming, UI components, and Tailwind integration.

## File Format

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
  },
  "isr": {
    "enabled": false,
    "revalidate": 60,
    "staleWhileRevalidate": true
  },
  "designSystem": {
    "enabled": false,
    "theme": {
      "default": "light",
      "modes": ["light", "dark"]
    },
    "components": {
      "prefix": "nn",
      "include": [],
      "exclude": []
    },
    "tailwind": {
      "enabled": true,
      "configOverrides": {}
    }
  },
  "middleware": {
    "security": {},
    "rateLimit": {},
    "cors": {},
    "compression": {}
  },
  "routing": {
    "ignorePatterns": [],
    "pageFileName": "page.cs",
    "routeFileName": "route.cs",
    "layoutFileName": "layout.cs"
  },
  "plugins": [],
  "devTools": {
    "enabled": true,
    "port": 3001
  },
  "edge": {
    "enabled": false,
    "memoryLimit": 128
  }
}
```

## Root Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `appDir` | `string` | `"app"` | Root directory for application pages and routes |
| `outputDir` | `string` | `"dist"` | Build output directory |
| `devPort` | `number` | `3000` | Development server port |
| `ssr` | `boolean` | `true` | Enable server side rendering |
| `ssg` | `boolean` | `false` | Enable static site generation |
| `streaming` | `boolean` | `true` | Enable streaming SSR |
| `serverActions` | `boolean` | `true` | Enable server actions |

## `designSystem` Options

V5 Design System configuration. Controls theming, UI components, and Tailwind CSS integration.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `false` | Enable the V5 design system |
| `theme.default` | `string` | `"light"` | Default theme mode |
| `theme.modes` | `string[]` | `["light", "dark"]` | Available theme modes |
| `theme.respectSystemPreference` | `boolean` | `true` | Respect `prefers-color-scheme` |
| `theme.storageKey` | `string` | `"nn-theme"` | LocalStorage key for theme persistence |
| `theme.transitionDuration` | `number` | `300` | Theme transition duration (ms) |
| `components.prefix` | `string` | `"nn"` | CSS class prefix for components |
| `components.include` | `string[]` | `[]` | Include specific components only |
| `components.exclude` | `string[]` | `[]` | Exclude specific components |
| `tailwind.enabled` | `boolean` | `true` | Generate Tailwind config from tokens |
| `tailwind.configOverrides` | `object` | `{}` | Extend/override generated Tailwind config |
| `tailwind.content` | `string[]` | Auto | Content paths for Tailwind class detection |
| `tailwind.purge` | `boolean` | `true` | Enable unused class purging in production |
| `tailwind.prefix` | `string` | `""` | Tailwind class prefix |
| `tailwind.important` | `boolean` | `false` | Tailwind `!important` mode |

## `rendering` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `prettyPrint` | `boolean` | `false` | Pretty print HTML output (dev only) |
| `maxRecursionDepth` | `number` | `128` | Maximum layout nesting depth |
| `minify` | `boolean` | `true` | Minify HTML output (production only) |

## `isr` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `false` | Enable Incremental Static Regeneration |
| `revalidate` | `number` | `60` | Revalidation interval in seconds |
| `staleWhileRevalidate` | `boolean` | `true` | Serve stale content during revalidation |
| `staleMaxAge` | `number` | `3600` | Max age for stale content in seconds |
| `revalidationToken` | `string` | `""` | Secret token for on demand revalidation |
| `cacheProvider` | `string` | `"memory"` | Cache backend (`memory`, `redis`, `file`) |

## `middleware` Options

### `middleware.security`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `hsts` | `boolean` | `true` | Enable HTTP Strict Transport Security |
| `hstsMaxAge` | `number` | `31536000` | HSTS max-age in seconds |
| `csp` | `string` | `""` | Content Security Policy directive |
| `xFrameOptions` | `string` | `"DENY"` | X-Frame-Options header |
| `xContentTypeOptions` | `boolean` | `true` | X-Content-Type-Options: nosniff |
| `referrerPolicy` | `string` | `"strict-origin-when-cross-origin"` | Referrer-Policy header |

### `middleware.rateLimit`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `false` | Enable rate limiting |
| `maxRequests` | `number` | `100` | Max requests per window |
| `windowMs` | `number` | `60000` | Time window in milliseconds |

### `middleware.cors`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `allowedOrigins` | `string[]` | `[]` | Allowed CORS origins |
| `allowedMethods` | `string[]` | `["GET","POST"]` | Allowed HTTP methods |
| `allowedHeaders` | `string[]` | `[]` | Allowed request headers |
| `allowCredentials` | `boolean` | `false` | Allow credentials |

### `middleware.compression`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `true` | Enable response compression |
| `level` | `string` | `"fast"` | Compression level (`fast`, `optimal`, `smallest`) |
| `minSize` | `number` | `1024` | Minimum response size in bytes |

## `routing` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ignorePatterns` | `string[]` | `[]` | Glob patterns to ignore during route discovery |
| `pageFileName` | `string` | `"page.cs"` | File name for page components |
| `routeFileName` | `string` | `"route.cs"` | File name for API route components |
| `layoutFileName` | `string` | `"layout.cs"` | File name for layout components |

## `serverActions` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `true` | Enable server actions |
| `csrf` | `boolean` | `true` | Enable CSRF protection |
| `originCheck` | `boolean` | `true` | Validate origin header |
| `maxRequestBodySize` | `number` | `10485760` | Max request body size (bytes) |
| `allowedOrigins` | `string[]` | `[]` | Allowed CORS origins for actions |
| `endpointPrefix` | `string` | `"/_actions"` | URL prefix for action endpoints |

## `devTools` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `true` | Enable DevTools dashboard |
| `port` | `number` | `3001` | DevTools dashboard port |
| `hotReload` | `boolean` | `true` | Enable hot reload |
| `errorOverlay` | `boolean` | `true` | Show error overlay in browser |
| `requestLogging` | `boolean` | `true` | Log requests in DevTools |
| `performanceMonitoring` | `boolean` | `true` | Track performance metrics |

## `edge` Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enabled` | `boolean` | `false` | Enable edge runtime |
| `entryPoint` | `string` | `""` | Edge entry point |
| `memoryLimit` | `number` | `128` | Memory limit in MB |
| `timeout` | `number` | `10000` | Request timeout in milliseconds |

## `plugins` Options

Each plugin entry in the array:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `name` | `string` | required | Plugin package name |
| `enabled` | `boolean` | `true` | Enable the plugin |
| `config` | `object` | `{}` | Plugin specific configuration |

## Example: Production Configuration

```json
{
  "appDir": "app",
  "outputDir": "dist",
  "ssr": true,
  "ssg": true,
  "streaming": true,
  "rendering": {
    "minify": true
  },
  "isr": {
    "enabled": true,
    "revalidate": 60,
    "cacheProvider": "redis"
  },
  "designSystem": {
    "enabled": true,
    "theme": {
      "default": "light",
      "modes": ["light", "dark"]
    },
    "components": {
      "prefix": "nn",
      "include": ["Button", "Card", "Input", "Modal", "Table", "Nav"]
    },
    "tailwind": {
      "enabled": true,
      "purge": true
    }
  },
  "middleware": {
    "security": {
      "hsts": true,
      "csp": "default-src 'self'",
      "xFrameOptions": "DENY"
    },
    "compression": {
      "enabled": true,
      "level": "optimal"
    }
  },
  "devTools": {
    "enabled": false
  }
}
```

## Example: Blog Configuration

```json
{
  "appDir": "app",
  "ssg": true,
  "ssr": false,
  "streaming": false,
  "isr": {
    "enabled": true,
    "revalidate": 300
  },
  "designSystem": {
    "enabled": true,
    "theme": {
      "default": "light",
      "modes": ["light", "dark"]
    },
    "components": {
      "prefix": "nn",
      "include": ["Button", "Card", "Input", "Badge", "Nav", "Accordion"]
    }
  },
  "rendering": {
    "prettyPrint": false,
    "minify": true
  }
}
```

## Related

- **Getting Started**: [Configuration](../getting-started/configuration.md)
- **Reference**: [CLI Reference](cli-reference.md)
- **Reference**: [API Reference](api-reference.md)
