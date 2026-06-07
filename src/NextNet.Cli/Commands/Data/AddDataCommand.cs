using NextNet.Cli.Config;
using NextNet.Cli.Errors;
using NextNet.Cli.Services;
using NextNet.Cli.UI;
using NextNet.Cli.UI.Messages;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Implements the <c>nextnet add data &lt;provider&gt;</c> command — installs a data
/// access provider (EF Core, Dapper, MongoDB) by adding NuGet packages and
/// updating <c>nextnet.config.json</c> with the provider configuration.
/// </summary>
public static class AddDataCommand
{
    /// <summary>
    /// Creates the <c>data</c> subcommand with provider argument and project option.
    /// </summary>
    public static Command Create()
    {
        var providerArg = new Argument<string>("provider", "Data provider to install (ef, dapper, mongo)");
        var projectOption = new Option<string>("--project", "Path to the project file (.csproj) or project directory");

        var command = new Command("data", "Add a data access provider (EF Core, Dapper, MongoDB)")
        {
            providerArg,
            projectOption
        };

        Handler.SetHandler(command, (InvocationContext context) =>
        {
            var provider = context.ParseResult.GetValueForArgument(providerArg);
            var project = context.ParseResult.GetValueForOption(projectOption);
            var plain = context.ParseResult.GetValueForOption(NextNetCli.PlainOption);
            var noColor = context.ParseResult.GetValueForOption(NextNetCli.NoColorOption);
            var verbose = context.ParseResult.GetValueForOption(NextNetCli.VerboseOption);

            var exitCode = ExecuteAsync(provider, project, verbose, plain, noColor).GetAwaiter().GetResult();
            context.ExitCode = exitCode;
        });

        return command;
    }

    /// <summary>
    /// Executes the add data command logic — validates the provider, resolves packages,
    /// runs <c>dotnet add package</c>, and updates the config file.
    /// </summary>
    /// <param name="provider">The data provider name (ef, dapper, mongo).</param>
    /// <param name="projectPath">Optional path to the project file or directory.</param>
    /// <param name="verbose">Enable verbose output.</param>
    /// <param name="plain">Plain text output (no Unicode, no colors).</param>
    /// <param name="noColor">Disable color output.</param>
    /// <returns>Exit code (0 = success, 2 = input error, 4 = execution error).</returns>
    public static async Task<int> ExecuteAsync(string? provider, string? projectPath = null, bool verbose = false, bool plain = false, bool noColor = false)
    {
        var console = NextNetConsole.Create(plain, noColor);

        try
        {
            // Validate provider
            if (string.IsNullOrWhiteSpace(provider))
            {
                ErrorMessage.Write(console, ErrorCodes.InvalidProvider,
                    "Provider name is required. Supported: ef, dapper, mongo.");
                return 2;
            }

            var packageInfo = PackageResolver.Resolve(provider);
            if (packageInfo is null)
            {
                ErrorMessage.Write(console, ErrorCodes.InvalidProvider,
                    $"'{provider}' is not a supported provider. Supported: {string.Join(", ", PackageResolver.GetKnownProviders())}.");
                return 2;
            }

            console.WriteLine($"Installing {packageInfo.Description}...");

            // Resolve project path
            string? resolvedProjectPath = null;
            if (!string.IsNullOrEmpty(projectPath))
            {
                if (File.Exists(projectPath) && projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedProjectPath = projectPath;
                }
                else if (Directory.Exists(projectPath))
                {
                    // Find the first .csproj in the directory
                    var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly);
                    if (csprojFiles.Length > 0)
                        resolvedProjectPath = csprojFiles[0];
                }
                else
                {
                    console.WriteWarning($"Project path '{projectPath}' not found. Will try auto-discovery.");
                }
            }

            // Install primary package
            console.WriteLine($"  Adding package {packageInfo.PackageId}...");
            var success = await DotNetCliService.AddPackageAsync(packageInfo.PackageId, resolvedProjectPath);

            if (!success)
            {
                ErrorMessage.Write(console, ErrorCodes.PackageAddFailed,
                    $"Failed to add package '{packageInfo.PackageId}'. Check NuGet source availability.");
                return 4;
            }

            // Install additional packages (e.g., EF Core needs SQLite provider)
            if (packageInfo.AdditionalPackages is { Length: > 0 })
            {
                foreach (var additionalPackage in packageInfo.AdditionalPackages)
                {
                    console.WriteLine($"  Adding additional package {additionalPackage}...");
                    var additionalSuccess = await DotNetCliService.AddPackageAsync(additionalPackage, resolvedProjectPath);

                    if (!additionalSuccess)
                    {
                        console.WriteWarning($"Failed to add additional package '{additionalPackage}'. You may need to install it manually.");
                    }
                }
            }

            // Update config
            console.WriteLine("Updating nextnet.config.json...");
            try
            {
                UpdateConfigWithProvider(provider, packageInfo.PackageId);
                console.WriteSuccess($"Data provider '{provider}' configured successfully!");
            }
            catch (Exception ex)
            {
                ErrorMessage.Write(console, ErrorCodes.ConfigUpdateFailed,
                    $"Failed to update configuration: {ex.Message}");
                return 4;
            }

            console.WriteLine();
            console.WriteInfo($"Run 'nextnet db init' to initialize your database.");
            return 0;
        }
        catch (Exception ex)
        {
            ErrorMessage.WriteSimple(console, ex.Message, ex);
            return 4;
        }
    }

    /// <summary>
    /// Updates <c>nextnet.config.json</c> with the selected data provider configuration.
    /// </summary>
    private static void UpdateConfigWithProvider(string provider, string packageId)
    {
        var config = ConfigLoader.LoadRequired();
        config.Data ??= new DataConfig();
        config.Data.Provider = provider;

        // Track the installed packages
        var allPackages = new List<string> { packageId };
        if (PackageResolver.Resolve(provider)?.AdditionalPackages is { } additional)
            allPackages.AddRange(additional);
        config.Data.Packages = allPackages.ToArray();

        ConfigLoader.Save(config);
    }
}
