# Changelog

All notable changes to NextNet will be documented in this file.

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

## Unreleased

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
