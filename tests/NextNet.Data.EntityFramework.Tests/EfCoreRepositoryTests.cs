namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="EfCoreRepository{T}"/> using InMemory database.
/// </summary>
public sealed class EfCoreRepositoryTests : IDisposable
{
    private readonly InMemoryDbContextFactory _fixture = new();
    private readonly EfCoreRepository<TestEntity> _repository;

    public EfCoreRepositoryTests()
    {
        var factory = _fixture.CreateFactory();
        _repository = new EfCoreRepository<TestEntity>(factory);
    }

    [Fact]
    public async Task FindAsync_Should_ReturnEntity_When_EntityExists()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Value = 100, IsActive = true };
        await _repository.InsertAsync(entity);

        // Act
        var result = await _repository.FindAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(100, result.Value);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task FindAsync_Should_ReturnNull_When_EntityDoesNotExist()
    {
        // Act
        var result = await _repository.FindAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllEntities()
    {
        // Arrange
        await _repository.InsertAsync(new TestEntity { Name = "A", Value = 10 });
        await _repository.InsertAsync(new TestEntity { Name = "B", Value = 20 });
        await _repository.InsertAsync(new TestEntity { Name = "C", Value = 30 });

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_Should_SupportPagination()
    {
        // Arrange
        for (var i = 1; i <= 10; i++)
        {
            await _repository.InsertAsync(new TestEntity { Name = $"Item{i}", Value = i });
        }

        var options = new RepositoryQueryOptions(Page: 1, PageSize: 3);

        // Act
        var result = await _repository.GetAllAsync(options);

        // Assert
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetAllAsync_Should_SupportLastPage()
    {
        // Arrange
        for (var i = 1; i <= 5; i++)
        {
            await _repository.InsertAsync(new TestEntity { Name = $"Item{i}", Value = i });
        }

        var options = new RepositoryQueryOptions(Page: 2, PageSize: 3);

        // Act
        var result = await _repository.GetAllAsync(options);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task GetAllAsync_Should_SupportSorting()
    {
        // Arrange
        await _repository.InsertAsync(new TestEntity { Name = "C", Value = 30 });
        await _repository.InsertAsync(new TestEntity { Name = "A", Value = 10 });
        await _repository.InsertAsync(new TestEntity { Name = "B", Value = 20 });

        var options = new RepositoryQueryOptions(SortBy: "Name", SortDescending: false);

        // Act
        var result = await _repository.GetAllAsync(options);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("A", result.Items[0].Name);
        Assert.Equal("B", result.Items[1].Name);
        Assert.Equal("C", result.Items[2].Name);
    }

    [Fact]
    public async Task GetAllAsync_Should_SupportSortingDescending()
    {
        // Arrange
        await _repository.InsertAsync(new TestEntity { Name = "C", Value = 30 });
        await _repository.InsertAsync(new TestEntity { Name = "A", Value = 10 });
        await _repository.InsertAsync(new TestEntity { Name = "B", Value = 20 });

        var options = new RepositoryQueryOptions(SortBy: "Name", SortDescending: true);

        // Act
        var result = await _repository.GetAllAsync(options);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("C", result.Items[0].Name);
        Assert.Equal("B", result.Items[1].Name);
        Assert.Equal("A", result.Items[2].Name);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnEmpty_When_NoEntities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task InsertAsync_Should_PersistEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "New", Value = 50, IsActive = true };

        // Act
        await _repository.InsertAsync(entity);

        // Assert - verify by finding it
        var result = await _repository.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("New", result.Name);
    }

    [Fact]
    public async Task InsertAsync_Should_GenerateId()
    {
        // Arrange
        var entity = new TestEntity { Name = "GeneratedId", Value = 99 };

        // Act
        await _repository.InsertAsync(entity);

        // Assert
        Assert.True(entity.Id > 0);
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Original", Value = 100 };
        await _repository.InsertAsync(entity);

        // Act
        entity.Name = "Updated";
        entity.Value = 200;
        await _repository.UpdateAsync(entity);

        // Assert
        var result = await _repository.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal(200, result.Value);
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "DeleteMe", Value = 42 };
        await _repository.InsertAsync(entity);

        // Act
        await _repository.DeleteAsync(entity.Id);

        // Assert
        var result = await _repository.FindAsync(entity.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_ThrowKeyNotFoundException_When_EntityNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _repository.DeleteAsync(999));
    }

    [Fact]
    public async Task MultipleOperations_Should_WorkSequentially()
    {
        // Arrange & Act
        await _repository.InsertAsync(new TestEntity { Name = "First", Value = 1 });
        await _repository.InsertAsync(new TestEntity { Name = "Second", Value = 2 });

        var all = await _repository.GetAllAsync();
        Assert.Equal(2, all.TotalCount);

        var first = await _repository.FindAsync(1);
        Assert.NotNull(first);
        Assert.Equal("First", first.Name);

        first.Name = "FirstUpdated";
        await _repository.UpdateAsync(first);

        var updated = await _repository.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("FirstUpdated", updated.Name);

        await _repository.DeleteAsync(2);
        var remaining = await _repository.GetAllAsync();
        Assert.Equal(1, remaining.TotalCount);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
