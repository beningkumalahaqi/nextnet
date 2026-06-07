using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

namespace NextNet.Templates.Abstractions;

/// <summary>
/// Defines the contract for the template generation engine that processes template
/// packages and produces output files.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ITemplateEngine"/> is the core orchestrator of the template system.
/// It takes a resolved <see cref="TemplatePackage"/> together with user-supplied
/// <see cref="IVariableContext"/> values and generates the final output files in the
/// specified directory.
/// </para>
/// <para>
/// The engine is also responsible for validating the template manifest and variable
/// context before generation begins, ensuring that required variables are provided,
/// feature dependencies are satisfied, and no conflicts exist.
/// </para>
/// <example>
/// <code>
/// var engine = new MyTemplateEngine();
/// var result = await engine.GenerateAsync(
///     package,
///     variables,
///     "./output",
///     CancellationToken.None);
///
/// foreach (var file in result.GeneratedFiles)
///     Console.WriteLine($"Generated: {file}");
/// </code>
/// </example>
/// </remarks>
public interface ITemplateEngine
{
    /// <summary>
    /// Generates output files from the provided template package and variable context,
    /// writing them to the specified output directory.
    /// </summary>
    /// <param name="package">The resolved template package containing manifest and files.</param>
    /// <param name="variables">The variable values to use during generation.</param>
    /// <param name="outputDirectory">The root directory where generated files will be written.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="GenerationResult"/> describing the outcome of generation.</returns>
    /// <exception cref="TemplateValidationException">Thrown when the manifest or variables fail validation.</exception>
    Task<GenerationResult> GenerateAsync(TemplatePackage package, IVariableContext variables, string outputDirectory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the given manifest and variable context are compatible and complete
    /// without performing actual file generation.
    /// </summary>
    /// <param name="manifest">The template manifest to validate.</param>
    /// <param name="variables">The variable values to validate against the manifest.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any validation issues found.</returns>
    Task<ValidationResult> ValidateAsync(TemplateManifest manifest, IVariableContext variables, CancellationToken cancellationToken = default);
}
