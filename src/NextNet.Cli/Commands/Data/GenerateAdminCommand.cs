using System.CommandLine;
using System.CommandLine.Invocation;
using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.Services;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet generate admin &lt;entity&gt;</c> command — generates
/// a full set of admin CRUD pages (AdminLayout, List, Detail, Create, Edit, Delete)
/// for the specified entity.
/// </summary>
/// <remarks>
/// <para>
/// Generated pages use NextNet's existing <c>IPage</c> and <c>ILayout</c> rendering
/// pipeline. They are placed under <c>app/admin/</c> and are automatically discovered
/// by the NextNet route scanner.
/// </para>
/// </remarks>
public static class GenerateAdminCommand
{
    /// <summary>
    /// Creates the <c>admin</c> subcommand with entity name argument and options.
    /// </summary>
    public static Command Create()
    {
        var nameArg = new Argument<string>("entity", "Entity name (PascalCase, e.g., \"User\", \"Product\")");
        var outputOption = new Option<string>("--output", () => ".", "Output directory for admin pages");
        outputOption.AddAlias("-o");
        var propertyOption = new Option<string[]>("--property", "Property in format \"Name:Type\" (can be repeated)");
        propertyOption.AddAlias("-p");
        var namespaceOption = new Option<string>("--namespace", "Override the target namespace for generated admin pages");
        var routePrefixOption = new Option<string>("--route-prefix", () => "admin", "URL prefix for admin routes");
        var layoutOption = new Option<string>("--layout", () => "AdminLayout", "Layout to use for admin pages");
        var forceOption = new Option<bool>("--force", "Overwrite existing files without confirmation");
        var dryRunOption = new Option<bool>("--dry-run", "Show what would be generated without writing");

        var command = new Command("admin", "Generate admin CRUD pages for the specified entity")
        {
            nameArg,
            outputOption,
            propertyOption,
            namespaceOption,
            routePrefixOption,
            layoutOption,
            forceOption,
            dryRunOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var name = context.ParseResult.GetValueForArgument(nameArg);
            var output = context.ParseResult.GetValueForOption(outputOption) ?? ".";
            var properties = context.ParseResult.GetValueForOption(propertyOption);
            var ns = context.ParseResult.GetValueForOption(namespaceOption);
            var routePrefix = context.ParseResult.GetValueForOption(routePrefixOption);
            var layout = context.ParseResult.GetValueForOption(layoutOption);
            var force = context.ParseResult.GetValueForOption(forceOption);
            var dryRun = context.ParseResult.GetValueForOption(dryRunOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);

            var exitCode = ExecuteAsync(name, output, properties, ns, routePrefix, layout, force, dryRun, verbose, plain, noColor)
                .GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the <c>generate admin</c> command logic.
    /// </summary>
    public static async Task<int> ExecuteAsync(
        string? entityName,
        string output = ".",
        string[]? properties = null,
        string? namespaceOverride = null,
        string? routePrefix = null,
        string? layoutName = null,
        bool force = false,
        bool dryRun = false,
        bool verbose = false,
        bool plain = false,
        bool noColor = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Step 1: Validate entity name
            if (!ScaffoldService.ValidateEntityName(entityName))
            {
                if (string.IsNullOrWhiteSpace(entityName))
                {
                    ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed,
                        "Entity name is required. Provide a PascalCase name (e.g., \"User\").");
                }
                else
                {
                    ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed,
                        $"Invalid entity name '{entityName}'. Names must start with a letter and contain only alphanumeric characters.");
                }
                return 2;
            }

            // Step 2: Validate route prefix
            if (!AdminScaffoldService.ValidateRoutePrefix(routePrefix))
            {
                if (routePrefix is not null)
                {
                    ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed,
                        $"Invalid route prefix '{routePrefix}'. Route prefix must be a valid URL segment (alphanumeric and hyphens only).");
                    return 2;
                }
                routePrefix = "admin";
            }

            // Step 3: Load config and resolve provider
            var (config, provider, errorCode) = AdminScaffoldService.LoadConfigAndProvider(console);
            if (config is null || provider is null)
                return errorCode ?? 2;

            // Step 4: Parse properties
            var (parsedProps, parseError) = ScaffoldService.ParseProperties(properties);
            if (parseError is not null)
            {
                console.WriteError(parseError);
                return 2;
            }

            // Step 5: Build options
            var scaffoldOptions = AdminScaffoldService.BuildScaffoldOptions(config, output, force, dryRun);
            var adminOptions = AdminScaffoldService.BuildAdminOptions(
                config, output, namespaceOverride, routePrefix, layoutName, force, dryRun, parsedProps);

            // Step 6: Generate admin pages
            if (verbose)
                console.WriteLine($"Generating admin pages for '{entityName}'...");

            var artifacts = await provider.GenerateAdminPagesAsync(entityName!, scaffoldOptions, adminOptions);

            if (artifacts.Length == 0)
            {
                ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed, "No files were generated.");
                return 4;
            }

            // Check for errors
            foreach (var artifact in artifacts)
            {
                if (artifact.ErrorMessage is not null)
                {
                    ErrorMessage.Write(console, ErrorCodes.AdminGenerateFailed, artifact.ErrorMessage);
                    return 4;
                }
            }

            // Step 7: Display results
            if (dryRun)
            {
                console.WriteInfo($"Would create {artifacts.Length} files:");
                foreach (var artifact in artifacts)
                {
                    console.WriteMuted($"  {artifact.RelativePath}");
                }
            }
            else
            {
                var created = artifacts.Where(a => !a.WasSkipped).ToList();
                var skipped = artifacts.Where(a => a.WasSkipped).ToList();

                if (created.Count > 0)
                {
                    console.WriteSuccess($"Generated {created.Count} admin page(s):");
                    foreach (var artifact in created)
                    {
                        console.WriteMuted($"  {artifact.RelativePath}  ({artifact.LinesOfCode} lines)");
                    }
                }

                if (skipped.Count > 0)
                {
                    console.WriteWarning($"Skipped {skipped.Count} existing file(s) (use --force to overwrite):");
                    foreach (var artifact in skipped)
                    {
                        console.WriteMuted($"  {artifact.RelativePath}");
                    }
                }

                // Update config navigation if not dry run
                if (!dryRun)
                {
                    UpdateConfigNavigation(config, entityName!, adminOptions.RoutePrefix);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, verbose ? ex : null);
            return 4;
        }
    }

    /// <summary>
    /// Updates the config to add the newly generated entity to admin navigation.
    /// </summary>
    private static void UpdateConfigNavigation(NextNetProjectConfig config, string entityName, string routePrefix)
    {
        try
        {
            var pluralName = char.ToLowerInvariant(entityName[0]) + entityName[1..] + "s";
            config.Admin ??= new AdminConfig();

            config.Admin.Navigation ??= new List<AdminNavItem>();
            var navItem = new AdminNavItem
            {
                Label = SplitPascalCase(entityName),
                Route = $"/{routePrefix}/{pluralName}"
            };

            // Avoid duplicates
            if (!config.Admin.Navigation.Any(n =>
                string.Equals(n.Route, navItem.Route, StringComparison.OrdinalIgnoreCase)))
            {
                config.Admin.Navigation.Add(navItem);
                ConfigLoader.Save(config);
            }
        }
        catch
        {
            // Config update is best-effort; don't fail the command
        }
    }

    private static string SplitPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}
