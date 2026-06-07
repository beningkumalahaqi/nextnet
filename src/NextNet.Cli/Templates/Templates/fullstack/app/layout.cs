namespace {{NAMESPACE}}.App;

/// <summary>
/// Root layout component — wraps all pages.
/// </summary>
public class RootLayout
{
    public string Render(string body)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{{PROJECT_NAME}}</title>
            <link rel="stylesheet" href="/app.css" />
        </head>
        <body>
            <header>
                <nav>
                    <a href="/">Home</a>
                    <a href="/about">About</a>
                    <a href="/blog">Blog</a>
                </nav>
            </header>
            <main>
                {body}
            </main>
            <footer>
                <p>&copy; {DateTime.Now.Year} {{PROJECT_NAME}}</p>
            </footer>
        </body>
        </html>
        """;
    }
}
