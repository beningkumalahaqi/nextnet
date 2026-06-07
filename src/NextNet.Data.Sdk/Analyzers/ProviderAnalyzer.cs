using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NextNet.Data.Sdk.Analyzers;

/// <summary>
/// Roslyn <see cref="DiagnosticAnalyzer"/> that validates NextNet Data Provider SDK conventions.
/// </summary>
/// <remarks>
/// <para>
/// This analyzer enforces three rules:
/// <list type="bullet">
///   <item><description>SKDataSdk001: DataProvider classes must be sealed or have [DataProvider] attribute.</description></item>
///   <item><description>SKDataSdk002: Repository&lt;T&gt; must have [DataRepository&lt;T&gt;] attribute.</description></item>
///   <item><description>SKDataSdk003: MigrationEngine must be in namespace ending with "Migrations".</description></item>
/// </list>
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProviderAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticDescriptors.SKDataSdk001,
            DiagnosticDescriptors.SKDataSdk002,
            DiagnosticDescriptors.SKDataSdk003);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (namedType.TypeKind != TypeKind.Class)
            return;

        // SKDataSdk001: IDataProvider implementors must be sealed or have [DataProvider]
        if (ImplementsInterface(namedType, "NextNet.Data.Abstractions.Abstractions.IDataProvider"))
        {
            var hasAttribute = namedType.HasAttribute("NextNet.Data.Sdk.DataProviderAttribute");
            if (!namedType.IsSealed && !hasAttribute)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.SKDataSdk001,
                    namedType.Locations[0],
                    namedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        // SKDataSdk002: IRepository<T> implementors must have [DataRepository<T>]
        if (ImplementsGenericInterface(namedType, "NextNet.Data.Abstractions.Abstractions.IRepository"))
        {
            var hasAttribute = namedType.HasAttribute("NextNet.Data.Sdk.DataRepositoryAttribute");
            if (!hasAttribute)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.SKDataSdk002,
                    namedType.Locations[0],
                    namedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        // SKDataSdk003: MigrationEngine must be in namespace ending with "Migrations"
        if (ImplementsInterface(namedType, "NextNet.Data.Abstractions.Abstractions.IMigrationEngine")
            || namedType.HasAttribute("NextNet.Data.Sdk.DataMigrationEngineAttribute"))
        {
            var ns = namedType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            if (!ns.EndsWith("Migrations", System.StringComparison.Ordinal))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.SKDataSdk003,
                    namedType.Locations[0],
                    namedType.Name,
                    ns);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceFullName)
    {
        if (type == null)
            return false;

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.ToDisplayString() == interfaceFullName)
                return true;
        }

        // Also check the type itself (for base type chains)
        var baseType = type.BaseType;
        while (baseType != null)
        {
            foreach (var iface in baseType.AllInterfaces)
            {
                if (iface.ToDisplayString() == interfaceFullName)
                    return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }

    private static bool ImplementsGenericInterface(INamedTypeSymbol type, string interfaceFullName)
    {
        if (type == null)
            return false;

        foreach (var iface in type.AllInterfaces)
        {
            var displayName = iface.OriginalDefinition?.ToDisplayString() ?? iface.ToDisplayString();
            if (displayName == interfaceFullName || displayName.StartsWith(interfaceFullName + "<", System.StringComparison.Ordinal))
                return true;
        }

        var baseType = type.BaseType;
        while (baseType != null)
        {
            foreach (var iface in baseType.AllInterfaces)
            {
                var displayName = iface.OriginalDefinition?.ToDisplayString() ?? iface.ToDisplayString();
                if (displayName == interfaceFullName || displayName.StartsWith(interfaceFullName + "<", System.StringComparison.Ordinal))
                    return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }
}

/// <summary>
/// Extension methods for Roslyn symbol operations used by SDK analyzers.
/// </summary>
internal static class SymbolExtensions
{
    /// <summary>
    /// Determines whether the given named type symbol has an attribute matching the specified full name.
    /// </summary>
    /// <param name="type">The named type symbol to inspect.</param>
    /// <param name="attributeFullName">The full metadata name of the attribute (e.g., "NextNet.Data.Sdk.DataProviderAttribute").</param>
    /// <returns><c>true</c> if the type has the attribute; otherwise <c>false</c>.</returns>
    public static bool HasAttribute(this INamedTypeSymbol type, string attributeFullName)
    {
        foreach (var attr in type.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null)
                continue;

            // Check the attribute's full name, handling generic arity
            var displayName = attrClass.ToDisplayString();
            if (displayName == attributeFullName)
                return true;

            // For generic attributes like DataRepositoryAttribute<T>, the compiler
            // emits DataRepositoryAttribute`1. Check both the non-generic name and
            // the constructed generic name.
            var constructedName = attrClass.OriginalDefinition?.ToDisplayString();
            if (constructedName == attributeFullName)
                return true;

            // Also try matching just the class name portion
            var attrName = attrClass.Name;
            var expectedName = attributeFullName.Contains('.')
                ? attributeFullName.Substring(attributeFullName.LastIndexOf('.') + 1)
                : attributeFullName;

            if (attrName == expectedName || attrName == expectedName + "`1")
                return true;
        }

        return false;
    }
}
