namespace NextNet.Cli.Errors;

/// <summary>
/// Central registry of all NextNet error codes (NN-001 through NN-045).
/// Each entry includes the code, category, human-readable message, and optional
/// usage/context hints for error display.
/// </summary>
public static class ErrorCodes
{
    // ── Input errors (NN-001 to NN-003) ──────────────────────────────

    /// <summary>NN-001: Project name is required.</summary>
    public static ErrorEntry ProjectNameRequired => new("NN-001", "Input", "Project name is required",
        Usage: "nextnet new <name>",
        Examples: new[] { "nextnet new my-app", "nextnet new my-app --template minimal" });

    /// <summary>NN-002: Invalid project name (kebab-case required).</summary>
    public static ErrorEntry InvalidProjectName => new("NN-002", "Input", "Invalid project name (use kebab-case)",
        Context: "Names must match ^[a-z][a-z0-9-]*$",
        Examples: new[] { "nextnet new my-app", "nextnet new blog-engine" });

    /// <summary>NN-003: Directory already exists.</summary>
    public static ErrorEntry DirectoryExists => new("NN-003", "Input", "Directory already exists",
        Context: "Remove the directory or use a different name.",
        Examples: new[] { "nextnet new my-app-2", "rm -rf ./my-app && nextnet new my-app" });

    // ── Config errors (NN-004 to NN-006) ─────────────────────────────

    /// <summary>NN-004: Configuration file not found.</summary>
    public static ErrorEntry ConfigFileNotFound => new("NN-004", "Config", "Configuration file not found",
        Usage: "nextnet new in the project root, or create nextnet.config.json",
        Context: "Run nextnet new in the project root, or create nextnet.config.json.");

    /// <summary>NN-005: Invalid configuration file.</summary>
    public static ErrorEntry InvalidConfigFile => new("NN-005", "Config", "Invalid configuration file",
        Context: "Check JSON syntax. Run nextnet doctor for validation.",
        Examples: new[] { "nextnet doctor" });

    /// <summary>NN-006: Unknown configuration key.</summary>
    public static ErrorEntry UnknownConfigKey => new("NN-006", "Config", "Unknown configuration key",
        Context: "Remove the unrecognized key. See schema at nextnet.dev/schemas/config.json.");

    // ── Build errors (NN-007 to NN-009) ──────────────────────────────

    /// <summary>NN-007: Compilation failed.</summary>
    public static ErrorEntry CompilationFailed => new("NN-007", "Build", "Compilation failed",
        Context: "Check the error details above. Run with --verbose for full output.",
        Usage: "nextnet build --verbose");

    /// <summary>NN-008: Route discovery failed.</summary>
    public static ErrorEntry RouteDiscoveryFailed => new("NN-008", "Build", "Route discovery failed",
        Context: "Ensure app/ directory exists and contains route files.",
        Examples: new[] { "ls app/", "nextnet doctor" });

    /// <summary>NN-009: Source generation failed.</summary>
    public static ErrorEntry SourceGenerationFailed => new("NN-009", "Build", "Source generation failed",
        Context: "Run with --verbose to see generated sources.",
        Usage: "nextnet build --verbose");

    // ── Runtime errors (NN-010 to NN-012) ────────────────────────────

    /// <summary>NN-010: Dev server failed to start.</summary>
    public static ErrorEntry DevServerFailed => new("NN-010", "Runtime", "Dev server failed to start",
        Context: "Check if another process is using the port. Try --port <number>.",
        Usage: "nextnet dev --port 3001");

    /// <summary>NN-011: Port already in use.</summary>
    public static ErrorEntry PortInUse => new("NN-011", "Runtime", "Port already in use",
        Context: "Use --port <number> to specify a different port.",
        Usage: "nextnet dev --port 3001");

    /// <summary>NN-012: File watcher error.</summary>
    public static ErrorEntry FileWatcherError => new("NN-012", "Runtime", "File watcher error",
        Context: "Check file permissions. Restart with nextnet dev.",
        Examples: new[] { "nextnet dev" });

    // ── Publish errors (NN-013 to NN-015) ────────────────────────────

    /// <summary>NN-013: Deployment target not found.</summary>
    public static ErrorEntry DeployTargetNotFound => new("NN-013", "Publish", "Deployment target not found",
        Context: "Available targets: azure, aws, docker, local.",
        Usage: "nextnet publish azure");

    /// <summary>NN-014: Authentication failed.</summary>
    public static ErrorEntry AuthFailed => new("NN-014", "Publish", "Authentication failed",
        Context: "Run az login (Azure) or configure credentials.",
        Examples: new[] { "az login", "nextnet publish azure" });

    /// <summary>NN-015: Upload failed.</summary>
    public static ErrorEntry UploadFailed => new("NN-015", "Publish", "Upload failed",
        Context: "Check network connection and retry.",
        Examples: new[] { "nextnet publish azure --retry" });

    // ── Plugin errors (NN-020 to NN-022) ─────────────────────────────

    /// <summary>NN-020: Plugin not found.</summary>
    public static ErrorEntry PluginNotFound => new("NN-020", "Plugin", "Plugin not found",
        Context: "Check plugin name. Run nextnet plugin list to see installed plugins.",
        Usage: "nextnet plugin list");

    /// <summary>NN-021: Plugin load failed.</summary>
    public static ErrorEntry PluginLoadFailed => new("NN-021", "Plugin", "Plugin load failed",
        Context: "Plugin may be incompatible. Check NextNet version requirements.");

    /// <summary>NN-022: Plugin version mismatch.</summary>
    public static ErrorEntry PluginVersionMismatch => new("NN-022", "Plugin", "Plugin version mismatch",
        Context: "Update the plugin: nextnet plugin add <name>@latest",
        Usage: "nextnet plugin add <name>@latest");

    // ── System errors (NN-030 to NN-033) ─────────────────────────────

    /// <summary>NN-030: .NET SDK not found.</summary>
    public static ErrorEntry DotNetSdkNotFound => new("NN-030", "System", ".NET SDK not found",
        Context: "Install .NET SDK from https://dot.net/download",
        Examples: new[] { "dotnet --version", "winget install Microsoft.DotNet.SDK.10" });

    /// <summary>NN-031: Unsupported .NET version.</summary>
    public static ErrorEntry UnsupportedDotNetVersion => new("NN-031", "System", "Unsupported .NET version",
        Context: "NextNet requires .NET 10.0 or later.",
        Examples: new[] { "dotnet --version", "dotnet upgrade" });

    /// <summary>NN-032: Permission denied.</summary>
    public static ErrorEntry PermissionDenied => new("NN-032", "System", "Permission denied",
        Context: "Check file/directory permissions. Try running with appropriate privileges.");

    /// <summary>NN-033: Disk space insufficient.</summary>
    public static ErrorEntry DiskSpaceInsufficient => new("NN-033", "System", "Disk space insufficient",
        Context: "Free up disk space. NextNet build requires ~50 MB.");

    // ── Data errors (NN-040 to NN-045) ───────────────────────────────

    /// <summary>NN-040: No data provider configured.</summary>
    public static ErrorEntry NoProviderConfigured => new("NN-040", "Data", "No data provider configured",
        Context: "Add a provider with 'nextnet add data <provider>' before initializing the database.",
        Usage: "nextnet add data ef",
        Examples: new[] { "nextnet add data ef", "nextnet add data dapper", "nextnet add data mongo" });

    /// <summary>NN-041: Invalid data provider specified.</summary>
    public static ErrorEntry InvalidProvider => new("NN-041", "Data", "Invalid data provider",
        Context: "Supported providers: ef, dapper, mongo.",
        Usage: "nextnet add data <provider>",
        Examples: new[] { "nextnet add data ef", "nextnet add data dapper", "nextnet add data mongo" });

    /// <summary>NN-042: Database initialization failed.</summary>
    public static ErrorEntry DatabaseInitFailed => new("NN-042", "Data", "Database initialization failed",
        Context: "Check the error details above. Ensure the target database is accessible.",
        Usage: "nextnet db init sqlite",
        Examples: new[] { "nextnet db init sqlite", "nextnet db init postgresql" });

    /// <summary>NN-043: Connection string is invalid or missing.</summary>
    public static ErrorEntry ConnectionStringInvalid => new("NN-043", "Data", "Connection string is invalid or missing",
        Context: "Provide a valid connection string or use a supported format.",
        Usage: "nextnet db init postgresql --connection-string \"...\"");

    /// <summary>NN-044: Package installation failed.</summary>
    public static ErrorEntry PackageAddFailed => new("NN-044", "Data", "Package installation failed",
        Context: "Check NuGet source availability and network connectivity.",
        Usage: "dotnet add package <package-name>",
        Examples: new[] { "nextnet add data ef", "nextnet add data dapper" });

    /// <summary>NN-045: Configuration file update failed.</summary>
    public static ErrorEntry ConfigUpdateFailed => new("NN-045", "Data", "Configuration file update failed",
        Context: "Ensure nextnet.config.json exists and is writable.",
        Usage: "nextnet doctor",
        Examples: new[] { "nextnet doctor" });

    // ── Migration errors (NN-050 to NN-056) ──────────────────────────

    /// <summary>NN-050: Migration creation failed.</summary>
    public static ErrorEntry MigrationAddFailed => new("NN-050", "Data/Migration", "Migration creation failed",
        Context: "Check the error details above. Ensure dotnet ef tooling is installed.",
        Usage: "nextnet db migration add <name>",
        Examples: new[] { "nextnet db migration add AddUserTable" });

    /// <summary>NN-051: Migration apply failed.</summary>
    public static ErrorEntry MigrationApplyFailed => new("NN-051", "Data/Migration", "Migration apply failed",
        Context: "Check database connectivity and migration state.",
        Usage: "nextnet db migrate",
        Examples: new[] { "nextnet db migrate", "nextnet db migrate --dry-run" });

    /// <summary>NN-052: Migration rollback failed.</summary>
    public static ErrorEntry MigrationRollbackFailed => new("NN-052", "Data/Migration", "Migration rollback failed",
        Context: "Check database connectivity and history table.",
        Usage: "nextnet db rollback",
        Examples: new[] { "nextnet db rollback", "nextnet db rollback --steps 2" });

    /// <summary>NN-053: Migration status check failed.</summary>
    public static ErrorEntry MigrationStatusFailed => new("NN-053", "Data/Migration", "Migration status check failed",
        Context: "Ensure the database is accessible and migrations are configured.",
        Usage: "nextnet db migration status",
        Examples: new[] { "nextnet db migration status", "nextnet db migration status --json" });

    /// <summary>NN-054: Migration not found.</summary>
    public static ErrorEntry MigrationNotFound => new("NN-054", "Data/Migration", "Migration not found",
        Context: "Check the migration name and ensure it exists.",
        Usage: "nextnet db migration add <name>",
        Examples: new[] { "nextnet db migration add AddUserTable" });

    /// <summary>NN-055: Confirmation required.</summary>
    public static ErrorEntry ConfirmationRequired => new("NN-055", "Data/Migration", "Confirmation required",
        Context: "Use --confirm to skip the confirmation prompt in non-interactive environments.",
        Usage: "nextnet db migrate --confirm",
        Examples: new[] { "nextnet db migrate --confirm", "nextnet db rollback --confirm" });

    /// <summary>NN-056: Dry-run mode — no changes were applied.</summary>
    public static ErrorEntry DryRunOnly => new("NN-056", "Data/Migration", "Dry-run mode — no changes were applied",
        Context: "Re-run without --dry-run to apply the changes.",
        Usage: "nextnet db migrate",
        Examples: new[] { "nextnet db migrate", "nextnet db rollback" });

    // ── Scaffold errors (NN-060 to NN-064) ───────────────────────────

    /// <summary>NN-060: Scaffold model failed.</summary>
    public static ErrorEntry ScaffoldModelFailed => new("NN-060", "Scaffold", "Scaffold model failed",
        Context: "Check directory permissions and disk space.",
        Usage: "nextnet generate model <name>",
        Examples: new[] { "nextnet generate model User", "nextnet generate model Product --property \"Name:string\"" });

    /// <summary>NN-061: Scaffold repository failed.</summary>
    public static ErrorEntry ScaffoldRepositoryFailed => new("NN-061", "Scaffold", "Scaffold repository failed",
        Context: "A data provider must be configured. Run 'nextnet add data <provider>' first.",
        Usage: "nextnet generate repository <name>",
        Examples: new[] { "nextnet add data ef", "nextnet generate repository User" });

    /// <summary>NN-062: Scaffold CRUD failed.</summary>
    public static ErrorEntry ScaffoldCrudFailed => new("NN-062", "Scaffold", "Scaffold CRUD failed",
        Context: "A data provider must be configured. Run 'nextnet add data <provider>' first.",
        Usage: "nextnet generate crud <name>",
        Examples: new[] { "nextnet add data ef", "nextnet generate crud Product" });

    /// <summary>NN-063: Scaffold template not found.</summary>
    public static ErrorEntry ScaffoldTemplateNotFound => new("NN-063", "Scaffold", "Scaffold template not found",
        Context: "Resource embedded template is missing for the selected provider.",
        Usage: "nextnet generate model <name>",
        Examples: new[] { "nextnet doctor" });

    /// <summary>NN-064: Scaffold output conflict.</summary>
    public static ErrorEntry ScaffoldOutputConflict => new("NN-064", "Scaffold", "Scaffold output conflict",
        Context: "A file already exists and --force was not specified. Use --force to overwrite.",
        Usage: "nextnet generate model <name> --force",
        Examples: new[] { "nextnet generate model User --force" });

    // ── Admin/Explore errors (NN-070 to NN-072) ─────────────────────────

    /// <summary>NN-070: Admin generate failed.</summary>
    public static ErrorEntry AdminGenerateFailed => new("NN-070", "Admin", "Admin generate failed",
        Context: "Check the error details above. Ensure a data provider is configured.",
        Usage: "nextnet generate admin <entity>",
        Examples: new[] { "nextnet add data ef", "nextnet generate admin Product" });

    /// <summary>NN-071: Db explore failed.</summary>
    public static ErrorEntry ExploreFailed => new("NN-071", "Explore", "Database exploration failed",
        Context: "Check the error details above. Ensure the database is accessible.",
        Usage: "nextnet db explore",
        Examples: new[] { "nextnet db explore", "nextnet db explore Users" });

    /// <summary>NN-072: Schema provider not found.</summary>
    public static ErrorEntry ExploreSchemaProviderNotFound => new("NN-072", "Explore", "Schema provider not found",
        Context: "The configured data provider does not support schema introspection. Try a different provider.",
        Usage: "nextnet db explore",
        Examples: new[] { "nextnet add data ef", "nextnet db explore" });

    /// <summary>NN-073: Could not connect to database.</summary>
    public static ErrorEntry ExploreConnectionFailed => new("NN-073", "Explore", "Could not connect to database",
        Context: "Check connection string in nextnet.config.json and database availability.",
        Usage: "nextnet db explore",
        Examples: new[] { "nextnet db explore", "nextnet doctor" });

    /// <summary>NN-074: Table or collection not found.</summary>
    public static ErrorEntry ExploreTableNotFound => new("NN-074", "Explore", "Table or collection not found",
        Context: "Verify the table name with 'nextnet db explore' (no arguments).",
        Usage: "nextnet db explore <table>",
        Examples: new[] { "nextnet db explore", "nextnet db explore Users" });

    /// <summary>NN-075: Schema introspection failed.</summary>
    public static ErrorEntry ExploreSchemaFailed => new("NN-075", "Explore", "Schema introspection failed",
        Context: "The provider encountered an error while reading database metadata.",
        Usage: "nextnet db explore",
        Examples: new[] { "nextnet db explore --verbose" });

    // ── Template errors (NN-100 to NN-103) ───────────────────────────

    /// <summary>NN-100: Template not found.</summary>
    public static ErrorEntry TemplateNotFound => new("NN-100", "Template", "Template not found",
        Usage: "nextnet template list",
        Examples: new[] { "nextnet new blog myblog" });

    /// <summary>NN-101: Template and name required.</summary>
    public static ErrorEntry NewNoArgs => new("NN-101", "Input", "Template and name required",
        Usage: "nextnet new <template> <name>",
        Examples: new[] { "nextnet new blog my-blog" });

    /// <summary>NN-102: Project generation failed.</summary>
    public static ErrorEntry NewGenerationFailed => new("NN-102", "Generation", "Project generation failed",
        Usage: "nextnet new <template> <name>",
        Examples: new[] { "nextnet new blog my-blog" });

    /// <summary>NN-103: Unexpected error during generation.</summary>
    public static ErrorEntry NewUnexpected => new("NN-103", "System", "Unexpected error during generation",
        Usage: "nextnet new <template> <name>",
        Examples: new[] { "nextnet new blog my-blog" });
}

/// <summary>
/// Represents a single error code entry with code, category, message, and usage hints.
/// </summary>
public sealed record ErrorEntry(
    string Code,
    string Category,
    string Message,
    string? Context = null,
    string? Usage = null,
    string[]? Examples = null);
