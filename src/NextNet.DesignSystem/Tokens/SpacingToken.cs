namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Represents a spacing design token used for margins, paddings, and layout gaps.
/// </summary>
/// <remarks>
/// Spacing tokens define the vertical and horizontal rhythm of a design system.
/// Values are CSS length strings such as <c>"0.25rem"</c>, <c>"1rem"</c>, or <c>"8px"</c>.
/// Tailwind-inspired naming convention: <c>spacing-0</c>, <c>spacing-1</c>, <c>spacing-4</c>, etc.
/// </remarks>
/// <example>
/// <code>
/// var small = new SpacingToken("spacing-1", "0.25rem");
/// var large = new SpacingToken("spacing-8", "2rem");
/// </code>
/// </example>
/// <param name="Name">The unique identifier for this token (e.g., <c>"spacing-4"</c>).</param>
/// <param name="Value">The CSS spacing value (e.g., <c>"1rem"</c>).</param>
public sealed record SpacingToken(string Name, string Value);
