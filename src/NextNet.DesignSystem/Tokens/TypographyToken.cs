namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Represents a typography design token defining font family, size, weight, line height, and letter spacing.
/// </summary>
/// <remarks>
/// Typography tokens capture the full set of font-related properties for a given text style.
/// They are used to maintain a consistent typographic scale across headings, body text, and labels.
/// </remarks>
/// <example>
/// <code>
/// var heading = new TypographyToken(
///     "heading-xl",
///     "Inter, system-ui, sans-serif",
///     "3rem",
///     "700",
///     "1.2",
///     "-0.02em"
/// );
/// </code>
/// </example>
/// <param name="Name">The unique identifier for this token (e.g., <c>"heading-xl"</c>).</param>
/// <param name="FontFamily">The CSS font-family string (e.g., <c>"Inter, system-ui, sans-serif"</c>).</param>
/// <param name="FontSize">The CSS font-size value (e.g., <c>"1rem"</c> or <c>"16px"</c>).</param>
/// <param name="FontWeight">The CSS font-weight value (e.g., <c>"400"</c> or <c>"700"</c>).</param>
/// <param name="LineHeight">The CSS line-height value (e.g., <c>"1.5"</c> or <c>"24px"</c>).</param>
/// <param name="LetterSpacing">The CSS letter-spacing value (e.g., <c>"normal"</c> or <c>"-0.02em"</c>).</param>
public sealed record TypographyToken(
    string Name,
    string FontFamily,
    string FontSize,
    string FontWeight,
    string LineHeight,
    string LetterSpacing);
