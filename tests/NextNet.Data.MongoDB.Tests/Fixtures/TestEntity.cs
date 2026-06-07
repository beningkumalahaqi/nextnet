namespace NextNet.Data.MongoDB.Tests.Fixtures;

/// <summary>
/// Simple test entity with a string Id using ObjectId representation.
/// </summary>
public sealed class TestEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Test entity with an ObjectId Id property.
/// </summary>
public sealed class TestEntityWithObjectId
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }
}

/// <summary>
/// Test entity with a custom integer Id.
/// </summary>
public sealed class TestEntityWithIntId
{
    [BsonId]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Test entity with BSON attributes for custom element names.
/// </summary>
public sealed class TestEntityWithAttributes
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("full_name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("user_age")]
    public int Age { get; set; }

    [BsonIgnore]
    public string Computed { get; set; } = string.Empty;
}

/// <summary>
/// Test entity with a custom collection name attribute.
/// </summary>
[CollectionName("custom_test_collection")]
public sealed class TestEntityWithCollectionAttribute
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
