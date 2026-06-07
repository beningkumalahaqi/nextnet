using NextNet.Data.MultiDb.Internal;

namespace NextNet.Data.MultiDb.Extensions;

/// <summary>
/// Extension methods for validating the multi-database configuration at startup.
/// </summary>
/// <remarks>
/// <para>
/// These extensions support eager validation of the multi-database configuration,
/// ensuring that all named connections are properly configured and their providers
/// are available before the application starts serving requests.
/// </para>
/// <example>
/// <code>
/// var guard = serviceProvider.GetRequiredService&lt;DatabaseSelectorGuard&gt;();
/// var errors = await guard.ProbeAllAsync();
/// if (errors.Count > 0)
/// {
///     throw new InvalidOperationException($"Database configuration errors: {string.Join(", ", errors)}");
/// }
/// </code>
/// </example>
    /// </remarks>
internal static class MultiDbValidationExtensions
{
    /// <summary>
    /// Validates all registered connections and throws if any validation errors are found.
    /// </summary>
    /// <param name="guard">The database selector guard.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="guard"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation errors are found.</exception>
    internal static async Task EnsureValidAsync(
        this DatabaseSelectorGuard guard,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(guard);

        var errors = guard.Validate();
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Multi-database configuration validation failed: {string.Join("; ", errors)}");
        }

        var probeErrors = await guard.ProbeAllAsync(cancellationToken);
        if (probeErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Multi-database health probe failed: {string.Join("; ", probeErrors)}");
        }
    }
}
