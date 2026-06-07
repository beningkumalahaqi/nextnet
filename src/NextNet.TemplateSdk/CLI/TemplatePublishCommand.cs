using System.CommandLine;

namespace NextNet.TemplateSdk.CLI;

/// <summary>
/// Implements the <c>template publish</c> command — publishes a packaged template
/// to the NextNet template registry.
/// </summary>
/// <remarks>
/// <para>
/// The <c>template publish</c> command uses <see cref="TemplatePublisher"/> to upload a
/// <c>.nntemplate</c> file to the registry. Authentication is handled via the bearer
/// token stored in the local <see cref="AuthorProfile"/> or provided on the command line.
/// </para>
/// <para>
/// Before publishing, ensure you have authenticated using the <c>template login</c>
/// command or set your API token via the <c>--api-token</c> option.
/// </para>
/// </remarks>
public sealed class TemplatePublishCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplatePublishCommand"/> class.
    /// </summary>
    public TemplatePublishCommand() : base("publish", "Publish a template to the registry")
    {
        var packageArg = new Argument<string>("package-path", "Path to the .nntemplate file");
        var apiTokenOption = new Option<string?>("--api-token", "API token for registry authentication");
        var registryUrlOption = new Option<string?>("--registry", "Registry URL");

        AddArgument(packageArg);
        AddOption(apiTokenOption);
        AddOption(registryUrlOption);

        this.SetHandler(HandleAsync, packageArg, apiTokenOption, registryUrlOption);
    }

    private static async Task<int> HandleAsync(string packagePath, string? apiToken, string? registryUrl)
    {
        try
        {
            using var http = new HttpClient();
            var publisher = new TemplatePublisher(http);
            var result = await publisher.PublishAsync(packagePath, new PublishOptions
            {
                ApiToken = apiToken,
                RegistryUrl = registryUrl
            });

            if (result.Success)
            {
                Console.WriteLine($"Published successfully! Version: {result.Version ?? "(unknown)"}");
                return 0;
            }

            Console.Error.WriteLine($"Publish failed: {result.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
