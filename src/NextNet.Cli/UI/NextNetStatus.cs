using Spectre.Console;

namespace NextNet.Cli.UI;

/// <summary>
/// Inline status badges for success, error, warning, and info indicators.
/// Provides both Spectre.Console Markup and plain text variants.
/// </summary>
public static class NextNetStatus
{
    // ── Markup variants (for use inside Spectre.Console tables/trees) ─

    /// <summary>Green checkmark badge.</summary>
    public static Markup Success(string text) =>
        new($"[bold {Theme.SuccessHex}]✓[/] [{Theme.SuccessHex}]{text}[/]");

    /// <summary>Red cross badge.</summary>
    public static Markup Error(string text) =>
        new($"[bold {Theme.ErrorHex}]✗[/] [{Theme.ErrorHex}]{text}[/]");

    /// <summary>Yellow warning badge.</summary>
    public static Markup Warning(string text) =>
        new($"[bold {Theme.WarningHex}]⚠[/] [{Theme.WarningHex}]{text}[/]");

    /// <summary>Blue info badge.</summary>
    public static Markup Info(string text) =>
        new($"[bold {Theme.InfoHex}]ℹ[/] [{Theme.InfoHex}]{text}[/]");

    /// <summary>Step indicator [current/total].</summary>
    public static Markup Step(int current, int total, string text) =>
        new($"[bold {Theme.NextNetTealHex}][[{current}/{total}]][/] [{Theme.NextNetTealHex}]{text}[/]");

    // ── Plain text variants ──────────────────────────────────────────

    /// <summary>Plain success: [OK] text</summary>
    public static string PlainSuccess(string text) => $"[OK] {text}";

    /// <summary>Plain error: [ERR] text</summary>
    public static string PlainError(string text) => $"[ERR] {text}";

    /// <summary>Plain warning: [WARN] text</summary>
    public static string PlainWarning(string text) => $"[WARN] {text}";

    /// <summary>Plain info: [INFO] text</summary>
    public static string PlainInfo(string text) => $"[INFO] {text}";

    /// <summary>Plain step: [current/total] text</summary>
    public static string PlainStep(int current, int total, string text) => $"[{current}/{total}] {text}";
}
