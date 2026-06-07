using NextNet.Cli.Commands.Data;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Cli.Services;

/// <summary>
/// Orchestrates admin page scaffolding operations, bridging the CLI
/// <c>generate admin</c> command with <see cref="IAdminScaffoldProvider"/>
/// implementations.
/// </summary>
/// <remarks>
/// <para>
/// The AdminScaffoldService handles:
/// <list type="bullet">
///   <item><description>Loading project configuration</description></item>
///   <item><description>Resolving the appropriate <see cref="IAdminScaffoldProvider"/></description></item>
///   <item><description>Building <see cref="AdminScaffoldOptions"/> from CLI arguments and config defaults</description></item>
///   <item><description>Executing the admin page generation</description></item>
/// </list>
/// </para>
/// </remarks>
public static class AdminScaffoldService
{
    /// <summary>
    /// Loads configuration and resolves the admin scaffold provider.
    /// </summary>
    /// <param name="console">The console for output messages.</param>
    /// <returns>A tuple with config, provider, and optional error code.</returns>
    public static (NextNetProjectConfig? config, IAdminScaffoldProvider? provider, int? errorCode) LoadConfigAndProvider(
        NextNetConsole console)
    {
        var config = ConfigLoader.Load();
        if (config is null)
        {
            ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound);
            return (null, null, 2);
        }

        // Resolve admin scaffold provider
        IAdminScaffoldProvider? provider = null;
        if (config.Data?.Provider is not null)
        {
            provider = AdminScaffoldProviderRegistry.GetProvider(config.Data.Provider);
            if (provider is null)
            {
                ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed,
                    $"The configured data provider '{config.Data.Provider}' does not support admin scaffolding.");
                return (config, null, 2);
            }
        }
        else
        {
            ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed,
                "No data provider configured. Run 'nextnet add data <provider>' first to enable admin page generation.");
            return (config, null, 2);
        }

        return (config, provider, null);
    }

    /// <summary>
    /// Builds admin scaffold options from CLI arguments and configuration defaults.
    /// </summary>
    public static AdminScaffoldOptions BuildAdminOptions(
        NextNetProjectConfig config,
        string outputDir,
        string? namespaceOverride,
        string? routePrefix,
        string? layoutName,
        bool force,
        bool dryRun,
        IReadOnlyList<ScaffoldProperty>? properties)
    {
        var projectNamespace = ResolveProjectNamespace(config, outputDir);
        var adminConfig = config.Admin;

        var resolvedRoutePrefix = routePrefix ?? adminConfig?.RoutePrefix ?? "admin";
        var resolvedLayoutName = layoutName ?? adminConfig?.Layout ?? "AdminLayout";
        var resolvedAdminTitle = adminConfig?.Title ?? $"{projectNamespace ?? "App"} Admin";

        // Determine admin namespace
        var adminNs = namespaceOverride ?? adminConfig?.AdminNamespace ?? $"{projectNamespace ?? "App"}.Admin";

        // Determine admin directory
        var adminDir = adminConfig?.AdminDirectory ?? Path.Combine(outputDir, "app", "admin");

        return new AdminScaffoldOptions(
            RoutePrefix: resolvedRoutePrefix,
            LayoutName: resolvedLayoutName,
            AdminNamespace: adminNs,
            AdminDirectory: adminDir,
            Properties: properties,
            AdminTitle: resolvedAdminTitle);
    }

    /// <summary>
    /// Builds standard scaffold options from CLI arguments and config defaults.
    /// </summary>
    public static ScaffoldOptions BuildScaffoldOptions(
        NextNetProjectConfig config,
        string outputDir,
        bool force,
        bool dryRun)
    {
        var projectNamespace = ResolveProjectNamespace(config, outputDir);
        var scaffoldConfig = config.Scaffolding;

        return new ScaffoldOptions(
            OutputDirectory: outputDir,
            ModelsDirectory: scaffoldConfig?.ModelsDirectory ?? "Models",
            RepositoriesDirectory: scaffoldConfig?.RepositoriesDirectory ?? "Repositories",
            ActionsDirectory: scaffoldConfig?.ActionsDirectory ?? "app/api",
            ModelsNamespace: scaffoldConfig?.ModelsNamespace ?? "{Project}.Models",
            RepositoriesNamespace: scaffoldConfig?.RepositoriesNamespace ?? "{Project}.Repositories",
            ActionsNamespace: scaffoldConfig?.ActionsNamespace ?? "{Project}.Actions",
            OverwriteExisting: force || scaffoldConfig?.OverwriteExisting == true,
            DryRun: dryRun,
            ProjectNamespace: projectNamespace);
    }

    /// <summary>
    /// Resolves the project's root namespace from config or falls back to directory name.
    /// </summary>
    public static string? ResolveProjectNamespace(NextNetProjectConfig config, string outputDir)
    {
        if (!string.IsNullOrWhiteSpace(config.Name))
        {
            var parts = config.Name.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return string.Concat(parts.Select(p =>
                    p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p));
        }

        var dirName = Path.GetFileName(outputDir);
        if (!string.IsNullOrWhiteSpace(dirName))
        {
            var parts = dirName.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return string.Concat(parts.Select(p =>
                    p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p));
        }

        return null;
    }

    /// <summary>
    /// Validates the admin route prefix (must be a valid URL segment).
    /// </summary>
    public static bool ValidateRoutePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return false;

        // Must contain only alphanumeric characters and hyphens
        foreach (var c in prefix)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
                return false;
        }

        return true;
    }
}
