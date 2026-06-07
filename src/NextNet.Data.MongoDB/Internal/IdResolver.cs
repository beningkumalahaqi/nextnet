using System.Collections.Concurrent;

namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Discovers the document <c>_id</c> property for an entity type.
/// Uses a priority chain: <c>[BsonId]</c> attribute → <c>Id</c> property → <c>{TypeName}Id</c> property.
/// </summary>
/// <remarks>
/// <para>
/// The <c>_id</c> field is resolved using the following priority:
/// <list type="number">
///   <item><description>Property decorated with <c>[BsonId]</c> attribute</description></item>
///   <item><description>Property named "Id" (case-insensitive)</description></item>
///   <item><description>Property named "{TypeName}Id" (e.g., "UserId", "ProductId")</description></item>
/// </list>
/// </para>
/// <para>
/// Results are cached in a <see cref="ConcurrentDictionary{TKey, TValue}"/> for performance.
/// </para>
/// </remarks>
internal static class IdResolver
{
    private static readonly ConcurrentDictionary<Type, IdPropertyInfo> Cache = new();

    /// <summary>
    /// Gets the ID property information for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>Information about the resolved ID property, or <c>null</c> if no ID property is found.</returns>
    public static IdPropertyInfo? For<T>() where T : class => For(typeof(T));

    /// <summary>
    /// Gets the ID property information for the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Information about the resolved ID property, or <c>null</c> if no ID property is found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityType"/> is null.</exception>
    public static IdPropertyInfo? For(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        return Cache.GetOrAdd(entityType, t => ResolveIdProperty(t)!);
    }

    private static IdPropertyInfo? ResolveIdProperty(Type type)
    {
        // Priority 1: [BsonId] attribute
        var bsonIdProp = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.GetCustomAttribute<BsonIdAttribute>() is not null);

        if (bsonIdProp is not null)
        {
            return CreateIdInfo(bsonIdProp);
        }

        // Priority 2: Property named "Id" (case-insensitive)
        var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (idProp is not null)
        {
            return CreateIdInfo(idProp);
        }

        // Priority 3: Property named "{TypeName}Id"
        var typeNameIdProp = type.GetProperty($"{type.Name}Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (typeNameIdProp is not null)
        {
            return CreateIdInfo(typeNameIdProp);
        }

        return null;
    }

    private static IdPropertyInfo CreateIdInfo(PropertyInfo property)
    {
        var hasBsonRepresentation = property.GetCustomAttribute<BsonRepresentationAttribute>() is not null;
        var isObjectId = property.PropertyType == typeof(ObjectId);
        var isString = property.PropertyType == typeof(string);

        return new IdPropertyInfo(
            Property: property,
            ElementName: "_id",
            PropertyName: property.Name,
            PropertyType: property.PropertyType,
            IsObjectId: isObjectId,
            IsString: isString,
            HasStringObjectIdRepresentation: hasBsonRepresentation && isString);
    }
}

/// <summary>
/// Information about an entity's ID property resolved by <see cref="IdResolver"/>.
/// </summary>
/// <param name="Property">The CLR property info for the ID.</param>
/// <param name="ElementName">The BSON element name (always "_id").</param>
/// <param name="PropertyName">The CLR property name (e.g., "Id").</param>
/// <param name="PropertyType">The CLR property type.</param>
/// <param name="IsObjectId">Whether the property is typed as <see cref="ObjectId"/>.</param>
/// <param name="IsString">Whether the property is typed as <see cref="string"/>.</param>
/// <param name="HasStringObjectIdRepresentation">Whether the property has <c>[BsonRepresentation(BsonType.ObjectId)]</c> and is a string.</param>
internal sealed record IdPropertyInfo(
    PropertyInfo Property,
    string ElementName,
    string PropertyName,
    Type PropertyType,
    bool IsObjectId,
    bool IsString,
    bool HasStringObjectIdRepresentation
);
