namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Represents a box-shadow design token used for elevation and depth effects.
/// </summary>
/// <remarks>
/// Shadow tokens define CSS box-shadow values that create visual depth hierarchies.
/// The <see cref="Value"/> property holds the full box-shadow declaration string,
/// such as <c>"0 1px 3px 0 rgba(0,0,0,0.1)"</c>.
/// </remarks>
/// <example>
/// <code>
/// var small = new ShadowToken("shadow-sm", "0 1px 2px 0 rgba(0,0,0,0.05)");
/// var large = new ShadowToken("shadow-lg", "0 10px 15px -3px rgba(0,0,0,0.1)");
/// </code>
/// </example>
/// <param name="Name">The unique identifier for this token (e.g., <c>"shadow-sm"</c>).</param>
/// <param name="Value">The CSS box-shadow value.</param>
public sealed record ShadowToken(string Name, string Value);
