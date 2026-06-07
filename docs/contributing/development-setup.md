---
uid: contributing/development-setup
title: Development Setup
description: Set up your environment to contribute to NextNet
---

# Development Setup `v1.0` `stable`

Set up your development environment to contribute to NextNet.

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0+ | Build and run the framework |
| An IDE | Any | VS 2025+, VS Code, Rider, Neovim |
| Git | Latest | Version control |

> [!NOTE]
> NextNet targets .NET 10 and uses C# 13 features.
> Run `dotnet --version` to verify your SDK version.

## Clone the Repository

```bash
git clone https://github.com/nextnet/nextnet.git
cd nextnet
```

## Build the Solution

Restore dependencies and build:

```bash
dotnet restore
dotnet build NextNet.sln
```

The solution includes all projects under `src/` and `tests/`.

> [!TIP]
> Use `dotnet build NextNet.sln --no-restore` for faster subsequent builds
> if dependencies haven't changed.

## Run Tests

Run the full test suite:

```bash
dotnet test
```

Run specific test projects:

```bash
# Routing tests only
dotnet test tests/NextNet.Routing.Tests/

# Core tests only
dotnet test tests/NextNet.Core.Tests/
```

Run tests by category:

```bash
# Unit tests (fast)
dotnet test --filter "Category=Unit"

# Integration tests
dotnet test --filter "Category=Integration"
```

Run tests matching a name:

```bash
dotnet test --filter "FullyQualifiedName~RouteParser"
```

## Project Structure

```text
nextnet/
├── src/
│   ├── NextNet.Core/            # Core abstractions
│   ├── NextNet.Routing/         # Route discovery
│   ├── NextNet.Layouts/         # Layout system
│   ├── NextNet.Rendering/       # SSR & streaming
│   ├── NextNet.Build/           # Build pipeline
│   ├── NextNet.Cli/             # CLI tooling
│   ├── NextNet.ServerActions/   # Server actions
│   ├── NextNet.SourceGenerators/ # Source generators
│   ├── NextNet.Isr/             # Incremental Static Regeneration
│   ├── NextNet.Plugins/         # Plugin system
│   ├── NextNet.Middleware/      # Middleware pipeline
│   ├── NextNet.Edge/            # Edge runtime
│   └── NextNet.DevTools/        # DevTools
├── tests/                        # Test projects
└── docs/                         # Documentation
```

## Development Workflow

### 1. Choose an issue

Browse issues tagged with `good-first-issue` or `help-wanted`.

### 2. Create a branch

```bash
git checkout -b feat/my-feature
```

### 3. Make changes

Follow the conventions in [AGENTS.md](../../AGENTS.md):
- File-scoped namespaces
- Primary constructors for simple DI
- XML documentation comments on all public types
- Tests follow `{Method}_Should_{Expected}_When_{Condition}` naming

### 4. Run tests before committing

```bash
dotnet test
```

### 5. Commit your changes

```bash
git add -A
git commit -m "feat: add support for optional route parameters"
```

### 6. Push and create a PR

```bash
git push origin feat/my-feature
```

Then create a pull request on GitHub.

## Testing Your Changes with a Sample App

Create a sample app to test changes:

```bash
# Build the CLI first
dotnet build src/NextNet.Cli/

# Create a test app
dotnet run --project src/NextNet.Cli/ -- new test-app

# Run it
cd test-app
dotnet run --project ../src/NextNet.Cli/ -- dev
```

> [!TIP]
> Use `dotnet run --project` with the NextNet.Cli project to test local changes
> without needing to install the global tool.

## Building the Documentation

```bash
# Install DocFX
dotnet tool update -g docfx

# Build and serve docs locally
docfx docs/docfx.json --serve
```

The documentation site is available at `http://localhost:8080`.

## Code Style

The project uses `.editorconfig` and `Directory.Build.props` for consistent code style:

```bash
# Format your code
dotnet format NextNet.sln
```

### Key Conventions

| Convention | Rule |
|-----------|------|
| File-scoped namespaces | `namespace NextNet.Routing;` (not block-scoped) |
| Primary constructors | `public class MyClass(IService service)` for simple DI |
| XML docs on public API | Every public type, method, and property |
| Immutable data models | Use `record` types for route metadata, config |
| `IIncrementalGenerator` | Never `ISyntaxReceiver` for source generators |
| Async all the way | No `.Result` or `.Wait()` calls |
| Nullable enabled | All projects use `Nullable: enable` |

## Debugging Source Generators

Source generators run during compilation. To debug them:

1. Set the `DEBUG_SOURCE_GENERATOR` environment variable:

```bash
export DEBUG_SOURCE_GENERATOR=1
dotnet build
```

2. Or add a Debugger.Launch in your generator:

```csharp
public void Initialize(IncrementalGeneratorInitializationContext context)
{
    #if DEBUG
    System.Diagnostics.Debugger.Launch();
    #endif
    // ...
}
```

## Common Build Issues

| Issue | Solution |
|-------|----------|
| `NU1105: Unable to find project` | Run `dotnet restore` first |
| `CS0117: 'NextNet' does not contain` | Rebuild: `dotnet build --no-cache` |
| Source generator not running | Clean: `dotnet clean && dotnet build` |
| Tests hanging | Check for `Task.Result` or `.Wait()` calls |
| `docfx: command not found` | Install: `dotnet tool update -g docfx` |

## Related

- **Contributing**: [Architecture](architecture.md)
- **Root**: [AGENTS.md](../../AGENTS.md)
- **Guides**: [Testing](../guides/testing.md)
