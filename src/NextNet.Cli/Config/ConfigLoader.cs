using System.Text.Json;
using System.Text.Json.Serialization;
using NextNet.Cli.Errors;
using NextNet.Data.Abstractions.Configuration;

namespace NextNet.Cli.Config;

/// <summary>
/// Loads and parses <c>nextnet.config.json</c> from the project root.
/// </summary>
public static class ConfigLoader
{
    /// <summary>
    /// Default file name for the NextNet configuration file.
    /// </summary>
    public const string ConfigFileName = "nextnet.config.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Load configuration from the specified directory.
    /// </summary>
    /// <param name="directory">The project root directory. Defaults to the current directory.</param>
    /// <returns>The loaded configuration, or null if not found.</returns>
    public static NextNetProjectConfig? Load(string? directory = null)
    {
        directory ??= Environment.CurrentDirectory;
        var configPath = Path.Combine(directory, ConfigFileName);

        if (!File.Exists(configPath))
            return null;

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<NextNetProjectConfig>(json, JsonOptions);

            if (config is null)
                return null;

            // Apply defaults
            config.Name ??= Path.GetFileName(directory);
            config.Version ??= "1.0.0";

            return config;
        }
        catch (JsonException ex)
        {
            throw new NextNetConfigException(ErrorCodes.InvalidConfigFile,
                $"Failed to parse {ConfigFileName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Load configuration or throw if not found.
    /// </summary>
    public static NextNetProjectConfig LoadRequired(string? directory = null)
    {
        var config = Load(directory);
        if (config is null)
            throw new NextNetConfigException(ErrorCodes.ConfigFileNotFound,
                $"{ConfigFileName} not found in {directory ?? Environment.CurrentDirectory}");
        return config;
    }

    /// <summary>
    /// Save the configuration to <c>nextnet.config.json</c> in the specified directory.
    /// </summary>
    /// <param name="config">The configuration to persist.</param>
    /// <param name="directory">The project root directory. Defaults to the current directory.</param>
    public static void Save(NextNetProjectConfig config, string? directory = null)
    {
        directory ??= Environment.CurrentDirectory;
        var configPath = Path.Combine(directory, ConfigFileName);

        try
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(configPath, json);
        }
        catch (Exception ex)
        {
            throw new NextNetConfigException(ErrorCodes.ConfigUpdateFailed,
                $"Failed to write {ConfigFileName}: {ex.Message}");
        }
    }
}

/// <summary>
/// Represents the <c>nextnet.config.json</c> file contents.
/// </summary>
public class NextNetProjectConfig
{
    /// <summary>Project name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Project version.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>Framework configuration.</summary>
    [JsonPropertyName("framework")]
    public FrameworkConfig? Framework { get; set; }

    /// <summary>Routing configuration.</summary>
    [JsonPropertyName("routing")]
    public RoutingConfig? Routing { get; set; }

    /// <summary>Build configuration.</summary>
    [JsonPropertyName("build")]
    public BuildConfig? Build { get; set; }

    /// <summary>Dev server configuration.</summary>
    [JsonPropertyName("dev")]
    public DevConfig? Dev { get; set; }

    /// <summary>Publish configuration.</summary>
    [JsonPropertyName("publish")]
    public PublishConfig? Publish { get; set; }

    /// <summary>SSG (Static Site Generation) configuration.</summary>
    [JsonPropertyName("ssg")]
    public SsgConfig? Ssg { get; set; }

    /// <summary>Installed plugins.</summary>
    [JsonPropertyName("plugins")]
    public string[]? Plugins { get; set; }

    /// <summary>Data provider configuration.</summary>
    [JsonPropertyName("data")]
    public DataConfig? Data { get; set; }

    /// <summary>Scaffolding configuration.</summary>
    [JsonPropertyName("scaffolding")]
    public ScaffoldingConfig? Scaffolding { get; set; }

    /// <summary>Migration configuration.</summary>
    [JsonPropertyName("migration")]
    public MigrationConfigSection? Migration { get; set; }

    /// <summary>Admin dashboard configuration.</summary>
    [JsonPropertyName("admin")]
    public AdminConfig? Admin { get; set; }

    /// <summary>Environment variables.</summary>
    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; set; }
}

/// <summary>Framework feature configuration.</summary>
public class FrameworkConfig
{
    [JsonPropertyName("ssr")] public bool Ssr { get; set; } = true;
    [JsonPropertyName("ssg")] public bool Ssg { get; set; }
    [JsonPropertyName("streaming")] public bool Streaming { get; set; } = true;
    [JsonPropertyName("serverActions")] public bool ServerActions { get; set; } = true;
}

/// <summary>Routing configuration.</summary>
public class RoutingConfig
{
    [JsonPropertyName("dir")] public string Dir { get; set; } = "app";
    [JsonPropertyName("dynamicSegments")] public bool DynamicSegments { get; set; } = true;
    [JsonPropertyName("catchAll")] public bool CatchAll { get; set; } = true;
}

/// <summary>Build configuration.</summary>
public class BuildConfig
{
    [JsonPropertyName("output")] public string Output { get; set; } = "dist";
    [JsonPropertyName("minify")] public bool Minify { get; set; } = true;
    [JsonPropertyName("sourcemap")] public bool Sourcemap { get; set; }
    [JsonPropertyName("target")] public string Target { get; set; } = "net10.0";
}

/// <summary>Dev server configuration.</summary>
public class DevConfig
{
    [JsonPropertyName("port")] public int Port { get; set; } = 3000;
    [JsonPropertyName("https")] public bool Https { get; set; }
    [JsonPropertyName("hmr")] public bool Hmr { get; set; } = true;
    [JsonPropertyName("openBrowser")] public bool OpenBrowser { get; set; } = true;
}

/// <summary>Publish/deploy configuration.</summary>
public class PublishConfig
{
    [JsonPropertyName("target")] public string? Target { get; set; }
    [JsonPropertyName("selfContained")] public bool SelfContained { get; set; }
    [JsonPropertyName("trim")] public bool Trim { get; set; } = true;
}

/// <summary>
/// SSG (Static Site Generation) configuration section for <c>nextnet.config.json</c>.
/// Maps to <see cref="NextNet.Build.StaticGeneration.SsgOptions"/>.
/// </summary>
public class SsgConfig
{
    /// <summary>Output directory for generated static files.</summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    /// <summary>Whether to minify generated HTML.</summary>
    [JsonPropertyName("minify")]
    public bool? Minify { get; set; }

    /// <summary>Whether to generate gzip-compressed copies.</summary>
    [JsonPropertyName("gzip")]
    public bool? Gzip { get; set; }

    /// <summary>Whether to generate a build manifest.</summary>
    [JsonPropertyName("generateManifest")]
    public bool? GenerateManifest { get; set; }

    /// <summary>Route patterns to exclude from static generation.</summary>
    [JsonPropertyName("excludePaths")]
    public string[]? ExcludePaths { get; set; }

    /// <summary>Maximum time per page render in seconds.</summary>
    [JsonPropertyName("renderTimeout")]
    public int? RenderTimeout { get; set; }

    /// <summary>Whether to clean the output directory before building.</summary>
    [JsonPropertyName("cleanOutput")]
    public bool? CleanOutput { get; set; }
}

/// <summary>
/// Migration configuration section in <c>nextnet.config.json</c>.
/// Maps to <see cref="NextNet.Data.Abstractions.Configuration.MigrationConfig"/> at runtime.
/// </summary>
public class MigrationConfigSection
{
    /// <summary>Whether to automatically apply pending migrations on startup.</summary>
    [JsonPropertyName("autoApply")]
    public bool AutoApply { get; set; }

    /// <summary>The directory where migration files are stored, relative to project root.</summary>
    [JsonPropertyName("directory")]
    public string? Directory { get; set; }

    /// <summary>The name of the migration history table.</summary>
    [JsonPropertyName("historyTableName")]
    public string? HistoryTableName { get; set; }

    /// <summary>Timeout for migration operations in seconds.</summary>
    [JsonPropertyName("timeoutSeconds")]
    public int? TimeoutSeconds { get; set; }
}

/// <summary>
/// Admin dashboard configuration section for <c>nextnet.config.json</c>.
/// </summary>
public class AdminConfig
{
    /// <summary>URL prefix for all admin routes. Default: "admin".</summary>
    [JsonPropertyName("routePrefix")]
    public string RoutePrefix { get; set; } = "admin";

    /// <summary>Page title shown in admin layout header.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Whether admin features are enabled. Default: true if admin pages exist.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>Admin layout page template (layout name, defaults to "AdminLayout").</summary>
    [JsonPropertyName("layout")]
    public string Layout { get; set; } = "AdminLayout";

    /// <summary>Navigation items for the admin sidebar (auto-generated if empty).</summary>
    [JsonPropertyName("navigation")]
    public List<AdminNavItem>? Navigation { get; set; }

    /// <summary>Custom namespace for generated admin pages.</summary>
    [JsonPropertyName("adminNamespace")]
    public string? AdminNamespace { get; set; }

    /// <summary>Custom output directory for admin pages.</summary>
    [JsonPropertyName("adminDirectory")]
    public string? AdminDirectory { get; set; }
}

/// <summary>
/// A single navigation entry in the admin sidebar.
/// </summary>
public class AdminNavItem
{
    /// <summary>Display label for the navigation item.</summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    /// <summary>Route URL for the navigation item.</summary>
    [JsonPropertyName("route")]
    public string Route { get; set; } = "";

    /// <summary>Optional icon name or class.</summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>Optional badge text (e.g., count).</summary>
    [JsonPropertyName("badge")]
    public string? Badge { get; set; }

    /// <summary>Child navigation items for submenus.</summary>
    [JsonPropertyName("children")]
    public List<AdminNavItem>? Children { get; set; }
}

/// <summary>Data provider configuration section in <c>nextnet.config.json</c>.</summary>
public class DataConfig
{
    /// <summary>The data provider type (ef, dapper, mongo).</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>The connection string for the database.</summary>
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    /// <summary>The database type (sqlite, postgresql).</summary>
    [JsonPropertyName("databaseType")]
    public string? DatabaseType { get; set; }

    /// <summary>Installed NuGet packages related to data.</summary>
    [JsonPropertyName("packages")]
    public string[]? Packages { get; set; }
}
