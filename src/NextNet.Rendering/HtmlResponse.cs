using System.Text;
using Microsoft.AspNetCore.Http;
using NextNet.Components;

namespace NextNet.Rendering;

/// <summary>
/// An ASP.NET Core <see cref="IResult"/> that writes HTML content to the HTTP response.
/// Sets appropriate headers including Content-Type, Cache-Control, and Content-Length.
/// </summary>
public class HtmlResponse : IResult
{
    /// <summary>
    /// Gets the HTML content to write to the response.
    /// </summary>
    public IHtmlContent Content { get; }

    /// <summary>
    /// Gets the HTTP status code for the response.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the Cache-Control header value, or <c>null</c> to omit.
    /// </summary>
    public string? CacheControl { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="HtmlResponse"/>.
    /// </summary>
    /// <param name="content">The HTML content to write.</param>
    /// <param name="statusCode">The HTTP status code (default 200).</param>
    /// <param name="cacheControl">The Cache-Control header value, or <c>null</c> to omit.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is null.</exception>
    public HtmlResponse(IHtmlContent content, int statusCode = 200, string? cacheControl = null)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        StatusCode = statusCode;
        CacheControl = cacheControl;
    }

    /// <summary>
    /// Executes the result by writing the HTML content to the HTTP response.
    /// Uses <see cref="IHtmlContent.WriteToAsync"/> to avoid allocating an intermediate string.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

        var response = httpContext.Response;

        // Set status code
        response.StatusCode = StatusCode;

        // Set content type
        response.ContentType = "text/html; charset=utf-8";

        // Set caching headers
        if (CacheControl != null)
        {
            response.Headers.CacheControl = CacheControl;
        }

        // Suppress content-length for unknown sizes (let ASP.NET Core handle chunked encoding)
        response.Headers.ContentLength = null;

        // Write the HTML content via the IHtmlContent writer, avoiding string allocation
        using var writer = new StreamWriter(response.Body, Encoding.UTF8, leaveOpen: true);
        await Content.WriteToAsync(writer);
        await writer.FlushAsync();
    }

    /// <summary>
    /// Converts the HTML content to a string representation (for debugging/inspection).
    /// </summary>
    public override string ToString()
        => Content.ToHtml();

    /// <summary>
    /// Creates a 404 Not Found HTML response.
    /// </summary>
    public static HtmlResponse NotFound()
    {
        var content = new NotFoundHtmlContent();
        return new HtmlResponse(content, StatusCodes.Status404NotFound, "no-cache");
    }

    /// <summary>
    /// Creates a 301 redirect response with an HTML body.
    /// </summary>
    public static HtmlResponse Redirect(string location)
    {
        var content = new RedirectHtmlContent(location);
        return new HtmlResponse(content, StatusCodes.Status301MovedPermanently, "no-cache");
    }

    /// <summary>
    /// IHtmlContent for 404 pages.
    /// </summary>
    private sealed class NotFoundHtmlContent : IHtmlContent
    {
        public Task WriteToAsync(TextWriter writer)
        {
            return writer.WriteAsync(ToHtml());
        }

        public string ToHtml()
            => "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">" +
               "<title>404 — Not Found</title>" +
               "<style>body{font-family:sans-serif;padding:2rem;text-align:center;}" +
               "h1{font-size:4rem;margin:0;color:#999;}p{color:#666;}</style>" +
               "</head><body><h1>404</h1><p>The requested page was not found.</p></body></html>";
    }

    /// <summary>
    /// IHtmlContent for redirect responses.
    /// </summary>
    private sealed class RedirectHtmlContent : IHtmlContent
    {
        private readonly string _location;

        public RedirectHtmlContent(string location)
        {
            _location = location ?? throw new ArgumentNullException(nameof(location));
        }

        public Task WriteToAsync(TextWriter writer)
        {
            return writer.WriteAsync(ToHtml());
        }

        public string ToHtml()
            => $"<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">" +
               $"<meta http-equiv=\"refresh\" content=\"0;url={System.Net.WebUtility.HtmlEncode(_location)}\">" +
               $"<title>Redirecting...</title></head><body>" +
               $"<p>Redirecting to <a href=\"{System.Net.WebUtility.HtmlEncode(_location)}\">{System.Net.WebUtility.HtmlEncode(_location)}</a>...</p>" +
               $"</body></html>";
    }
}
