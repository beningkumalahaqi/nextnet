namespace NextNet.Data.Sdk;

/// <summary>
/// Marks a repository class as an <see cref="IRepository{T}"/> implementation.
/// Enables analyzer validation of the repository signature and tooling support.
/// </summary>
/// <typeparam name="TEntity">The entity type this repository manages.</typeparam>
/// <remarks>
/// <para>
/// Apply this attribute to repository classes to enable SDK tooling support,
/// including analyzer validation that the class correctly implements
/// <see cref="IRepository{T}"/> and follows naming conventions.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [DataRepository&lt;User&gt;]
/// public class UserRepository : RepositoryBase&lt;User&gt;
/// {
///     // Repository implementation
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataRepositoryAttribute<TEntity> : Attribute
    where TEntity : class
{
    /// <summary>
    /// Gets or sets an optional connection name override for the repository.
    /// </summary>
    public string? ConnectionName { get; set; }
}
