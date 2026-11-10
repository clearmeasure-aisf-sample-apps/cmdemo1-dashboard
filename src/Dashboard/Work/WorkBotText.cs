using System.Globalization;
using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>How long items waited in one column. <see cref="Longest"/> is the wait the factory should look at first.</summary>
public sealed record ColumnWait(string Column, WorkDuration Duration, bool Longest);

/// <summary>One stay in a column: when it started, how long it lasted, and whether the item is still there.</summary>
public sealed record ColumnStay(string Column, DateTimeOffset? EnteredAt, TimeSpan? Spent, bool Open);

/// <summary>
/// The factory detail of bot work: time in column, throughput, merge to production, and the lines of one item's
/// detail. A number the file gives is used; otherwise it is counted from the items. Nothing is invented when neither
/// is there.
/// </summary>
public static class WorkBotText
{
    /// <summary>
    /// Counts for one audience, from the items, when the file did not publish that slice. Completed items only for
    /// cycle and lead time. Weeks follow merge time and the production deployment time.
    /// </summary>
    public static WorkItemsStats FromItems(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            return WorkItemsStats.Empty;
        }

        var cycle = Durations(items, lead: false);
        var lead = Durations(items, lead: true);
        var weeks = Weeks(items);
        var rework = ReworkOf(items);
        var spent = items.Sum(item => item.CostUsd ?? 0);
        var anyCost = items.Any(item => item.CostUsd is not null);
        return new WorkItemsStats(
            Wip(items),
            cycle,
            lead,
            weeks,
            rework,
            anyCost ? new WorkCost(spent, null) : null,
            null,
            null,
            null);
    }

    /// <summary>Where items wait, longest first. The file's <c>timePerColumnHours</c> wins; otherwise the column history.</summary>
    public static IReadOnlyList<ColumnWait> ColumnWaits(WorkItemsStats? published, IReadOnlyList<WorkItem> items, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(items);
        var durations = published?.TimePerColumnHours is { Count: > 0 } given
            ? given.Select(pair => (Column: WorkColumns.Canonical(pair.Key), pair.Value))
            : Measured(items, now);
        var ordered = durations
            .OrderByDescending(wait => wait.Value.Median ?? -1)
            .ThenBy(wait => wait.Column, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ordered.Count == 0)
        {
            return [];
        }

        var longest = ordered[0].Value.Median;
        return
        [
            .. ordered.Select(wait => new ColumnWait(wait.Column, wait.Value, longest is not null && wait.Value.Median == longest)),
        ];
    }

    /// <summary>The column the bots wait in longest; null when no wait was timed.</summary>
    public static ColumnWait? Longest(IReadOnlyList<ColumnWait> waits)
    {
        ArgumentNullException.ThrowIfNull(waits);
        return waits.FirstOrDefault(wait => wait.Longest);
    }

    /// <summary>Finished work per bot. The file's <c>throughput</c> wins; otherwise merged pull requests and production deploys.</summary>
    public static IReadOnlyList<WorkThroughput> Throughput(WorkItemsStats? published, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (published?.Throughput is { Count: > 0 } given)
        {
            return given;
        }

        return
        [
            .. items.GroupBy(item => item.Agent ?? "unnamed", StringComparer.OrdinalIgnoreCase)
                .Select(group => new WorkThroughput(
                    group.Key,
                    group.Count(item => item.PullRequests.Any(Merged)),
                    group.Count(item => item.Environments.Prod is not null)))
                .OrderByDescending(row => row.Merged ?? 0)
                .ThenBy(row => row.Agent, StringComparer.OrdinalIgnoreCase),
        ];
    }

    public static string ThroughputLine(WorkThroughput row)
    {
        ArgumentNullException.ThrowIfNull(row);
        if (row.Done is { } done && row.Merged is null && row.DeployedProd is null)
        {
            return done == 1 ? $"{row.Agent}: 1 done" : string.Create(CultureInfo.InvariantCulture, $"{row.Agent}: {done} done");
        }

        var merged = row.Merged ?? 0;
        var deployed = row.DeployedProd ?? 0;
        return $"{row.Agent}: {merged} merged, {deployed} deployed to prod";
    }

    public static string NoteHeading(WorkNote note) =>
        string.Equals(note.Kind, "blocker", StringComparison.OrdinalIgnoreCase) ? "Blocker" : "Status";

    /// <summary>Merge to production. The file's <c>mergeToProdHours</c> wins; otherwise merged-at against the prod deployment.</summary>
    public static WorkDuration? MergeToProd(WorkItemsStats? published, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (published?.MergeToProdHours is { } given)
        {
            return given;
        }

        var hours = new List<double>();
        foreach (var item in items)
        {
            DateTimeOffset? merged = null;
            foreach (var pull in item.PullRequests.Where(Merged))
            {
                if (pull.MergedAt is { } at && (merged is null || at < merged))
                {
                    merged = at;
                }
            }

            if (merged is not { } mergedAt || item.Environments.Prod?.DeployedAt is not { } deployed || deployed < mergedAt)
            {
                continue;
            }

            hours.Add((deployed - mergedAt).TotalHours);
        }

        return Summarize(hours);
    }

    /// <summary>The column history as stays. The last stay is open when the item is still in that column.</summary>
    public static IReadOnlyList<ColumnStay> Timeline(WorkItem item, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(item);
        var visits = item.ColumnHistory.Where(visit => visit.Column is not null).ToList();
        var stays = new List<ColumnStay>(visits.Count);
        for (var index = 0; index < visits.Count; index++)
        {
            var visit = visits[index];
            TimeSpan? spent = null;
            var open = false;
            if (visit.Hours is { } hours)
            {
                spent = TimeSpan.FromHours(hours);
                open = visit.LeftAt is null;
            }
            else if (visit.EnteredAt is { } entered)
            {
                DateTimeOffset? end = index + 1 < visits.Count ? visits[index + 1].EnteredAt : null;
                if (end is null && visit.Column is { } column && WorkColumns.Same(item.Column, column))
                {
                    end = now;
                    open = true;
                }

                if (end is { } until && until >= entered)
                {
                    spent = until - entered;
                }
            }

            stays.Add(new ColumnStay(WorkColumns.Canonical(visit.Column!), visit.EnteredAt, spent, open));
        }

        return stays;
    }

    public static string Stay(ColumnStay stay)
    {
        ArgumentNullException.ThrowIfNull(stay);
        if (stay.Spent is not { } spent)
        {
            return stay.Column;
        }

        var length = TimeText.Span(spent);
        return stay.Open ? $"{stay.Column} · {length} so far" : $"{stay.Column} · {length}";
    }

    /// <summary><c>#181 · merged · checks passed · merged 2 h ago · a1b2c3d</c>. A part the pull request does not have is left out.</summary>
    public static string PullLine(WorkPullRequest pull, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(pull);
        var parts = new List<string> { WorkItemsText.PullLabel(pull) };
        if (pull.State is { } state)
        {
            parts.Add(state);
        }

        if (pull.Checks is { } checks)
        {
            parts.Add($"checks {checks}");
        }

        if (pull.MergedAt is { } merged)
        {
            parts.Add($"merged {TimeText.Ago(merged, now)}");
        }

        if (pull.MergeSha is { } sha)
        {
            parts.Add(BuildText.ShortCommit(sha));
        }

        return string.Join(" · ", parts);
    }

    public static bool Merged(WorkPullRequest pull) =>
        pull.MergedAt is not null || WorkItemsText.IsMerged(pull.State);

    private static Dictionary<string, int> Wip(IReadOnlyList<WorkItem> items)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in WorkColumns.Active)
        {
            counts[column] = items.Count(item => WorkColumns.Same(item.Column, column));
        }

        foreach (var extra in items.Where(item => !WorkColumns.IsKnown(item.Column)))
        {
            var name = WorkColumns.Canonical(extra.Column);
            counts[name] = counts.GetValueOrDefault(name) + 1;
        }

        return counts;
    }

    private static WorkDuration? Durations(IReadOnlyList<WorkItem> items, bool lead)
    {
        var hours = new List<double>();
        foreach (var item in items)
        {
            var done = WorkItemsText.Entered(item, WorkColumns.Done);
            if (done is null)
            {
                continue;
            }

            var start = lead ? First(item) : FirstWorking(item);
            if (start is { } began && done.Value >= began)
            {
                hours.Add((done.Value - began).TotalHours);
            }
        }

        return Summarize(hours);
    }

    private static DateTimeOffset? First(WorkItem item)
    {
        DateTimeOffset? found = null;
        foreach (var visit in item.ColumnHistory)
        {
            if (visit.EnteredAt is { } at && (found is null || at < found))
            {
                found = at;
            }
        }

        return found;
    }

    private static DateTimeOffset? FirstWorking(WorkItem item)
    {
        DateTimeOffset? found = null;
        foreach (var visit in item.ColumnHistory)
        {
            if (visit.EnteredAt is { } at && visit.Column is not null && !WorkColumns.Same(visit.Column, WorkColumns.Todo) && (found is null || at < found))
            {
                found = at;
            }
        }

        return found ?? First(item);
    }

    private static List<WorkWeek> Weeks(IReadOnlyList<WorkItem> items)
    {
        var merged = new Dictionary<DateTimeOffset, int>();
        var deployed = new Dictionary<DateTimeOffset, int>();
        foreach (var item in items)
        {
            foreach (var pull in item.PullRequests)
            {
                if (pull.MergedAt is { } at)
                {
                    var week = WeekOf(at);
                    merged[week] = merged.GetValueOrDefault(week) + 1;
                }
            }

            if (item.Environments.Prod?.DeployedAt is { } prod)
            {
                var week = WeekOf(prod);
                deployed[week] = deployed.GetValueOrDefault(week) + 1;
            }
        }

        return
        [
            .. merged.Keys.Concat(deployed.Keys).Distinct().Order().Select(week => new WorkWeek(week, merged.GetValueOrDefault(week), deployed.GetValueOrDefault(week))),
        ];
    }

    private static DateTimeOffset WeekOf(DateTimeOffset time)
    {
        var date = time.UtcDateTime.Date;
        var monday = date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
        return new DateTimeOffset(monday, TimeSpan.Zero);
    }

    private static WorkRework? ReworkOf(IReadOnlyList<WorkItem> items)
    {
        var known = items.Where(item => item.Rework is not null).ToList();
        if (known.Count == 0)
        {
            return null;
        }

        var failures = known.Sum(item => item.Rework!.FtFailures ?? 0);
        var reverts = known.Sum(item => item.Rework!.Reverts ?? 0);
        var touched = known.Count(item => (item.Rework!.FtFailures ?? 0) > 0 || (item.Rework!.Reverts ?? 0) > 0);
        return new WorkRework(failures, reverts, items.Count == 0 ? null : (double)touched / items.Count);
    }

    private static IEnumerable<(string Column, WorkDuration Value)> Measured(IReadOnlyList<WorkItem> items, DateTimeOffset now)
    {
        var hours = new Dictionary<string, List<double>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            foreach (var stay in Timeline(item, now))
            {
                if (stay.Spent is not { } spent)
                {
                    continue;
                }

                if (!hours.TryGetValue(stay.Column, out var list))
                {
                    list = [];
                    hours[stay.Column] = list;
                }

                list.Add(spent.TotalHours);
            }
        }

        return hours.Select(pair => (pair.Key, Summarize(pair.Value)!));
    }

    private static WorkDuration? Summarize(List<double> hours)
    {
        if (hours.Count == 0)
        {
            return null;
        }

        hours.Sort();
        return new WorkDuration(Percentile(hours, 0.5), Percentile(hours, 0.85), hours.Count);
    }

    private static double Percentile(List<double> sorted, double fraction)
    {
        var rank = (int)Math.Ceiling(sorted.Count * fraction);
        var index = Math.Clamp(rank, 1, sorted.Count) - 1;
        return Math.Round(sorted[index], 2);
    }
}
