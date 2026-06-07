---
uid: reference/cli-reference
title: CLI Reference
description: Complete reference for the nextnet command-line tool
---

# CLI Reference `v1.0` `stable`

Complete reference for the `nextnet` command-line tool.

## Global Options

| Option | Short | Description |
|--------|-------|-------------|
| `--version` | `-v` | Show version information |
| `--help` | `-h` | Show help information |
| `--verbosity` | | Set output verbosity: `quiet`, `normal`, `detailed` |

## Commands

### `nextnet new`

Scaffold a new NextNet project.

```bash
nextnet new <name> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `name` | Yes | Project name and directory name |

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--template` | `-t` | `default` | Project template (`default`, `empty`, `blog`, `api`) |
| `--output` | `-o` | Current dir | Output directory |
| `--force` | | `false` | Overwrite existing directory |
| `--no-restore` | | `false` | Skip `dotnet restore` after creation |

**Examples:**

```bash
nextnet new my-app
nextnet new my-blog --template blog
nextnet new my-api --template api --output ./projects
```

---

### `nextnet dev`

Start the development server.

```bash
nextnet dev [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--port` | `-p` | `3000` | Dev server port |
| `--host` | | `localhost` | Host address |
| `--open` | `-o` | `false` | Open browser on start |
| `--devtools` | | `true` | Enable DevTools dashboard |
| `--hot-reload` | | `true` | Enable hot reload |
| `--app-dir` | | `"app"` | Application directory |

**Examples:**

```bash
nextnet dev
nextnet dev --port 4000 --open
nextnet dev --host 0.0.0.0 --port 3000
```

**Output:**

```bash
$ nextnet dev
✓ NextNet dev server running on http://localhost:3000
  → 12 routes discovered
  → SSR enabled
  → Hot reload active
  → DevTools at http://localhost:3001
```

---

### `nextnet build`

Build the project for production.

```bash
nextnet build [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--ssg` | | `false` | Enable static generation |
| `--output` | `-o` | `"dist"` | Output directory |
| `--clean` | | `true` | Clean output directory before build |
| `--optimize` | | `true` | Enable build optimizations |
| `--analyze` | | `false` | Generate build analysis report |
| `--target` | | `"server"` | Build target (`server`, `edge`, `hybrid`) |

**Examples:**

```bash
nextnet build
nextnet build --ssg
nextnet build --analyze
nextnet build --target edge --output ./edge-dist
```

**Output:**

```bash
$ nextnet build
✓ Build complete
  → 12 pages generated
  → 4 API routes registered
  → 2.4 MB total output
  → Duration: 1.2s
```

---

### `nextnet publish`

Publish the project for production deployment.

```bash
nextnet publish [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--configuration` | `-c` | `Release` | Build configuration |
| `--output` | `-o` | `"./publish"` | Output directory |
| `--runtime` | `-r` | Current | Target runtime identifier |
| `--trim` | | `true` | Enable assembly trimming |
| `--single-file` | | `false` | Publish as single file |

**Examples:**

```bash
nextnet publish
nextnet publish --runtime linux-x64
nextnet publish --single-file --output ./release
```

**Output:**

```bash
$ nextnet publish
✓ Published successfully
  → Output: ./publish/
  → Runtime: linux-x64
  → Size: 28.4 MB
  → Ready for deployment
```

---

### `nextnet info`

Display environment and project information.

```bash
nextnet info [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--verbose` | `-v` | `false` | Show detailed information |

**Examples:**

```bash
nextnet info
```

**Output:**

```bash
$ nextnet info
NextNet CLI: 1.0.0
.NET Version: 10.0.0
OS: macOS 15.0
Project: my-app
  Routes found: 12
  Templates installed: default, blog, api
```

---

### `nextnet new --help`

```text
USAGE
  nextnet new <name> [options]

ARGUMENTS
  name          Project name (required)

OPTIONS
  -t, --template  Project template (default, empty, blog, api)
  -o, --output    Output directory (default: current directory)
  --force         Overwrite existing directory
  --no-restore    Skip dotnet restore
  -h, --help      Show help
```

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2` | Invalid arguments |
| `3` | Build failure |
| `4` | Route discovery failure |

## Related

- **Guide**: [Quickstart](../getting-started/quickstart.md)
- **Reference**: [Configuration Reference](configuration-reference.md)
- **Guide**: [Development Setup](../contributing/development-setup.md)
