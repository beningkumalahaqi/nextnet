#if NET8_0_OR_GREATER
using System;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Registration;
using NextNet.Data.Sdk.Base;

namespace NextNet.Data.Sdk.Extensions;

/// <summary>
/// Extension methods for registering NextNet Data Provider SDK components in the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// These extensions simplify the registration of provider SDK base classes and services.
/// Provider authors can call <c>services.AddNextNetDataProvider&lt;T&gt;()</c> in their
/// service collection extension methods to register the provider, connection manager,
/// health check provider, and other SDK services.
/// </para>
/// </remarks>
public static class ProviderSdkExtensions
{
    /// <summary>
    /// Registers a NextNet data provider and its associated services in the DI container.
    /// </summary>
    /// <typeparam name="TProvider">The data provider type (must implement <see cref="IDataProvider"/>).</typeparam>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="configureOptions">An optional delegate to configure provider-specific options.</param>
    /// <returns>The <see cref="NextNetDataBuilder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static NextNetDataBuilder AddDataProvider<TProvider>(
        this NextNetDataBuilder builder,
        Action<object>? configureOptions = null)
        where TProvider : class, IDataProvider
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.AddSingleton<IDataProvider, TProvider>();

        return builder;
    }

    /// <summary>
    /// Registers a repository with the DI container.
    /// </summary>
    /// <typeparam name="TRepository">The repository type.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <returns>The <see cref="NextNetDataBuilder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static NextNetDataBuilder AddRepository<TRepository, TEntity>(
        this NextNetDataBuilder builder)
        where TRepository : class, IRepository<TEntity>
        where TEntity : class
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.AddSingleton<IRepository<TEntity>, TRepository>();

        return builder;
    }
}
#endif
