using System.Text.Json;
using NextNet.Logging;
using NextNet.Plugins.Errors;

namespace NextNet.Plugins.Config;

/// <summary>
/// Represents the "plugins" section from <c>nextnet.config.json</c>.
/// </summary>
public sealed record PluginManifestConfig
{
    /// <summary>
    /// Gets or sets the list of plugin entries declared in configuration.
    /// </summary>
    public IReadOnlyList<PluginConfigEntry> Plugins { get; set; } = Array.Empty<PluginConfigEntry>();
}

/// <summary>
/// Represents a single plugin entry declared in <c>nextnet.config.json</c>.
/// </summary>
public sealed record PluginConfigEntry
{
    /// <summary>
    /// Gets or sets the plugin name (must match <see cref="INextNetPlugin.Name"/>).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the plugin assembly DLL.
    /// If relative, it is resolved relative to the config file location.
    /// </summary>
    public string? AssemblyPath { get; set; }

    /// <summary>
    /// Gets or sets whether the plugin is enabled. Defaults to <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets optional per-plugin configuration values.
    /// </summary>
    public Dictionary<string, JsonElement>? Settings { get; set; }
}

/// <summary>
/// Loads and parses the <c>plugins</c> section from <c>nextnet.config.json</c>.
/// Provides a <see cref="PluginManifestConfig"/> with per-plugin configuration.
/// </summary>
public class PluginConfigLoader
{
    private readonly INextNetLogger _logger;

    /// <summary>
    /// The default name of the NextNet configuration file.
    /// </summary>
    public const string ConfigFileName = "nextnet.config.json";

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfigLoader"/> class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    public PluginConfigLoader(INextNetLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Loads the plugin configuration from <c>nextnet.config.json</c> in the specified directory.
    /// </summary>
    /// <param name="basePath">The directory containing <c>nextnet.config.json</c>.</param>
    /// <returns>
    /// A <see cref="PluginManifestConfig"/> describing the configured plugins,
    /// or an empty config if the file does not exist or parsing fails.
    /// </returns>
    public PluginManifestConfig Load(string basePath)
    {
        if (string.IsNullOrEmpty(basePath))
            throw new ArgumentException($"[{PluginErrorCodes.ConfigInvalid}] Base path must not be null or empty.", nameof(basePath));

        var configPath = Path.Combine(basePath, ConfigFileName);

        if (!File.Exists(configPath))
        {
            _logger.Debug("Configuration file not found at {0}. Using empty plugin config.", configPath);
            return new PluginManifestConfig();
        }

        try
        {
            var json = File.ReadAllText(configPath);
            return Parse(json, basePath);
        }
        catch (JsonException ex)
        {
            _logger.Warn("[{0}] Failed to parse plugin configuration from {1}: {2}", PluginErrorCodes.ConfigInvalid, configPath, ex.Message);
            return new PluginManifestConfig();
        }
        catch (IOException ex)
        {
            _logger.Warn("Failed to read configuration file {0}: {1}", configPath, ex.Message);
            return new PluginManifestConfig();
        }
    }

    /// <summary>
    /// Parses the plugin configuration from a JSON string.
    /// </summary>
    /// <param name="json">The JSON content of the configuration file.</param>
    /// <param name="basePath">The base path used to resolve relative assembly paths.</param>
    /// <returns>A <see cref="PluginManifestConfig"/> with the parsed plugin entries.</returns>
    public PluginManifestConfig Parse(string json, string basePath)
    {
        if (string.IsNullOrEmpty(json))
            return new PluginManifestConfig();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Try to read the "plugins" section
        if (!root.TryGetProperty("plugins", out var pluginsElement))
        {
            _logger.Debug("No 'plugins' section found in configuration.");
            return new PluginManifestConfig();
        }

        if (pluginsElement.ValueKind != JsonValueKind.Array)
        {
            _logger.Warn("[{0}] The 'plugins' section must be a JSON array.", PluginErrorCodes.ConfigInvalid);
            return new PluginManifestConfig();
        }

        var entries = new List<PluginConfigEntry>();

        foreach (var item in pluginsElement.EnumerateArray())
        {
            try
            {
                var entry = ParsePluginEntry(item, basePath);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("[{0}] Failed to parse a plugin configuration entry: {1}", PluginErrorCodes.ConfigInvalid, ex.Message);
            }
        }

        return new PluginManifestConfig { Plugins = entries };
    }

    private PluginConfigEntry? ParsePluginEntry(JsonElement element, string basePath)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            _logger.Warn("[{0}] Plugin entry must be a JSON object.", PluginErrorCodes.ConfigInvalid);
            return null;
        }

        if (!element.TryGetProperty("name", out var nameProp) ||
            nameProp.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(nameProp.GetString()))
        {
            _logger.Warn("[{0}] Plugin entry is missing a valid 'name' property. Skipping.", PluginErrorCodes.ConfigInvalid);
            return null;
        }

        var entry = new PluginConfigEntry
        {
            Name = nameProp.GetString()!.Trim()
        };

        if (element.TryGetProperty("assemblyPath", out var assemblyProp) &&
            assemblyProp.ValueKind == JsonValueKind.String)
        {
            var assemblyPath = assemblyProp.GetString();
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                entry.AssemblyPath = Path.IsPathRooted(assemblyPath)
                    ? assemblyPath
                    : Path.GetFullPath(Path.Combine(basePath, assemblyPath));
            }
        }

        if (element.TryGetProperty("enabled", out var enabledProp) &&
            enabledProp.ValueKind == JsonValueKind.False)
        {
            entry.Enabled = false;
        }

        if (element.TryGetProperty("settings", out var settingsProp) &&
            settingsProp.ValueKind == JsonValueKind.Object)
        {
            entry.Settings = new Dictionary<string, JsonElement>();
            foreach (var setting in settingsProp.EnumerateObject())
            {
                entry.Settings[setting.Name] = setting.Value;
            }
        }

        return entry;
    }
}
