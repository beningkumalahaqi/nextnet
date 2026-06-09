using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NextNet.TemplateSdk;

/// <summary>
/// Publishes compiled template packages to the NextNet template registry.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TemplatePublisher"/> uploads a <c>.nntemplate</c> package to the
/// configured registry endpoint. Authentication is performed using a bearer token
/// obtained from the <see cref="AuthorProfile"/> or provided directly via
/// <see cref="PublishOptions"/>.
/// </para>
/// <para>
/// If no API token is provided in the options, the publisher attempts to load
/// the local <see cref="AuthorProfile"/> automatically.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var http = new HttpClient();
/// var publisher = new TemplatePublisher(http);
/// var result = await publisher.PublishAsync("./output/my-template.nntemplate",
///     new PublishOptions { ApiToken = "sk_abc123" });
/// Console.WriteLine(result.Success ? "Published!" : $"Failed: {result.Message}");
/// </code>
/// </example>
public sealed class TemplatePublisher
{
    private readonly HttpClient _http;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplatePublisher"/> class.
    /// </summary>
    /// <param name="http">The <see cref="HttpClient"/> used for registry communication.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="http"/> is null.</exception>
    public TemplatePublisher(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <summary>
    /// Publishes the template package to the configured registry.
    /// </summary>
    /// <param name="packagePath">The path to the <c>.nntemplate</c> package file.</param>
    /// <param name="options">Publishing options including API token and registry URL.</param>
    /// <returns>A <see cref="PublishResult"/> indicating success or failure.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="packagePath"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the package file does not exist.</exception>
    public async Task<PublishResult> PublishAsync(string packagePath, PublishOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentNullException.ThrowIfNull(options);

        if (!File.Exists(packagePath))
            throw new TemplateSdkException(TemplateSdkErrorCodes.InvalidArgument, $"Package not found: {packagePath}");

        if (string.IsNullOrEmpty(options.ApiToken))
        {
            var profile = await AuthorProfile.LoadAsync();
            options = options with { ApiToken = profile?.ApiToken };
        }

        if (string.IsNullOrEmpty(options.ApiToken))
            return new PublishResult { Success = false, ErrorCode = TemplateSdkErrorCodes.PublishMissingToken, Message = "No API token. Run 'nextnet template login' first." };

        var registryUrl = options.RegistryUrl ?? "https://registry.nextnet.dev";

        try
        {
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(packagePath);
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(fileContent, "package", Path.GetFileName(packagePath));

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{registryUrl}/api/templates/publish");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);
            request.Content = form;
            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return new PublishResult
                {
                    Success = true,
                    Message = "Template published successfully",
                    Version = result.TryGetProperty("version", out var v) ? v.GetString() ?? "" : ""
                };
            }

            var error = await response.Content.ReadAsStringAsync();
            return new PublishResult { Success = false, ErrorCode = TemplateSdkErrorCodes.PublishServerError, Message = $"Publish failed: {error}" };
        }
        catch (Exception ex)
        {
            return new PublishResult { Success = false, ErrorCode = TemplateSdkErrorCodes.PublishServerError, Message = $"Publish error: {ex.Message}" };
        }
    }
}

/// <summary>
/// Configuration options for publishing a template to the registry.
/// </summary>
/// <remarks>
/// If <see cref="PublishOptions.ApiToken"/> is not set, the publisher falls back to the
/// token stored in the local <see cref="AuthorProfile"/>.
/// </remarks>
public sealed record PublishOptions
{
    /// <summary>
    /// The bearer token for registry authentication.
    /// If empty, falls back to the token stored in the local <see cref="AuthorProfile"/>.
    /// </summary>
    public string? ApiToken { get; init; }

    /// <summary>
    /// The base URL of the NextNet template registry API.
    /// Defaults to <c>https://registry.nextnet.dev</c>.
    /// </summary>
    public string? RegistryUrl { get; init; }

    /// <summary>
    /// An optional explicit version to assign to the published template.
    /// If not specified, the version from the manifest is used.
    /// </summary>
    public string? Version { get; init; }
}

/// <summary>
/// Describes the result of a <see cref="TemplatePublisher.PublishAsync"/> operation.
/// </summary>
/// <remarks>
/// Check <see cref="PublishResult.Success"/> to determine whether the publish operation
/// completed successfully. <see cref="PublishResult.Message"/> contains error details
/// on failure, and <see cref="PublishResult.Version"/> is set on success.
/// </remarks>
public sealed record PublishResult
{
    /// <summary>
    /// <c>true</c> if the template was published successfully; otherwise <c>false</c>.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The SDK error code when <see cref="Success"/> is <c>false</c>, if applicable.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// A human-readable message describing the result.
    /// Contains error details when <see cref="Success"/> is <c>false</c>.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// The version of the template that was published.
    /// Only set when <see cref="Success"/> is <c>true</c>.
    /// </summary>
    public string? Version { get; init; }
}
