using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NextNet.Data.Dapper.Internal;

/// <summary>
/// Caches table name, column list, and primary key information for entity types.
/// Uses reflection to discover metadata with attribute-based overrides.
/// </summary>
/// <remarks>
/// <para>
/// Metadata resolution follows this order:
/// <list type="number">
///   <item>Check the concurrent cache — return immediately if cached.</item>
///   <item>Resolve table name from <c>[TableName]</c> attribute or fall back to pluralized type name.</item>
///   <item>Resolve columns from public readable/writable properties.</item>
///   <item>Identify primary key from <c>[Key]</c> attribute or <c>Id</c> / <c>{TypeName}Id</c> convention.</item>
///   <item>Cache and return the metadata.</item>
/// </list>
/// </para>
/// <para>
/// This class is thread-safe. All public static methods are safe to call from multiple threads.
/// </para>
/// </remarks>
internal sealed class EntityMetadata
{
    private static readonly ConcurrentDictionary<Type, EntityMetadata> Cache = new();

    /// <summary>
    /// Gets the table name (e.g., "Users", "dbo.Orders").
    /// </summary>
    internal string TableName { get; }

    /// <summary>
    /// Gets the optional schema name (e.g., "dbo").
    /// </summary>
    internal string? Schema { get; }

    /// <summary>
    /// Gets the fully qualified table name with schema prefix if applicable.
    /// </summary>
    internal string QualifiedTableName =>
        string.IsNullOrEmpty(Schema) ? TableName : $"[{Schema}].[{TableName}]";

    /// <summary>
    /// Gets the primary key column name (e.g., "Id").
    /// </summary>
    internal string KeyColumn { get; }

    /// <summary>
    /// Gets the list of column names for INSERT operations (excluding auto-generated key).
    /// </summary>
    internal IReadOnlyList<string> InsertColumns { get; }

    /// <summary>
    /// Gets the list of column names for UPDATE SET clause (excluding key).
    /// </summary>
    internal IReadOnlyList<string> UpdateColumns { get; }

    /// <summary>
    /// Gets all column names for SELECT * queries.
    /// </summary>
    internal IReadOnlyList<string> AllColumns { get; }

    private EntityMetadata(
        string tableName,
        string? schema,
        string keyColumn,
        IReadOnlyList<string> insertColumns,
        IReadOnlyList<string> updateColumns,
        IReadOnlyList<string> allColumns)
    {
        TableName = tableName;
        Schema = schema;
        KeyColumn = keyColumn;
        InsertColumns = insertColumns;
        UpdateColumns = updateColumns;
        AllColumns = allColumns;
    }

    /// <summary>
    /// Gets or creates metadata for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="options">Optional repository options to override defaults.</param>
    /// <returns>Cached or freshly resolved <see cref="EntityMetadata"/>.</returns>
    internal static EntityMetadata For<T>(DapperRepositoryOptions? options = null) where T : class
    {
        return Cache.GetOrAdd(typeof(T), _ => Resolve<T>(options));
    }

    /// <summary>
    /// Clears the metadata cache. Used primarily in tests.
    /// </summary>
    internal static void ClearCache() => Cache.Clear();

    private static EntityMetadata Resolve<T>(DapperRepositoryOptions? options) where T : class
    {
        var type = typeof(T);
        var useColumnAttr = options?.UseColumnAttribute ?? true;

        // Step 1: Resolve table name
        var tableName = options?.TableName;
        var schema = options?.Schema ?? options?.SchemaName;

        if (string.IsNullOrEmpty(schema))
        {
            schema = null;
        }

        if (tableName is null)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            if (tableAttr is not null)
            {
                tableName = tableAttr.Name;
                if (string.IsNullOrEmpty(schema) && tableAttr.Schema is not null)
                {
                    schema = tableAttr.Schema;
                }
            }
            else
            {
                tableName = Pluralize(type.Name);
            }
        }

        // Step 2: Resolve columns
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToList();

        var excluded = options?.ExcludedColumns ?? new HashSet<string>();

        // Step 3: Identify key column
        var keyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() is not null);

        if (keyProperty is null)
        {
            keyProperty = properties.FirstOrDefault(p =>
                string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name, $"{type.Name}Id", StringComparison.OrdinalIgnoreCase));
        }

        var keyColumn = options?.KeyColumn ?? keyProperty?.Name ?? "Id";
        var excludedSet = new HashSet<string>(excluded, StringComparer.OrdinalIgnoreCase) { keyColumn };

        // Step 4: Build column lists
        var allColumns = properties
            .Where(p => !excludedSet.Contains(p.Name))
            .Select(p => GetColumnName(p, useColumnAttr))
            .ToList();

        var insertColumns = properties
            .Where(p => !excludedSet.Contains(p.Name))
            .Select(p => GetColumnName(p, useColumnAttr))
            .ToList();

        var updateColumns = properties
            .Where(p => !excludedSet.Contains(p.Name) &&
                       !string.Equals(p.Name, keyColumn, StringComparison.OrdinalIgnoreCase))
            .Select(p => GetColumnName(p, useColumnAttr))
            .ToList();

        return new EntityMetadata(tableName, schema, keyColumn, insertColumns, updateColumns, allColumns);
    }

    /// <summary>
    /// Gets the column name for a property, respecting <c>[Column]</c> attribute.
    /// </summary>
    /// <summary>
    /// Gets the column name for a property, optionally respecting <c>[Column]</c> attribute.
    /// </summary>
    /// <param name="property">The property to resolve.</param>
    /// <param name="useColumnAttribute">Whether to check for a <c>[Column]</c> attribute override.</param>
    /// <returns>The column name for the property.</returns>
    internal static string GetColumnName(PropertyInfo property, bool useColumnAttribute = true)
    {
        if (useColumnAttribute)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr?.Name is not null)
            {
                return columnAttr.Name;
            }
        }

        return property.Name;
    }

    /// <summary>
    /// Simple pluralization by adding "s" or "es" to the end of the name.
    /// Delegates to the canonical <see cref="NextNet.Data.Abstractions.Internal.Pluralizer"/> implementation.
    /// </summary>
    internal static string Pluralize(string name)
        => NextNet.Data.Abstractions.Internal.Pluralizer.Pluralize(name);
}
