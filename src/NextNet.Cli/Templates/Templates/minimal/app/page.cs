namespace {{NAMESPACE}}.App.Pages;

/// <summary>
/// Home page — the root route (/).
/// </summary>
public class HomePage
{
    public string Render()
    {
        return $"""
        <h1>Welcome to {{PROJECT_NAME}}</h1>
        <p>Built with NextNet.</p>
        """;
    }
}
