using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Rendering.Composition;

namespace NextNet.UI.Rendering.Pages;

/// <summary>
/// A generic theme-aware page implementation with typed state for data-driven pages.
/// </summary>
/// <typeparam name="TState">The type of the page's state data (e.g., a view model or DTO).</typeparam>
/// <remarks>
/// <para>
/// <see cref="UiPage{TState}"/> extends <see cref="UiPage"/> with a typed
/// <see cref="State"/> property that carries data from the data source to the
/// component tree. Use this when your page needs to pass database records,
/// API responses, or other typed data to its components.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var page = new UiPage&lt;ProductViewModel&gt;
/// {
///     Title = "Product Details",
///     ThemeName = "light",
///     State = new ProductViewModel { Id = 42, Name = "Widget" },
///     ComponentTree = new[] {
///         new ComponentNode(new Card
///         {
///             Title = "Product: {State.Name}",
///             Children = new[] { new ComponentNode(new Button { Label = "Buy Now" }) }
///         })
///     }
/// };
/// var html = await page.Render();
/// </code>
/// </example>
public class UiPage<TState> : UiPage
{
    /// <summary>
    /// Gets or sets the typed state data for this page.
    /// This data is typically populated by a data source or controller
    /// and passed to components during rendering.
    /// </summary>
    public TState? State { get; init; }

    /// <summary>
    /// Gets or sets a factory delegate that transforms the page state into
    /// a component tree. Invoked during <see cref="Render"/>.
    /// </summary>
    public Func<TState, IReadOnlyList<ComponentNode>>? ComponentTreeFactory { get; init; }

    /// <inheritdoc />
    public override async Task<IHtmlContent> Render()
    {
        // If a component tree factory is provided, invoke it with the current state
        if (ComponentTreeFactory != null && State != null)
        {
            ComponentTree = ComponentTreeFactory(State);
        }

        // Also place state in Props for downstream consumers
        if (State != null)
        {
            var updatedProps = new Dictionary<string, object>(Props)
            {
                ["State"] = State
            };
            Props = updatedProps;
        }

        return await base.Render();
    }
}
