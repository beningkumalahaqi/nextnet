using System.Xml.Linq;
using NextNet.Plugins.Hooks;
using NextNet.Routing;

namespace NextNet.Plugins.BuiltIn;

/// <summary>
/// Built-in plugin that generates a <c>sitemap.xml</c> from the route manifest
/// at build time. Implements <see cref="IRouteScannerHook"/> to capture routes
/// and <see cref="IBuildHook"/> to write the sitemap after the build completes.
/// </summary>
public sealed class SitemapPlugin : NextNetPlugin, IRouteScannerHook, IBuildHook
{
    private RouteManifest? _manifest;
    private readonly object _lock = new();

    /// <inheritdoc />
    public override string Name => "SitemapGenerator";

    /// <inheritdoc />
    public override string Description => "Generates sitemap.xml from the route manifest at build time.";

    /// <inheritdoc />
    public override Version Version => new(1, 0, 0);

    /// <summary>
    /// Gets or sets the output directory for the sitemap file.
    /// Defaults to <c>public</c>.
    /// </summary>
    public string OutputDirectory { get; set; } = "public";

    /// <summary>
    /// Gets or sets the base URL used to build absolute sitemap entries.
    /// Must be set before build (e.g., "https://example.com").
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <inheritdoc />
    public Task OnRoutesDiscovered(PluginContext ctx, RouteManifest manifest)
    {
        lock (_lock)
        {
            _manifest = manifest;
        }

        ctx.Logger.Info("[SitemapPlugin] Captured {0} routes from manifest.", manifest.Routes.Count);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task OnBuildStart(PluginContext ctx)
    {
        // No action needed before build
        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task OnBuildEnd(PluginContext ctx)
    {
        RouteManifest? snapshot;
        lock (_lock)
        {
            snapshot = _manifest;
        }

        if (snapshot == null)
        {
            ctx.Logger.Warn("[SitemapPlugin] No route manifest available. Skipping sitemap generation.");
            return;
        }

        var baseUrl = BaseUrl ?? "https://localhost";
        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";

        var outputDir = Path.GetFullPath(OutputDirectory);

        try
        {
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, "sitemap.xml");

            var doc = BuildSitemapDocument(snapshot, baseUrl);
            doc.Save(outputPath);

            ctx.Logger.Info("[SitemapPlugin] Generated sitemap at {0} with {1} entries.",
                outputPath, snapshot.Pages.Count);
        }
        catch (Exception ex)
        {
            ctx.Logger.Error("[SitemapPlugin] Failed to generate sitemap: {0}", ex.Message);
        }

        await Task.CompletedTask;
    }

    private static XDocument BuildSitemapDocument(RouteManifest manifest, string baseUrl)
    {
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var urlset = new XElement(ns + "urlset");

        foreach (var page in manifest.Pages)
        {
            var route = page.RoutePattern.TrimStart('/');
            var url = baseUrl + route;

            var urlElement = new XElement(ns + "url",
                new XElement(ns + "loc", url),
                new XElement(ns + "changefreq", "weekly"),
                new XElement(ns + "priority", "0.5")
            );

            urlset.Add(urlElement);
        }

        // Include the root page if pages exist but root isn't explicitly listed
        if (manifest.Pages.Count > 0 && !manifest.Pages.Any(p =>
                p.RoutePattern == "/" || p.RoutePattern == ""))
        {
            urlset.AddFirst(new XElement(ns + "url",
                new XElement(ns + "loc", baseUrl.TrimEnd('/')),
                new XElement(ns + "changefreq", "daily"),
                new XElement(ns + "priority", "1.0")
            ));
        }

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            urlset
        );
    }
}
