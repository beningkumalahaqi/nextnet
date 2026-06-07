namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Parses JSON filter strings into MongoDB <see cref="FilterDefinition{T}"/> objects.
/// Validates filters and rejects dangerous operators like <c>$where</c>.
/// </summary>
/// <remarks>
/// <para>
/// The filter string is parsed as a BSON document and wrapped in a
/// <see cref="BsonDocumentFilterDefinition{T}"/>. This approach allows users
/// to write MongoDB-native query syntax for advanced filtering.
/// </para>
/// <para>
/// For injection safety:
/// <list type="bullet">
///   <item><description>The filter string is parsed as BSON, not evaluated as JavaScript</description></item>
///   <item><description>The <c>$where</c> operator is rejected (potential injection vector)</description></item>
/// </list>
/// </para>
/// </remarks>
internal static class FilterParser
{
    private static readonly HashSet<string> DangerousOperators = new(StringComparer.Ordinal)
    {
        "$where",
        "$function",
        "$accumulator",
    };

    /// <summary>
    /// Parses a JSON filter string into a <see cref="FilterDefinition{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="filterJson">The JSON filter string in MongoDB query syntax.</param>
    /// <returns>A <see cref="FilterDefinition{T}"/> for use with MongoDB queries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filterJson"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the filter contains dangerous operators or is not valid BSON JSON.</exception>
    public static FilterDefinition<T> Parse<T>(string filterJson) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filterJson);

        BsonDocument filterDoc;
        try
        {
            filterDoc = BsonDocument.Parse(filterJson);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "MongoDB filter must be valid JSON matching BSON query syntax. " +
                "See https://www.mongodb.com/docs/manual/tutorial/query-documents/", ex);
        }

        // Validate for dangerous operators
        ValidateNoDangerousOperators(filterDoc);

        return new BsonDocumentFilterDefinition<T>(filterDoc);
    }

    /// <summary>
    /// Creates an empty filter definition.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>An empty <see cref="FilterDefinition{T}"/> that matches all documents.</returns>
    public static FilterDefinition<T> Empty<T>() where T : class
    {
        return Builders<T>.Filter.Empty;
    }

    private static void ValidateNoDangerousOperators(BsonDocument document)
    {
        foreach (var element in document)
        {
            if (DangerousOperators.Contains(element.Name))
            {
                throw new InvalidOperationException(
                    $"The '{element.Name}' operator is not allowed in repository filters for security reasons. " +
                    "Use the IMongoCollection<T> escape hatch directly for advanced query scenarios.");
            }

            // Recursively check nested documents
            if (element.Value is BsonDocument nestedDoc)
            {
                ValidateNoDangerousOperators(nestedDoc);
            }

            // Check arrays of documents
            if (element.Value is BsonArray array)
            {
                foreach (var item in array)
                {
                    if (item is BsonDocument arrayDoc)
                    {
                        ValidateNoDangerousOperators(arrayDoc);
                    }
                }
            }
        }
    }
}
