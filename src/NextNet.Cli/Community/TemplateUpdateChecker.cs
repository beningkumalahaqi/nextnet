using NextNet.Templates.Abstractions;

namespace NextNet.Cli.Community;

/// <summary>
/// Checks installed community templates for available updates against the
/// template registry.
/// </summary>
/// <remarks>
/// <para>
/// Iterates over all installed templates in the <see cref="InstalledTemplateRegistry"/>
/// and queries the <see cref="ITemplateRegistry"/> for the latest available version
/// of each one. Templates whose installed version differs from the latest are
/// reported as having an available update.
/// </para>
/// <para>
/// If a template's version cannot be checked (e.g., network error or the template
/// was removed from the upstream registry), it is silently skipped.
/// </para>
/// </remarks>
public sealed class TemplateUpdateChecker
{
    private readonly ITemplateRegistry _registry;
    private readonly InstalledTemplateRegistry _installed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateUpdateChecker"/> class.
    /// </summary>
    /// <param name="registry">The template registry used to query available versions.</param>
    /// <param name="installed">The local registry of installed templates.</param>
    public TemplateUpdateChecker(ITemplateRegistry registry, InstalledTemplateRegistry installed)
    {
        _registry = registry;
        _installed = installed;
    }

    /// <summary>
    /// Checks all installed templates for available updates.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A list of <see cref="TemplateUpdateInfo"/> records describing templates
    /// that have a newer version available.
    /// </returns>
    public async Task<IReadOnlyList<TemplateUpdateInfo>> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var updates = new List<TemplateUpdateInfo>();
        foreach (var template in _installed.All.Values)
        {
            try
            {
                var versions = await _registry.GetVersionsAsync(template.Name, cancellationToken);
                var latest = versions.OrderByDescending(v => v).FirstOrDefault();
                if (latest is not null && latest != template.Version)
                {
                    updates.Add(new TemplateUpdateInfo
                    {
                        Name = template.Name,
                        InstalledVersion = template.Version,
                        LatestVersion = latest
                    });
                }
            }
            catch
            {
                // Skip templates we can't check (network error, removed from registry, etc.)
            }
        }
        return updates;
    }
}

/// <summary>
/// Describes an available update for an installed community template.
/// </summary>
public sealed record TemplateUpdateInfo
{
    /// <summary>
    /// Gets the name of the template that has an update available.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the currently installed version string.
    /// </summary>
    public string InstalledVersion { get; init; } = "";

    /// <summary>
    /// Gets the latest available version string from the registry.
    /// </summary>
    public string LatestVersion { get; init; } = "";
}
