---
uid: getting-started/installation
title: Installation
description: Install NextNet CLI and create your first project
---

# Installation `v1.0` `stable`

Install NextNet and its prerequisites to start building modern .NET web applications.

## Prerequisites

Before installing NextNet, ensure you have the following:

| Requirement | Version | Link |
|-------------|---------|------|
| .NET SDK | 10.0+ | [Download .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) |
| An IDE | Any | VS 2025+, VS Code, Rider, or Neovim |

> [!WARNING]
> NextNet requires .NET 10 or later. It uses C# 13 features including file-scoped namespaces, primary constructors, and collection expressions.
> Verify your .NET version with `dotnet --version`.

## Install the Templates

NextNet provides a .NET project template for scaffolding new applications:

```bash
dotnet new install NextNet.Templates
```

This installs the `nextnet` template that the `nextnet new` command uses to scaffold projects.

## Install the Global Tool

Install the NextNet CLI as a .NET global tool:

```bash
dotnet tool install --global NextNet.Cli
```

Verify the installation:

```bash
nextnet --version
```

> [!TIP]
> You can update NextNet at any time:
> ```bash
> dotnet tool update --global NextNet.Cli
> ```

## Package Installation (Manual)

If you prefer to add NextNet to an existing project, install the NuGet packages:

```bash
dotnet add package NextNet.Core
dotnet add package NextNet.Routing
dotnet add package NextNet.Rendering
```

For CLI support in an existing project:

```bash
dotnet add package NextNet.Cli
```

## Quick Verification

Create a test project to verify everything works:

```bash
nextnet new hello-world
cd hello-world
nextnet dev
```

Open `http://localhost:3000` in your browser. You should see the NextNet welcome page.

```text
✓ NextNet dev server running on http://localhost:3000
  → 1 route discovered
  → SSR enabled
```

> [!CAUTION]
> If you see a port conflict error, another process is using port 3000.
> Use `nextnet dev --port 3001` to specify a different port, or update the `devPort` in `nextnet.config.json`.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `nextnet: command not found` | Ensure the .NET global tools directory is in your PATH: `dotnet tool list --global` |
| `dotnet new install` fails | Ensure .NET 10 SDK is installed: `dotnet --list-sdks` |
| Template not found | Try `dotnet new install NextNet.Templates` again or use the manual approach |
| NuGet restore errors | Check your NuGet sources: `dotnet nuget list source` |

## Next Steps

- [Quickstart Guide](quickstart.md) — Build your first page in 2 minutes
- [Project Structure](project-structure.md) — Understand the NextNet project layout
- [Configuration](configuration.md) — Configure NextNet for your needs
