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
    /// is how many are still being checked. The not-healthy count is the rest. Every arm takes its noun from
    /// <see cref="Plural.Noun(int, string, string?)" />, so a count of one says "node" and any other count says
    /// "nodes" in every sentence.
    /// </summary>
    public static string Text(SummaryLevel level, int total, int healthy, int pending)
    {
        var notHealthy = total - healthy - pending;
        var noun = Plural.Noun(total, "node");

        return level switch
        {
            SummaryLevel.Empty => $"No {noun} in the topology",
            SummaryLevel.Problems => $"{notHealthy} of {total} {noun} not healthy",
            SummaryLevel.Pending when healthy == 0 => $"Checking {total} {noun}",
            SummaryLevel.Pending => $"{healthy} of {total} {noun} healthy, {pending} being checked",
            SummaryLevel.AllHealthy => Plural.Noun(total, $"The only {noun} is healthy", $"All {total} {noun} healthy"),
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
    /// The CSS class name the summary element carries, the part after <c>summary--</c>. The four are fixed words, named
    /// here and not derived from <see cref="SummaryLevel.ToString" />, so renaming a level never changes the markup.
    /// The style sheet gives them no rule today (the icon and the sentence carry the state); they are a stable hook,
    /// and any <c>.summary--*</c> rule <c>app.css</c> adds must name one of these words (pinned by
    /// <c>SummaryClassGuardTests</c>).
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
