namespace NextNet.Plugins.Tests.TestInfrastructure;

/// <summary>
/// A minimal mock service provider for use in tests.
/// Returns null for all service resolution requests.
/// </summary>
public class MockServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return null;
    }
}
