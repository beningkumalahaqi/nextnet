using System.Reflection;

namespace NextNet.Templates.Manifest;

/// <summary>
/// Provides access to the embedded template manifest JSON Schema (draft-07) used for
/// validating <c>template.json</c> manifest files.
/// </summary>
/// <remarks>
/// <para>
/// The schema is embedded as a managed resource in the <c>NextNet.Templates</c> assembly
/// at path <c>Manifest/Schema/template-manifest.schema.json</c>. This class exposes the
/// schema content as a string or as a readable <see cref="Stream"/>.
/// </para>
/// <para>
/// The schema can be used with any JSON Schema validation library to validate template
/// manifest files before they are parsed and processed by the template engine.
/// </para>
/// <example>
/// <code>
/// // Retrieve the schema as a JSON string
/// string schemaJson = ManifestSchema.GetSchemaJson();
///
/// // Or retrieve it as a stream for streaming consumption
/// using var schemaStream = ManifestSchema.GetSchemaStream();
/// </code>
/// </example>
/// </remarks>
public static class ManifestSchema
{
    private const string EmbeddedResourceName = "NextNet.Templates.Manifest.Schema.template-manifest.schema.json";

    /// <summary>
    /// Retrieves the embedded template manifest JSON Schema as a string.
    /// </summary>
    /// <returns>The JSON Schema content as a <see cref="string"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded schema resource is not found.</exception>
    public static string GetSchemaJson()
    {
        using var stream = GetSchemaStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Retrieves the embedded template manifest JSON Schema as a readable <see cref="Stream"/>.
    /// </summary>
    /// <returns>A <see cref="Stream"/> containing the JSON Schema content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the embedded schema resource is not found.</exception>
    public static Stream GetSchemaStream()
    {
        var assembly = typeof(ManifestSchema).Assembly;
        var stream = assembly.GetManifestResourceStream(EmbeddedResourceName);

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Embedded schema resource '{EmbeddedResourceName}' was not found in assembly '{assembly.GetName().Name}'.");
        }

        return stream;
    }
}
