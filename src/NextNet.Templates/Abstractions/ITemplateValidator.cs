using NextNet.Templates.Models;

namespace NextNet.Templates.Abstractions;

/// <summary>
/// Defines the contract for validating template manifests and packages for correctness,
/// completeness, and consistency.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ITemplateValidator"/> performs structural and semantic validation of
/// template metadata. It checks that required fields are present, variable types are
/// consistent, feature dependencies and conflicts are resolvable, and that all
/// referenced files exist within the package.
/// </para>
/// <para>
/// Validation does NOT involve rendering or code generation — it is a pure metadata
/// check that can be run on a manifest or package at any time.
/// </para>
/// <example>
/// <code>
/// var validator = new MyTemplateValidator();
/// var result = await validator.ValidateAsync(manifest);
///
/// if (!result.IsValid)
///     foreach (var error in result.Errors)
///         Console.WriteLine($"Error: {error}");
/// </code>
/// </example>
/// </remarks>
public interface ITemplateValidator
{
    /// <summary>
    /// Validates a <see cref="TemplateManifest"/> for structural correctness and completeness.
    /// </summary>
    /// <param name="manifest">The manifest to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any issues found.</returns>
    Task<ValidationResult> ValidateAsync(TemplateManifest manifest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a fully resolved <see cref="TemplatePackage"/>, including checking that
    /// all files referenced in the manifest are present in the package.
    /// </summary>
    /// <param name="package">The package to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any issues found.</returns>
    Task<ValidationResult> ValidateAsync(TemplatePackage package, CancellationToken cancellationToken = default);
}
