namespace NextNet.UI.DesignSystem.Tests;

/// <summary>
/// A minimal <see cref="IServiceProvider"/> implementation that returns <c>null</c>
/// for all service requests. Used in unit tests where service resolution is not required.
/// </summary>
public sealed class EmptyServiceProvider : IServiceProvider
{
    /// <summary>
    /// Returns <c>null</c> for any service type requested.
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns><c>null</c> for all service types.</returns>
    public object? GetService(Type serviceType) => null;
}
