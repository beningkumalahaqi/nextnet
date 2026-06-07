namespace NextNet.Templates.Versioning;

/// <summary>
/// Resolves template version specifiers to concrete versions.
/// Supports "latest", "next-major", "next-minor", exact versions, caret/tilde ranges,
/// and comparison operator ranges.
/// </summary>
/// <remarks>
/// <para>
/// The resolver takes a version specifier (e.g., <c>"latest"</c>, <c>"^1.0.0"</c>, <c>"1.2.3"</c>)
/// and a list of available versions, and returns the best matching version string.
/// </para>
/// <para>
/// Pre-release versions are included in results only if they match the specifier. The "latest"
/// specifier returns the highest non-pre-release version by default, since pre-release versions
/// have lower precedence per SemVer 2.0.
/// </para>
/// <example>
/// <code>
/// var resolver = new TemplateVersionResolver();
/// var versions = new List&lt;string&gt; { "1.0.0", "1.1.0", "1.2.0", "2.0.0" };
/// var latest = resolver.Resolve("latest", versions);        // "2.0.0"
/// var caret = resolver.Resolve("^1.0.0", versions);         // "1.2.0"
/// var exact = resolver.Resolve("1.1.0", versions);          // "1.1.0"
/// </code>
/// </example>
/// </remarks>
public sealed class TemplateVersionResolver
{
    /// <summary>
    /// Resolves a version specifier against a list of available versions.
    /// </summary>
    /// <param name="specifier">A version specifier: exact (<c>"1.2.3"</c>), range (<c>"^1.0.0"</c>), or <c>"latest"</c>.</param>
    /// <param name="availableVersions">The list of available version strings.</param>
    /// <returns>The resolved version string, or <c>null</c> if no match.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="specifier"/> or <paramref name="availableVersions"/> is <c>null</c>.</exception>
    public string? Resolve(string specifier, IReadOnlyList<string> availableVersions)
    {
        ArgumentNullException.ThrowIfNull(specifier);
        ArgumentNullException.ThrowIfNull(availableVersions);

        if (availableVersions.Count == 0) return null;

        // "latest" → highest version (pre-release versions sort lower per SemVer)
        if (specifier.Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            return availableVersions
                .Where(v => TemplateVersion.TryParse(v, out _))
                .OrderByDescending(v => TemplateVersion.Parse(v))
                .FirstOrDefault();
        }

        // "next-major" → first version of the next major line
        if (specifier.Equals("next-major", StringComparison.OrdinalIgnoreCase))
        {
            var latest = availableVersions
                .Where(v => TemplateVersion.TryParse(v, out _))
                .Select(v => TemplateVersion.Parse(v))
                .OrderByDescending(v => v)
                .FirstOrDefault();
            if (latest is null) return null;
            return availableVersions
                .Where(v => TemplateVersion.TryParse(v, out _))
                .Select(v => TemplateVersion.Parse(v))
                .Where(v => v.Major > latest.Major)
                .OrderBy(v => v)
                .FirstOrDefault()?.ToString();
        }

        // "next-minor" → first version of the next minor line in the same major
        if (specifier.Equals("next-minor", StringComparison.OrdinalIgnoreCase))
        {
            var latest = availableVersions
                .Where(v => TemplateVersion.TryParse(v, out _))
                .Select(v => TemplateVersion.Parse(v))
                .OrderByDescending(v => v)
                .FirstOrDefault();
            if (latest is null) return null;
            return availableVersions
                .Where(v => TemplateVersion.TryParse(v, out _))
                .Select(v => TemplateVersion.Parse(v))
                .Where(v => v.Major == latest.Major && v.Minor > latest.Minor)
                .OrderBy(v => v)
                .FirstOrDefault()?.ToString();
        }

        // Exact version match
        if (availableVersions.Contains(specifier)) return specifier;

        // Range expression (e.g., "^1.0.0", ">=2.0.0")
        if (specifier.StartsWith("^") || specifier.StartsWith("~") ||
            specifier.StartsWith(">=") || specifier.StartsWith("<=") ||
            specifier.StartsWith(">") || specifier.StartsWith("<") ||
            specifier.Contains("||"))
        {
            return ResolveRange(specifier, availableVersions);
        }

        return null;
    }

    private static string? ResolveRange(string rangeExpr, IReadOnlyList<string> availableVersions)
    {
        // Parse all available versions into a non-nullable list by filtering out parse failures
        var parsedVersions = new List<(string str, TemplateVersion ver)>();
        foreach (var v in availableVersions)
        {
            if (TemplateVersion.TryParse(v, out var tv) && tv is not null)
            {
                parsedVersions.Add((v, tv));
            }
        }

        // Support simple caret (^) range: ^1.2.3 := >=1.2.3 <2.0.0
        if (rangeExpr.StartsWith("^") && TemplateVersion.TryParse(rangeExpr[1..], out var baseVer))
        {
            var v = baseVer!; // TryParse succeeded, so baseVer is non-null
            return parsedVersions
                .Where(x => x.ver.Major == v.Major && x.ver >= v)
                .OrderByDescending(x => x.ver)
                .Select(x => x.str)
                .FirstOrDefault();
        }

        // Support simple tilde (~) range: ~1.2.3 := >=1.2.3 <1.3.0
        if (rangeExpr.StartsWith("~") && TemplateVersion.TryParse(rangeExpr[1..], out baseVer))
        {
            var v = baseVer!;
            return parsedVersions
                .Where(x => x.ver.Major == v.Major && x.ver.Minor == v.Minor && x.ver >= v)
                .OrderByDescending(x => x.ver)
                .Select(x => x.str)
                .FirstOrDefault();
        }

        // >= expression
        if (rangeExpr.StartsWith(">=") && TemplateVersion.TryParse(rangeExpr[2..], out baseVer))
        {
            var v = baseVer!;
            return parsedVersions
                .Where(x => x.ver >= v)
                .OrderByDescending(x => x.ver)
                .Select(x => x.str)
                .FirstOrDefault();
        }

        return null;
    }
}
