namespace Dashboard.Runtime;

/// <param name="State"><c>serving</c>, <c>standby</c>, <c>down</c>, <c>checking</c> or <c>neutral</c>.</param>
/// <param name="Label">The words of the region's mark.</param>
/// <param name="NameLink">Where the region's name leads; null without a link.</param>
public sealed record RuntimeRegionMark(string Alias, string State, string Label, RuntimeLink? NameLink = null);
