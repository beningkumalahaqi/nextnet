using System.ComponentModel.DataAnnotations;

namespace NextNet.Data.EntityFramework.Tests.Fixtures;

/// <summary>
/// Simple test entity for use in repository and provider tests.
/// </summary>
public sealed class TestEntity
{
    /// <summary>
    /// Gets or sets the primary key.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the entity value.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets whether the entity is active.
    /// </summary>
    public bool IsActive { get; set; }
}
