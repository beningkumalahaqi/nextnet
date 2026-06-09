namespace NextNet.UI.Tailwind.Extensions;

/// <summary>
/// Configuration options for the NextNet Tailwind CSS integration.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TailwindOptions"/> controls how the Tailwind integration behaves,
/// including which file paths are scanned for class usage (content paths) and
/// which class patterns are always included in the generated CSS (safelist patterns).
/// These options are typically set during service registration via
/// <see cref="ServiceCollectionExtensions.AddNextNetTailwind"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddNextNetTailwind(options =>
/// {
///     options.ContentPaths = new[] { "./Pages/**/*.cshtml", "./Components/**/*.razor" };
///     options.SafelistPatterns = new[] { "btn-*", "badge-*" };
/// });
/// </code>
/// </example>
public sealed record TailwindOptions
{
    /// <summary>
    /// Gets or sets the glob file paths that Tailwind should scan for CSS class usage.
    /// Defaults to <c>["./**/*.{html,cshtml,razor}"]</c>.
    /// </summary>
    public IReadOnlyList<string> ContentPaths { get; set; } = new[] { "./**/*.{html,cshtml,razor}" };

    /// <summary>
    /// Gets or sets the list of class patterns that Tailwind should always include
    /// in the generated CSS, even if not detected in content files.
    /// Defaults to an empty list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Safelist patterns are useful for dynamically constructed class names that
    /// Tailwind's static analysis cannot detect. Use glob-style patterns such as
    /// <c>"btn-*"</c> or <c>"badge-*"</c> to ensure all variants are available.
    /// </para>
    /// </remarks>
    public IReadOnlyList<string> SafelistPatterns { get; set; } = Array.Empty<string>();
}
