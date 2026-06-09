namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Represents a responsive breakpoint design token defining a viewport width threshold.
/// </summary>
/// <remarks>
/// Breakpoint tokens define the screen width boundaries at which layout changes occur.
/// Values are CSS-compatible width strings such as <c>"640px"</c>, <c>"768px"</c>,
/// or <c>"1024px"</c>. These correspond to the <c>min-width</c> media query values.
/// Common naming follows Tailwind conventions: <c>sm</c>, <c>md</c>, <c>lg</c>, <c>xl</c>, <c>2xl</c>.
/// </remarks>
/// <example>
/// <code>
/// var md = new BreakpointToken("md", "768px");
/// var lg = new BreakpointToken("lg", "1024px");
/// </code>
/// </example>
/// <param name="Name">The unique identifier for this token (e.g., <c>"md"</c>).</param>
/// <param name="Value">The CSS width value for the breakpoint (e.g., <c>"768px"</c>).</param>
public sealed record BreakpointToken(string Name, string Value);
