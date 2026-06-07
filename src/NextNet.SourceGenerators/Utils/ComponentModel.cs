using System;

namespace NextNet.SourceGenerators.Utils
{
    /// <summary>
    /// Represents metadata about a discovered component class (page, layout, API route).
    /// This is a value-equatable type used in the incremental generator pipeline.
    /// Only string data is stored — no ISymbol or SyntaxNode references (prevents memory leaks).
    /// </summary>
    internal sealed record ComponentModel
    {
        /// <summary>
        /// The fully qualified type name (e.g. <c>global::App.AboutPage</c>).
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// The absolute file path of the source file.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The route pattern from the marker attribute (if set).
        /// </summary>
        public string RoutePattern { get; set; } = string.Empty;

        /// <summary>
        /// The component type: "Page", "Layout", or "Api".
        /// </summary>
        public string ComponentType { get; set; } = string.Empty;
    }
}
