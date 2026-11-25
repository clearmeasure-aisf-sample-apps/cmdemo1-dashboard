namespace Dashboard.Work;

/// <summary>
/// The words of the All / Bot / Team filter: the name on the strip, the address segment, the worker word on a row,
/// the items and stats a filter holds, and bot beside team. <see cref="WorkItemsText"/> is the page's door to these.
/// </summary>
public static class WorkAudienceText
{
    /// <summary>
    /// The words of one audience: its name on the strip, its segment of the address, and the worker whose items it
    /// holds. The segment is also that worker's word on a row. All holds no one worker, so it has none.
    /// </summary>
    private sealed record AudienceWords(WorkAudience Audience, string Label, string Segment, WorkWorker? Worker);

    /// <summary>The one table of audience words. A new audience is one more row here.</summary>
    private static readonly AudienceWords[] Audiences =
    [
        new(WorkAudience.All, "All", string.Empty, Worker: null),
        new(WorkAudience.Bot, "Bot", "bot", WorkWorker.Bot),
        new(WorkAudience.Team, "Team", "team", WorkWorker.Team),
    ];

    private static AudienceWords? WordsOf(WorkAudience audience) => Array.Find(Audiences, words => words.Audience == audience);

    public static string Label(WorkAudience audience) =>
        WordsOf(audience)?.Label
        ?? throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no name.");

    /// <summary>The first segment after <c>work-items</c> in the address: <c>bot</c>, <c>team</c>, or anything else for all.</summary>
    public static WorkAudience Of(string? segment) => WordsFor(segment)?.Audience ?? WorkAudience.All;

    /// <summary>
    /// The file's <c>worker</c> word on an item: <c>bot</c>, <c>team</c>, or anything else for
    /// <see cref="WorkWorker.Unknown"/>. Read from the same table as <see cref="Of"/>, so an item's worker and a
    /// filter's audience are always the same words.
    /// </summary>
    internal static WorkWorker WorkerOf(string? word) => WordsFor(word)?.Worker ?? WorkWorker.Unknown;

    /// <summary>The audience words whose segment is <paramref name="word"/>, trimmed and in any case; null when none is.</summary>
    private static AudienceWords? WordsFor(string? word)
    {
        var trimmed = word?.Trim();
        return Array.Find(Audiences, words => string.Equals(words.Segment, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    public static string Hash(WorkAudience audience) =>
        WordsOf(audience)?.Segment
        ?? throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no address.");

    /// <summary>The items of one filter, in the file's order. An item with no worker is only in All.</summary>
    public static IReadOnlyList<WorkItem> Select(IReadOnlyList<WorkItem> items, WorkAudience audience)
    {
        ArgumentNullException.ThrowIfNull(items);
        return audience switch
        {
            WorkAudience.Bot => [.. items.Where(item => item.Worker == WorkWorker.Bot)],
            WorkAudience.Team => [.. items.Where(item => item.Worker == WorkWorker.Team)],
            WorkAudience.All => items,
            _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no items."),
        };
    }

    public static string NoneOf(WorkAudience audience) => audience switch
    {
        WorkAudience.Bot => "No bot work items.",
        WorkAudience.Team => "No team work items.",
        WorkAudience.All => WorkItemsText.None,
        _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no empty state."),
    };

    /// <summary>The worker's word on a row: its audience's address segment. Null when the file does not say who.</summary>
    public static string? WorkerLabel(WorkWorker worker) =>
        worker == WorkWorker.Unknown
            ? null
            : Array.Find(Audiences, words => words.Worker == worker)?.Segment
              ?? throw new ArgumentOutOfRangeException(nameof(worker), worker, "A worker has no name.");

    /// <summary>
    /// The stats strip for a filter. All is the file's board (<c>stats.all</c>, or the older top-level fields). Bot
    /// and team use the file's slice when it has one, and otherwise what the items themselves can answer.
    /// </summary>
    public static WorkItemsStats Slice(WorkItemsReport report, WorkAudience audience)
    {
        ArgumentNullException.ThrowIfNull(report);
        var published = audience switch
        {
            WorkAudience.Bot => report.BotStats,
            WorkAudience.Team => report.TeamStats,
            WorkAudience.All => report.Stats,
            _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no stats."),
        };
        return published ?? WorkBotText.FromItems(Select(report.Items, audience));
    }

    /// <summary>
    /// Bot beside team, for the All strip: cycle time and the latest week. Null when the file has no split and no
    /// item says who did the work, or when neither side has a cycle time or a week.
    /// </summary>
    public static WorkComparison? Comparison(WorkItemsReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (report.BotStats is null && report.TeamStats is null && report.Items.All(item => item.Worker == WorkWorker.Unknown))
        {
            return null;
        }

        var bot = Slice(report, WorkAudience.Bot);
        var team = Slice(report, WorkAudience.Team);
        var botWeeks = WorkWeekText.Chart(bot.PerWeek);
        var teamWeeks = WorkWeekText.Chart(team.PerWeek);
        if (bot.CycleTimeHours is null && team.CycleTimeHours is null && botWeeks is null && teamWeeks is null)
        {
            return null;
        }

        return new WorkComparison(bot.CycleTimeHours, team.CycleTimeHours, botWeeks, teamWeeks);
    }
}
