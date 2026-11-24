namespace Dashboard.Runtime;

/// <param name="Id">The relationship's id.</param>
/// <param name="State"><c>active</c> (carries the traffic), <c>idle</c>, <c>down</c>, <c>checking</c> or <c>neutral</c>.</param>
/// <param name="Number">The number of the number line: null for a relationship without one (no slot); a dash until a source of calls per minute exists.</param>
/// <param name="Unit">The unit after the number.</param>
/// <param name="Text">The role of the relationship, under the number.</param>
/// <param name="Title">The tooltip.</param>
/// <param name="Link">Where the number leads; null without a link.</param>
/// <param name="Trend">The sparkline next to the number; null without one.</param>
public sealed record RuntimeEdgeMark(
    string Id,
    string State,
    string? Number,
    string? Unit,
    string? Text,
    string Title,
    RuntimeLink? Link = null,
    RuntimeTrend? Trend = null);
