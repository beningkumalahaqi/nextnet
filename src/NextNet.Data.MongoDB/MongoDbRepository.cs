using System.Collections.Concurrent;
using NextNet.Data.MongoDB.Internal;

namespace NextNet.Data.MongoDB;

/// <summary>
/// Generic MongoDB implementation of <see cref="IRepository{T}"/>.
/// Uses <see cref="IMongoCollection{T}"/> for CRUD operations with
/// <see cref="FilterDefinition{T}"/> for query predicates.
/// </summary>
/// <typeparam name="T">The entity type. Must be a class that can be serialized to BSON.</typeparam>
/// <remarks>
/// <para>
/// Queries use MongoDB's <see cref="FilterDefinition{T}"/> builders for
/// type-safe predicate construction. The <see cref="FindAsync"/> method
/// supports filtering by <c>_id</c> (ObjectId or string).
/// </para>
/// <para>
/// Each repository operation:
/// <list type="number">
///   <item>Resolves the <see cref="IMongoCollection{T}"/> via <see cref="MongoClientManager"/></item>
///   <item>Builds the filter using <see cref="Builders{T}.Filter"/></item>
///   <item>Executes the operation against the collection</item>
///   <item>Returns the result</item>
/// </list>
/// </para>
/// <para>
/// The <c>_id</c> field is resolved from the entity type's property decorated
/// with <see cref="BsonIdAttribute"/>, or a property named <c>Id</c> or <c>{TypeName}Id</c>.
/// </para>
/// </remarks>
public sealed class MongoDbRepository<T> : IRepository<T> where T : class
{
    private static readonly ConcurrentDictionary<Type, HashSet<string>> ValidSortFieldsCache = new();

    private readonly MongoClientManager _clientManager;
    private readonly MongoDbRepositoryOptions _options;
    private readonly ILogger<MongoDbRepository<T>> _logger;
    private readonly string _connectionName;
    private readonly IdPropertyInfo? _idInfo;
    private readonly HashSet<string> _validSortFields;

    /// <summary>
    /// Initializes a new instance of <see cref="MongoDbRepository{T}"/>.
    /// </summary>
    /// <param name="clientManager">The MongoDB client manager.</param>
    /// <param name="options">Optional repository configuration (collection name, filter defaults).</param>
    /// <param name="connectionName">Optional connection name override. Defaults to "Default".</param>
    /// <param name="logger">Optional logger for query diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="clientManager"/> is null.</exception>
    public MongoDbRepository(
        MongoClientManager clientManager,
        MongoDbRepositoryOptions? options = null,
        string? connectionName = null,
        ILogger<MongoDbRepository<T>>? logger = null)
    {
        _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        _options = options ?? new MongoDbRepositoryOptions();
        _connectionName = connectionName ?? "Default";
        _logger = logger ?? new NullLogger<MongoDbRepository<T>>();

        _idInfo = IdResolver.For<T>();

        // Cache valid sort fields from entity BSON element names
        _validSortFields = ValidSortFieldsCache.GetOrAdd(typeof(T), _ =>
        {
            var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Use BsonElement attribute name if present, otherwise property name (camelCase)
                var bsonElement = prop.GetCustomAttribute<BsonElementAttribute>();
                if (bsonElement is not null && bsonElement.ElementName is not null)
                {
                    fields.Add(bsonElement.ElementName);
                }
                else
                {
                    // CamelCase the property name to match BSON conventions
                    fields.Add(char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..]);
                }

                // Also add the raw property name for convenience
                fields.Add(prop.Name);
            }

            return fields;
        });
    }

    /// <summary>
    /// Finds a single document by its <c>_id</c> field.
    /// Uses <c>FilterDefinition{T}.Eq("_id", id)</c> to match the ID.
    /// </summary>
    /// <param name="id">The document ID. Can be an <c>ObjectId</c>,
    /// a <see cref="string"/>, or any BSON-compatible value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The document if found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// If the entity's ID property is typed as <c>string</c> with
    /// <c>[BsonRepresentation(BsonType.ObjectId)]</c>, the <paramref name="id"/>
    /// value is parsed as an <c>ObjectId</c> before querying.
    /// </remarks>
    public async Task<T?> FindAsync(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var collection = await GetCollectionAsync(cancellationToken);
        var filterId = ConvertIdForQuery(id);
        var filter = Builders<T>.Filter.Eq("_id", filterId);

        LogQuery("FindAsync", filter);

        return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all documents with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="options">Optional query options (filter, sort, pagination).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A paged result of matching documents.</returns>
    /// <exception cref="InvalidOperationException">Thrown when sort field is not valid or filter contains dangerous operators.</exception>
    /// <remarks>
    /// <para>
    /// The <c>options.Filter</c> is parsed as a JSON BSON filter document when
    /// provided as a string. For complex filters, use <c>Builders&lt;T&gt;.Filter</c>
    /// directly on the <see cref="IMongoCollection{T}"/>.
    /// </para>
    /// <para>
    /// Sorting is applied using <see cref="Builders{T}.Sort"/> with the field
    /// name from <c>options.SortBy</c>. Field names are validated against the
    /// entity's BSON element names to prevent injection.
    /// </para>
    /// </remarks>
    public async Task<PagedResult<T>> GetAllAsync(RepositoryQueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RepositoryQueryOptions();

        var page = options.EffectivePage;
        var pageSize = options.EffectivePageSize;
        var collection = await GetCollectionAsync(cancellationToken);

        // Build filter
        FilterDefinition<T> filter;
        if (!string.IsNullOrWhiteSpace(options.Filter))
        {
            filter = FilterParser.Parse<T>(options.Filter);
        }
        else
        {
            filter = Builders<T>.Filter.Empty;
        }

        // Validate and build sort
        SortDefinition<T>? sort = null;
        if (!string.IsNullOrWhiteSpace(options.SortBy))
        {
            if (!_validSortFields.Contains(options.SortBy))
            {
                throw new InvalidOperationException(
                    $"[{MongoDbErrorCodes.ConfigurationInvalid}] Invalid sort field '{options.SortBy}'. Valid fields for '{typeof(T).Name}': " +
                    $"{string.Join(", ", _validSortFields.OrderBy(f => f))}");
            }

            sort = options.SortDescending
                ? Builders<T>.Sort.Descending(options.SortBy)
                : Builders<T>.Sort.Ascending(options.SortBy);
        }

        // Get total count for pagination metadata
        var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        // Build the query
        var find = collection.Find(filter);

        if (sort is not null)
        {
            find = find.Sort(sort);
        }

        var skip = (page - 1) * pageSize;
        var items = await find.Skip(skip).Limit(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items.AsReadOnly(), totalCount, page, pageSize);
    }

    /// <summary>
    /// Inserts a new document into the collection.
    /// Uses <c>InsertOneAsync</c> which generates an <c>_id</c> if
    /// the entity's ID is of type <c>ObjectId</c> or <c>Guid</c> and is the default value.
    /// </summary>
    /// <param name="entity">The entity to insert. Must not be null.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <remarks>
    /// After insertion, the entity's ID property is updated with the
    /// server-generated <c>_id</c> value if applicable.
    /// </remarks>
    public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Auto-generate ObjectId for string Id properties with [BsonRepresentation(BsonType.ObjectId)]
        if (_idInfo?.HasStringObjectIdRepresentation == true && _idInfo.Property.CanWrite)
        {
            var currentValue = _idInfo.Property.GetValue(entity) as string;
            if (string.IsNullOrEmpty(currentValue))
            {
                var generatedId = ObjectId.GenerateNewId().ToString();
                _idInfo.Property.SetValue(entity, generatedId);
                _logger.LogDebug("Generated ObjectId '{Id}' for entity of type '{EntityType}'.", generatedId, typeof(T).Name);
            }
        }

        var collection = await GetCollectionAsync(cancellationToken);
        await collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

        _logger.LogDebug("Inserted entity of type '{EntityType}'.", typeof(T).Name);
    }

    /// <summary>
    /// Replaces an existing document matched by <c>_id</c>.
    /// Uses <c>ReplaceOneAsync</c> with an <c>_id</c> filter.
    /// </summary>
    /// <param name="entity">The entity with updated values. Must not be null.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no document with the given ID exists.</exception>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var idValue = GetEntityIdValue(entity);
        var filterId = ConvertIdForQuery(idValue);
        var filter = Builders<T>.Filter.Eq("_id", filterId);

        var collection = await GetCollectionAsync(cancellationToken);
        var result = await collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException(
                $"[{MongoDbErrorCodes.CollectionNotFound}] No entity of type '{typeof(T).Name}' with id '{idValue}' was found. Update failed.");
        }

        _logger.LogDebug("Updated entity of type '{EntityType}' with id '{Id}'.", typeof(T).Name, idValue);
    }

    /// <summary>
    /// Deletes a document by its <c>_id</c> field.
    /// Uses <c>DeleteOneAsync</c> with an <c>_id</c> filter.
    /// </summary>
    /// <param name="id">The primary key value of the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no document with the given ID exists.</exception>
    public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var filterId = ConvertIdForQuery(id);
        var filter = Builders<T>.Filter.Eq("_id", filterId);

        var collection = await GetCollectionAsync(cancellationToken);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new KeyNotFoundException(
                $"[{MongoDbErrorCodes.CollectionNotFound}] No entity of type '{typeof(T).Name}' with id '{id}' was found. Delete failed.");
        }

        _logger.LogDebug("Deleted entity of type '{EntityType}' with id '{Id}'.", typeof(T).Name, id);
    }

    private async Task<IMongoCollection<T>> GetCollectionAsync(CancellationToken cancellationToken)
    {
        return await _clientManager.GetCollectionAsync<T>(_connectionName, _options, cancellationToken);
    }

    private object ConvertIdForQuery(object id)
    {
        // If the ID property has string ObjectId representation, parse the string as ObjectId
        if (_idInfo?.HasStringObjectIdRepresentation == true && id is string stringId)
        {
            if (ObjectId.TryParse(stringId, out var objectId))
            {
                return objectId;
            }

            throw new InvalidOperationException(
                $"[{MongoDbErrorCodes.DocumentSerializationFailed}] The value '{stringId}' is not a valid 24-character hex ObjectId string. " +
                "Entity type '{typeof(T).Name}' has a string Id with [BsonRepresentation(BsonType.ObjectId)], " +
                "so the ID value must be a valid ObjectId string.");
        }

        return id;
    }

    private object GetEntityIdValue(T entity)
    {
        if (_idInfo is null)
        {
            throw new InvalidOperationException(
                $"[{MongoDbErrorCodes.RepositoryNotInitialized}] Cannot resolve ID property for entity type '{typeof(T).Name}'. " +
                "Ensure the entity has a property decorated with [BsonId] or named 'Id'.");
        }

        return _idInfo.Property.GetValue(entity)
            ?? throw new InvalidOperationException(
                $"[{MongoDbErrorCodes.RepositoryNotInitialized}] The ID property '{_idInfo.PropertyName}' on entity type '{typeof(T).Name}' is null.");
    }

    [Conditional("DEBUG")]
    private void LogQuery(string method, FilterDefinition<T> filter)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("MongoDbRepository.{Method} executing with filter.", method);
        }
    }
}
