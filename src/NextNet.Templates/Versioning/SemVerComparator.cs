namespace NextNet.Templates.Versioning;

/// <summary>
/// Comparator for SemVer pre-release and build metadata ordering.
/// </summary>
/// <remarks>
/// <para>
/// Implements the SemVer 2.0 §11 precedence rules for pre-release identifiers:
/// </para>
/// <list type="number">
///   <item>Identifiers are compared dot-segment by dot-segment.</item>
///   <item>Numeric identifiers are compared numerically.</item>
///   <item>Alphanumeric identifiers are compared lexically in ordinal sort order.</item>
///   <item>Numeric identifiers always have lower precedence than alphanumeric identifiers.</item>
///   <item>A larger set of pre-release fields has higher precedence than a smaller set, if all prior fields are equal.</item>
/// </list>
/// <para>
/// Build metadata is ignored for precedence per SemVer 2.0 §10.
/// </para>
/// <example>
/// <code>
/// int result = SemVerComparator.ComparePreRelease("alpha.1", "beta");
/// // result is negative because "alpha" &lt; "beta" lexically
/// </code>
/// </example>
/// </remarks>
public static class SemVerComparator
{
    /// <summary>
    /// Compares two pre-release identifiers per SemVer 2.0 §11.
    /// </summary>
    /// <param name="left">The left pre-release identifier (e.g., "alpha.1").</param>
    /// <param name="right">The right pre-release identifier (e.g., "beta").</param>
    /// <returns>
    /// Negative if <paramref name="left"/> has lower precedence,
    /// positive if higher, 0 if equal.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="left"/> or <paramref name="right"/> is <c>null</c>.</exception>
    public static int ComparePreRelease(string left, string right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftIds = left.Split('.');
        var rightIds = right.Split('.');
        var minLen = Math.Min(leftIds.Length, rightIds.Length);

        for (int i = 0; i < minLen; i++)
        {
            var l = leftIds[i];
            var r = rightIds[i];
            var lIsNum = int.TryParse(l, out var lNum);
            var rIsNum = int.TryParse(r, out var rNum);

            if (lIsNum && rIsNum)
            {
                // Numeric comparison
                if (lNum != rNum) return lNum.CompareTo(rNum);
            }
            else if (lIsNum)
            {
                // Numeric identifiers always have lower precedence than alphanumeric
                return -1;
            }
            else if (rIsNum)
            {
                return 1;
            }
            else
            {
                // Lexical comparison (ordinal)
                var cmp = string.Compare(l, r, StringComparison.Ordinal);
                if (cmp != 0) return cmp;
            }
        }

        // A longer set of pre-release fields has higher precedence if all prior fields are equal
        return leftIds.Length.CompareTo(rightIds.Length);
    }
}
