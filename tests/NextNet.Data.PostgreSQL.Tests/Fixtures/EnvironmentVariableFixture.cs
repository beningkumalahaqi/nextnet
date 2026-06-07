namespace NextNet.Data.PostgreSQL.Tests.Fixtures;

/// <summary>
/// Fixture that sets up environment variables for connection resolver tests.
/// Restores original values on Dispose.
/// </summary>
public sealed class EnvironmentVariableFixture : IDisposable
{
    private readonly Dictionary<string, string?> _originalValues = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets an environment variable to the specified value, saving the original value.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    /// <param name="value">The value to set.</param>
    public void SetVariable(string name, string value)
    {
        _originalValues[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }

    /// <summary>
    /// Clears an environment variable, saving the original value.
    /// </summary>
    /// <param name="name">The environment variable name.</param>
    public void ClearVariable(string name)
    {
        _originalValues[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, null);
    }

    /// <summary>
    /// Restores all original environment variable values.
    /// </summary>
    public void Dispose()
    {
        foreach (var (name, value) in _originalValues)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
