using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>One column of the work-in-progress strip and how many items the file counts there.</summary>
public sealed record WorkWip(string Column, int Count);

/// <summary>The items that sit in one column, in the file's order.</summary>
public sealed record WorkColumnGroup(string Column, IReadOnlyList<WorkItem> Items);

/// <summary>The repo and project the board was read from, in the words of the header.</summary>
/// <param name="Label">What the line shows for the repository.</param>
/// <param name="Url">Where that label leads; null when the file gave a name and no address.</param>
/// <param name="Project">The project number, in digits; null when the file does not say.</param>
public sealed record WorkScopeLine(string? Label, Uri? Url, string? Project)
{
    public string? Title => Url is null || Label is null ? null : Label + LinkText.GitHubSuffix;
}

/// <summary>One week of the chart: the two counts, and the sentence a reader of the bar hears.</summary>
public sealed record WorkWeekBar(string Label, int Merged, int DeployedProd, string Title);

/// <summary>The merged and deployed-to-prod counts, oldest week first, on one scale.</summary>
/// <param name="Scale">The tallest bar. At least 1, so a week of zeros still has a scale.</param>
public sealed record WorkWeekChart(IReadOnlyList<WorkWeekBar> Bars, int Scale, string Title);

/// <summary>Where an item is deployed, in environment order, for the badges. An environment that has not got it is absent.</summary>
public sealed record WorkPlace(string Name, string? Version, DateTimeOffset? DeployedAt);

/// <summary>Bot beside team on the All strip. A side is null when that slice does not say.</summary>
public sealed record WorkComparison(WorkDuration? BotCycle, WorkDuration? TeamCycle, WorkWeekChart? BotWeeks, WorkWeekChart? TeamWeeks);

/// <summary>The work items file in the words of the page. The view only places them.</summary>
/// <summary>
/// The work items domain's one door for the page, in the house style of <see cref="DeliveryText"/>: the fixed
/// sentences and the availability of the board are said here, and every other word is forwarded to the class that
/// owns its concern - <see cref="WorkAudienceText"/> (the All / Bot / Team filter), <see cref="WorkBoardText"/> (columns,
/// work in progress, recently done), <see cref="WorkMeasureText"/> (durations, rework, cost), <see cref="WorkWeekText"/>
/// (the weekly chart), <see cref="WorkScopeText"/> (the header's scope and a release's carries) and
/// <see cref="WorkRowText"/> (one row's number, people, pulls, sessions and places).
/// </summary>
public static class WorkItemsText
{
    public const string Loading = "Loading work items";

    public const string NoAddress = "The topology names no delivery file, so there is nowhere to read work items.";

    public const string NotPublished =
        "Work items are not published yet. The page reads workitems.json next to the delivery facts, and that file is not there yet.";

    public const string None = "The work items file lists no items.";

    public const string NoActive = "No work item is in progress.";

    /// <summary>The height of the weekly chart, in its own units. A bar of the largest count fills it.</summary>
    public const double ChartHeight = 36;

    public const string NotMetered = "n/a";

    /// <summary>What a cell shows when the file does not have that part. Cost uses <see cref="NotMetered"/>.</summary>
    public const string Missing = "—";

    public static WorkItemsAvailability Availability(bool hasAddress, bool asked, WorkItemsReport? report) =>
        report is not null ? WorkItemsAvailability.Ready
        : !hasAddress ? WorkItemsAvailability.NoAddress
        : !asked ? WorkItemsAvailability.Loading
        : WorkItemsAvailability.NotPublished;

    /// <summary>
    /// The sentence a state stands for. A value the enum does not name is refused by name first, so the switch
    /// underneath says the four states and nothing else: a fifth state of the enum is a build error (CS8509, and
    /// warnings are errors) rather than a paragraph that says nothing on the page.
    /// </summary>
    public static string Unavailable(WorkItemsAvailability availability)
    {
        if (!Enum.IsDefined(availability))
        {
            throw new ArgumentOutOfRangeException(nameof(availability), availability, "A work items state has no sentence.");
        }

#pragma warning disable CS8524 // Every named state has its arm below; the guard refused every value that is not one.
        return availability switch
        {
            WorkItemsAvailability.Loading => Loading,
            WorkItemsAvailability.NoAddress => NoAddress,
            WorkItemsAvailability.NotPublished => NotPublished,
            WorkItemsAvailability.Ready => string.Empty, // The board is on the page, so there is nothing to say.
        };
#pragma warning restore CS8524
    }

    // ---- the All / Bot / Team filter: WorkAudienceText ----

    public static string AudienceLabel(WorkAudience audience) => WorkAudienceText.Label(audience);

    /// <summary>The first segment after <c>work-items</c> in the address: <c>bot</c>, <c>team</c>, or anything else for all.</summary>
    public static WorkAudience AudienceOf(string? segment) => WorkAudienceText.Of(segment);

    public static string AudienceHash(WorkAudience audience) => WorkAudienceText.Hash(audience);

    /// <summary>The items of one filter, in the file's order. An item with no worker is only in All.</summary>
    public static IReadOnlyList<WorkItem> Select(IReadOnlyList<WorkItem> items, WorkAudience audience) => WorkAudienceText.Select(items, audience);

    public static string NoneOf(WorkAudience audience) => WorkAudienceText.NoneOf(audience);

    /// <summary>The worker's word on a row: its audience's address segment. Null when the file does not say who.</summary>
    public static string? WorkerLabel(WorkWorker worker) => WorkAudienceText.WorkerLabel(worker);

    /// <summary>The stats strip for a filter: the file's slice when it has one, and otherwise what the items answer.</summary>
    public static WorkItemsStats Slice(WorkItemsReport report, WorkAudience audience) => WorkAudienceText.Slice(report, audience);

    /// <summary>Bot beside team, for the All strip; null when there is nothing to set side by side.</summary>
    public static WorkComparison? Comparison(WorkItemsReport report) => WorkAudienceText.Comparison(report);

    // ---- the board's columns: WorkBoardText ----

    /// <summary>Work in progress, one count per board column. Done is not work in progress.</summary>
    public static IReadOnlyList<WorkWip> Wip(WorkItemsReport report) => Wip(report.Stats, report.Items);

    /// <summary>Work in progress for one slice; see <see cref="WorkBoardText.Wip"/>.</summary>
    public static IReadOnlyList<WorkWip> Wip(WorkItemsStats stats, IReadOnlyList<WorkItem> items) => WorkBoardText.Wip(stats, items);

    /// <summary>The board's columns that hold work now, in order, then Unplaced. Done is not among them.</summary>
    public static IReadOnlyList<WorkColumnGroup> Active(IReadOnlyList<WorkItem> items) => WorkBoardText.Active(items);

    /// <summary>The items in Done, the one that arrived there last first. An item with no arrival time is last.</summary>
    public static IReadOnlyList<WorkItem> RecentlyDone(IReadOnlyList<WorkItem> items) => WorkBoardText.RecentlyDone(items);

    /// <summary>When the item entered <paramref name="column"/>, the latest time the history gives; null when it never says.</summary>
    public static DateTimeOffset? Entered(WorkItem item, string column) => WorkBoardText.Entered(item, column);

    /// <summary><c>since 3 h ago</c>; null when the history does not say when the item entered its column.</summary>
    public static string? Since(WorkItem item, DateTimeOffset now) => WorkBoardText.Since(item, now);

    /// <summary>An element id for a column heading: <c>work-col-in-progress</c>.</summary>
    public static string ColumnId(string column) => WorkBoardText.ColumnId(column);

    // ---- the measures: WorkMeasureText ----

    /// <summary><c>median 18.5 h · p85 3 d · 11 items</c>, or <see cref="NotMetered"/> when the file gives none of it.</summary>
    public static string Duration(WorkDuration? duration) => WorkMeasureText.Duration(duration);

    /// <summary><c>2 functional-test failures · 1 revert · 12.5 %</c>, or <see cref="NotMetered"/> when nothing is there.</summary>
    public static string Rework(WorkRework? rework) => WorkMeasureText.Rework(rework);

    /// <summary>True when at least one item carries a metered Cursor agent cost.</summary>
    public static bool HasMeteredCost(IReadOnlyList<WorkItem> items) => WorkMeasureText.HasMeteredCost(items);

    /// <summary><c>$2.00 per item · $18.00 total</c>; null when no item was metered.</summary>
    public static string? Cost(WorkCost? cost, IReadOnlyList<WorkItem> items) => WorkMeasureText.Cost(cost, items);

    /// <summary>What an item's own cost cell says: the amount, or <see cref="NotMetered"/>.</summary>
    public static string ItemCost(double? costUsd) => WorkMeasureText.ItemCost(costUsd);

    // ---- the weekly chart: WorkWeekText ----

    /// <summary>The weekly chart; null when the file gives no week.</summary>
    public static WorkWeekChart? Chart(IReadOnlyList<WorkWeek> weeks) => WorkWeekText.Chart(weeks);

    /// <summary>How tall a bar is in the chart's units. Zero stays zero, so a week with nothing is a gap, not a sliver.</summary>
    public static double BarHeight(int value, int scale) => WorkWeekText.BarHeight(value, scale);

    /// <summary>The newest week, in words under the chart.</summary>
    public static string LatestWeek(WorkWeekChart chart) => WorkWeekText.LatestWeek(chart);

    // ---- the header's scope and a release's carries: WorkScopeText ----

    public static WorkScopeLine? Scope(WorkItemsScope? scope) => WorkScopeText.Scope(scope);

    /// <summary><c>carries #12, #15</c>; null when the file names no such release, or it carries none.</summary>
    public static string? Carries(WorkItemsReport? report, string? version) => WorkScopeText.Carries(report, version);

    // ---- one row: WorkRowText ----

    public static string Number(WorkItem item) => WorkRowText.Number(item);

    public static string IssueTitle(WorkItem item) => WorkRowText.IssueTitle(item);

    /// <summary>The bot handling the item, or who it is assigned to when no bot is named.</summary>
    public static string Agent(WorkItem item) => WorkRowText.Agent(item);

    /// <summary>Who it is assigned to, when that is someone other than the bot; null otherwise.</summary>
    public static string? Assignee(WorkItem item) => WorkRowText.Assignee(item);

    public static string PullLabel(WorkPullRequest pull) => WorkRowText.PullLabel(pull);

    public static string PullTitle(WorkPullRequest pull) => WorkRowText.PullTitle(pull);

    public static bool IsMerged(string? state) => WorkRowText.IsMerged(state);

    public static string SessionTitle(int index, int count) => WorkRowText.SessionTitle(index, count);

    public static string SessionLabel(int index, int count) => WorkRowText.SessionLabel(index, count);

    /// <summary>tdd, then uat, then prod, and only the environments the item has reached.</summary>
    public static IReadOnlyList<WorkPlace> Places(WorkItem item) => WorkRowText.Places(item);

    public static string PlaceLabel(WorkPlace place) => WorkRowText.PlaceLabel(place);

    public static string PlaceTitle(WorkPlace place, DateTimeOffset now) => WorkRowText.PlaceTitle(place, now);

    public static string? CandidateTitle(ReleaseCandidate candidate, DateTimeOffset now) => WorkRowText.CandidateTitle(candidate, now);
}
