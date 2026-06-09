namespace NextNet.Data.Sdk;

/// <summary>
/// Defines standardized error codes for the NextNet.Data.Sdk package (DS-590 to DS-604).
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used as prefixes in exception messages to enable structured
/// error identification, logging, and monitoring. Each code maps to a specific error
/// scenario within the data provider SDK.
/// </para>
/// </remarks>
public static class SdkErrorCodes
{
    /// <summary>
    /// The <see cref="DataProviderAttribute"/> is required but missing from the provider class.
    /// </summary>
    public const string ProviderAttributeRequired = "DS-590";

    /// <summary>
    /// The <see cref="DataRepositoryAttribute"/> is required but missing from the repository class.
    /// </summary>
    public const string RepositoryAttributeRequired = "DS-591";

    /// <summary>
    /// The <see cref="DataMigrationEngineAttribute"/> is required but missing from the migration engine class.
    /// </summary>
    public const string MigrationEngineAttributeRequired = "DS-592";

    /// <summary>
    /// An attribute was applied to an invalid target type.
    /// </summary>
    public const string InvalidAttributeTarget = "DS-593";

    /// <summary>
    /// Code generation failed during source generation or scaffolding.
    /// </summary>
    public const string CodeGenerationFailed = "DS-594";

    /// <summary>
    /// A type could not be resolved during code generation or provider resolution.
    /// </summary>
    public const string TypeResolutionFailed = "DS-595";

    /// <summary>
    /// An expected interface is not implemented by the target type.
    /// </summary>
    public const string InterfaceNotImplemented = "DS-596";

    /// <summary>
    /// A generic type constraint was violated.
    /// </summary>
    public const string GenericTypeConstraintViolation = "DS-597";

    /// <summary>
    /// Writing the generated source output to the compilation failed.
    /// </summary>
    public const string SourceOutputFailed = "DS-598";

    /// <summary>
    /// A diagnostic error was reported by an analyzer or source generator.
    /// </summary>
    public const string DiagnosticError = "DS-599";

    /// <summary>
    /// The compilation of generated code failed.
    /// </summary>
    public const string CompilationFailed = "DS-600";

    /// <summary>
    /// A required symbol (type, method, etc.) was not found in the compilation.
    /// </summary>
    public const string SymbolNotFound = "DS-601";

    /// <summary>
    /// An argument passed to an attribute is invalid.
    /// </summary>
    public const string AttributeArgumentInvalid = "DS-602";

    /// <summary>
    /// A namespace conflict was detected during code generation.
    /// </summary>
    public const string NamespaceConflict = "DS-603";

    /// <summary>
    /// An unexpected internal error occurred within the SDK.
    /// </summary>
    public const string InternalError = "DS-604";
}
