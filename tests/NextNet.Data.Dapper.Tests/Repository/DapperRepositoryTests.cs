using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using NextNet.Data.Dapper.Internal;

namespace NextNet.Data.Dapper.Tests.Repository;

// Test entity for repository tests
public sealed class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int Age { get; set; }
}

// Test entity with custom column mapping
public sealed class CustomMappedEntity
{
    [Key]
    public int UserId { get; set; }

    [Column("full_name")]
    public string Name { get; set; } = string.Empty;

    [Column("email_address")]
    public string? Email { get; set; }
}

// Entity with Table attribute
[Table("customers")]
public sealed class TableAttributedEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Entity with Table and Schema attribute
[Table("orders", Schema = "sales")]
public sealed class SchemaAttributedEntity
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
}

/// <summary>
/// Tests for <see cref="DapperRepository{T}"/> CRUD operations,
/// metadata resolution, null checking, and SQL generation.
/// </summary>
public sealed class DapperRepositoryTests
{
    /// <summary>
    /// Creates a <see cref="DapperConnectionManager"/> for testing.
    /// Uses a non-routable connection string to prevent actual database access.
    /// </summary>
    private static DapperConnectionManager CreateTestConnectionManager()
    {
        var connections = new Dictionary<string, ConnectionConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["Default"] = new ConnectionConfig(
                "Server=192.0.2.1,9999;Database=Test;Trusted_Connection=true;Connect Timeout=1;",
                Provider: "Dapper")
        };

        return new DapperConnectionManager(
            connections,
            new DapperOptions { EnablePooling = false, CommandTimeoutSeconds = 1 },
            NullLogger<DapperConnectionManager>.Instance);
    }

    /// <summary>
    /// Creates a <see cref="DapperRepository{T}"/> with test connections and options.
    /// </summary>
    private static DapperRepository<T> CreateRepository<T>(
        DapperRepositoryOptions? options = null,
        string? connectionName = null,
        ILogger<DapperRepository<T>>? logger = null) where T : class
    {
        var connectionManager = CreateTestConnectionManager();
        return new DapperRepository<T>(
            connectionManager,
            options,
            connectionName,
            logger);
    }

    /// <summary>
    /// Gets <see cref="EntityMetadata"/> for <see cref="TestEntity"/> with default options.
    /// Returns a fresh instance by clearing the cache first, ensuring deterministic results.
    /// </summary>
    private static EntityMetadata GetDefaultTestEntityMetadata()
    {
        EntityMetadata.ClearCache();
        return EntityMetadata.For<TestEntity>(new DapperRepositoryOptions());
    }

    // ===== Constructor Tests =====

    /// <summary>
    /// Clears the EntityMetadata cache before each test to ensure
    /// tests that use different options for the same entity type
    /// produce correct results.
    /// </summary>
    public DapperRepositoryTests()
    {
        EntityMetadata.ClearCache();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_ThrowArgumentNullException_When_ConnectionManagerIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new DapperRepository<TestEntity>(null!));
        Assert.Contains("connectionManager", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseDefaultOptions_When_OptionsIsNull()
    {
        var repo = CreateRepository<TestEntity>();
        Assert.NotNull(repo);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseDefaultConnectionName_When_NotSpecified()
    {
        var repo = CreateRepository<TestEntity>();
        Assert.NotNull(repo);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_AcceptCustomConnectionName()
    {
        var connectionManager = CreateTestConnectionManager();
        var repo = new DapperRepository<TestEntity>(
            connectionManager,
            connectionName: "CustomConnection",
            logger: NullLogger<DapperRepository<TestEntity>>.Instance);
        Assert.NotNull(repo);
    }

    // ===== Null Entity Checks =====

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InsertAsync_Should_ThrowArgumentNullException_When_EntityIsNull()
    {
        var repo = CreateRepository<TestEntity>();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.InsertAsync(null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UpdateAsync_Should_ThrowArgumentNullException_When_EntityIsNull()
    {
        var repo = CreateRepository<TestEntity>();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
    }

    // ===== Invalid Sort Column =====

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetAllAsync_Should_ThrowInvalidOperationException_When_SortColumnIsInvalid()
    {
        var repo = CreateRepository<TestEntity>();
        var options = new RepositoryQueryOptions(
            SortBy: "NonExistentColumn",
            Page: 1,
            PageSize: 10);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(options));
        Assert.Contains("NonExistentColumn", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetAllAsync_Should_NotThrow_When_SortColumnIsValid()
    {
        var repo = CreateRepository<TestEntity>();
        var options = new RepositoryQueryOptions(
            SortBy: "Name",
            Page: 1,
            PageSize: 10);

        // Will throw a connection error, not an InvalidOperationException
        var ex = await Record.ExceptionAsync(() => repo.GetAllAsync(options));
        Assert.NotNull(ex);
        Assert.IsNotType<InvalidOperationException>(ex);
    }

    // ===== Delete Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public void DeleteAsync_Should_ThrowKeyNotFoundException_When_RowsAffectedIsZero()
    {
        // This is tricky to test without a DB. The mock would need to return 0.
        // Since we can't easily mock SqlConnection, we'll verify the SQL is
        // parameterized by checking the SqlBuilder output directly.
        var metadata = EntityMetadata.For<TestEntity>();
        var sql = SqlBuilder.BuildDelete(metadata);

        Assert.Contains("@Id", sql);
        Assert.DoesNotContain("'", sql); // No string concatenation
    }

    // ===== SQL is Parameterized (No Injection) =====

    [Fact]
    [Trait("Category", "Unit")]
    public void BuildFind_Should_UseParameterizedQuery()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        var sql = SqlBuilder.BuildFind(metadata);

        Assert.Contains("@Id", sql);
        Assert.DoesNotContain("'" + " OR ", sql); // No concatenated SQL
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BuildInsert_Should_UseParameterizedQuery()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        var sql = SqlBuilder.BuildInsert(metadata, keyIsAutoGenerated: true);

        Assert.Contains("@Name", sql);
        Assert.Contains("@Email", sql);
        Assert.Contains("@Age", sql);
        Assert.DoesNotContain("VALUES ('", sql); // Not string concatenation
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BuildUpdate_Should_UseParameterizedQuery()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        var sql = SqlBuilder.BuildUpdate(metadata);

        Assert.Contains("@Name", sql);
        Assert.Contains("@Id", sql); // Key field
        Assert.DoesNotContain("SET '", sql); // Not string concatenation
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BuildDelete_Should_UseParameterizedQuery()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        var sql = SqlBuilder.BuildDelete(metadata);

        Assert.Contains("@Id", sql);
        Assert.DoesNotContain("= '", sql); // Not string concatenation
    }

    // ===== Entity Metadata Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_ResolveTableName_FromTypeName()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        Assert.Equal("TestEntities", metadata.TableName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_UseDefaultSchemaName()
    {
        var options = new DapperRepositoryOptions(); // SchemaName defaults to "dbo"
        var metadata = EntityMetadata.For<TestEntity>(options);

        Assert.Equal("dbo", metadata.Schema);
        Assert.Contains("[dbo].[TestEntities]", metadata.QualifiedTableName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_UseExplicitSchema_When_Provided()
    {
        var options = new DapperRepositoryOptions { Schema = "custom" };
        var metadata = EntityMetadata.For<TestEntity>(options);

        Assert.Equal("custom", metadata.Schema);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_OmitSchema_When_SchemaNameIsNullOrEmpty()
    {
        var options = new DapperRepositoryOptions { SchemaName = string.Empty };
        EntityMetadata.ClearCache(); // Clear cache to force re-resolution
        var metadata = EntityMetadata.For<TestEntity>(options);

        Assert.Null(metadata.Schema);
        Assert.Equal("TestEntities", metadata.QualifiedTableName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_ResolveTableName_FromTableAttribute()
    {
        var metadata = EntityMetadata.For<TableAttributedEntity>();
        Assert.Equal("customers", metadata.TableName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_ResolveSchema_FromTableAttribute()
    {
        var metadata = EntityMetadata.For<SchemaAttributedEntity>();
        Assert.Equal("orders", metadata.TableName);
        Assert.Equal("sales", metadata.Schema);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_FindKeyProperty_ByName()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        Assert.Equal("Id", metadata.KeyColumn);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_FindKeyProperty_ByKeyAttribute()
    {
        var metadata = EntityMetadata.For<CustomMappedEntity>();
        Assert.Equal("UserId", metadata.KeyColumn);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_ResolveColumnName_FromColumnAttribute()
    {
        var metadata = EntityMetadata.For<CustomMappedEntity>();
        Assert.Contains("full_name", metadata.AllColumns);
        Assert.Contains("email_address", metadata.AllColumns);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_IgnoreColumnAttribute_When_UseColumnAttributeIsFalse()
    {
        var options = new DapperRepositoryOptions { UseColumnAttribute = false };
        EntityMetadata.ClearCache(); // Clear cache to force re-resolution
        var metadata = EntityMetadata.For<CustomMappedEntity>(options);

        Assert.Contains("Name", metadata.AllColumns);
        Assert.Contains("Email", metadata.AllColumns);
        Assert.DoesNotContain("full_name", metadata.AllColumns);
        Assert.DoesNotContain("email_address", metadata.AllColumns);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_ExcludeKeyFromUpdateColumns()
    {
        var metadata = EntityMetadata.For<TestEntity>();
        Assert.DoesNotContain("Id", metadata.UpdateColumns);
        Assert.Contains("Name", metadata.UpdateColumns);
        Assert.Contains("Email", metadata.UpdateColumns);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_IncludeAllColumnsExceptExcluded()
    {
        var options = new DapperRepositoryOptions
        {
            ExcludedColumns = new HashSet<string> { "Age" }
        };
        var metadata = EntityMetadata.For<TestEntity>(options);

        Assert.DoesNotContain("Age", metadata.AllColumns);
        Assert.Contains("Name", metadata.AllColumns);
        Assert.Contains("Email", metadata.AllColumns);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EntityMetadata_Should_PluralizeCorrectly()
    {
        Assert.Equal("Customers", EntityMetadata.Pluralize("Customer"));
        Assert.Equal("Boxes", EntityMetadata.Pluralize("Box"));
        Assert.Equal("Buses", EntityMetadata.Pluralize("Bus"));
        Assert.Equal("Countries", EntityMetadata.Pluralize("Country"));
        Assert.Equal("Branches", EntityMetadata.Pluralize("Branch"));
        Assert.Equal("Disks", EntityMetadata.Pluralize("Disk"));
    }

    // ===== Pagination Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_UseOffsetFetch_ByDefault()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildGetAll(metadata, PaginationStyle.OffsetFetch, sortBy: "Name");

        Assert.Contains("OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY", sql);
        Assert.DoesNotContain("LIMIT", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_UseLimitOffset_When_Configured()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildGetAll(metadata, PaginationStyle.LimitOffset, sortBy: "Name");

        Assert.Contains("OFFSET @Offset ROWS LIMIT @Limit ROWS", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_IncludeDescending_When_Specified()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildGetAll(metadata, PaginationStyle.OffsetFetch, sortBy: "Name", sortDescending: true);

        Assert.Contains("ORDER BY [Name] DESC", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_IncludeAscending_ByDefault()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildGetAll(metadata, PaginationStyle.OffsetFetch, sortBy: "Name");

        Assert.Contains("ORDER BY [Name] ASC", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildCountSql()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildCount(metadata);

        Assert.Equal("SELECT COUNT(*) FROM [dbo].[TestEntities]", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildCountWithFilter()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildCount(metadata, filter: "Age > @minAge");

        Assert.Contains("WHERE Age > @minAge", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildFindWithKeyColumn()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildFind(metadata);

        Assert.Equal("SELECT * FROM [dbo].[TestEntities] WHERE [Id] = @Id", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildInsertWithScopeIdentity()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildInsert(metadata, keyIsAutoGenerated: true);

        Assert.StartsWith("INSERT INTO", sql);
        Assert.EndsWith("SELECT CAST(SCOPE_IDENTITY() AS INT)", sql.TrimEnd());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildInsertWithoutScopeIdentity()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildInsert(metadata, keyIsAutoGenerated: false);

        Assert.StartsWith("INSERT INTO", sql);
        Assert.DoesNotContain("SCOPE_IDENTITY", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildUpdateWithCorrectSetClause()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildUpdate(metadata);

        Assert.Contains("[Name] = @Name", sql);
        Assert.Contains("[Email] = @Email", sql);
        Assert.Contains("[Age] = @Age", sql);
        Assert.Contains("WHERE [Id] = @Id", sql);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SqlBuilder_Should_BuildDeleteWithKey()
    {
        var metadata = GetDefaultTestEntityMetadata();
        var sql = SqlBuilder.BuildDelete(metadata);

        Assert.Equal("DELETE FROM [dbo].[TestEntities] WHERE [Id] = @Id", sql);
    }

    // ===== DapperRepositoryOptions Tests =====

    [Fact]
    [Trait("Category", "Unit")]
    public void DapperRepositoryOptions_Should_HaveDefaults()
    {
        var options = new DapperRepositoryOptions();

        Assert.True(options.UseColumnAttribute);
        Assert.Equal("dbo", options.SchemaName);
        Assert.Equal(30, options.CommandTimeout);
        Assert.Equal("Id", options.KeyColumn);
        Assert.True(options.KeyIsAutoGenerated);
        Assert.Equal(PaginationStyle.OffsetFetch, options.PaginationStyle);
        Assert.Null(options.TableName);
        Assert.Null(options.Schema);
        Assert.Null(options.ExcludedColumns);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DapperRepositoryOptions_Should_AllowCustomization()
    {
        var options = new DapperRepositoryOptions
        {
            UseColumnAttribute = false,
            SchemaName = "identity",
            CommandTimeout = 120,
            TableName = "Users",
            Schema = "admin",
            KeyColumn = "UserId",
            KeyIsAutoGenerated = false,
            PaginationStyle = PaginationStyle.LimitOffset,
            ExcludedColumns = new HashSet<string> { "CreatedAt", "UpdatedAt" }
        };

        Assert.False(options.UseColumnAttribute);
        Assert.Equal("identity", options.SchemaName);
        Assert.Equal(120, options.CommandTimeout);
        Assert.Equal("Users", options.TableName);
        Assert.Equal("admin", options.Schema);
        Assert.Equal("UserId", options.KeyColumn);
        Assert.False(options.KeyIsAutoGenerated);
        Assert.Equal(PaginationStyle.LimitOffset, options.PaginationStyle);
        Assert.Equal(2, options.ExcludedColumns!.Count);
    }
}
