using BenchmarkDotNet.Attributes;

namespace NextNet.Data.Benchmarks.Benchmarks;

/// <summary>
/// Benchmarks simulating repository CRUD throughput.
/// These benchmarks use in-memory operations to measure overhead
/// without actual database round-trips.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
public class RepositoryBenchmarks
{
    private readonly List<BenchmarkEntity> _entities = new();
    private int _nextId;

    /// <summary>
    /// Seeds a small in-memory dataset for CRUD benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < 100; i++)
        {
            _entities.Add(new BenchmarkEntity
            {
                Id = i + 1,
                Name = $"Entity_{i + 1}",
                Value = i * 10
            });
        }
        _nextId = 101;
    }

    /// <summary>
    /// Simulates inserting a new entity.
    /// SLA: &lt; 50ms (actual database).
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Repository")]
    public int Repository_Insert()
    {
        var entity = new BenchmarkEntity
        {
            Id = Interlocked.Increment(ref _nextId),
            Name = "NewEntity",
            Value = 42
        };
        lock (_entities)
        {
            _entities.Add(entity);
        }
        return entity.Id;
    }

    /// <summary>
    /// Simulates finding an entity by its identifier.
    /// SLA: &lt; 30ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Repository")]
    public BenchmarkEntity? Repository_FindById()
    {
        lock (_entities)
        {
            return _entities.FirstOrDefault(e => e.Id == 50);
        }
    }

    /// <summary>
    /// Simulates retrieving a page of results.
    /// SLA: &lt; 50ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Repository")]
    public List<BenchmarkEntity> Repository_GetAll()
    {
        lock (_entities)
        {
            return _entities.Skip(0).Take(20).ToList();
        }
    }

    /// <summary>
    /// Simulates updating an existing entity.
    /// SLA: &lt; 50ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Repository")]
    public bool Repository_Update()
    {
        lock (_entities)
        {
            var entity = _entities.FirstOrDefault(e => e.Id == 50);
            if (entity is null) return false;
            entity.Name = "Updated";
            return true;
        }
    }

    /// <summary>
    /// Simulates deleting an entity.
    /// SLA: &lt; 30ms.
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Repository")]
    public bool Repository_Delete()
    {
        lock (_entities)
        {
            var entity = _entities.FirstOrDefault(e => e.Id == 50);
            if (entity is null) return false;
            return _entities.Remove(entity);
        }
    }
}

/// <summary>
/// Simple entity class used by repository benchmarks.
/// </summary>
public sealed class BenchmarkEntity
{
    /// <summary>Gets or sets the entity identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the entity name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a numeric value.</summary>
    public int Value { get; set; }
}
