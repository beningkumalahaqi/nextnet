namespace NextNet.Data.EntityFramework.Internal;

/// <summary>
/// Proxy implementation of <see cref="IDbContextFactory{AppDbContext}"/> that wraps
/// another factory and applies additional configuration.
/// </summary>
/// <remarks>
/// <para>
/// This proxy is used to intercept DbContext creation and apply settings from
/// <see cref="EfCoreOptions"/> that are not covered by the standard
/// <c>AddDbContextFactory</c> configuration.
/// </para>
/// </remarks>
internal sealed class DbContextFactoryProxy : IDbContextFactory<AppDbContext>
{
    private readonly IDbContextFactory<AppDbContext> _innerFactory;
    private readonly EfCoreOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextFactoryProxy"/> class.
    /// </summary>
    /// <param name="innerFactory">The inner DbContext factory to delegate to.</param>
    /// <param name="options">The EF Core provider options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerFactory"/> or <paramref name="options"/> is null.</exception>
    public DbContextFactoryProxy(
        IDbContextFactory<AppDbContext> innerFactory,
        EfCoreOptions options)
    {
        _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Creates a new <see cref="AppDbContext"/> instance.
    /// </summary>
    /// <returns>A new DbContext instance.</returns>
    public AppDbContext CreateDbContext()
    {
        var context = _innerFactory.CreateDbContext();
        ConfigureContext(context);
        return context;
    }

    /// <summary>
    /// Creates a new <see cref="AppDbContext"/> instance asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A new DbContext instance.</returns>
    public async Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var context = await _innerFactory.CreateDbContextAsync(cancellationToken);
        ConfigureContext(context);
        return context;
    }

    private void ConfigureContext(AppDbContext context)
    {
        if (_options.EnableSensitiveDataLogging)
        {
            context.ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}
