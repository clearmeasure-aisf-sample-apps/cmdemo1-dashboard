using Dashboard.Health;

namespace Dashboard.Work;

/// <summary>
/// The board's columns in words and counts: work in progress per column, the items grouped by the column they hold,
/// the recently-done order, when an item entered its column, and a column heading's element id. Read beside
/// <see cref="WorkColumns"/>; <see cref="WorkItemsText"/> is the page's door to these.
/// </summary>
public static class WorkBoardText
{
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
            : items.Count(IsUnplaced);
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

        var unplaced = items.Where(IsUnplaced).ToList();
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

    /// <summary>An element id for a column heading: <c>work-col-in-progress</c>.</summary>
    public static string ColumnId(string column)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(column);
        var slug = new string(column.Trim().ToLowerInvariant().Select(character => char.IsAsciiLetterOrDigit(character) ? character : '-').ToArray());
        return "work-col-" + string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>An item whose column is not one of the board's and is not Done: it is counted under Unplaced.</summary>
    private static bool IsUnplaced(WorkItem item) => !WorkColumns.IsKnown(item.Column) && !WorkColumns.IsDone(item.Column);

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
