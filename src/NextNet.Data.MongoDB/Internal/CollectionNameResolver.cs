namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Resolves MongoDB collection names from entity types using a priority chain:
/// options override → attribute → pluralized type name.
/// </summary>
/// <remarks>
/// <para>
/// Collection name resolution follows this priority:
/// <list type="number">
///   <item><description><see cref="MongoDbRepositoryOptions.CollectionName"/> (explicit override)</description></item>
///   <item><description><c>[CollectionName("name")]</c> attribute on entity class</description></item>
///   <item><description>Pluralized, camelCase version of entity type name (e.g., "User" → "users")</description></item>
/// </list>
/// </para>
/// </remarks>
internal static class CollectionNameResolver
{
    /// <summary>
    /// Resolves the collection name for the given entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="options">Optional repository-level options that may specify a collection name.</param>
    /// <returns>The resolved collection name.</returns>
    public static string Resolve<T>(MongoDbRepositoryOptions? options = null) where T : class
    {
        return Resolve(typeof(T), options);
    }

    /// <summary>
    /// Resolves the collection name for the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="options">Optional repository-level options that may specify a collection name.</param>
    /// <returns>The resolved collection name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityType"/> is null.</exception>
    public static string Resolve(Type entityType, MongoDbRepositoryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        // Priority 1: Options override
        if (options?.CollectionName is not null)
        {
            return options.CollectionName;
        }

        // Priority 2: [CollectionName] attribute
        var attribute = entityType.GetCustomAttribute<CollectionNameAttribute>();
        if (attribute?.Name is not null)
        {
            return attribute.Name;
        }

        // Priority 3: Pluralized, camelCase type name
        var pluralized = Pluralizer.Pluralize(entityType.Name);
        return Pluralizer.ToCamelCase(pluralized);
    }
}

/// <summary>
/// Specifies the MongoDB collection name for an entity class.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to entity classes to override the default collection name
/// resolution (which would otherwise pluralize and camelCase the type name).
/// </para>
/// <example>
/// <code>
/// [CollectionName("user_accounts")]
/// public class User { ... }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CollectionNameAttribute : Attribute
{
    /// <summary>
    /// Gets the collection name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionNameAttribute"/> class.
    /// </summary>
    /// <param name="name">The MongoDB collection name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public CollectionNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Collection name must not be null or empty.", nameof(name));
        Name = name;
    }
}
