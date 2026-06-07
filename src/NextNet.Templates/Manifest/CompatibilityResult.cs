namespace NextNet.Templates.Manifest;

/// <summary>
/// Describes the result of a version compatibility check between a template manifest
/// and a specific NextNet SDK version.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="CompatibilityResult"/> is produced by
/// <see cref="VersionCompatibilityChecker.IsCompatible"/> and indicates whether the
/// template's <c>NextNetVersion</c> constraint is satisfied by the given SDK version.
/// </para>
/// <para>
/// When <see cref="IsCompatible"/> is <c>false</c>, the <see cref="Message"/> property
/// provides a human-readable explanation of why the versions are incompatible.
/// </para>
/// <example>
/// <code>
/// var result = checker.IsCompatible(manifest, new Version(4, 0, 0));
/// if (!result.IsCompatible)
///     Console.WriteLine(result.Message);
/// </code>
/// </example>
/// </remarks>
/// <param name="IsCompatible"><c>true</c> if the SDK version satisfies the template's constraint; otherwise <c>false</c>.</param>
/// <param name="TemplateVersion">The version of the template manifest.</param>
/// <param name="Constraint">The NextNet version constraint string from the manifest.</param>
/// <param name="SdkVersion">The SDK version that was checked.</param>
/// <param name="Message">An optional human-readable message describing the result.</param>
public sealed record CompatibilityResult(
    bool IsCompatible,
    string TemplateVersion,
    string Constraint,
    Version SdkVersion,
    string? Message = null
);
