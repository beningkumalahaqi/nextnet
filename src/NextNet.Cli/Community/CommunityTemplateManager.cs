using NextNet.IO;
using NextNet.Templates.Abstractions;

namespace NextNet.Cli.Community;

/// <summary>
/// Manages the lifecycle of community templates: install, update, remove, and list.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="CommunityTemplateManager"/> coordinates between the remote
/// <see cref="ITemplateRegistry"/> (where templates are discovered and downloaded)
/// and the local <see cref="InstalledTemplateRegistry"/> (which persists metadata
/// about what is installed). It delegates filesystem operations to an
/// <see cref="ISharpFileSystem"/> abstraction for testability.
/// </para>
/// <para>
/// Installation flow:
/// <list type="number">
///   <item>Resolve the version to install (latest or specified).</item>
///   <item>Retrieve the template manifest from the registry.</item>
///   <item>Compute the install path under <c>~/.nextnet/templates/</c>.</item>
///   <item>Create the directory and write the manifest file.</item>
///   <item>Register the template in the local installed-templates manifest.</item>
/// </list>
/// </para>
/// <para>
/// The actual package file download is deferred to a future phase (Plan 17).
/// This implementation saves the manifest only.
/// </para>
/// </remarks>
public sealed class CommunityTemplateManager
{
    private readonly ITemplateRegistry _registry;
    private readonly InstalledTemplateRegistry _installed;
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunityTemplateManager"/> class.
    /// </summary>
    /// <param name="registry">The remote template registry for discovery and download.</param>
    /// <param name="installed">The local registry tracking installed templates.</param>
    /// <param name="fileSystem">The filesystem abstraction for disk operations.</param>
    public CommunityTemplateManager(
        ITemplateRegistry registry,
        InstalledTemplateRegistry installed,
        ISharpFileSystem fileSystem)
    {
        _registry = registry;
        _installed = installed;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Installs a community template from the registry.
    /// </summary>
    /// <param name="name">The name of the template to install.</param>
    /// <param name="options">Optional installation options (version pin, force reinstall).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="InstallResult"/> describing the outcome.</returns>
    public async Task<InstallResult> InstallAsync(
        string name,
        InstallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        options ??= new InstallOptions();

        try
        {
            // 1. Resolve latest version (or specified)
            var versions = await _registry.GetVersionsAsync(name, cancellationToken);
            if (versions.Count == 0)
                return InstallResult.CreateFailure($"No versions found for template '{name}'.");

            var versionToInstall = options.Version ?? versions.OrderByDescending(v => v).First();
            if (!versions.Contains(versionToInstall))
                return InstallResult.CreateFailure($"Version '{versionToInstall}' not found for template '{name}'.");

            // 2. Get metadata
            var manifest = await _registry.GetManifestAsync(name, versionToInstall, cancellationToken);

            // 3. Determine install path
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var installDir = Path.Combine(home, ".nextnet", "templates", $"{manifest.Author}-{name}");

            if (_fileSystem.DirectoryExists(installDir) && !options.Force)
                return InstallResult.CreateFailure($"Template '{name}' is already installed. Use --force to reinstall.");

            // 4. Download and save manifest (the actual package download is deferred to Plan 17)
            _fileSystem.CreateDirectory(installDir);
            var manifestPath = Path.Combine(installDir, "template.json");
            var manifestJson = System.Text.Json.JsonSerializer.Serialize(manifest,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await _fileSystem.WriteAllTextAsync(manifestPath, manifestJson);

            // 5. Register
            _installed.Register(new InstalledTemplate
            {
                Name = name,
                Author = manifest.Author ?? "unknown",
                Version = versionToInstall,
                InstallPath = installDir,
                InstalledAt = DateTime.UtcNow
            });

            return InstallResult.CreateSuccess(name, versionToInstall, installDir);
        }
        catch (Exception ex)
        {
            return InstallResult.CreateFailure($"Installation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates one or all installed templates to their latest version.
    /// </summary>
    /// <param name="name">
    /// An optional template name. If <c>null</c> or empty, all installed templates are updated.
    /// </param>
    /// <param name="options">Optional update options (pre-release, etc.).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="UpdateResult"/> listing updated and failed templates.</returns>
    public async Task<UpdateResult> UpdateAsync(
        string? name = null,
        UpdateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var updated = new List<string>();
        var failed = new List<string>();

        var toUpdate = string.IsNullOrEmpty(name)
            ? _installed.All.Values.ToList()
            : new[] { _installed.Get(name) }.Where(t => t is not null).Cast<InstalledTemplate>().ToList();

        foreach (var template in toUpdate)
        {
            try
            {
                var versions = await _registry.GetVersionsAsync(template.Name, cancellationToken);
                var latest = versions.OrderByDescending(v => v).FirstOrDefault();
                if (latest is null || latest == template.Version)
                    continue;

                var result = await InstallAsync(template.Name, new InstallOptions { Version = latest, Force = true }, cancellationToken);
                if (result.Success)
                    updated.Add(template.Name);
                else
                    failed.Add(template.Name);
            }
            catch
            {
                failed.Add(template.Name);
            }
        }

        return new UpdateResult
        {
            Updated = updated,
            Failed = failed
        };
    }

    /// <summary>
    /// Removes an installed template from disk and the local registry.
    /// </summary>
    /// <param name="name">The name of the template to remove.</param>
    /// <returns>A <see cref="RemoveResult"/> describing the outcome.</returns>
    public RemoveResult Remove(string name)
    {
        var template = _installed.Get(name);
        if (template is null)
            return new RemoveResult { Success = false, Message = $"Template '{name}' is not installed." };

        try
        {
            if (_fileSystem.DirectoryExists(template.InstallPath))
            {
                _fileSystem.DeleteDirectory(template.InstallPath, recursive: true);
            }
            _installed.Unregister(name);
            return new RemoveResult { Success = true, Message = $"Removed template '{name}'." };
        }
        catch (Exception ex)
        {
            return new RemoveResult { Success = false, Message = $"Remove failed: {ex.Message}" };
        }
    }

    /// <summary>
    /// Lists all installed community templates.
    /// </summary>
    /// <returns>A read-only list of <see cref="InstalledTemplate"/> records.</returns>
    public IReadOnlyList<InstalledTemplate> List() => _installed.All.Values.ToList();
}

/// <summary>
/// Options for installing a community template.
/// </summary>
/// <remarks>
/// Use <see cref="Version"/> to pin a specific version, or leave as <c>null</c> to use the latest.
/// Set <see cref="Force"/> to <c>true</c> to reinstall even if already present.
/// </remarks>
public sealed record InstallOptions
{
    /// <summary>
    /// Gets an optional specific version to install. If <c>null</c>, the latest version is used.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets a value indicating whether to force reinstallation even if already installed.
    /// </summary>
    public bool Force { get; init; }
}

/// <summary>
/// Options for updating installed community templates.
/// </summary>
/// <remarks>
/// When <see cref="PreRelease"/> is <c>true</c>, pre-release versions are considered as valid updates.
/// </remarks>
public sealed record UpdateOptions
{
    /// <summary>
    /// Gets a value indicating whether to include pre-release versions when checking for updates.
    /// </summary>
    public bool PreRelease { get; init; }
}
