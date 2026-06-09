using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace NextNet.Edge.Middleware;

/// <summary>
/// Provides an edge-compatible HTTP context abstraction.
/// In edge environments, the full <see cref="HttpContext"/> may not be available,
/// so this wraps the minimal set of features needed for NextNet to function.
/// </summary>
public sealed class EdgeHttpContext
{
    /// <summary>
    /// Gets the edge request.
    /// </summary>
    public EdgeRequest Request { get; }

    /// <summary>
    /// Gets the edge response.
    /// </summary>
    public EdgeResponse Response { get; }

    /// <summary>
    /// Gets a per-request items dictionary for sharing data.
    /// </summary>
    public IDictionary<string, object?> Items { get; }

    /// <summary>
    /// Gets or sets the request trace identifier.
    /// </summary>
    public string TraceIdentifier { get; set; }

    /// <summary>
    /// Gets the connection information (limited on edge).
    /// </summary>
    public EdgeConnectionInfo Connection { get; }

    /// <summary>
    /// Gets the user principal, if authentication is available.
    /// </summary>
    public ClaimsPrincipal? User { get; }

    /// <summary>
    /// Gets or sets the request aborted cancellation token.
    /// </summary>
    public CancellationToken RequestAborted { get; set; }

    /// <summary>
    /// Gets the underlying ASP.NET Core <see cref="HttpContext"/>, if available.
    /// Null when running on a pure edge runtime (Cloudflare Workers, Deno Deploy).
    /// </summary>
    public HttpContext? AspNetCoreHttpContext { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeHttpContext"/>.
    /// </summary>
    /// <param name="request">The edge request.</param>
    /// <param name="response">The edge response.</param>
    /// <param name="httpContext">Optional underlying ASP.NET Core HTTP context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> or <paramref name="response"/> is null.</exception>
    public EdgeHttpContext(
        EdgeRequest request,
        EdgeResponse response,
        HttpContext? httpContext = null)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Response = response ?? throw new ArgumentNullException(nameof(response));
        AspNetCoreHttpContext = httpContext;
        Items = new Dictionary<string, object?>();
        TraceIdentifier = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
        Connection = new EdgeConnectionInfo();
        RequestAborted = CancellationToken.None;
    }

    /// <summary>
    /// Converts this edge HTTP context to an ASP.NET Core <see cref="HttpContext"/>,
    /// or returns the underlying one if it exists.
    /// </summary>
    /// <returns>An <see cref="HttpContext"/> compatible with ASP.NET Core middleware.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no underlying ASP.NET Core context exists and conversion is not possible.</exception>
    public HttpContext ToAspNetCoreHttpContext()
    {
        if (AspNetCoreHttpContext != null)
            return AspNetCoreHttpContext;

        throw new InvalidOperationException(
            $"[{EdgeErrorCodes.CannotConvertToAspNetCoreContext}] Cannot convert EdgeHttpContext to ASP.NET Core HttpContext " +
            "when running on a pure edge runtime. Wrap edge-specific code in " +
            "#if !EDGE_RUNTIME conditionals.");
    }
}

/// <summary>
/// Provides minimal connection information available on edge runtimes.
/// </summary>
public sealed record EdgeConnectionInfo
{
    /// <summary>
    /// Gets or sets the remote IP address (may be null on some edge providers).
    /// </summary>
    public string? RemoteIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the remote port (may be null on some edge providers).
    /// </summary>
    public int? RemotePort { get; set; }

    /// <summary>
    /// Gets or sets the country code from the edge provider's geo-location data.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the city from the edge provider's geo-location data.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the continent code from the edge provider's geo-location data.
    /// </summary>
    public string? Continent { get; set; }

    /// <summary>
    /// Gets or sets the latitude from the edge provider's geo-location data.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude from the edge provider's geo-location data.
    /// </summary>
    public double? Longitude { get; set; }
}
