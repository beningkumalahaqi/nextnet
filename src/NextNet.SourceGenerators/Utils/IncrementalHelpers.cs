using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace NextNet.SourceGenerators.Utils
{
    /// <summary>
    /// Helper utilities for the incremental generator pipeline.
    /// </summary>
    internal static class IncrementalHelpers
    {
        /// <summary>
        /// Converts a relative route file path to a valid C# type name.
        /// File path segments are PascalCased; special directory names like <c>[slug]</c>
        /// are converted to friendly names like <c>Slug</c>.
        /// </summary>
        /// <param name="filePath">The route file path (e.g. <c>app/about/page.cs</c>).</param>
        /// <param name="appDir">The application directory prefix to strip (e.g. <c>app/</c>).</param>
        /// <returns>A valid C# type name (e.g. <c>AboutPage</c>).</returns>
        public static string FilePathToTypeName(string filePath, string appDir)
        {
            if (string.IsNullOrEmpty(filePath))
                return "Unknown";

            // Normalize separators
            var normalized = filePath.Replace('\\', '/');
            var app = string.IsNullOrEmpty(appDir)
                ? string.Empty
                : appDir.Replace('\\', '/').TrimEnd('/') + "/";

            // Strip the app directory prefix
            var relative = normalized;
            if (!string.IsNullOrEmpty(app) && normalized.StartsWith(app, StringComparison.OrdinalIgnoreCase))
            {
                relative = normalized.Substring(app.Length);
            }

            // Remove the file extension (.cs)
            if (relative.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                relative = relative.Substring(0, relative.Length - 3);
            }

            // Split into segments and convert each to PascalCase
            var segments = relative.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return "IndexPage";

            // The last segment is the file name (e.g. "page", "layout", "route", "error")
            // Build type name from segments
            var parts = new List<string>();
            foreach (var segment in segments)
            {
                var cleaned = SanitizeSegment(segment);
                if (!string.IsNullOrEmpty(cleaned))
                {
                    parts.Add(cleaned);
                }
            }

            if (parts.Count == 0)
                return "UnknownPage";

            // Determine suffix based on file type
            var fileName = segments[segments.Length - 1].ToLowerInvariant();
            var baseName = string.Concat(parts);

            // Special case: if there's only one segment (just the filename, no directory),
            // use friendlier names for root items
            if (segments.Length == 1)
            {
                return fileName switch
                {
                    "page" => "IndexPage",
                    "layout" => "RootLayout",
                    "route" => "RootRoute",
                    "error" => "ErrorPage",
                    _ => baseName
                };
            }

            return fileName switch
            {
                "page" => EnsureSuffix(baseName, "Page"),
                "layout" => EnsureSuffix(baseName, "Layout"),
                "route" => EnsureSuffix(baseName, "Route"),
                "error" => EnsureSuffix(baseName, "Error"),
                _ => baseName
            };
        }

        /// <summary>
        /// Sanitizes a path segment into a PascalCase identifier.
        /// Handles bracket notation: <c>[slug]</c> → <c>Slug</c>, <c>[...path]</c> → <c>Path</c>, etc.
        /// </summary>
        private static string SanitizeSegment(string segment)
        {
            if (string.IsNullOrEmpty(segment))
                return string.Empty;

            // Remove bracket notation: [[...name]] or [...name] or [name]
            var cleaned = Regex.Replace(segment, @"\[{1,2}\.{0,3}(\w+)\]{1,2}", "$1");

            // Remove any remaining non-alphanumeric characters
            cleaned = Regex.Replace(cleaned, @"[^a-zA-Z0-9]", " ");

            // Split by whitespace, capitalize each word
            var words = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0], CultureInfo.InvariantCulture) +
                               words[i].Substring(1).ToLowerInvariant();
                }
            }

            var result = string.Concat(words);
            return result.Length > 0 ? result : "Segment";
        }

        /// <summary>
        /// Ensures a type name ends with the specified suffix, deduplicating if already present.
        /// </summary>
        private static string EnsureSuffix(string name, string suffix)
        {
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return name;
            return name + suffix;
        }

        /// <summary>
        /// Extracts route parameter names from a route pattern.
        /// E.g. <c>/blog/{slug}</c> → <c>["slug"]</c>.
        /// </summary>
        public static List<string> ExtractRouteParameters(string routePattern)
        {
            var parameters = new List<string>();
            if (string.IsNullOrEmpty(routePattern))
                return parameters;

            var matches = Regex.Matches(routePattern, @"\{(\*?)(\w+)\}");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var name = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(name))
                {
                    parameters.Add(name);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Determines if a route pattern contains a catch-all parameter (e.g. <c>{*path}</c>).
        /// </summary>
        public static bool HasCatchAll(string routePattern)
        {
            return !string.IsNullOrEmpty(routePattern) &&
                   Regex.IsMatch(routePattern, @"\{\*\w+\}");
        }

        /// <summary>
        /// Determines if a route pattern contains dynamic parameters.
        /// </summary>
        public static bool HasDynamicParams(string routePattern)
        {
            return !string.IsNullOrEmpty(routePattern) &&
                   Regex.IsMatch(routePattern, @"\{(\*?)(\w+)\}");
        }

        /// <summary>
        /// Generates a wrapper class name from a route entry.
        /// Uses the file path to derive a unique, readable type name (e.g. <c>NextNet_AboutPage</c>).
        /// Falls back to <c>NextNet_Route</c> if the path cannot be resolved.
        /// </summary>
        /// <param name="entry">The route entry.</param>
        /// <param name="appDir">The application directory prefix to strip.</param>
        /// <returns>A valid C# type name for the wrapper class.</returns>
        public static string GetWrapperName(Models.RouteEntryModel entry, string appDir)
        {
            var baseName = FilePathToTypeName(entry.FilePath, appDir);
            return string.IsNullOrEmpty(baseName) || baseName == "Unknown"
                ? "NextNet_Route"
                : "NextNet_" + baseName;
        }
    }
}
