namespace NextNet.Routing.Models;

/// <summary>
/// Describes the kind of a route segment in terms of its parameterisation.
/// </summary>
public enum RouteSegmentKind
{
    /// <summary>
    /// A literal segment that matches a fixed path part (e.g. <c>/about</c>).
    /// </summary>
    Static,

    /// <summary>
    /// A single parameter segment (e.g. <c>{slug}</c> from <c>[slug]</c>).
    /// </summary>
    Dynamic,

    /// <summary>
    /// A catch-all segment that matches one or more path parts (e.g. <c>{*path}</c> from <c>[...path]</c>).
    /// </summary>
    CatchAll,

    /// <summary>
    /// An optional catch-all segment that matches zero or more path parts
    /// (e.g. <c>{{*path}}</c> from <c>[[...path]]</c>).
    /// </summary>
    OptionalCatchAll,
}
