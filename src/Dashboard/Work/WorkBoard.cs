namespace Dashboard.Work;

/// <summary>
/// The items counted and grouped across the board's <see cref="WorkColumns"/>: work in progress per column, the
/// active groups, and Done newest first. Page wording stays on <see cref="WorkItemsText"/>.
/// </summary>
internal static class WorkBoard
{
    /// <summary>
    /// Work in progress for one slice. The file's counts win for the board's columns; without them the items are
    /// counted. A name that is not one of those columns is counted under <see cref="WorkColumns.Unplaced"/>.
    /// </summary>
    internal static IReadOnlyList<WorkWip> Wip(WorkItemsStats stats, IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(stats);
        ArgumentNullException.ThrowIfNull(items);
        var counts = stats.WipByColumn;
        var fromFile = counts.Count > 0;
        var rows = new List<WorkWip>();
        foreach (var column in WorkColumns.Active)
        {
            rows.Add(new WorkWip(column, fromFile ? CountOf(counts, column) : items.Count(item => WorkColumns.Same(item.Identity.Column, column))));
        }

        var unplaced = fromFile
            ? counts.Where(pair => !WorkColumns.IsKnown(pair.Key)).Sum(pair => pair.Value)
            : items.Count(item => IsUnplaced(item.Identity.Column));
        if (unplaced > 0)
        {
            rows.Add(new WorkWip(WorkColumns.Unplaced, unplaced));
        }

        return rows;
    }

    /// <summary>The board's active columns that hold work, in order, then <see cref="WorkColumns.Unplaced"/>.</summary>
    internal static IReadOnlyList<WorkColumnGroup> Active(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var groups = new List<WorkColumnGroup>();
        foreach (var column in WorkColumns.Active)
        {
            var found = items.Where(item => WorkColumns.Same(item.Identity.Column, column)).ToList();
            if (found.Count > 0)
            {
                groups.Add(new WorkColumnGroup(column, found));
            }
        }

        var unplaced = items.Where(item => IsUnplaced(item.Identity.Column)).ToList();
        if (unplaced.Count > 0)
        {
            groups.Add(new WorkColumnGroup(WorkColumns.Unplaced, unplaced));
        }

        return groups;
    }

    /// <summary>The items in Done, the one that arrived there last first. An item with no arrival time is last.</summary>
    internal static IReadOnlyList<WorkItem> RecentlyDone(IReadOnlyList<WorkItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return
        [
            .. items.Where(item => WorkColumns.IsDone(item.Identity.Column))
                .OrderByDescending(item => Entered(item, WorkColumns.Done) ?? DateTimeOffset.MinValue)
                .ThenByDescending(item => item.Identity.Number ?? int.MinValue),
        ];
    }

    /// <summary>When the item entered <paramref name="column"/>, the latest time the history gives; null when it never says.</summary>
    internal static DateTimeOffset? Entered(WorkItem item, string column)
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

    /// <summary>A column that is neither one of the board's nor Done: blank, or a name the board does not have.</summary>
    private static bool IsUnplaced(string? column) => !WorkColumns.IsKnown(column) && !WorkColumns.IsDone(column);

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
}
