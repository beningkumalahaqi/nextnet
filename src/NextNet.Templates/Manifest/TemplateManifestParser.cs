using System.Text.Json;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

namespace NextNet.Templates.Manifest;

/// <summary>
/// Parses template manifest JSON documents (typically <c>template.json</c>) into
/// <see cref="TemplateManifest"/> instances with configurable JSON serialization options.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemplateManifestParser"/> provides both <see cref="Stream"/> and
/// <see cref="string"/>-based parsing. It wraps <see cref="JsonException"/> in
/// <see cref="TemplateValidationException"/> with line-number context for better
/// error reporting.
/// </para>
/// <para>
/// After deserialization, the parser validates that required fields (<c>Name</c>,
/// <c>Version</c>, <c>NextNetVersion</c>) are present and throws if they are missing.
/// </para>
/// <example>
/// <code>
/// var parser = new TemplateManifestParser();
/// await using var stream = File.OpenRead("template.json");
/// var manifest = await parser.ParseAsync(stream);
/// Console.WriteLine(manifest.Name);
/// </code>
/// </example>
/// </remarks>
public sealed class TemplateManifestParser
{
    /// <summary>
    /// Gets the default JSON serialization options used when no custom options are provided.
    /// </summary>
    /// <remarks>
    /// These options use camelCase naming, are case-insensitive, skip comments, and allow
    /// trailing commas — providing lenient parsing suitable for hand-written template manifests.
    /// </remarks>
    public static JsonSerializerOptions DefaultJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Parses a <see cref="TemplateManifest"/> from a JSON stream using default options.
    /// </summary>
    /// <param name="stream">The stream containing JSON manifest data.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The deserialized <see cref="TemplateManifest"/>.</returns>
    /// <exception cref="TemplateValidationException">Thrown when JSON is invalid or required fields are missing.</exception>
    public Task<TemplateManifest> ParseAsync(Stream stream, CancellationToken ct = default)
        => ParseAsync(stream, DefaultJsonOptions, ct);

    /// <summary>
    /// Parses a <see cref="TemplateManifest"/> from a JSON string using default options.
    /// </summary>
    /// <param name="json">The JSON string containing manifest data.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The deserialized <see cref="TemplateManifest"/>.</returns>
    /// <exception cref="TemplateValidationException">Thrown when JSON is invalid or required fields are missing.</exception>
    public Task<TemplateManifest> ParseAsync(string json, CancellationToken ct = default)
        => ParseAsync(json, DefaultJsonOptions, ct);

    /// <summary>
    /// Parses a <see cref="TemplateManifest"/> from a JSON stream with custom options.
    /// </summary>
    /// <param name="stream">The stream containing JSON manifest data.</param>
    /// <param name="options">Custom JSON serialization options. If <c>null</c>, <see cref="DefaultJsonOptions"/> are used.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The deserialized <see cref="TemplateManifest"/>.</returns>
    /// <exception cref="TemplateValidationException">Thrown when JSON is invalid or required fields are missing.</exception>
    public async Task<TemplateManifest> ParseAsync(Stream stream, JsonSerializerOptions? options, CancellationToken ct = default)
    {
        options ??= DefaultJsonOptions;

        try
        {
            var manifest = await JsonSerializer.DeserializeAsync<TemplateManifest>(stream, options, ct);

            if (manifest is null)
            {
                throw new TemplateValidationException(new[]
                {
                    "Failed to deserialize template manifest: deserialization returned null."
                });
            }

            ValidateRequiredFields(manifest);
            return manifest;
        }
        catch (JsonException ex)
        {
            throw new TemplateValidationException(new[]
            {
                $"Invalid JSON at line {ex.LineNumber.GetValueOrDefault() + 1}, position {ex.BytePositionInLine.GetValueOrDefault()}: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Parses a <see cref="TemplateManifest"/> from a JSON string with custom options.
    /// </summary>
    /// <param name="json">The JSON string containing manifest data.</param>
    /// <param name="options">Custom JSON serialization options. If <c>null</c>, <see cref="DefaultJsonOptions"/> are used.</param>
    /// <param name="ct">A cancellation token to observe (this overload performs no I/O but accepts the token for API consistency).</param>
    /// <returns>The deserialized <see cref="TemplateManifest"/>.</returns>
    /// <exception cref="TemplateValidationException">Thrown when JSON is invalid or required fields are missing.</exception>
    public Task<TemplateManifest> ParseAsync(string json, JsonSerializerOptions? options, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        options ??= DefaultJsonOptions;

        try
        {
            var manifest = JsonSerializer.Deserialize<TemplateManifest>(json, options);

            if (manifest is null)
            {
                throw new TemplateValidationException(new[]
                {
                    "Failed to deserialize template manifest: deserialization returned null."
                });
            }

            ValidateRequiredFields(manifest);
            return Task.FromResult(manifest);
        }
        catch (JsonException ex)
        {
            throw new TemplateValidationException(new[]
            {
                $"Invalid JSON at line {ex.LineNumber.GetValueOrDefault() + 1}, position {ex.BytePositionInLine.GetValueOrDefault()}: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Validates that required fields (<c>Name</c>, <c>Version</c>, <c>NextNetVersion</c>)
    /// are present and non-null after deserialization.
    /// </summary>
    /// <param name="manifest">The manifest to validate.</param>
    /// <exception cref="TemplateValidationException">Thrown when any required field is missing.</exception>
    private static void ValidateRequiredFields(TemplateManifest manifest)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(manifest.Name))
            errors.Add("Required field 'name' is missing or empty.");

        if (string.IsNullOrWhiteSpace(manifest.Version))
            errors.Add("Required field 'version' is missing or empty.");

        if (string.IsNullOrWhiteSpace(manifest.NextNetVersion))
            errors.Add("Required field 'nextnetVersion' is missing or empty.");

        if (errors.Count > 0)
            throw new TemplateValidationException(errors);
    }
}
