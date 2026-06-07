namespace NextNet.Templates.Versioning;

/// <summary>
/// Convenience wrapper for working with template version range expressions.
/// </summary>
/// <remarks>
/// <para>
/// Provides a simple API for checking whether a <see cref="TemplateVersion"/> falls within
/// a version range string. Supports exact versions and comparison operators
/// (<c>&gt;=</c>, <c>&lt;=</c>, <c>&gt;</c>, <c>&lt;</c>).
/// </para>
/// <para>
/// For more complex range expressions (caret, tilde, AND/OR combinations), use
/// <see cref="TemplateVersionResolver"/> instead.
/// </para>
/// <example>
/// <code>
/// var range = new TemplateVersionRange(">=1.0.0");
/// var version = TemplateVersion.Parse("2.0.0");
/// bool contained = range.Contains(version); // true
/// </code>
/// </example>
/// </remarks>
/// <param name="Expression">The version range expression (e.g., <c>"&gt;=1.0.0"</c>, <c>"1.2.3"</c>).</param>
public sealed record TemplateVersionRange(string Expression)
{
    /// <summary>
    /// Determines whether the specified version satisfies this range expression.
    /// </summary>
    /// <param name="version">The version to check.</param>
    /// <returns><c>true</c> if the version is within the range; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="version"/> is <c>null</c>.</exception>
    public bool Contains(TemplateVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        // Exact version (with optional leading "=")
        if (TemplateVersion.TryParse(Expression.TrimStart('='), out var exact))
            return version == exact;

        // >= expression
        if (Expression.StartsWith(">=") && TemplateVersion.TryParse(Expression[2..], out var min))
            return version >= min;

        // <= expression
        if (Expression.StartsWith("<=") && TemplateVersion.TryParse(Expression[2..], out var max))
            return version <= max;

        // > expression
        if (Expression.StartsWith(">") && TemplateVersion.TryParse(Expression[1..], out var gt))
            return version > gt;

        // < expression
        if (Expression.StartsWith("<") && TemplateVersion.TryParse(Expression[1..], out var lt))
            return version < lt;

        return false;
    }
}
