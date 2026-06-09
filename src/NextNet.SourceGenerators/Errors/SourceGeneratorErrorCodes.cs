using Microsoft.CodeAnalysis;

namespace NextNet.SourceGenerators.Errors;
    /// <summary>
    /// Defines DS-250..DS-259 diagnostic descriptors used by the NextNet source generator.
    /// These complement the SKG-prefixed descriptors in <see cref="Utils.DiagnosticDescriptors"/>
    /// and cover generator-level failures distinct from manifest/route issues.
    /// </summary>
    /// <example>
    /// <code>
    /// // Reporting a generator initialization failure:
    /// context.ReportDiagnostic(Diagnostic.Create(
    ///     SourceGeneratorErrorCodes.GeneratorInitializationFailed,
    ///     Location.None,
    ///     "Failed to load assembly metadata"));
    /// </code>
    /// </example>
    internal static class SourceGeneratorErrorCodes
    {
        private const string Category = "NextNet.SourceGenerators";

        /// <summary>
        /// DS-250: The generator failed during initialization.
        /// </summary>
        /// <example>
        /// Reported when <c>RegisterPostInitializationOutput</c> encounters an unexpected condition.
        /// </example>
        public static readonly DiagnosticDescriptor GeneratorInitializationFailed = new DiagnosticDescriptor(
            id: "DS-250",
            title: "Generator initialization failed",
            messageFormat: "The NextNet source generator failed during initialization: {0}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-251: A syntax tree could not be parsed.
        /// </summary>
        /// <example>
        /// Reported when an input file contains malformed C# that prevents route discovery.
        /// </example>
        public static readonly DiagnosticDescriptor SyntaxTreeParseFailed = new DiagnosticDescriptor(
            id: "DS-251",
            title: "Syntax tree parse failed",
            messageFormat: "Could not parse the syntax tree for '{0}': {1}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-252: Symbol resolution failed for a discovered type or member.
        /// </summary>
        /// <example>
        /// Reported when a type referenced in a route manifest cannot be resolved in the compilation.
        /// </example>
        public static readonly DiagnosticDescriptor SymbolResolutionFailed = new DiagnosticDescriptor(
            id: "DS-252",
            title: "Symbol resolution failed",
            messageFormat: "Could not resolve symbol '{0}': {1}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-253: Code emission failed during source output generation.
        /// </summary>
        /// <example>
        /// Reported when a runtime exception occurs inside an emitter (WrapperEmitter, EndpointEmitter, etc.).
        /// </example>
        public static readonly DiagnosticDescriptor CodeEmissionFailed = new DiagnosticDescriptor(
            id: "DS-253",
            title: "Code emission failed",
            messageFormat: "Failed to emit generated source code: {0}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-254: A required attribute was not found on the expected target.
        /// </summary>
        /// <example>
        /// Reported when a class expected to have <c>[PageComponent]</c> is missing the attribute.
        /// </example>
        public static readonly DiagnosticDescriptor AttributeNotFound = new DiagnosticDescriptor(
            id: "DS-254",
            title: "Attribute not found",
            messageFormat: "The attribute '{0}' was not found on '{1}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-255: A marker attribute was applied to an incompatible target.
        /// </summary>
        /// <example>
        /// Reported when <c>[ServerAction]</c> is applied to a property instead of a method or class.
        /// </example>
        public static readonly DiagnosticDescriptor InvalidAttributeTarget = new DiagnosticDescriptor(
            id: "DS-255",
            title: "Invalid attribute target",
            messageFormat: "The attribute '{0}' cannot be applied to '{1}' — expected a {2}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-256: A type has multiple route marker attributes.
        /// </summary>
        /// <example>
        /// Reported when a single class has both <c>[PageComponent]</c> and <c>[ApiRoute]</c>.
        /// </example>
        public static readonly DiagnosticDescriptor MultipleRouteAttributes = new DiagnosticDescriptor(
            id: "DS-256",
            title: "Multiple route attributes",
            messageFormat: "'{0}' has multiple route marker attributes: {1}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-257: A route pattern in the manifest is empty or whitespace.
        /// </summary>
        /// <example>
        /// Reported when a route entry has an empty <c>RoutePattern</c> after deserialization.
        /// </example>
        public static readonly DiagnosticDescriptor RoutePatternEmpty = new DiagnosticDescriptor(
            id: "DS-257",
            title: "Route pattern is empty",
            messageFormat: "Route entry '{0}' has an empty or whitespace route pattern",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-258: The generator failed to emit a source file via <c>AddSource</c>.
        /// </summary>
        /// <example>
        /// Reported when <c>SourceProductionContext.AddSource</c> throws or fails unexpectedly.
        /// </example>
        public static readonly DiagnosticDescriptor SourceOutputFailed = new DiagnosticDescriptor(
            id: "DS-258",
            title: "Source output failed",
            messageFormat: "Failed to emit generated source '{0}': {1}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>
        /// DS-259: An unexpected internal error occurred in the generator.
        /// </summary>
        /// <example>
        /// Catch-all for exceptions not covered by other DS codes.
        /// </example>
        public static readonly DiagnosticDescriptor GeneratorInternalError = new DiagnosticDescriptor(
            id: "DS-259",
            title: "Generator internal error",
            messageFormat: "Unexpected internal error in NextNet source generator: {0}",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
