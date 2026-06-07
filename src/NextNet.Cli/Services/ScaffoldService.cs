using NextNet.Cli.Commands.Data;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Internal;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Cli.Services;

/// <summary>
/// Orchestrates scaffolding operations, bridging CLI commands with
/// <see cref="IScaffoldProvider"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// The ScaffoldService handles:
/// <list type="bullet">
///   <item><description>Loading project configuration from <c>nextnet.config.json</c></description></item>
///   <item><description>Resolving the appropriate <see cref="IScaffoldProvider"/> based on config</description></item>
///   <item><description>Building <see cref="ScaffoldOptions"/> from CLI arguments and config defaults</description></item>
///   <item><description>Executing the scaffold operation and returning results</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ScaffoldService
{
    /// <summary>
    /// Loads project configuration and resolves the scaffold provider.
    /// </summary>
    /// <param name="console">The console for output messages.</param>
    /// <returns>A tuple containing the config and resolved provider, or null with an error code.</returns>
    public static (NextNetProjectConfig? config, IScaffoldProvider? provider, int? errorCode) LoadConfigAndProvider(
        UI.NextNetConsole console)
    {
        // Load config
        var config = ConfigLoader.Load();
        if (config is null)
        {
            UI.Messages.ErrorMessage.Write(console, ErrorCodes.ConfigFileNotFound);
            return (null, null, 2);
        }

        // Resolve provider
        IScaffoldProvider? provider;
        if (config.Data?.Provider is not null)
        {
            provider = ScaffoldProviderRegistry.GetProvider(config.Data.Provider);
            if (provider is null)
            {
                UI.Messages.ErrorMessage.Write(console, ErrorCodes.InvalidProvider,
                    $"The configured data provider '{config.Data.Provider}' does not support scaffolding or is not installed.");
                return (config, null, 2);
            }
        }
        else
        {
            provider = ScaffoldProviderRegistry.GetModelOnlyProvider();
        }

        return (config, provider, null);
    }

    /// <summary>
    /// Builds <see cref="ScaffoldOptions"/> from CLI arguments and configuration defaults.
    /// </summary>
    public static ScaffoldOptions BuildOptions(
        NextNetProjectConfig config,
        string outputDir,
        string? namespaceOverride,
        bool force,
        bool dryRun,
        IReadOnlyList<ScaffoldProperty>? properties = null)
    {
        var projectNamespace = ResolveProjectNamespace(config, outputDir);

        // Determine default namespace patterns
        var scaffoldConfig = config.Scaffolding;
        var modelsNs = namespaceOverride ?? scaffoldConfig?.ModelsNamespace ?? "{Project}.Models";
        var reposNs = namespaceOverride ?? scaffoldConfig?.RepositoriesNamespace ?? "{Project}.Repositories";
        var actionsNs = namespaceOverride ?? scaffoldConfig?.ActionsNamespace ?? "{Project}.Actions";

        return new ScaffoldOptions(
            OutputDirectory: outputDir,
            ModelsDirectory: scaffoldConfig?.ModelsDirectory ?? "Models",
            RepositoriesDirectory: scaffoldConfig?.RepositoriesDirectory ?? "Repositories",
            ActionsDirectory: scaffoldConfig?.ActionsDirectory ?? "app/api",
            ModelsNamespace: modelsNs,
            RepositoriesNamespace: reposNs,
            ActionsNamespace: actionsNs,
            OverwriteExisting: force || scaffoldConfig?.OverwriteExisting == true,
            DryRun: dryRun,
            ProjectNamespace: projectNamespace,
            Properties: properties
        );
    }

    /// <summary>
    /// Parses property strings in "Name:Type" format into <see cref="ScaffoldProperty"/> records.
    /// </summary>
    /// <param name="propertyStrings">The property strings from CLI --property options.</param>
    /// <returns>A list of parsed scaffold properties, or null with an error message if parsing fails.</returns>
    public static (IReadOnlyList<ScaffoldProperty>? properties, string? error) ParseProperties(
        IEnumerable<string>? propertyStrings)
    {
        if (propertyStrings is null)
            return (null, null);

        var result = new List<ScaffoldProperty>();
        foreach (var propStr in propertyStrings)
        {
            var parts = propStr.Split(':', 2);
            var name = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(name))
                return (null, $"Invalid property format: '{propStr}'. Use 'Name:Type' syntax (e.g., 'FirstName:string').");

            var type = parts.Length > 1 ? parts[1].Trim() : "string";

            result.Add(new ScaffoldProperty(
                Name: name,
                Type: type));
        }

        return (result.AsReadOnly(), null);
    }

    /// <summary>
    /// Validates an entity name (must be PascalCase, start with a letter, contain only alphanumeric).
    /// </summary>
    /// <param name="name">The entity name to validate.</param>
    /// <returns>True if the name is valid.</returns>
    public static bool ValidateEntityName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Must start with a letter
        if (!char.IsLetter(name[0]))
            return false;

        // Must contain only alphanumeric characters
        foreach (var c in name)
        {
            if (!char.IsLetterOrDigit(c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Resolves the project's root namespace from config or .csproj, or falls back to directory name.
    /// </summary>
    private static string? ResolveProjectNamespace(NextNetProjectConfig config, string outputDir)
    {
        // Try config name
        if (!string.IsNullOrWhiteSpace(config.Name))
        {
            // Convert kebab-case to PascalCase for namespace
            var parts = config.Name.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return string.Concat(parts.Select(p =>
                    p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p));
            }
        }

        // Try directory name
        var dirName = Path.GetFileName(outputDir);
        if (!string.IsNullOrWhiteSpace(dirName))
        {
            var parts = dirName.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return string.Concat(parts.Select(p =>
                    p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p));
            }
        }

        return null;
    }
}
