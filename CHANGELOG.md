# Changelog

All notable changes to NextNet will be documented in this file.

## Unreleased

### Added

- _(no unreleased changes yet)_

## [0.2.0] - 2026-06-09

### Added — V1 Core Engine

- **Page base class** — Abstract `Page` with `Route.Params["slug"]` access pattern (PRD V1)
- **IComponentContextAware** — Interface for injecting `ComponentContext` into page components
- **Route wrapper** — Sealed `Route` class wrapping route parameters dictionary
- **58 HtmlHelper element methods** — `H1`–`H6`, `P`, `Div`, `Span`, `Ul`, `Ol`, `Li`, `A`, `Img`, `Input`, `Button`, `Form`, `Table`, `Thead`, `Tbody`, `Tr`, `Th`, `Td`, `Section`, `Header`, `Footer`, `Nav`, `Main`, `Article`, `Aside`, `Pre`, `Code`, `Blockquote`, `Hr`, `Br`
- **HtmlHelper CSS/style/data helpers** — `WithClass()`, `AddClass()`, `WithStyle()`, `WithData()` methods
- **Template install/update/remove CLI commands** — `nextnet template install/update/remove` for community templates
- **Template login command** — `nextnet template login` for API token management

### Fixed — V1 Core Engine

- **SSR renderer** now injects `ComponentContext` into `Page` components via `IComponentContextAware`
- **AdminPageGenerator** generates compilable code (`FindAsync` instead of non-existent `GetByIdAsync`)
- **SourceGenerators** converted to file-scoped namespaces (16 files)
- **EfCoreScaffoldProvider** — replaced `.GetAwaiter().GetResult()` with proper `async`/`await`

### Fixed — V2 Data Layer

- **SqlBuilder** — fixed invalid `OFFSET @Offset ROWS LIMIT @Limit ROWS` SQL to `LIMIT @Limit OFFSET @Offset`
- **Dapper DeleteAsync** — fixed wrong error code (`ResultMappingFailed` → new `EntityNotFound` DS-460)
- **Pluralizer** — consolidated 4 duplicate implementations into single canonical `Pluralizer` class in `NextNet.Data.Abstractions`
- **MultiDb duplicate detection** — fixed to compare connection names instead of provider names
- **ConnectionConfig** — added `Name` property for proper connection name tracking
- **EfCoreAdminSchemaProvider** — SQLite methods now use `SqliteConnection` instead of hardcoded `SqlConnection`

### Fixed — V3 Template Engine

- **Feature resolver** — transitive dependencies are now fed into the variable context (previously discarded)
- **TemplateVersionResolver.ResolveRange** — added `<=`, `<`, `>`, and `||` operators (previously only `^`, `~`, `>=`)
- **BlockProcessor** — new class for inline `{{#if condition}}...{{else}}...{{/if}}` and `{{#unless}}` conditionals in template file content
- **TemplatePublisher** — fixed thread-unsafe `HttpClient.DefaultRequestHeaders` mutation; now uses per-request `HttpRequestMessage`
- **Ed25519** — changed silent `return false` stub to `throw NotSupportedException`
- **AuthorProfile** — API tokens now written with restrictive Unix file permissions (`chmod 600`)
- **KeyNotFoundException** — renamed to `PublisherKeyNotFoundException` to avoid BCL name collision
- **TemplatePackages.Tests** — fixed broken project references

### Added — V5 Design System & UI Ecosystem

- **Design token system** — `ColorToken`, `TypographyToken`, `SpacingToken`, `BorderToken`, `ShadowToken`, `BreakpointToken` with `DesignTokenSet`
- **Missing color scales** — added `secondary` (slate) and `info` (sky-blue) color scales (11 shades each) to `DefaultTokens`
- **Theme engine** — `ThemeManager` with runtime theme switching, `Theme` and `ThemeMetadata` models
- **System dark mode** — `DarkMode` enum (`Light`/`Dark`/`System`), `ISystemPreferenceResolver` interface
- **Theme JSON config** — `ThemeJsonLoader` reads `nextnet.theme.json` for token overrides
- **Light/Dark theme presets** — `LightTheme.Create()` and `DarkTheme.Create()` with `baseTokens` overload
- **CSS variable generation** — `CssCustomPropertyGenerator` outputs `--color-*` custom properties from token resolution
- **XSS protection** — `HtmlContentBuilder.Raw()` now validates input; new `SafeRaw()` method for untrusted content
- **11 UI components** — Alert, Avatar, Badge, Button, Card, Dropdown, Input, Modal, Table, Tabs, Toggle
- **Component abstractions** — `IBaseComponent`, `IRenderableComponent`, `IComponentRenderer`, `RenderContext`
- **Tailwind integration** — `TailwindConfigGenerator`, `TailwindStyleBuilder`, component class mappers
- **UI rendering pipeline** — `ComponentTreeBuilder`, `ComponentTreeRenderer`, `UiPage`, `UiLayout`
- **Theme head injection** — `ThemeHeadInjector` injects CSS variables and theme styles into `<head>`

### Changed — Cross-cutting

- **Solution file** — added 12 missing projects (5 source + 7 test) to `NextNet.sln`; cleaned 4 orphaned GUIDs
- **Solution now contains 76 projects** (was 66)
- **Test count** — 3,560 tests passing (was 3,418); all 38 test projects now run in CI
- **Build** — 0 errors, 0 warnings across entire solution
- **All code** — comprehensive XML documentation on public APIs
- **Error codes** — added error code files for all framework projects (DS-xxx, NN-xxx schemes)
- **Documentation** — added design system, theming, Tailwind integration, and UI component docs

## [3.0.0] - 2026-06-07

### Added

- **Template Engine** — New `nextnet new` command generates projects from templates
- **4 Official Templates** — Blog, API, Dashboard, SaaS
- **Interactive Project Generator** — `nextnet new myapp` with guided prompts
- **Template SDK** — `nextnet template create/validate/package/publish` for authors
- **Template Registry** — HTTP client for community templates
- **Variable Substitution** — `{{variable}}` with dot-notation nested access
- **Conditional Generation** — Boolean expression evaluation for file inclusion
- **Feature Resolution** — Topological sort for feature dependencies
- **Manifest System** — JSON schema validation, SemVer range parsing
- **Template Versioning** — SemVer 2.0 with pre-release and build metadata
- **Package Management** — Download, extract, cache, verify `.sktemplate` files
- **Security** — SHA-256 checksums, RSA-2048 signatures, trusted publishers
- **Marketplace Foundation** — Data layer for future marketplace UI (V4+)
- **CLI Commands**:
  - `nextnet new [template] [name]`
  - `nextnet template list/info/create/validate/package/publish`
  - `nextnet template install/update/remove`

### Changed

- Replaced inline V2 template system with V3 engine
- New error codes: SK-100 to SK-103 (template system)
- CLI syntax: `nextnet new <template> <name>` (positional) replaces `nextnet new <name> --template=<template>`
- Template variables now use `{{variable}}` syntax instead of `@VariableName@`
- Improved JSON serialization using System.Text.Json throughout

### Technical

- 8 new projects: Templates, TemplateEngine, Templates.Official, TemplateSdk, TemplateRegistry, TemplatePackages, TemplateSecurity, TemplateMarketplace
- 5 new community template scenarios supported
- Defense-in-depth security: checksums + signatures + trusted publishers
- Async/await throughout the entire template pipeline

## [0.1.0] - 2026-01-01

### Added

- Initial public release of NextNet
- File-based routing with convention-over-configuration
- Server-side rendering (SSR) engine
- Streaming SSR support
- Layout chain resolution and composition
- Static Site Generation (SSG)
- CLI tooling (`nextnet new`, `nextnet dev`, `nextnet build`)
- Roslyn incremental source generators for route discovery
- Server Actions support
- Route middleware pipeline
- Incremental Static Regeneration (ISR)
- Plugin loading and registration system
- Edge runtime support
- Developer tooling and debugging utilities
