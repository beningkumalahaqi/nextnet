namespace NextNet.Data.Sdk;

/// <summary>
/// Marks a class as a migration engine implementation.
/// Validated by SDK analyzers to ensure all <see cref="IMigrationEngine"/>
/// methods are implemented.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a migration engine class. The SDK analyzers verify
/// that the class implements <see cref="IMigrationEngine"/> and follows the
/// required naming conventions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [DataMigrationEngine]
/// public class MyCustomMigrationEngine : MigrationEngineBase
/// {
///     // Migration engine implementation
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataMigrationEngineAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the migration file format (e.g., "sql", "cs", "json"). Defaults to "sql".
    /// </summary>
    public string MigrationFormat { get; set; } = "sql";
}
