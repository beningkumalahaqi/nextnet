---
uid: api/index
title: API Reference
description: Landing page for the NextNet API reference section
---

# API Reference

Welcome to the NextNet API Reference. This section documents the public API surface of the NextNet framework.

## Namespaces

| Namespace | Assembly | Description |
|-----------|----------|-------------|
| `NextNet.Components` | `NextNet.Core` | Core abstractions: `IPage`, `ILayout`, `IErrorPage`, `IHtmlContent`, `HtmlHelper`, `ComponentContext` |
| `NextNet.Routing` | `NextNet.Routing` | Route discovery, parsing, and resolution: `RouteScanner`, `RoutePatternParser`, `IRouteComponentResolver` |
| `NextNet.Rendering` | `NextNet.Rendering` | Server-side rendering and streaming engines: `SsrRenderer` |
| `NextNet.Layouts` | `NextNet.Layouts` | Layout chain resolution and composition: `LayoutRenderer` |
| `NextNet.Build` | `NextNet.Build` | Build pipeline and static generation: `BuildPipeline` |
| `NextNet.Cli` | `NextNet.Cli` | CLI commands and utilities |
| `NextNet.Middleware` | `NextNet.Middleware` | Middleware pipeline: `IMiddleware`, `MiddlewareContext` |
| `NextNet.ServerActions` | `NextNet.ServerActions` | Server action generation and runtime: `IServerAction`, `ServerActionExecutor` |
| `NextNet.Plugins` | `NextNet.Plugins` | Plugin system: `INextNetPlugin`, `PluginContext` |
| `NextNet.Isr` | `NextNet.Isr` | Incremental Static Regeneration: `IsrRevalidationManager` |
| `NextNet.Edge` | `NextNet.Edge` | Edge runtime support |

## Core Interfaces

- [`IPage`](xref:NextNet.Components.IPage) — Interface for page components
- [`ILayout`](xref:NextNet.Components.ILayout) — Interface for layout components
- [`IErrorPage`](xref:NextNet.Components.IErrorPage) — Interface for error pages
- [`IHtmlContent`](xref:NextNet.Components.IHtmlContent) — Represents HTML output
- [`IMiddleware`](xref:NextNet.Middleware.IMiddleware) — Interface for middleware components
- [`INextNetPlugin`](xref:NextNet.Plugins.INextNetPlugin) — Interface for plugins
- [`IServerAction`](xref:NextNet.ServerActions.ServerActions.IServerAction) — Optional interface for DI in server actions

## Core Classes

- [`HtmlHelper`](xref:NextNet.Components.HtmlHelper) — Static HTML builder methods
- [`ComponentContext`](xref:NextNet.Components.ComponentContext) — Request context for components
- [`MiddlewareContext`](xref:NextNet.Middleware.MiddlewareContext) — Context for middleware execution
- [`PluginContext`](xref:NextNet.Plugins.PluginContext) — Context for plugin initialization
- [`RouteScanner`](xref:NextNet.Routing.RouteScanner) — File-based route discovery
- [`SsrRenderer`](xref:NextNet.Rendering.SsrRenderer) — Server-side rendering engine
- [`LayoutRenderer`](xref:NextNet.Layouts.LayoutRenderer) — Layout chain renderer
- [`BuildPipeline`](xref:NextNet.Build.BuildPipeline) — Production build pipeline
- [`IsrRevalidationManager`](xref:NextNet.Isr.Revalidation.IsrRevalidationManager) — ISR revalidation manager

## Related

- [API Reference](api-reference.md) — Detailed API documentation with examples
- [Configuration Reference](reference/configuration-reference.md) — Configuration options
- [CLI Reference](reference/cli-reference.md) — Command-line tool reference
