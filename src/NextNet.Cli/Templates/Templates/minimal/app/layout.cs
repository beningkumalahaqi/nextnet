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
        </head>
        <body>
            <main>
                {body}
            </main>
        </body>
        </html>
        """;
    }
}
