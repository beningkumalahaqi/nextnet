#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace NextNet.Data.Sdk.Base;

/// <summary>
/// Describes the current state of a data provider.
/// </summary>
public enum ProviderStatus
{
    /// <summary>The provider instance has been created but not yet initialized.</summary>
    Created = 0,

    /// <summary>The provider is currently initializing.</summary>
    Initializing = 1,

    /// <summary>The provider has been successfully initialized and is ready.</summary>
    Initialized = 2,

    /// <summary>The provider initialization failed.</summary>
    InitializationFailed = 3,

    /// <summary>The provider has been disposed and is no longer usable.</summary>
    Disposed = 4,

    /// <summary>The provider is in a degraded state (some connections unhealthy).</summary>
    Degraded = 5,

    /// <summary>The provider is unhealthy (all connections down).</summary>
    Unhealthy = 6
}

/// <summary>
/// Exception thrown when a data provider fails to initialize.
/// </summary>
public sealed class ProviderInitializationException : Exception
{
    /// <summary>
    /// Gets the validation errors that caused initialization to fail, if any.
    /// </summary>
    public IReadOnlyList<string>? ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderInitializationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">Optional inner exception.</param>
    public ProviderInitializationException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderInitializationException"/> class with validation errors.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="validationErrors">The list of validation errors.</param>
    /// <param name="inner">Optional inner exception.</param>
    public ProviderInitializationException(string message, IReadOnlyList<string> validationErrors, Exception? inner = null)
        : base(message, inner)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown when a repository operation fails.
/// </summary>
public sealed class RepositoryException : Exception
{
    /// <summary>
    /// Gets the entity ID that was involved in the failed operation, if applicable.
    /// </summary>
    public object? EntityId { get; }

    /// <summary>
    /// Gets the name of the operation that failed (e.g., "Find", "Insert", "Update", "Delete").
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">Optional inner exception.</param>
    public RepositoryException(string message, Exception? inner = null)
        : base(message, inner)
    {
        Operation = "Unknown";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryException"/> class with entity and operation details.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="entityId">The entity ID involved in the operation.</param>
    /// <param name="operation">The operation name.</param>
    /// <param name="inner">Optional inner exception.</param>
    public RepositoryException(string message, object? entityId, string operation, Exception? inner = null)
        : base(message, inner)
    {
        EntityId = entityId;
        Operation = operation;
    }
}

/// <summary>
/// Base class for NextNet data providers. Implements <see cref="IDataProvider"/>
/// with lifecycle management, configuration validation, and health check dispatch.
/// </summary>
/// <remarks>
/// <para>
/// Provider authors override <see cref="InitializeCoreAsync"/> to perform
/// provider-specific initialization (e.g., opening connection pools, creating
/// DbContext factories, verifying database reachability). The base class handles
/// config validation, status tracking, and the dispose pattern.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [DataProvider(Name = "MyCustom", Description = "My custom provider")]
/// public class MyCustomDataProvider : DataProviderBase
/// {
///     public MyCustomDataProvider(ILogger&lt;MyCustomDataProvider&gt; logger)
///         : base(logger) { }
///
///     protected override Task InitializeCoreAsync(DataConfig config, CancellationToken ct)
///     {
///         // Provider-specific initialization
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public abstract class DataProviderBase : IDataProvider, IDisposable
{
    private readonly ILogger? _logger;
    private bool _disposed;

    /// <summary>
    /// Gets the provider name. Derived from <see cref="DataProviderAttribute.Name"/>
    /// or the class name (stripped of "DataProvider" suffix).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display-friendly provider name.
    /// </summary>
    public string DisplayName { get; protected set; }

    /// <summary>
    /// Gets the provider version.
    /// </summary>
    public Version Version { get; protected set; }

    /// <summary>
    /// Gets whether the provider has been successfully initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Gets the timestamp of the last initialization attempt.
    /// </summary>
    public DateTime? LastInitializedAt { get; private set; }

    /// <summary>
    /// Gets the current provider status.
    /// </summary>
    public ProviderStatus Status { get; protected set; } = ProviderStatus.Created;

    /// <summary>
    /// Gets the health check provider. Created by <see cref="CreateHealthCheckProvider"/>
    /// during initialization. The base class uses this for <see cref="IsHealthyAsync"/>.
    /// </summary>
    protected IHealthCheckProvider? HealthCheckProvider { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataProviderBase"/> class.
    /// </summary>
    /// <param name="logger">An optional logger for diagnostic output.</param>
    protected DataProviderBase(ILogger? logger = null)
    {
        _logger = logger;
        Name = DeriveProviderName();
        DisplayName = Name;
        Version = new Version(0, 1, 0);
    }

    /// <summary>
    /// Initializes the provider. Validates <paramref name="config"/>, calls
    /// <see cref="InitializeCoreAsync"/>, creates the health check provider,
    /// and sets status to <see cref="ProviderStatus.Initialized"/>.
    /// </summary>
    /// <param name="config">The data configuration.</param>
    /// <param name="cancellationToken">A token to cancel initialization.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    /// <exception cref="ProviderInitializationException">Thrown when initialization fails.</exception>
    public async Task InitializeAsync(DataConfig config, CancellationToken cancellationToken = default)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        if (_disposed)
            throw new ObjectDisposedException(Name);

        Status = ProviderStatus.Initializing;
        _logger?.LogInformation("Initializing data provider '{ProviderName}'...", Name);

        try
        {
            // Validate configuration
            var validationErrors = ValidateConfig(config);
            if (validationErrors is { Count: > 0 })
            {
                Status = ProviderStatus.InitializationFailed;
                var errorMessage = $"Provider '{Name}' configuration validation failed: {string.Join("; ", validationErrors)}";
                _logger?.LogError(errorMessage);
                throw new ProviderInitializationException(errorMessage, validationErrors);
            }

            // Provider-specific initialization
            await InitializeCoreAsync(config, cancellationToken).ConfigureAwait(false);

            // Create health check provider
            HealthCheckProvider = CreateHealthCheckProvider();

            // Mark as initialized
            IsInitialized = true;
            LastInitializedAt = DateTime.UtcNow;
            Status = ProviderStatus.Initialized;

            _logger?.LogInformation("Data provider '{ProviderName}' initialized successfully.", Name);
        }
        catch (OperationCanceledException)
        {
            Status = ProviderStatus.Created;
            _logger?.LogWarning("Data provider '{ProviderName}' initialization was cancelled.", Name);
            throw;
        }
        catch (ProviderInitializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Status = ProviderStatus.InitializationFailed;
            _logger?.LogError(ex, "Data provider '{ProviderName}' initialization failed.", Name);
            throw new ProviderInitializationException($"Provider '{Name}' initialization failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Provider-specific initialization logic. Called by <see cref="InitializeAsync"/>
    /// after configuration validation.
    /// </summary>
    /// <param name="config">The validated data configuration.</param>
    /// <param name="cancellationToken">A token to cancel initialization.</param>
    protected abstract Task InitializeCoreAsync(DataConfig config, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the provider is healthy. Delegates to <see cref="HealthCheckProvider"/>
    /// if available; otherwise returns a healthy result with a warning.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the health check.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating the provider's health.</returns>
    public async Task<HealthCheckResult> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        if (HealthCheckProvider != null)
            return await HealthCheckProvider.GetHealthCheckAsync(cancellationToken).ConfigureAwait(false);

        return new HealthCheckResult(
            IsHealthy: true,
            Status: "Healthy",
            Duration: TimeSpan.Zero,
            Message: "No health check provider registered; assumed healthy.");
    }

    /// <summary>
    /// Creates the provider's health check implementation.
    /// Called during <see cref="InitializeAsync"/> after <see cref="InitializeCoreAsync"/>.
    /// Override to provide a custom health check. Returns <c>null</c> by default.
    /// </summary>
    /// <returns>An <see cref="IHealthCheckProvider"/> instance, or <c>null</c>.</returns>
    protected virtual IHealthCheckProvider? CreateHealthCheckProvider()
    {
        return null;
    }

    /// <summary>
    /// Validates the provider-specific configuration.
    /// Called by <see cref="InitializeAsync"/> before <see cref="InitializeCoreAsync"/>.
    /// Return validation error messages; return empty list if valid.
    /// </summary>
    /// <param name="config">The data configuration to validate.</param>
    /// <returns>A list of validation errors. Empty if valid.</returns>
    protected virtual IReadOnlyList<string> ValidateConfig(DataConfig config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.DefaultConnection))
            errors.Add("DefaultConnection is not configured.");

        if (config.Connections == null || config.Connections.Count == 0)
            errors.Add("No connections are configured.");

        return errors;
    }

    /// <summary>
    /// Disposes managed resources. Override to clean up provider-specific resources
    /// (connections, pools, etc.). Called when the provider is shut down.
    /// </summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/>; <c>false</c> from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Status = ProviderStatus.Disposed;
            IsInitialized = false;
        }

        _disposed = true;
    }

    /// <summary>
    /// Disposes the provider and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private string DeriveProviderName()
    {
        var className = GetType().Name;
        const string suffix = "DataProvider";
        if (className.EndsWith(suffix, StringComparison.Ordinal) && className.Length > suffix.Length)
            return className.Substring(0, className.Length - suffix.Length);
        return className;
    }
}
#endif
