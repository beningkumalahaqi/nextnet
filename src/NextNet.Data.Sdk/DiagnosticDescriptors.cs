using Microsoft.CodeAnalysis;

namespace NextNet.Data.Sdk;

/// <summary>
/// Defines all <see cref="DiagnosticDescriptor"/> instances for NextNet Data SDK analyzers.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "NextNet.Data.Sdk.Design";
    private const string HelpLinkBase = "https://nextnet.dev/docs/data/providers/sdk/errors/";

    /// <summary>
    /// SKDataSdk001: DataProvider classes must be sealed or correctly attributed.
    /// </summary>
    public static readonly DiagnosticDescriptor SKDataSdk001 = new DiagnosticDescriptor(
        id: "SKDataSdk001",
        title: "DataProvider class must be sealed or have [DataProvider] attribute",
        messageFormat: "Class '{0}' implements IDataProvider but is not sealed and is missing the [DataProvider] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes implementing IDataProvider should either be sealed or annotated with the [DataProvider] attribute for proper SDK tooling support.",
        helpLinkUri: $"{HelpLinkBase}SKDataSdk001");

    /// <summary>
    /// SKDataSdk002: Repository&lt;T&gt; must have [DataRepository&lt;T&gt;] attribute.
    /// </summary>
    public static readonly DiagnosticDescriptor SKDataSdk002 = new DiagnosticDescriptor(
        id: "SKDataSdk002",
        title: "Repository<T> must have [DataRepository<T>] attribute",
        messageFormat: "Class '{0}' implements IRepository<T> but is missing the [DataRepository<T>] attribute",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Repository classes implementing IRepository<T> should be annotated with the [DataRepository<T>] attribute for proper SDK tooling support.",
        helpLinkUri: $"{HelpLinkBase}SKDataSdk002");

    /// <summary>
    /// SKDataSdk003: MigrationEngine must be in namespace ending with "Migrations".
    /// </summary>
    public static readonly DiagnosticDescriptor SKDataSdk003 = new DiagnosticDescriptor(
        id: "SKDataSdk003",
        title: "MigrationEngine must be in namespace ending with 'Migrations'",
        messageFormat: "Migration engine class '{0}' is in namespace '{1}' which does not end with 'Migrations'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Migration engine classes should be placed in a namespace that ends with 'Migrations' for consistency and discoverability.",
        helpLinkUri: $"{HelpLinkBase}SKDataSdk003");
}
