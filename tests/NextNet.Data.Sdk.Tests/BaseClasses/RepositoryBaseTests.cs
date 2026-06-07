using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;
using NextNet.Data.Sdk.Base;

namespace NextNet.Data.Sdk.Tests.BaseClasses;

/// <summary>
/// Tests for <see cref="RepositoryBase{T}"/>.
/// </summary>
public class RepositoryBaseTests
{
    [Fact]
    public async Task FindAsync_Should_ReturnEntity_WhenFound()
    {
        var repo = new TestRepository();
        var result = await repo.FindAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task FindAsync_Should_Throw_WhenIdIsNull()
    {
        var repo = new TestRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.FindAsync(null!));
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllEntities()
    {
        var repo = new TestRepository();
        var result = await repo.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnPagedResult()
    {
        var repo = new TestRepository();
        var result = await repo.GetAllAsync(new RepositoryQueryOptions(Page: 1, PageSize: 10));

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task InsertAsync_Should_InsertEntity()
    {
        var repo = new TestRepository();
        var entity = new TestEntity { Id = 4, Name = "New" };

        await repo.InsertAsync(entity);

        Assert.Contains(entity, repo.Entities);
    }

    [Fact]
    public async Task InsertAsync_Should_Throw_WhenEntityIsNull()
    {
        var repo = new TestRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.InsertAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateEntity()
    {
        var repo = new TestRepository();
        var entity = new TestEntity { Id = 1, Name = "Updated" };

        await repo.UpdateAsync(entity);

        var updated = await repo.FindAsync(1);
        Assert.Equal("Updated", updated!.Name);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_WhenEntityIsNull()
    {
        var repo = new TestRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_Should_DeleteEntity()
    {
        var repo = new TestRepository();
        await repo.DeleteAsync(1);

        var result = await repo.FindAsync(1);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_WhenIdIsNull()
    {
        var repo = new TestRepository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.DeleteAsync(null!));
    }

    [Fact]
    public async Task RepositoryException_Should_ContainOperationName()
    {
        var repo = new FailingRepository();

        var ex = await Assert.ThrowsAsync<RepositoryException>(() => repo.FindAsync(1));
        Assert.Equal("Find", ex.Operation);
    }

    /// <summary>
    /// Test entity record.
    /// </summary>
    private sealed record TestEntity
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    /// <summary>
    /// Repository for testing with in-memory storage.
    /// </summary>
    private sealed class TestRepository : RepositoryBase<TestEntity>
    {
        public List<TestEntity> Entities { get; } = new()
        {
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" },
            new TestEntity { Id = 3, Name = "Charlie" }
        };

        public TestRepository() : base(null) { }

        protected override Task<TestEntity?> FindCoreAsync(object id, CancellationToken cancellationToken)
        {
            var entity = Entities.FirstOrDefault(e => e.Id == (int)id);
            return Task.FromResult(entity);
        }

        protected override Task<IReadOnlyList<TestEntity>> GetAllCoreAsync(
            RepositoryQueryOptions? options, CancellationToken cancellationToken)
        {
            IReadOnlyList<TestEntity> result = Entities.ToList();
            return Task.FromResult(result);
        }

        protected override Task InsertCoreAsync(TestEntity entity, CancellationToken cancellationToken)
        {
            Entities.Add(entity);
            return Task.CompletedTask;
        }

        protected override Task UpdateCoreAsync(TestEntity entity, CancellationToken cancellationToken)
        {
            var index = Entities.FindIndex(e => e.Id == entity.Id);
            if (index >= 0)
                Entities[index] = entity;
            return Task.CompletedTask;
        }

        protected override Task DeleteCoreAsync(object id, CancellationToken cancellationToken)
        {
            Entities.RemoveAll(e => e.Id == (int)id);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Repository that always fails for testing exception wrapping.
    /// </summary>
    private sealed class FailingRepository : RepositoryBase<TestEntity>
    {
        public FailingRepository() : base(null) { }

        protected override Task<TestEntity?> FindCoreAsync(object id, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Database failure");

        protected override Task<IReadOnlyList<TestEntity>> GetAllCoreAsync(
            RepositoryQueryOptions? options, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Database failure");

        protected override Task InsertCoreAsync(TestEntity entity, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Database failure");

        protected override Task UpdateCoreAsync(TestEntity entity, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Database failure");

        protected override Task DeleteCoreAsync(object id, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Database failure");
    }
}
