namespace Dashboard.Health;

/// <summary>
/// What a <see cref="SummaryLevel" /> says: the sentence shown to the reader, the badge's health state and the
/// summary's CSS class. Every per-level saying lives here and nowhere else, so the four levels carry one meaning
/// each rather than three copies that drift apart.
/// </summary>
public static class SummaryWords
{
    /// <summary>
    /// The sentence the header announces for the given counts. <paramref name="total" /> is every node (Front Door
    /// endpoints and web apps), <paramref name="healthy" /> is how many answered HTTP 200, and <paramref name="pending" />
    /// is how many are still being checked. The not-healthy count is the rest.
    /// </summary>
    public static string Text(SummaryLevel level, int total, int healthy, int pending)
    {
        var notHealthy = total - healthy - pending;
        var noun = total == 1 ? "node" : "nodes";

        return level switch
        {
            SummaryLevel.Empty => "No nodes in the topology",
            SummaryLevel.Problems => $"{notHealthy} of {total} {noun} not healthy",
            SummaryLevel.Pending when healthy == 0 => $"Checking {total} {noun}",
            SummaryLevel.Pending => $"{healthy} of {total} {noun} healthy, {pending} being checked",
            SummaryLevel.AllHealthy => total == 1 ? "The only node is healthy" : $"All {total} nodes healthy",
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }

    /// <summary>
    /// The <see cref="HealthState" /> the header's badge shows for the given level. When the topology could not be
    /// read (<paramref name="hasErrors" />) the badge is unhealthy regardless of the level; otherwise the level
    /// determines the badge: healthy when every node is healthy, unhealthy when a problem is found, and pending
    /// while the checks run.
    /// </summary>
    public static HealthState BadgeState(SummaryLevel level, bool hasErrors) => level switch
    {
        SummaryLevel.AllHealthy => HealthState.Healthy,
        SummaryLevel.Problems => HealthState.Unhealthy,
        _ when hasErrors => HealthState.Unhealthy,
        _ => HealthState.Pending,
    };

    /// <summary>
    /// The CSS class name the summary element carries, the part after <c>summary--</c>. The four are fixed words the
    /// stylesheet matches; renaming a level here would rename a style, so the words are named and not derived from
    /// <see cref="SummaryLevel.ToString" />.
    /// </summary>
    public static string CssClass(SummaryLevel level) => level switch
    {
        SummaryLevel.Empty => "empty",
        SummaryLevel.Pending => "pending",
        SummaryLevel.AllHealthy => "allhealthy",
        SummaryLevel.Problems => "problems",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
    };
}
