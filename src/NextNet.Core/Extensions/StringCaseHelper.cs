namespace NextNet.Core.Extensions;

/// <summary>
/// Static helper for converting strings between naming conventions.
/// </summary>
public static class StringCaseHelper
{
    /// <summary>
    /// Converts a kebab-case or underscore-separated name to PascalCase.
    /// </summary>
    /// <param name="name">The input string (e.g., "my-blog", "hello_world").</param>
    /// <returns>PascalCase string, or "App" if input is null/empty.</returns>
    public static string ToPascalCase(string? name)
    {
        if (string.IsNullOrEmpty(name)) return "App";
        return string.Concat(name.Split('-', '_').Select(s =>
            s.Length > 0 ? char.ToUpperInvariant(s[0]) + s[1..] : ""));
    }
}
