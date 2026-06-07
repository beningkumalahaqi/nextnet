# NextNet Development Guide

> For AI agents and contributors working on the NextNet framework.

## Codebase Structure

```
nextnet/
├── src/
│   ├── NextNet.Core/            # Core abstractions (Page, Layout, Html helpers)
│   ├── NextNet.Routing/         # File-based route discovery and parsing
│   ├── NextNet.Layouts/         # Layout chain resolution and rendering
│   ├── NextNet.Rendering/       # SSR and Streaming engine
│   ├── NextNet.Build/           # Build pipeline and SSG
│   ├── NextNet.Cli/             # CLI commands (new, dev, build, publish)
│   ├── NextNet.ServerActions/   # Server action generation and runtime
│   ├── NextNet.SourceGenerators/ # Roslyn incremental generators
│   ├── NextNet.Isr/             # Incremental Static Regeneration
│   ├── NextNet.Plugins/         # Plugin loading system
│   ├── NextNet.Middleware/      # Route middleware
│   ├── NextNet.Edge/            # Edge runtime support
│   └── NextNet.DevTools/        # Developer tooling
├── tests/
│   ├── NextNet.Core.Tests/
│   ├── NextNet.Routing.Tests/
│   ├── NextNet.Rendering.Tests/
│   ├── NextNet.Layouts.Tests/
│   ├── NextNet.Build.Tests/
│   ├── NextNet.Cli.Tests/
│   ├── NextNet.ServerActions.Tests/
│   ├── NextNet.Plugins.Tests/
│   ├── NextNet.Middleware.Tests/
│   ├── NextNet.Edge.Tests/
│   ├── NextNet.Isr.Tests/
│   └── NextNet.DevTools.Tests/
├── docs/                         # Documentation site source
├── .plan/                        # Planning and research documents
├── nextnet.config.json          # Framework configuration
└── NextNet.sln                  # Solution file
```

## Build Commands

```bash
# Restore all dependencies
dotnet restore

# Build the entire solution
dotnet build NextNet.sln

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/NextNet.Routing.Tests/

# Run tests by category
dotnet test tests/ --filter "Category=Unit"

# Run tests matching a name
dotnet test --filter "FullyQualifiedName~RouteParser"

# Build documentation locally (requires docfx configuration)
# See docs/README.md for setup instructions
```

## Key Entry Points

| Component | File |
|-----------|------|
| Route Discovery | `src/NextNet.Routing/RouteScanner.cs` |
| Route Parser | `src/NextNet.Routing/RoutePatternParser.cs` |
| Source Generator | `src/NextNet.SourceGenerators/RouteDiscoveryGenerator.cs` |
| Page Base Class | `src/NextNet.Core/Components/IPage.cs` |
| HTML Builder | `src/NextNet.Core/Components/HtmlHelper.cs` |
| Layout Renderer | `src/NextNet.Layouts/LayoutRenderer.cs` |
| SSR Engine | `src/NextNet.Rendering/SsrRenderer.cs` |
| Build Pipeline | `src/NextNet.Build/StaticGeneration/BuildPipeline.cs` |
| CLI Entry Point | `src/NextNet.Cli/Program.cs` |
| Server Actions | `src/NextNet.ServerActions/ServerActions/ServerActionExecutor.cs` |
| ISR Manager | `src/NextNet.Isr/Revalidation/IsrRevalidationManager.cs` |
| Plugin Loader | `src/NextNet.Plugins/PluginLoader.cs` |

## Conventions

- All public types **must** have XML documentation comments with `<summary>`, `<remarks>`, and `<example>` where applicable
- Tests follow `{MethodName}_Should_{ExpectedBehavior}_When_{Condition}` naming pattern
- Use `record` types for immutable data models (route metadata, config models)
- Source generators use `IIncrementalGenerator` — never `ISyntaxReceiver`
- File-based routes use `[slug]` syntax matching Next.js conventions
- Use file-scoped namespaces (`namespace X.Y;`) throughout
- Prefer primary constructors for simple DI scenarios

## Testing

```bash
# Unit tests (fast, no external dependencies)
dotnet test tests/ --filter "Category=Unit"

# Integration tests
dotnet test tests/ --filter "Category=Integration"

# Performance benchmarks (if configured)
# dotnet run --project tests/NextNet.Benchmarks/

# Test a specific feature area
dotnet test --filter "FullyQualifiedName~Routing"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

### Writing Tests

```csharp
// ✅ GOOD: Clear, focused test
[Fact]
[Category("Unit")]
public void ParseRoute_Should_ExtractSlugParameter_When_BracketNotation()
{
    var result = RoutePatternParser.Parse("blog/[slug]");
    Assert.Equal("slug", result.Parameters[0].Name);
}

// ❌ BAD: Vague, unfocused test
[Fact]
public void TestRoute()
{
    // Tests too many things at once
}
```

## Common Tasks

### Add a new route type (e.g., `[id:uuid]`)

1. Update route parser in `src/NextNet.Routing/RoutePatternParser.cs`
2. Add tests in `tests/NextNet.Routing.Tests/`
3. Update source generator if parameter type affects codegen
4. Add documentation in `docs/core-concepts/routing.md`

### Add a new CLI command

1. Add command class in `src/NextNet.Cli/Commands/`
2. Register in `src/NextNet.Cli/Program.cs` command configuration
3. Add tests in `tests/NextNet.Cli.Tests/`
4. Add docs in `docs/reference/cli-reference.md`

### Modify the HTML rendering engine

1. Core rendering logic: `src/NextNet.Rendering/`
2. Layout composition: `src/NextNet.Layouts/`
3. All pages implement `IPage` in `src/NextNet.Core/Components/`
4. Update `HtmlHelper` class if adding new HTML elements

### Add a new configuration option

1. Add property to config model in `src/NextNet.Core/Configuration/`
2. Update `nextnet.config.json` default
3. Wire into relevant pipeline component
4. Update `docs/reference/configuration-reference.md`

## Warnings

> [!CAUTION]
> **Source Generators**: Never store `ISymbol` or `SyntaxNode` in pipeline models.
> Always extract string representations. Storing symbols causes memory leaks and IDE slowdowns.

> [!WARNING]
> **Assembly Loading**: Use `AssemblyLoadContext` for plugin isolation.
> Loading plugins into the default context causes assembly version conflicts.

> [!WARNING]
> **Route Discovery**: Must complete in under 100ms for a good DX.
> Cache the route manifest aggressively and use incremental computation.

> [!CAUTION]
> **HTML Encoding**: Always encode user content. Use `HtmlEncoder.Default.Encode()`.
> Never use `Html.Raw()` with untrusted input.

> [!NOTE]
> **XML Comments**: All public API surfaces must have complete XML doc comments.
> These are used by DocFX to generate the API reference documentation.

## Anti-Patterns to Avoid

| Anti-Pattern | Why It's Bad |
|-------------|-------------|
| Storing `ISymbol` in generator pipelines | Memory leaks, IDE hangs |
| Loading plugins into default ALC | Assembly version conflicts |
| Bypassing `HtmlEncoder` for user content | XSS vulnerabilities |
| Manual route registration | Defeats file-based routing purpose |
| Blocking async calls with `.Result` | Deadlocks in ASP.NET Core |
| Modifying the route manifest at runtime | Inconsistent state, hard-to-debug bugs |
