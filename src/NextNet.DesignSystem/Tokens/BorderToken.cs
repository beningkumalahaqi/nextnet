namespace NextNet.DesignSystem.Tokens;

/// <summary>
/// Represents a border design token including width, style, color, and border-radius.
/// </summary>
/// <remarks>
/// Border tokens define the visual style of borders and border radii used across components.
/// The <see cref="Width"/>, <see cref="Style"/>, and <see cref="Color"/> properties describe the
/// border line itself, while <see cref="Radius"/> controls the corner rounding.
/// Values are CSS-compatible strings.
/// </remarks>
/// <example>
/// <code>
/// var card = new BorderToken(
///     "card",
///     "1px",
///     "solid",
///     "#E5E7EB",
///     "0.5rem"
/// );
/// </code>
/// </example>
/// <param name="Name">The unique identifier for this token (e.g., <c>"card"</c>).</param>
/// <param name="Width">The CSS border-width value (e.g., <c>"1px"</c> or <c>"2px"</c>).</param>
/// <param name="Style">The CSS border-style value (e.g., <c>"solid"</c> or <c>"dashed"</c>).</param>
/// <param name="Color">The CSS border-color value (e.g., <c>"#E5E7EB"</c>).</param>
/// <param name="Radius">The CSS border-radius value (e.g., <c>"0.375rem"</c>).</param>
public sealed record BorderToken(
    string Name,
    string Width,
    string Style,
    string Color,
    string Radius);
