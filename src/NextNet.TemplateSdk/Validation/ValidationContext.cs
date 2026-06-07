using NextNet.Templates.Models;

namespace NextNet.TemplateSdk.Validation;

/// <summary>
/// Provides contextual data to each <see cref="ValidationRule"/> during validation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ValidationContext"/> carries the template manifest, the file dictionary,
/// and a cancellation token so that rules can access all necessary data without
/// coupling to the orchestrator. It is constructed by the <see cref="TemplateValidator"/>
/// and passed to every rule's <c>Validate</c> method.
/// </para>
/// </remarks>
public sealed class ValidationContext
{
    /// <summary>
    /// Gets the template manifest being validated.
    /// </summary>
    public TemplateManifest Manifest { get; }

    /// <summary>
    /// Gets a read-only dictionary mapping relative source paths to their raw byte content.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]> Files { get; }

    /// <summary>
    /// Gets the cancellation token for signalling early termination.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationContext"/> class.
    /// </summary>
    /// <param name="manifest">The template manifest to validate.</param>
    /// <param name="files">A dictionary of file paths to raw byte content.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    public ValidationContext(
        TemplateManifest manifest,
        IReadOnlyDictionary<string, byte[]> files,
        CancellationToken cancellationToken = default)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Files = files ?? throw new ArgumentNullException(nameof(files));
        CancellationToken = cancellationToken;
    }
}
