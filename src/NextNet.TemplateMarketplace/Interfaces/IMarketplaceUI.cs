namespace NextNet.TemplateMarketplace;

/// <summary>
/// Future marketplace UI integration point.
/// V4+ will provide implementations for browser, desktop, and in-editor UIs.
/// </summary>
public interface IMarketplaceUI
{
    /// <summary>Opens the marketplace UI, optionally scoped to a specific template.</summary>
    /// <param name="templateName">Optional template name to highlight.</param>
    /// <param name="ct">Cancellation token.</param>
    Task OpenAsync(string? templateName = null, CancellationToken ct = default);
}
