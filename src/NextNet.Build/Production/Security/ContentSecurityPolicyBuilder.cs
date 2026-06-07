using System.Text;

namespace NextNet.Build.Production.Security;

/// <summary>
/// Builder for constructing Content-Security-Policy headers with a fluent API.
/// </summary>
public class ContentSecurityPolicyBuilder
{
    private readonly Dictionary<string, List<string>> _directives = new();
    private bool _reportOnly;

    /// <summary>
    /// Creates a new CSP builder with secure defaults.
    /// </summary>
    public static ContentSecurityPolicyBuilder CreateDefault()
    {
        return new ContentSecurityPolicyBuilder()
            .WithDefaultSrc("'self'")
            .WithScriptSrc("'self'")
            .WithStyleSrc("'self'")
            .WithImgSrc("'self'", "data:", "https:")
            .WithFontSrc("'self'")
            .WithConnectSrc("'self'")
            .WithFrameAncestors("'none'")
            .WithFormAction("'self'")
            .WithBaseUri("'self'")
            .WithObjectSrc("'none'");
    }

    /// <summary>
    /// Sets the default-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithDefaultSrc(params string[] sources)
    {
        _directives["default-src"] = new List<string>(sources);
        return this;
    }

    /// <summary>
    /// Adds sources to the script-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithScriptSrc(params string[] sources)
    {
        AddDirective("script-src", sources);
        return this;
    }

    /// <summary>
    /// Adds sources to the style-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithStyleSrc(params string[] sources)
    {
        AddDirective("style-src", sources);
        return this;
    }

    /// <summary>
    /// Adds sources to the img-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithImgSrc(params string[] sources)
    {
        AddDirective("img-src", sources);
        return this;
    }

    /// <summary>
    /// Adds sources to the font-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithFontSrc(params string[] sources)
    {
        AddDirective("font-src", sources);
        return this;
    }

    /// <summary>
    /// Adds sources to the connect-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithConnectSrc(params string[] sources)
    {
        AddDirective("connect-src", sources);
        return this;
    }

    /// <summary>
    /// Sets the frame-ancestors directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithFrameAncestors(params string[] sources)
    {
        AddDirective("frame-ancestors", sources);
        return this;
    }

    /// <summary>
    /// Sets the form-action directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithFormAction(params string[] sources)
    {
        AddDirective("form-action", sources);
        return this;
    }

    /// <summary>
    /// Sets the base-uri directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithBaseUri(params string[] sources)
    {
        AddDirective("base-uri", sources);
        return this;
    }

    /// <summary>
    /// Sets the object-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithObjectSrc(params string[] sources)
    {
        AddDirective("object-src", sources);
        return this;
    }

    /// <summary>
    /// Sets the frame-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithFrameSrc(params string[] sources)
    {
        AddDirective("frame-src", sources);
        return this;
    }

    /// <summary>
    /// Sets the worker-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithWorkerSrc(params string[] sources)
    {
        AddDirective("worker-src", sources);
        return this;
    }

    /// <summary>
    /// Sets the manifest-src directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithManifestSrc(params string[] sources)
    {
        AddDirective("manifest-src", sources);
        return this;
    }

    /// <summary>
    /// Sets the report-uri for CSP violation reports.
    /// </summary>
    public ContentSecurityPolicyBuilder WithReportUri(string uri)
    {
        AddDirective("report-uri", new[] { uri });
        return this;
    }

    /// <summary>
    /// Sets the report-to for CSP violation reports (Reporting API).
    /// </summary>
    public ContentSecurityPolicyBuilder WithReportTo(string groupName)
    {
        AddDirective("report-to", new[] { groupName });
        return this;
    }

    /// <summary>
    /// Configures the policy as report-only (Content-Security-Policy-Report-Only).
    /// </summary>
    public ContentSecurityPolicyBuilder AsReportOnly()
    {
        _reportOnly = true;
        return this;
    }

    /// <summary>
    /// Adds a custom directive.
    /// </summary>
    public ContentSecurityPolicyBuilder WithDirective(string directive, params string[] sources)
    {
        AddDirective(directive, sources);
        return this;
    }

    /// <summary>
    /// Builds the CSP header value string.
    /// </summary>
    public string Build()
    {
        var sb = new StringBuilder();

        foreach (var directive in _directives)
        {
            if (sb.Length > 0)
                sb.Append("; ");

            sb.Append(directive.Key);
            sb.Append(' ');
            sb.Append(string.Join(' ', directive.Value));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the header name to use (Content-Security-Policy or Content-Security-Policy-Report-Only).
    /// </summary>
    public string GetHeaderName()
    {
        return _reportOnly
            ? "Content-Security-Policy-Report-Only"
            : "Content-Security-Policy";
    }

    private void AddDirective(string directive, string[] sources)
    {
        if (_directives.TryGetValue(directive, out var existing))
        {
            existing.AddRange(sources);
        }
        else
        {
            _directives[directive] = new List<string>(sources);
        }
    }
}
