namespace NextNet.TemplateSdk.Validation.Rules;

/// <summary>
/// Validates that no executable or binary files are included in the template package.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="NoExecutableFilesRule"/> scans all files in the template package and
/// reports an error for any that have a forbidden extension such as <c>.exe</c>,
/// <c>.dll</c>, <c>.so</c>, <c>.dylib</c>, <c>.sh</c>, <c>.bat</c>, <c>.cmd</c>,
/// <c>.ps1</c>, <c>.msi</c>, or <c>.app</c>.
/// </para>
/// <para>
/// Templates should contain source files, not compiled binaries or executables, to
/// ensure portability, security, and maintainability.
/// </para>
/// </remarks>
public sealed class NoExecutableFilesRule : ValidationRule
{
    private static readonly HashSet<string> ForbiddenExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".so", ".dylib", ".sh", ".bat", ".cmd", ".ps1", ".msi", ".app"
    };

    /// <inheritdoc />
    public override string Name => "no-executable-files";

    /// <inheritdoc />
    public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

    /// <inheritdoc />
    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var results = new List<ValidationResult>();
        foreach (var path in context.Files.Keys)
        {
            var ext = System.IO.Path.GetExtension(path);
            if (ForbiddenExtensions.Contains(ext))
            {
                results.Add(new ValidationResult
                {
                    RuleName = Name,
                    Severity = ValidationSeverity.Error,
                    Message = $"Executable file '{path}' is not allowed in templates.",
                    File = path,
                    Suggestion = "Remove the executable. Templates should contain source files, not compiled binaries."
                });
            }
        }
        return results;
    }
}
