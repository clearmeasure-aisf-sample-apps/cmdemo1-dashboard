using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>Whether the work items view has a file to show.</summary>
public enum WorkItemsAvailability
{
    /// <summary>The address is known and the first read has not finished.</summary>
    Loading,

    /// <summary>The topology names no delivery file, so there is no sibling to read.</summary>
    NoAddress,

    /// <summary>The read found nothing: the file is not published yet, or it is not this file.</summary>
    NotPublished,

    /// <summary>A file was read.</summary>
    Ready,
}

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

    public static string Unavailable(WorkItemsAvailability availability) => availability switch
    {
        WorkItemsAvailability.Loading => Loading,
        WorkItemsAvailability.NoAddress => NoAddress,
        WorkItemsAvailability.NotPublished => NotPublished,
        WorkItemsAvailability.Ready => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(availability), availability, "A work items state has no sentence."),
    };

    public static string AudienceLabel(WorkAudience audience) => audience switch
    {
        WorkAudience.All => "All",
        WorkAudience.Bot => "Bot",
        WorkAudience.Team => "Team",
        _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no name."),
    };

    /// <summary>The first segment after <c>work-items</c> in the address: <c>bot</c>, <c>team</c>, or anything else for all.</summary>
    public static WorkAudience AudienceOf(string? segment) => segment?.Trim().ToLowerInvariant() switch
    {
        "bot" => WorkAudience.Bot,
        "team" => WorkAudience.Team,
        _ => WorkAudience.All,
    };

    public static string AudienceHash(WorkAudience audience) => audience switch
    {
        WorkAudience.Bot => "bot",
        WorkAudience.Team => "team",
        WorkAudience.All => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(audience), audience, "A work filter has no address."),
    };

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

    public static string? WorkerLabel(WorkWorker worker) => worker switch
    {
        WorkWorker.Bot => "bot",
        WorkWorker.Team => "team",
        WorkWorker.Unknown => null,
        _ => throw new ArgumentOutOfRangeException(nameof(worker), worker, "A worker has no name."),
    };

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
        return published ?? (audience == WorkAudience.All ? report.Stats : WorkBotText.FromItems(Select(report.Items, audience)));
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
    public static IReadOnlyList<WorkWip> Wip(WorkItemsStats stats, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(stats);
        ArgumentNullException.ThrowIfNull(items);
        var counts = stats.WipByColumn;
        var fromFile = counts.Count > 0;
        var rows = new List<WorkWip>();
        foreach (var column in WorkColumns.Active)
        {
            rows.Add(new WorkWip(column, fromFile ? CountOf(counts, column) : items.Count(item => WorkColumns.Same(item.Column, column))));
        }

        var unplaced = fromFile
            ? counts.Where(pair => !WorkColumns.IsKnown(pair.Key)).Sum(pair => pair.Value)
            : items.Count(item => !WorkColumns.IsKnown(item.Column) && !WorkColumns.IsDone(item.Column));
        if (unplaced > 0)
        {
            rows.Add(new WorkWip(WorkColumns.Unplaced, unplaced));
        }

        return rows;
    }

    /// <summary>
    /// The board's columns that hold work now, in order, then <see cref="WorkColumns.Unplaced"/> for any item whose
    /// column is blank or not one of those. Done is not among them.
    /// </summary>
    public static IReadOnlyList<WorkColumnGroup> Active(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var groups = new List<WorkColumnGroup>();
        foreach (var column in WorkColumns.Active)
        {
            var found = items.Where(item => WorkColumns.Same(item.Column, column)).ToList();
            if (found.Count > 0)
            {
                groups.Add(new WorkColumnGroup(column, found));
            }
        }

        var unplaced = items.Where(item => !WorkColumns.IsKnown(item.Column) && !WorkColumns.IsDone(item.Column)).ToList();
        if (unplaced.Count > 0)
        {
            groups.Add(new WorkColumnGroup(WorkColumns.Unplaced, unplaced));
        }

        return groups;
    }

    /// <summary>The items in Done, the one that arrived there last first. An item with no arrival time is last.</summary>
    public static IReadOnlyList<WorkItem> RecentlyDone(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return
        [
            .. items.Where(item => WorkColumns.IsDone(item.Column))
                .OrderByDescending(item => Entered(item, WorkColumns.Done) ?? DateTimeOffset.MinValue)
                .ThenByDescending(item => item.Number ?? int.MinValue),
        ];
    }

    /// <summary>When the item entered <paramref name="column"/>, the latest time the history gives; null when it never says.</summary>
    public static DateTimeOffset? Entered(WorkItem item, string column)
    {
        ArgumentNullException.ThrowIfNull(item);
        DateTimeOffset? found = null;
        foreach (var visit in item.ColumnHistory)
        {
            if (WorkColumns.Same(visit.Column, column) && visit.EnteredAt is { } at)
            {
                found = at;
            }
        }

        return found;
    }

    /// <summary><c>since 3 h ago</c>; null when the history does not say when the item entered its column.</summary>
    public static string? Since(WorkItem item, DateTimeOffset now) =>
        item.Column is { } column && Entered(item, column) is { } at ? $"since {TimeText.Ago(at, now)}" : null;

    /// <summary><c>median 18.5 h · p85 3 d · 11 items</c>, or <see cref="NotMetered"/> when the file gives none of it.</summary>
    public static string Duration(WorkDuration? duration)
    {
        if (duration is null)
        {
            return NotMetered;
        }

        var parts = new List<string>();
        if (duration.Median is { } median)
        {
            parts.Add($"median {DeliveryText.LeadTime(median)}");
        }

        if (duration.P85 is { } p85)
        {
            parts.Add($"p85 {DeliveryText.LeadTime(p85)}");
        }

        if (duration.N is { } counted)
        {
            parts.Add(counted == 1 ? "1 item" : string.Create(CultureInfo.InvariantCulture, $"{counted} items"));
        }

        return parts.Count == 0 ? NotMetered : string.Join(" · ", parts);
    }

    /// <summary>
    /// <c>2 functional-test failures · 1 revert · 12.5 %</c>. <see cref="WorkRework.Rate"/> at most 1 is a fraction and
    /// is shown as a percent; a larger number is shown as the file gave it. <see cref="NotMetered"/> when nothing is there.
    /// </summary>
    public static string Rework(WorkRework? rework)
    {
        if (rework is null)
        {
            return NotMetered;
        }

        var parts = new List<string>();
        if (rework.FtFailures is { } failures)
        {
            parts.Add(failures == 1 ? "1 functional-test failure" : string.Create(CultureInfo.InvariantCulture, $"{failures} functional-test failures"));
        }

        if (rework.Reverts is { } reverts)
        {
            parts.Add(reverts == 1 ? "1 revert" : string.Create(CultureInfo.InvariantCulture, $"{reverts} reverts"));
        }

        if (rework.Rate is { } rate)
        {
            parts.Add(Rate(rate));
        }

        return parts.Count == 0 ? NotMetered : string.Join(" · ", parts);
    }

    /// <summary>
    /// True when at least one item carries a metered Cursor agent cost. The cost tile is shown only then: a note
    /// without a metered amount is not a cost source.
    /// </summary>
    public static bool HasMeteredCost(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Any(item => item.CostUsd is not null);
    }

    /// <summary>
    /// <c>$2.00 per item · $18.00 total</c> when at least one of <paramref name="items"/> was metered. Null when none
    /// was: the tile stays off, and a note without a total is not shown as "n/a". Per item is the total divided by how
    /// many items the filter lists, Done included. The published total wins; without one the metered item amounts are
    /// summed. The file's note is kept only with a published total.
    /// </summary>
    public static string? Cost(WorkCost? cost, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (!HasMeteredCost(items))
        {
            return null;
        }

        var total = cost?.TotalUsd ?? items.Sum(item => item.CostUsd ?? 0);
        var amount = CostText.Money(total, "USD");
        var text = $"{CostText.Money(total / items.Count, "USD")} per item · {amount} total";
        return cost?.TotalUsd is not null && cost.Note is { } why ? $"{text} · {why}" : text;
    }

    /// <summary>What an item's own cost cell says: the amount, or <see cref="NotMetered"/>.</summary>
    public static string ItemCost(double? costUsd) => costUsd is { } cost ? CostText.Money(cost, "USD") : NotMetered;

    /// <summary>The weekly chart; null when the file gives no week.</summary>
    public static WorkWeekChart? Chart(IReadOnlyList<WorkWeek> weeks)
    {
        ArgumentNullException.ThrowIfNull(weeks);
        if (weeks.Count == 0)
        {
            return null;
        }

        var bars = weeks.Select(week =>
        {
            var merged = week.Merged ?? 0;
            var deployed = week.DeployedProd ?? 0;
            var label = week.WeekStart is { } start
                ? string.Create(CultureInfo.InvariantCulture, $"Week of {start.UtcDateTime:yyyy-MM-dd}")
                : "Week";
            return new WorkWeekBar(
                label,
                merged,
                deployed,
                string.Create(CultureInfo.InvariantCulture, $"{label}: {merged} merged, {deployed} deployed to prod"));
        }).ToList();
        var scale = Math.Max(1, bars.Max(bar => Math.Max(bar.Merged, bar.DeployedProd)));
        return new WorkWeekChart(bars, scale, "Merged and deployed to prod per week. " + string.Join(" ", bars.Select(bar => bar.Title)));
    }

    /// <summary>How tall a bar is in the chart's units. Zero stays zero, so a week with nothing is a gap, not a sliver.</summary>
    public static double BarHeight(int value, int scale) =>
        scale <= 0 || value <= 0 ? 0 : Math.Round(ChartHeight * value / scale, 2);

    /// <summary>The newest week, in words under the chart.</summary>
    public static string LatestWeek(WorkWeekChart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        var last = chart.Bars[^1];
        return $"{last.Label}: {last.Merged} merged, {last.DeployedProd} deployed to prod";
    }

    public static WorkScopeLine? Scope(WorkItemsScope? scope)
    {
        if (scope is null)
        {
            return null;
        }

        var url = JsonRead.Address(scope.Repo);
        var label = url is null ? scope.Repo : RepoLabel(url);
        var project = scope.Project is { } number ? number.ToString(CultureInfo.InvariantCulture) : null;
        return label is null && project is null ? null : new WorkScopeLine(label, url, project);
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
        return numbers.Count == 0 ? null : "carries " + string.Join(", ", numbers.Select(number => $"#{number.ToString(CultureInfo.InvariantCulture)}"));
    }

    /// <summary>An element id for a column heading: <c>work-col-in-progress</c>.</summary>
    public static string ColumnId(string column)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        var slug = new string(column.Trim().ToLowerInvariant().Select(character => char.IsAsciiLetterOrDigit(character) ? character : '-').ToArray());
        return "work-col-" + string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string Number(WorkItem item) =>
        item.Number is { } number ? "#" + number.ToString(CultureInfo.InvariantCulture) : "Item";

    public static string IssueTitle(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var name = item.Number is { } number ? $"Work item #{number.ToString(CultureInfo.InvariantCulture)}" : "Work item";
        return item.State is { } state ? $"{name}, {state}{LinkText.GitHubSuffix}" : name + LinkText.GitHubSuffix;
    }

    /// <summary>The bot handling the item, or who it is assigned to when no bot is named.</summary>
    public static string Agent(WorkItem item) => item.Agent ?? item.Assignee ?? Missing;

    /// <summary>Who it is assigned to, when that is someone other than the bot; null otherwise.</summary>
    public static string? Assignee(WorkItem item) =>
        item.Assignee is { } who && !string.Equals(who, item.Agent, StringComparison.Ordinal) ? who : null;

    public static string PullLabel(WorkPullRequest pull) =>
        pull.Number is { } number ? "#" + number.ToString(CultureInfo.InvariantCulture) : "Pull request";

    public static string PullTitle(WorkPullRequest pull)
    {
        ArgumentNullException.ThrowIfNull(pull);
        var name = pull.Number is { } number ? $"Pull request #{number.ToString(CultureInfo.InvariantCulture)}" : "Pull request";
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

    private static int CountOf(IReadOnlyDictionary<string, int> counts, string column)
    {
        foreach (var (name, count) in counts)
        {
            if (WorkColumns.Same(name, column))
            {
                return count;
            }
        }

        return 0;
    }

    private static string RepoLabel(Uri url)
    {
        var path = url.AbsolutePath.Trim('/');
        return path.Length > 0 ? path : url.Host;
    }

    private static string Rate(double rate) =>
        rate <= 1
            ? string.Create(CultureInfo.InvariantCulture, $"{Math.Round(rate * 100, 1):0.#} %")
            : rate.ToString("0.##", CultureInfo.InvariantCulture);
}
