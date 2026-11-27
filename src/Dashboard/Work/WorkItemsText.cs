using System.Globalization;
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

    /// <summary>
    /// The line these three parts make - or nothing at all, when the line would say nothing: no label and no
    /// project, whatever the address between them. The address alone is not a line, since the words of a line are
    /// the label and the number, and an address gives no words of its own to either.
    /// </summary>
    internal static WorkScopeLine? Of(string? label, Uri? url, string? project) =>
        label is null && project is null ? null : new WorkScopeLine(label, url, project);
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
public static class WorkItemsText
{
    public const string Loading = "Loading work items";

    public const string NoAddress = "The topology names no delivery file, so there is nowhere to read work items.";

    public const string NotPublished =
        "Work items are not published yet. The page reads workitems.json next to the delivery facts, and that file is not there yet.";

    public const string None = "The work items file lists no items.";

    public const string NoActive = "No work item is in progress.";

    /// <summary>The height of the weekly chart, in its own units. A bar of the largest count fills it.</summary>
    public const double ChartHeight = WorkWeekCharts.Height;

    /// <summary>
    /// The format the week chart writes its bar coordinates with: at most <see cref="WorkWeekCharts.HeightDecimals" />
    /// decimals, a whole number without a point. It keeps exactly that many places, so it must change with
    /// <see cref="WorkWeekCharts.HeightDecimals" /> or a bar's number and its drawn height disagree.
    /// </summary>
    public const string PixelFormat = WorkWeekCharts.PixelFormat;

    public const string NotMetered = "n/a";

    /// <summary>What a cell shows when the file does not have that part. Cost uses <see cref="NotMetered"/>.</summary>
    public const string Missing = "—";

    public static WorkItemsAvailability Availability(bool hasAddress, bool asked, WorkItemsReport? report) =>
        report is not null ? WorkItemsAvailability.Ready
        : !hasAddress ? WorkItemsAvailability.NoAddress
        : !asked ? WorkItemsAvailability.Loading
        : WorkItemsAvailability.NotPublished;

    /// <summary>The state the three named facts of a read settle on: a report beats both, the address beats the ask.</summary>
    public static WorkItemsAvailability Availability(WorkItemsRead read)
    {
        ArgumentNullException.ThrowIfNull(read);
        return Availability(hasAddress: read.HasAddress, asked: read.Asked, report: read.Report);
    }

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

    public static string AudienceLabel(WorkAudience audience) => WorkAudienceWords.Label(audience);

    /// <summary>The first segment after <c>work-items</c> in the address: <c>bot</c>, <c>team</c>, or anything else for all.</summary>
    public static WorkAudience AudienceOf(string? segment) => WorkAudienceWords.AudienceOf(segment);

    public static string AudienceHash(WorkAudience audience) => WorkAudienceWords.Hash(audience);

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
        WorkAudience.All => None,
        _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no empty state."),
    };

    /// <summary>The worker's word on a row: its audience's address segment. Null when the file does not say who.</summary>
    public static string? WorkerLabel(WorkWorker worker) => WorkAudienceWords.WorkerLabel(worker);

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

        var bot = report.BotStats ?? WorkBotText.FromItems(Select(report.Items, WorkAudience.Bot));
        var team = report.TeamStats ?? WorkBotText.FromItems(Select(report.Items, WorkAudience.Team));
        var botWeeks = Chart(bot.PerWeek);
        var teamWeeks = Chart(team.PerWeek);
        if (bot.CycleTimeHours is null && team.CycleTimeHours is null && botWeeks is null && teamWeeks is null)
        {
            return null;
        }

        return new WorkComparison(bot.CycleTimeHours, team.CycleTimeHours, botWeeks, teamWeeks);
    }

    /// <summary>Work in progress, one count per board column. Done is not work in progress.</summary>
    public static IReadOnlyList<WorkWip> Wip(WorkItemsReport report) => Wip(report.Stats, report.Items);

    /// <summary>
    /// Work in progress for one slice. The file's counts win for the board's columns; without them the items are
    /// counted. A name that is not one of those columns is not a board column: its items are counted under
    /// <see cref="WorkColumns.Unplaced"/>.
    /// </summary>
    public static IReadOnlyList<WorkWip> Wip(WorkItemsStats stats, IReadOnlyList<WorkItem> items) => WorkBoard.Wip(stats, items);

    /// <summary>
    /// The board's columns that hold work now, in order, then <see cref="WorkColumns.Unplaced"/> for any item whose
    /// column is blank or not one of those. Done is not among them.
    /// </summary>
    public static IReadOnlyList<WorkColumnGroup> Active(IReadOnlyList<WorkItem> items) => WorkBoard.Active(items);

    /// <summary>The items in Done, the one that arrived there last first. An item with no arrival time is last.</summary>
    public static IReadOnlyList<WorkItem> RecentlyDone(IReadOnlyList<WorkItem> items) => WorkBoard.RecentlyDone(items);

    /// <summary>When the item entered <paramref name="column"/>, the latest time the history gives; null when it never says.</summary>
    public static DateTimeOffset? Entered(WorkItem item, string column) => WorkBoard.Entered(item, column);

    /// <summary><c>since 3 h ago</c>; null when the history does not say when the item entered its column.</summary>
    public static string? Since(WorkItem item, DateTimeOffset now) =>
        item.Identity.Column is { } column && Entered(item, column) is { } at ? $"since {TimeText.Ago(at, now)}" : null;

    /// <summary><c>median 18.5 h · p85 3 d · 11 items</c>, or <see cref="NotMetered"/> when the file gives none of it.</summary>
    public static string Duration(WorkDuration? duration) => WorkMeasureText.Duration(duration);

    /// <summary>
    /// <c>2 functional-test failures · 1 revert · 12.5 %</c>. <see cref="WorkRework.Rate"/> at most 1 is a fraction and
    /// is shown as a percent; a larger number is shown as the file gave it. <see cref="NotMetered"/> when nothing is there.
    /// </summary>
    public static string Rework(WorkRework? rework) => WorkMeasureText.Rework(rework);

    /// <summary>
    /// True when at least one item carries a metered Cursor agent cost. The cost tile is shown only then: a note
    /// without a metered amount is not a cost source.
    /// </summary>
    public static bool HasMeteredCost(IReadOnlyList<WorkItem> items) => WorkCostText.HasMeteredCost(items);

    /// <summary>
    /// <c>$2.00 per item · $18.00 total</c> when at least one of <paramref name="items"/> was metered. Null when none
    /// was: the tile stays off, and a note without a total is not shown as "n/a". Per item is the total divided by how
    /// many items the filter lists, Done included. The published total wins; without one the metered item amounts are
    /// summed. The file's note is kept only with a published total.
    /// </summary>
    public static string? Cost(WorkCost? cost, IReadOnlyList<WorkItem> items) => WorkCostText.Cost(cost, items);

    /// <summary>What an item's own cost cell says: the amount, or <see cref="NotMetered"/>.</summary>
    public static string ItemCost(double? costUsd) => WorkCostText.ItemCost(costUsd);

    /// <summary>The weekly chart; null when the file gives no week.</summary>
    public static WorkWeekChart? Chart(IReadOnlyList<WorkWeek> weeks) => WorkWeekCharts.Chart(weeks);

    /// <summary>How tall a bar is in the chart's units. Zero stays zero, so a week with nothing is a gap, not a sliver.</summary>
    public static double BarHeight(int value, int scale) => WorkWeekCharts.BarHeight(value, scale);

    /// <summary>The newest week, in words under the chart.</summary>
    public static string LatestWeek(WorkWeekChart chart) => WorkWeekCharts.LatestWeek(chart);

    public static WorkScopeLine? Scope(WorkItemsScope? scope)
    {
        if (scope is null)
        {
            return null;
        }

        var url = JsonRead.Address(scope.Repo);
        var label = url is null ? scope.Repo : RepoLabel(url);
        var project = scope.Project is { } number ? number.ToString(CultureInfo.InvariantCulture) : null;
        return WorkScopeLine.Of(label, url, project);
    }

    /// <summary>
    /// <c>carries #12, #15</c>, the work items of the release this version is, lowest number first. Null when the
    /// work items file names no such release, or the release carries none. The <c>+</c> build metadata of a version
    /// is ignored, as it is everywhere else on the page.
    /// </summary>
    public static string? Carries(WorkItemsReport? report, string? version)
    {
        var shown = VersionText.Display(version);
        if (report is null || shown is null)
        {
            return null;
        }

        var numbers = report.Releases
            .Where(release => string.Equals(VersionText.Display(release.Version), shown, StringComparison.OrdinalIgnoreCase))
            .SelectMany(release => release.Items)
            .Distinct()
            .Order()
            .ToList();
        return numbers.Count == 0 ? null : "carries " + string.Join(", ", numbers.Select(NumberMark));
    }

    /// <summary>An element id for a column heading: <c>work-col-in-progress</c>.</summary>
    public static string ColumnId(string column)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        var slug = new string(column.Trim().ToLowerInvariant().Select(character => char.IsAsciiLetterOrDigit(character) ? character : '-').ToArray());
        return "work-col-" + string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string Number(WorkItem item) =>
        item.Identity.Number is { } number ? NumberMark(number) : "Item";

    public static string IssueTitle(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var name = item.Identity.Number is { } number ? $"Work item {NumberMark(number)}" : "Work item";
        return item.Identity.State is { } state ? $"{name}, {state}{LinkText.GitHubSuffix}" : name + LinkText.GitHubSuffix;
    }

    /// <summary>The bot handling the item, or who it is assigned to when no bot is named.</summary>
    public static string Agent(WorkItem item) => item.Agent ?? item.Assignee ?? Missing;

    /// <summary>Who it is assigned to, when that is someone other than the bot; null otherwise.</summary>
    public static string? Assignee(WorkItem item) =>
        item.Assignee is { } who && !string.Equals(who, item.Agent, StringComparison.Ordinal) ? who : null;

    public static string PullLabel(WorkPullRequest pull) =>
        pull.Number is { } number ? NumberMark(number) : "Pull request";

    public static string PullTitle(WorkPullRequest pull)
    {
        ArgumentNullException.ThrowIfNull(pull);
        var name = pull.Number is { } number ? $"Pull request {NumberMark(number)}" : "Pull request";
        var when = pull.MergedAt is { } merged ? $", merged {TimeText.Iso(merged)}" : string.Empty;
        return name + when + LinkText.GitHubSuffix;
    }

    public static bool IsMerged(string? state) => string.Equals(state, "merged", StringComparison.OrdinalIgnoreCase);

    public static string SessionTitle(int index, int count) =>
        (count == 1 ? "Cursor session" : string.Create(CultureInfo.InvariantCulture, $"Cursor session {index}")) + LinkText.GitHubSuffix;

    public static string SessionLabel(int index, int count) =>
        count == 1 ? "Session" : string.Create(CultureInfo.InvariantCulture, $"Session {index}");

    /// <summary>tdd, then uat, then prod, and only the environments the item has reached.</summary>
    public static IReadOnlyList<WorkPlace> Places(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var places = new List<WorkPlace>(3);
        Add(places, "tdd", item.Environments.Tdd);
        Add(places, "uat", item.Environments.Uat);
        Add(places, "prod", item.Environments.Prod);
        return places;
    }

    public static string PlaceLabel(WorkPlace place) =>
        place.Version is { } version ? $"{place.Name} {version}" : place.Name;

    public static string PlaceTitle(WorkPlace place, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(place);
        var version = place.Version is { } deployed ? $" as {deployed}" : string.Empty;
        var when = place.DeployedAt is { } at ? $", {TimeText.Ago(at, now)}" : string.Empty;
        return $"Deployed to {place.Name}{version}{when}";
    }

    public static string? CandidateTitle(ReleaseCandidate candidate, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return candidate.CreatedAt is { } at
            ? $"Octopus release, cut {TimeText.Ago(at, now)}"
            : "Octopus release";
    }

    private static void Add(List<WorkPlace> places, string name, WorkDeployment? deployment)
    {
        if (deployment is not null)
        {
            places.Add(new WorkPlace(name, deployment.Version, deployment.DeployedAt));
        }
    }

    private static string RepoLabel(Uri url)
    {
        var path = url.AbsolutePath.Trim('/');
        return path.Length > 0 ? path : url.Host;
    }

    /// <summary>A work item or pull request number the way the page shows it: <c>#</c> and the digits in the invariant culture.</summary>
    private static string NumberMark(int number) => "#" + number.ToString(CultureInfo.InvariantCulture);
}
