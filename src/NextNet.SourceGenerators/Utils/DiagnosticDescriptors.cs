using Microsoft.CodeAnalysis;

namespace NextNet.SourceGenerators.Utils;
    /// <summary>
    /// Defines all diagnostic descriptors used by the NextNet source generator.
    /// Never throw — use <see cref="GeneratorExecutionContext.ReportDiagnostic"/> instead.
    /// </summary>
    /// <example>
    /// <code>
    /// // Reporting a diagnostic from within RegisterSourceOutput:
    /// spc.ReportDiagnostic(Diagnostic.Create(
    ///     DiagnosticDescriptors.ManifestNotFound, Location.None));
    /// </code>
    /// </example>
    internal static class DiagnosticDescriptors
    {
        private const string Category = "NextNet.SourceGenerators";

        /// <summary>
        /// SKG001: Could not find or parse routes.json manifest.
        /// </summary>
        public static readonly DiagnosticDescriptor ManifestNotFound = new DiagnosticDescriptor(
            id: "SKG001",
            title: "Route manifest not found",
            messageFormat: "Could not find or parse route manifest 'nextnet.routes.json'. " +
                           "Ensure the pre-build step has generated the manifest file.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// SKG002: Route manifest is empty (no routes discovered).
        /// </summary>
        public static readonly DiagnosticDescriptor ManifestEmpty = new DiagnosticDescriptor(
            id: "SKG002",
            title: "Route manifest is empty",
            messageFormat: "The route manifest contains no route entries. " +
                           "Verify that route files exist in the application directory.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        /// <summary>
        /// SKG003: Invalid route entry in manifest.
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidRouteEntry = new DiagnosticDescriptor(
            id: "SKG003",
            title: "Invalid route entry",
            messageFormat: "Route entry '{0}' has invalid or missing data: {1}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// SKG004: Route conflict detected.
        /// </summary>
        public static readonly DiagnosticDescriptor RouteConflict = new DiagnosticDescriptor(
            id: "SKG004",
            title: "Route conflict",
            messageFormat: "Route conflict: {0} ({1})",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// SKG005: Could not determine type name from file path.
        /// </summary>
        public static readonly DiagnosticDescriptor UnresolvedType = new DiagnosticDescriptor(
            id: "SKG005",
            title: "Could not resolve type name",
            messageFormat: "Could not resolve a valid type name for file path '{0}'. " +
                           "The generated code will use a fallback name.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// SKG006: Internal generator error (should not happen — indicates a bug).
        /// </summary>
        public static readonly DiagnosticDescriptor InternalError = new DiagnosticDescriptor(
            id: "SKG006",
            title: "Internal generator error",
            messageFormat: "NextNet source generator encountered an internal error: {0}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
