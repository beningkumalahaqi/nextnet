using System.Text.Json;

namespace NextNet.Cli.Community;

/// <summary>
/// Tracks installed community templates in a local JSON manifest file at
/// <c>~/.nextnet/templates/manifest.json</c>.
/// </summary>
/// <remarks>
/// <para>
/// The registry persists installed template metadata to disk so that the CLI
/// can list, update, and remove templates across sessions. The manifest file
/// is written atomically using a temporary file and rename pattern to prevent
/// corruption.
/// </para>
/// <para>
/// This class is thread-safe for read operations but does not synchronize
/// concurrent writes. In practice, the CLI processes commands sequentially.
/// </para>
/// </remarks>
public sealed class InstalledTemplateRegistry
{
    private readonly string _manifestPath;
    private readonly Dictionary<string, InstalledTemplate> _templates = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="InstalledTemplateRegistry"/> class.
    /// </summary>
    /// <remarks>
    /// Creates the <c>~/.nextnet/templates</c> directory if it does not exist and
    /// loads any existing manifest from disk.
    /// </remarks>
    public InstalledTemplateRegistry()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var templatesDir = Path.Combine(home, ".nextnet", "templates");
        Directory.CreateDirectory(templatesDir);
        _manifestPath = Path.Combine(templatesDir, "manifest.json");
        Load();
    }

    /// <summary>
    /// Gets a read-only dictionary of all installed templates, keyed by template name.
    /// </summary>
    public IReadOnlyDictionary<string, InstalledTemplate> All => _templates;

    /// <summary>
    /// Retrieves an installed template by name.
    /// </summary>
    /// <param name="name">The name of the template to look up.</param>
    /// <returns>The matching <see cref="InstalledTemplate"/>, or <c>null</c> if not found.</returns>
    public InstalledTemplate? Get(string name) => _templates.TryGetValue(name, out var t) ? t : null;

    /// <summary>
    /// Registers a newly installed template or updates an existing one.
    /// </summary>
    /// <param name="template">The template metadata to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is <c>null</c>.</exception>
    public void Register(InstalledTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _templates[template.Name] = template;
        Save();
    }

    /// <summary>
    /// Removes a template from the registry by name.
    /// </summary>
    /// <param name="name">The name of the template to unregister.</param>
    /// <returns><c>true</c> if the template was found and removed; <c>false</c> if it was not registered.</returns>
    public bool Unregister(string name)
    {
        var removed = _templates.Remove(name);
        if (removed) Save();
        return removed;
    }

    private void Load()
    {
        if (!File.Exists(_manifestPath)) return;
        try
        {
            var json = File.ReadAllText(_manifestPath);
            var list = JsonSerializer.Deserialize<List<InstalledTemplate>>(json, JsonOptions);
            if (list is not null)
            {
                foreach (var t in list)
                    _templates[t.Name] = t;
            }
        }
        catch
        {
            // Ignore corrupted manifest — start with an empty registry
        }
    }

    private void Save()
    {
        var tempPath = _manifestPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(_templates.Values.ToList(), JsonOptions));
        File.Move(tempPath, _manifestPath, overwrite: true);
    }
}
