using System.Collections.Generic;

namespace NextNet.SourceGenerators.Models;
    /// <summary>
    /// Root model for the <c>nextnet.routes.json</c> manifest produced by the RouteScanner.
    /// Uses a dedicated <see cref="Utils.RouteManifestModelComparer"/> for pipeline equality.
    /// </summary>
    /// <example>
    /// Typical JSON structure deserialized into this model:
    /// <code>
    /// {
    ///   "Routes": [...],
    ///   "Pages": [{ "RoutePattern": "/", "FilePath": "app/page.cs", "Type": "Page", "SegmentKind": "Static" }],
    ///   "Layouts": [],
    ///   "ApiRoutes": [],
    ///   "ErrorPage": null,
    ///   "Conflicts": []
    /// }
    /// </code>
    /// </example>
    public record RouteManifestModel
    {
        /// <summary>
        /// All discovered route entries.
        /// </summary>
        public List<RouteEntryModel> Routes { get; set; } = new List<RouteEntryModel>();

        /// <summary>
        /// Page entries only.
        /// </summary>
        public List<RouteEntryModel> Pages { get; set; } = new List<RouteEntryModel>();

        /// <summary>
        /// Layout entries only.
        /// </summary>
        public List<RouteEntryModel> Layouts { get; set; } = new List<RouteEntryModel>();

        /// <summary>
        /// API route entries only.
        /// </summary>
        public List<RouteEntryModel> ApiRoutes { get; set; } = new List<RouteEntryModel>();

        /// <summary>
        /// Error page entry, if any.
        /// </summary>
        public RouteEntryModel? ErrorPage { get; set; }

        /// <summary>
        /// Discovered conflicts.
        /// </summary>
        public List<RouteConflictModel> Conflicts { get; set; } = new List<RouteConflictModel>();
    }

    /// <summary>
    /// Represents a single route entry from the manifest.
    /// Value equality compares RoutePattern, FilePath, Type, and SegmentKind (not LayoutChain).
    /// Custom equality is required because the synthesized record equality would compare
    /// <see cref="LayoutChain"/> by reference, which breaks pipeline caching.
    /// </summary>
    /// <example>
    /// <code>
    /// new RouteEntryModel
    /// {
    ///     RoutePattern = "/blog/{slug}",
    ///     FilePath = "app/blog/[slug]/page.cs",
    ///     Type = "Page",
    ///     SegmentKind = "Dynamic",
    ///     HttpMethods = new List&lt;string&gt;()
    /// };
    /// </code>
    /// </example>
    public record RouteEntryModel
    {
        /// <summary>
        /// The route pattern (e.g. "/blog/{slug}").
        /// </summary>
        public string RoutePattern { get; set; } = string.Empty;

        /// <summary>
        /// The relative file path (e.g. "app/blog/[slug]/page.cs").
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The route type: "Page", "Layout", "Api", or "Error".
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The segment kind: "Static", "Dynamic", "CatchAll", or "OptionalCatchAll".
        /// </summary>
        public string SegmentKind { get; set; } = string.Empty;

        /// <summary>
        /// File path of the nearest layout, if any.
        /// </summary>
        public string? LayoutPath { get; set; }

        /// <summary>
        /// The ordered layout chain from nearest to root.
        /// </summary>
        public List<string> LayoutChain { get; set; } = new List<string>();

        /// <summary>
        /// The set of HTTP methods supported by this API route (e.g. "GET", "POST").
        /// Populated for "Api" type entries. When empty, all standard
        /// HTTP methods are assumed available.
        /// </summary>
        public List<string> HttpMethods { get; set; } = new List<string>();

        /// <inheritdoc />
        public virtual bool Equals(RouteEntryModel? other)
        {
            if (other is null)
                return false;

            return string.Equals(RoutePattern, other.RoutePattern, System.StringComparison.Ordinal)
                && string.Equals(FilePath, other.FilePath, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(Type, other.Type, System.StringComparison.Ordinal)
                && string.Equals(SegmentKind, other.SegmentKind, System.StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + System.StringComparer.Ordinal.GetHashCode(RoutePattern ?? string.Empty);
                hash = hash * 31 + System.StringComparer.OrdinalIgnoreCase.GetHashCode(FilePath ?? string.Empty);
                hash = hash * 31 + System.StringComparer.Ordinal.GetHashCode(Type ?? string.Empty);
                hash = hash * 31 + System.StringComparer.Ordinal.GetHashCode(SegmentKind ?? string.Empty);
                return hash;
            }
        }
    }

    /// <summary>
    /// Represents a route conflict from the manifest.
    /// Value equality compares Message, RoutePattern, and Severity (not ConflictingFiles).
    /// Custom equality is required because the synthesized record equality would compare
    /// <see cref="ConflictingFiles"/> by reference, which breaks pipeline caching.
    /// </summary>
    /// <example>
    /// <code>
    /// new RouteConflictModel
    /// {
    ///     Message = "Two pages share the same route pattern '/about'",
    ///     RoutePattern = "/about",
    ///     Severity = "Warning",
    ///     ConflictingFiles = { "app/about/page.cs", "app/about/page2.cs" }
    /// };
    /// </code>
    /// </example>
    public record RouteConflictModel
    {
        /// <summary>
        /// Human-readable conflict description.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The conflicting route pattern.
        /// </summary>
        public string RoutePattern { get; set; } = string.Empty;

        /// <summary>
        /// File paths involved in the conflict.
        /// </summary>
        public List<string> ConflictingFiles { get; set; } = new List<string>();

        /// <summary>
        /// Severity: "Warning" or "Error".
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <inheritdoc />
        public virtual bool Equals(RouteConflictModel? other)
        {
            if (other is null)
                return false;

            return string.Equals(Message, other.Message, System.StringComparison.Ordinal)
                && string.Equals(RoutePattern, other.RoutePattern, System.StringComparison.Ordinal)
                && string.Equals(Severity, other.Severity, System.StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + System.StringComparer.Ordinal.GetHashCode(Message ?? string.Empty);
                hash = hash * 31 + System.StringComparer.Ordinal.GetHashCode(RoutePattern ?? string.Empty);
                hash = hash * 31 + System.StringComparer.Ordinal.GetHashCode(Severity ?? string.Empty);
                return hash;
            }
        }
    }
