using System.Text.Json.Serialization;

namespace NextNet.Templates.Models;

/// <summary>
/// Represents a fully resolved template package containing both the manifest metadata
/// and the raw file contents ready for generation.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="TemplatePackage"/> is produced by an <c>ITemplateProvider</c> after
/// resolving a template manifest and loading its constituent files into memory.
/// It is the primary input to the template engine (<c>ITemplateEngine</c>) for
/// code generation.
/// </para>
/// <para>
/// The <see cref="Files"/> dictionary maps source paths (relative to the template root)
/// to their raw byte content. Binary files (images, archives) are included as-is;
/// text files are encoded as UTF-8 bytes.
/// </para>
/// <example>
/// <code>
/// var manifest = new TemplateManifest("my-template", "1.0.0", "3.0.0");
/// var files = new Dictionary&lt;string, byte[]&gt;
/// {
///     ["src/Program.cs"] = Encoding.UTF8.GetBytes("..."),
///     ["src/Startup.cs"] = Encoding.UTF8.GetBytes("...")
/// };
/// var package = new TemplatePackage(manifest, files);
/// </code>
/// </example>
/// </remarks>
/// <param name="Manifest">The parsed metadata describing this template.</param>
/// <param name="Files">A dictionary mapping relative source paths to their raw content bytes.</param>
public sealed record TemplatePackage(
    [property: JsonPropertyName("manifest")] TemplateManifest Manifest,
    [property: JsonPropertyName("files")] IReadOnlyDictionary<string, byte[]>? Files = null
);
