using System.Text.RegularExpressions;

namespace NextNet.Templates.Versioning;

/// <summary>
/// Strongly-typed SemVer 2.0 version representation.
/// Distinct from <see cref="System.Version"/> which doesn't support pre-release/build metadata.
/// </summary>
/// <remarks>
/// <para>
/// Format: <c>MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]</c> per semver.org.
/// </para>
/// <para>
/// Examples: <c>1.0.0</c>, <c>2.1.3-alpha.1</c>, <c>1.0.0+build.42</c>
/// </para>
/// </remarks>
public sealed class TemplateVersion : IComparable<TemplateVersion>, IEquatable<TemplateVersion>
{
    private static readonly Regex SemVerRegex = new(
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<pre>[a-zA-Z0-9.-]+))?(?:\+(?<build>[a-zA-Z0-9.-]+))?$",
        RegexOptions.Compiled);

    /// <summary>
    /// Gets the major version number.
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// Gets the minor version number.
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// Gets the patch version number.
    /// </summary>
    public int Patch { get; }

    /// <summary>
    /// Gets the pre-release identifier (e.g., "alpha.1"), or <c>null</c> if none.
    /// </summary>
    public string? PreRelease { get; }

    /// <summary>
    /// Gets the build metadata (e.g., "build.42"), or <c>null</c> if none.
    /// </summary>
    public string? BuildMetadata { get; }

    /// <summary>
    /// Gets a value indicating whether this is a pre-release version.
    /// </summary>
    public bool IsPreRelease => !string.IsNullOrEmpty(PreRelease);

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateVersion"/> class.
    /// </summary>
    /// <param name="major">The major version number (must be non-negative).</param>
    /// <param name="minor">The minor version number (must be non-negative).</param>
    /// <param name="patch">The patch version number (must be non-negative).</param>
    /// <param name="preRelease">Optional pre-release identifier.</param>
    /// <param name="buildMetadata">Optional build metadata.</param>
    /// <exception cref="ArgumentException">Thrown when any version component is negative.</exception>
    public TemplateVersion(int major, int minor, int patch, string? preRelease = null, string? buildMetadata = null)
    {
        if (major < 0 || minor < 0 || patch < 0)
            throw new ArgumentException("Version components must be non-negative.");
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        BuildMetadata = buildMetadata;
    }

    /// <summary>
    /// Parses a SemVer 2.0 version string into a <see cref="TemplateVersion"/>.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
    /// <returns>A <see cref="TemplateVersion"/> representing the parsed version.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid SemVer version.</exception>
    /// <example>
    /// <code>
    /// var v = TemplateVersion.Parse("2.1.3-alpha.1");
    /// Console.WriteLine(v.Major); // 2
    /// Console.WriteLine(v.PreRelease); // "alpha.1"
    /// </code>
    /// </example>
    public static TemplateVersion Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new FormatException("Version string cannot be null or empty.");

        var match = SemVerRegex.Match(version.Trim());
        if (!match.Success)
            throw new FormatException($"Invalid SemVer version: '{version}'");

        return new TemplateVersion(
            int.Parse(match.Groups["major"].Value),
            int.Parse(match.Groups["minor"].Value),
            int.Parse(match.Groups["patch"].Value),
            string.IsNullOrEmpty(match.Groups["pre"].Value) ? null : match.Groups["pre"].Value,
            string.IsNullOrEmpty(match.Groups["build"].Value) ? null : match.Groups["build"].Value
        );
    }

    /// <summary>
    /// Attempts to parse a SemVer 2.0 version string without throwing.
    /// </summary>
    /// <param name="version">The version string to parse.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="TemplateVersion"/> or <c>null</c>.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? version, out TemplateVersion? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(version)) return false;
        try
        {
            result = Parse(version);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public int CompareTo(TemplateVersion? other)
    {
        if (other is null) return 1;
        var c = Major.CompareTo(other.Major);
        if (c != 0) return c;
        c = Minor.CompareTo(other.Minor);
        if (c != 0) return c;
        c = Patch.CompareTo(other.Patch);
        if (c != 0) return c;

        // Per SemVer 2.0 §11: a pre-release version has lower precedence than a normal version
        // Example: 1.0.0-alpha < 1.0.0
        if (IsPreRelease && !other.IsPreRelease) return -1;
        if (!IsPreRelease && other.IsPreRelease) return 1;
        if (IsPreRelease && other.IsPreRelease)
            return SemVerComparator.ComparePreRelease(PreRelease!, other.PreRelease!);
        return 0;
    }

    /// <inheritdoc />
    public bool Equals(TemplateVersion? other) => other is not null && CompareTo(other) == 0;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as TemplateVersion);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease ?? "");

    /// <summary>
    /// Returns the SemVer 2.0 string representation of this version.
    /// </summary>
    /// <returns>A string like <c>"1.2.3-alpha.1+build.42"</c>.</returns>
    public override string ToString()
    {
        var s = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(PreRelease)) s += $"-{PreRelease}";
        if (!string.IsNullOrEmpty(BuildMetadata)) s += $"+{BuildMetadata}";
        return s;
    }

    /// <summary>Determines whether one specified <see cref="TemplateVersion"/> is less than another.</summary>
    public static bool operator <(TemplateVersion? a, TemplateVersion? b) => a is not null && b is not null && a.CompareTo(b) < 0;

    /// <summary>Determines whether one specified <see cref="TemplateVersion"/> is greater than another.</summary>
    public static bool operator >(TemplateVersion? a, TemplateVersion? b) => a is not null && b is not null && a.CompareTo(b) > 0;

    /// <summary>Determines whether one specified <see cref="TemplateVersion"/> is less than or equal to another.</summary>
    public static bool operator <=(TemplateVersion? a, TemplateVersion? b) => a is not null && b is not null && a.CompareTo(b) <= 0;

    /// <summary>Determines whether one specified <see cref="TemplateVersion"/> is greater than or equal to another.</summary>
    public static bool operator >=(TemplateVersion? a, TemplateVersion? b) => a is not null && b is not null && a.CompareTo(b) >= 0;

    /// <summary>Determines whether two specified <see cref="TemplateVersion"/> instances are equal.</summary>
    public static bool operator ==(TemplateVersion? a, TemplateVersion? b) => a?.Equals(b) ?? b is null;

    /// <summary>Determines whether two specified <see cref="TemplateVersion"/> instances are not equal.</summary>
    public static bool operator !=(TemplateVersion? a, TemplateVersion? b) => !(a == b);
}
