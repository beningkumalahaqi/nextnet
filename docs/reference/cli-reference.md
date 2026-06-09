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

## V5 Design System Commands `v5.0` `stable`

New commands in NextNet V5 for managing the design system, UI components, and theming. V5 is now fully implemented across all packages.

### `nextnet add ui`

Install the complete design system into your project.

```bash
nextnet add ui [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--components` | `-c` | `all` | Components to include (`all`, `minimal`, or comma-separated list) |
| `--with-tailwind` | | `true` | Include Tailwind CSS integration |
| `--with-darkmode` | | `true` | Include dark mode support |
| `--prefix` | | `nn` | CSS class prefix for components |
| `--force` | | `false` | Overwrite existing design system files |

**Examples:**

```bash
nextnet add ui
nextnet add ui --components minimal --no-tailwind
nextnet add ui --components Button,Card,Input --prefix app
```

**Output:**

```bash
$ nextnet add ui
✓ Design system installed
  → 17 components added
  → Theme engine configured (light + dark)
  → Tailwind config generated
  → CSS tokens generated
  → Runtime theme switcher added
```

---

### `nextnet add tailwind`

Install and configure Tailwind CSS for your project.

```bash
nextnet add tailwind [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--version` | | `4` | Tailwind major version (`3`, `4`) |
| `--prefix` | | `""` | Tailwind class prefix |
| `--purge` | | `true` | Enable unused class purging |
| `--config-only` | | `false` | Generate config only, skip npm install |
| `--force` | | `false` | Overwrite existing config |

**Examples:**

```bash
nextnet add tailwind
nextnet add tailwind --version 3
nextnet add tailwind --config-only
nextnet add tailwind --prefix tw-
```

**Output:**

```bash
$ nextnet add tailwind
✓ Tailwind CSS installed
  → Version: 4.x
  → Config generated: _design/tailwind.config.js
  → PostCSS configured
  → Build pipeline updated
  → Theme tokens mapped to Tailwind utilities
```

---

### `nextnet add <component>`

Install individual UI components from the design system.

```bash
nextnet add <component> [options]
```

**Available components:**

| Component | Description |
|-----------|-------------|
| `button` | Action button with variants |
| `card` | Content container |
| `input` | Text input field |
| `select` | Dropdown selector |
| `checkbox` | Multi-select input |
| `radio` | Single-select input |
| `toggle` | On/off switch |
| `modal` | Dialog overlay |
| `drawer` | Slide-out panel |
| `toast` | Notification toast |
| `badge` | Status indicator |
| `table` | Data table |
| `tabs` | Tabbed content |
| `accordion` | Expandable sections |
| `nav` | Navigation menu |
| `progress` | Loading indicator |
| `avatar` | User avatar |

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--output` | `-o` | `_design/components` | Output directory |
| `--force` | | `false` | Overwrite existing files |

**Examples:**

```bash
nextnet add button
nextnet add modal
nextnet add table --force
nextnet add card input button
```

**Output:**

```bash
$ nextnet add button
✓ Component added: Button
  → File: _design/components/button.css
  → Variants: primary, secondary, ghost, danger, link
  → Sizes: sm, md, lg
```

---

### `nextnet generate component`

Scaffold a custom UI component with the design system pattern.

```bash
nextnet generate component [name] [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `name` | Yes | Component name (PascalCase) |

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--output` | `-o` | `app/_components` | Output directory |
| `--no-styles` | | `false` | Skip CSS file generation |
| `--with-types` | | `true` | Generate options/types class |
| `--interactive` | | `false` | Add JavaScript interactivity |
| `--prefix` | | `nn` | CSS class prefix |

**Examples:**

```bash
nextnet generate component PricingCard
nextnet generate component DataChart --interactive
nextnet generate component HeroSection --output app/_components/marketing
```

**Generated files:**

```text
app/_components/
├── _PricingCard.cs        # Component class
├── PricingCardOptions.cs   # Options class (--with-types)
├── pricing-card.css        # Component styles (--no-styles to skip)
└── pricing-card.js         # Interactivity (--interactive)
```

**Output:**

```bash
$ nextnet generate component PricingCard
✓ Component generated: PricingCard
  → _PricingCard.cs
  → PricingCardOptions.cs
  → pricing-card.css
  → Register in _components/index.cs to expose globally
```

---

### `nextnet add darkmode`

Add dark mode support to your project, including theme switching and persistence.

```bash
nextnet add darkmode [options]
```

**Options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--strategy` | `-s` | `class` | Theme strategy (`class`, `data-attribute`, `media-query`) |
| `--storage-key` | | `nn-theme` | localStorage key for persistence |
| `--respect-system` | | `true` | Respect `prefers-color-scheme` |
| `--toggle-position` | | `bottom-right` | Theme toggle position (`top-right`, `bottom-right`, `top-left`, `bottom-left`, `none`) |
| `--transition` | | `300` | Theme transition duration in ms |
| `--force` | | `false` | Overwrite existing theme files |

**Examples:**

```bash
nextnet add darkmode
nextnet add darkmode --strategy media-query --toggle-position none
nextnet add darkmode --storage-key my-app-theme --no-respect-system
```

**Generated files:**

```text
_design/
├── theme.css               # Light and dark theme variables
├── theme-runtime.js         # Theme switching runtime
└── components/
    └── theme-toggle.css     # Theme toggle button styles
```

**Output:**

```bash
$ nextnet add darkmode
✓ Dark mode configured
  → Strategy: data-attribute
  → Light theme variables generated
  → Dark theme variables generated
  → Theme toggle added (bottom-right)
  → Persistence: localStorage (nn-theme)
  → System preference detection: enabled
```

---

### `nextnet new` (V5 Templates)

Updated `nextnet new` now supports design system templates:

```bash
nextnet new my-app --template default --with-ui --with-darkmode
nextnet new my-app --template blog --with-ui --with-tailwind
```

**New template options:**

| Option | Short | Default | Description |
|--------|-------|---------|-------------|
| `--with-ui` | | `false` | Include design system |
| `--with-tailwind` | | `false` | Include Tailwind CSS |
| `--with-darkmode` | | `false` | Include dark mode support |
| `--components` | | `all` | Components to include |

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
