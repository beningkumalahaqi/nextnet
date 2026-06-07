namespace {{NAMESPACE}}.App.Pages;

/// <summary>
/// Home page — the root route (/).
/// </summary>
public class HomePage
{
    public string Render()
    {
        return $"""
        <section class="hero">
            <h1>Welcome to {{PROJECT_NAME}}</h1>
            <p>Built with NextNet — a full-stack web framework for .NET.</p>
            <a href="/about" class="btn">Learn More</a>
        </section>
        """;
    }
}
