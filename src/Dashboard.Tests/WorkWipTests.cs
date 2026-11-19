namespace Dashboard.Tests;

/// <summary>
/// The work-in-progress strip, written by <see cref="WorkItemsText.Wip(WorkItemsStats, IReadOnlyList{WorkItem})"/>
/// itself rather than through a render. The file's counts win for the board's columns; without them the items are
/// counted. Done is never work in progress, and a column that is not one of the board's is gathered under
/// <see cref="WorkColumns.Unplaced"/> last.
/// </summary>
public class WorkWipTests
{
    private static readonly IReadOnlyList<WorkItem> NoItems = [];

    private static WorkItem Item(int? number, string? column = null) =>
        new(number, $"Item {number}", null, null, column, [], null, null, [], [], null, WorkItemEnvironments.None, null);

    /// <summary><c>stats.wipByColumn</c> as the file gives it: the column's own spelling, matched however it is cased.</summary>
    private static WorkItemsStats Stats(params (string Column, int Count)[] counts) =>
        new(counts.ToDictionary(pair => pair.Column, pair => pair.Count, StringComparer.OrdinalIgnoreCase), null, null, [], null, null);

    // ---- The file's counts win ----

    [Fact]
    public void TheFilesCountsWinAndTheItemsAreNotCountedAlongsideThem()
    {
        var rows = WorkItemsText.Wip(
            Stats(("Todo", 3), ("In Progress", 2), ("Review", 1), ("Merged", 4), ("TDD", 0), ("UAT", 1)),
            [Item(1, "Todo"), Item(2, "Todo"), Item(3, "Todo"), Item(4, "Todo"), Item(5, "Blocked"), Item(6, "Done")]);

        Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
        Assert.Equal([3, 2, 1, 4, 0, 1], rows.Select(row => row.Count));
    }

    [Fact]
    public void AColumnTheFileCountsNothingInIsZeroAndNotAbsent()
    {
        var rows = WorkItemsText.Wip(Stats(("Review", 1)), [Item(1, "Todo"), Item(2, "Todo"), Item(3, "Done")]);

        Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
        Assert.Equal([0, 0, 1, 0, 0, 0], rows.Select(row => row.Count));
    }

    [Fact]
    public void AFileCountBelongsToItsColumnHoweverTheFileSpelledIt()
    {
        var rows = WorkItemsText.Wip(Stats(("todo", 2), ("IN PROGRESS", 1), ("tdd", 5)), NoItems);

        Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
        Assert.Equal([2, 1, 0, 0, 5, 0], rows.Select(row => row.Count));
    }

    // ---- Counted from the items when the file gives no counts ----

    [Fact]
    public void WithoutTheFilesCountsTheItemsAreCountedInTheirColumns()
    {
        var rows = WorkItemsText.Wip(WorkItemsStats.Empty,
        [
            Item(1, "todo"), Item(2, "Todo"), Item(3, "IN PROGRESS"), Item(4, "reViEw"),
            Item(5, "merged"), Item(6, "tdd"), Item(7, "UAT"), Item(8, "Uat"),
        ]);

        Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
        Assert.Equal([2, 1, 1, 1, 1, 2], rows.Select(row => row.Count));
    }

    [Fact]
    public void NothingOnTheBoardIsEveryActiveColumnAtZero()
    {
        var rows = WorkItemsText.Wip(WorkItemsStats.Empty, NoItems);

        Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
        Assert.Equal([0, 0, 0, 0, 0, 0], rows.Select(row => row.Count));
    }

    // ---- Done is not work in progress ----

    [Fact]
    public void DoneIsWorkFinishedNeitherFromTheFileNorFromTheItems()
    {
        var fromFile = WorkItemsText.Wip(Stats(("Todo", 1), ("Done", 9)), NoItems);
        var fromItems = WorkItemsText.Wip(WorkItemsStats.Empty, [Item(1, "Todo"), Item(2, WorkColumns.Done), Item(3, "done")]);

        foreach (var rows in new[] { fromFile, fromItems })
        {
            Assert.DoesNotContain(rows, row => WorkColumns.IsDone(row.Column));
            Assert.DoesNotContain(rows, row => row.Column == WorkColumns.Unplaced);
            Assert.Equal(WorkColumns.Active, rows.Select(row => row.Column));
            Assert.Equal([1, 0, 0, 0, 0, 0], rows.Select(row => row.Count));
        }
    }

    // ---- A column that is not the board's is Unplaced ----

    [Fact]
    public void AFileCountForAColumnThatIsNotTheBoardIsUnplaced()
    {
        var rows = WorkItemsText.Wip(
            Stats(("Todo", 2), ("Blocked", 3), ("Release Queue", 1), ("Done", 9)),
            [Item(1, "Technical Design"), Item(2, "Functional Testing")]);

        Assert.Equal(WorkColumns.Active.Append(WorkColumns.Unplaced), rows.Select(row => row.Column));
        Assert.Equal([2, 0, 0, 0, 0, 0, 4], rows.Select(row => row.Count));
        Assert.Equal(4, rows.Single(row => row.Column == WorkColumns.Unplaced).Count);
    }

    [Fact]
    public void AnItemInAColumnTheBoardKnowsNothingAboutIsUnplaced()
    {
        var rows = WorkItemsText.Wip(WorkItemsStats.Empty,
        [
            Item(1, "Blocked"), Item(2, "Release Queue"), Item(3, null), Item(4, ""), Item(5, "   "),
            Item(6, WorkColumns.Unplaced), Item(7, WorkColumns.Review), Item(8, WorkColumns.Done),
        ]);

        Assert.Equal(WorkColumns.Active.Append(WorkColumns.Unplaced), rows.Select(row => row.Column));
        Assert.Equal([0, 0, 1, 0, 0, 0, 6], rows.Select(row => row.Count));
        Assert.Equal(1, rows.Single(row => row.Column == WorkColumns.Review).Count);
    }

    [Fact]
    public void ThereIsNoUnplacedRowWhenNothingIsUnplaced()
    {
        Assert.DoesNotContain(WorkItemsText.Wip(Stats(("Todo", 1), ("Review", 0)), [Item(1, "Blocked")]), row => row.Column == WorkColumns.Unplaced);
        Assert.DoesNotContain(WorkItemsText.Wip(WorkItemsStats.Empty, [Item(1, "Todo"), Item(2, WorkColumns.Done)]), row => row.Column == WorkColumns.Unplaced);
        Assert.DoesNotContain(WorkItemsText.Wip(WorkItemsStats.Empty, NoItems), row => row.Column == WorkColumns.Unplaced);
    }

    // ---- Order ----

    [Fact]
    public void TheRowsFollowTheBoardsOrderWithUnplacedLastWhicheverWayTheyWereCounted()
    {
        var fromFile = WorkItemsText.Wip(Stats(("UAT", 1), ("Todo", 2), ("Blocked", 1)), NoItems);
        var fromItems = WorkItemsText.Wip(WorkItemsStats.Empty, [Item(1, "UAT"), Item(2, "Todo"), Item(3, "Blocked"), Item(4, "Done"), Item(5, "todo")]);

        foreach (var rows in new[] { fromFile, fromItems })
        {
            Assert.Equal(WorkColumns.Active.Append(WorkColumns.Unplaced), rows.Select(row => row.Column));
            Assert.Equal([2, 0, 0, 0, 0, 1, 1], rows.Select(row => row.Count));
        }
    }

    // ---- Nothing in hand ----

    [Fact]
    public void StatsThatAreNothingAndItemsThatAreNothingAreRefused()
    {
        var noStats = Assert.Throws<ArgumentNullException>(() => WorkItemsText.Wip(null!, [Item(1, "Todo")]));
        var noItems = Assert.Throws<ArgumentNullException>(() => WorkItemsText.Wip(WorkItemsStats.Empty, null!));

        Assert.Equal("stats", noStats.ParamName);
        Assert.Equal("items", noItems.ParamName);
    }
}
