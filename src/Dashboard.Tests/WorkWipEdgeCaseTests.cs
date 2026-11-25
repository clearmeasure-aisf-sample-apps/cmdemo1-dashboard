namespace Dashboard.Tests;

/// <summary>
/// <see cref="WorkWip"/> is one column of the work-in-progress strip and how many items are counted there.
/// <see cref="WorkWipTests"/> pins how <see cref="WorkItemsText.Wip(WorkItemsStats, IReadOnlyList{WorkItem})"/>
/// fills the strip; what is pinned here is what that leaves open: the record's own value semantics (the parts kept
/// as given, at the ends of their range and when blank or absent, equality, copies, deconstruction and its words),
/// the report overload agreeing with the slice overload, and the counts the file gives that are not ordinary.
/// </summary>
public class WorkWipEdgeCaseTests
{
    private static readonly IReadOnlyList<WorkItem> NoItems = [];

    private static WorkItem Item(int? number, string? column = null) =>
        new(number, $"Item {number}", null, null, column, [], null, null, [], [], null, WorkItemEnvironments.None, null);

    private static WorkItemsStats Stats(params (string Column, int Count)[] counts) =>
        new(counts.ToDictionary(pair => pair.Column, pair => pair.Count, StringComparer.OrdinalIgnoreCase), null, null, [], null, null);

    // ----- the parts, kept as given -----

    [Theory]
    [InlineData("Todo", 0)]
    [InlineData("In Progress", 1)]
    [InlineData("Unplaced", int.MaxValue)]
    [InlineData("Review", int.MinValue)]
    [InlineData("Review", -1)]
    public void TheColumnAndTheCountAreKeptExactlyAsGiven(string column, int count)
    {
        var wip = new WorkWip(column, count);

        Assert.Equal(column, wip.Column);
        Assert.Equal(count, wip.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" todo ")]
    public void ABlankOrUntrimmedColumnIsKeptAndNotTidied(string column)
    {
        Assert.Equal(column, new WorkWip(column, 2).Column);
    }

    [Fact]
    public void AnAbsentColumnIsKeptAbsentAndNotRefused()
    {
        var wip = new WorkWip(null!, 0);

        Assert.Null(wip.Column);
        Assert.Equal(0, wip.Count);
    }

    // ----- equality -----

    [Fact]
    public void TwoRowsWithTheSameColumnAndCountAreEqualAndHashAlike()
    {
        var first = new WorkWip("Todo", 3);
        var second = new WorkWip("Todo", 3);

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void ADifferentCountOrColumnIsADifferentRow()
    {
        var row = new WorkWip("Todo", 3);

        Assert.NotEqual(row, new WorkWip("Todo", 4));
        Assert.NotEqual(row, new WorkWip("Review", 3));
    }

    [Fact]
    public void TheColumnIsComparedOrdinallySoCasingMatters()
    {
        Assert.NotEqual(new WorkWip("Todo", 1), new WorkWip("todo", 1));
    }

    [Fact]
    public void ARowIsNotEqualToNothingOrToAnotherKindOfObject()
    {
        var row = new WorkWip("Todo", 1);

        Assert.False(row.Equals(null));
        Assert.False(row.Equals((object)"Todo"));
        Assert.False(row == null);
        Assert.True(null == (WorkWip?)null);
    }

    [Fact]
    public void RowsWithAnAbsentColumnAreEqualToEachOther()
    {
        Assert.Equal(new WorkWip(null!, 0), new WorkWip(null!, 0));
        Assert.NotEqual(new WorkWip(null!, 0), new WorkWip(string.Empty, 0));
    }

    [Fact]
    public void EqualRowsCollapseInASet()
    {
        var set = new HashSet<WorkWip> { new("Todo", 1), new("Todo", 1), new("Todo", 2) };

        Assert.Equal(2, set.Count);
    }

    // ----- copies, deconstruction, words -----

    [Fact]
    public void ACopyThatTurnsTheCountKeepsTheColumnAndLeavesTheOriginal()
    {
        var original = new WorkWip("Review", 1);
        var copy = original with { Count = 5 };

        Assert.Equal(new WorkWip("Review", 5), copy);
        Assert.Equal(1, original.Count);
        Assert.NotSame(original, copy);
    }

    [Fact]
    public void ACopyThatTurnsNothingIsEqualButNotTheSameInstance()
    {
        var original = new WorkWip("UAT", 2);
        var copy = original with { };

        Assert.Equal(original, copy);
        Assert.NotSame(original, copy);
    }

    [Fact]
    public void ARowFallsApartIntoItsColumnAndCount()
    {
        var (column, count) = new WorkWip(WorkColumns.Unplaced, 7);

        Assert.Equal(WorkColumns.Unplaced, column);
        Assert.Equal(7, count);
    }

    [Fact]
    public void ARowSaysItsColumnAndCountInItsWords()
    {
        Assert.Equal("WorkWip { Column = Todo, Count = 3 }", new WorkWip("Todo", 3).ToString());
        Assert.Equal("WorkWip { Column = , Count = 0 }", new WorkWip(null!, 0).ToString());
    }

    // ----- the strip, as rows of this record -----

    [Fact]
    public void AnEmptyBoardIsTheActiveColumnsAtZeroAsEqualRows()
    {
        var expected = WorkColumns.Active.Select(column => new WorkWip(column, 0));

        Assert.Equal(expected, WorkItemsText.Wip(WorkItemsStats.Empty, NoItems));
    }

    [Fact]
    public void TheReportOverloadReadsTheReportsStatsAndItems()
    {
        var stats = Stats(("Todo", 2), ("Blocked", 1));
        var items = new[] { Item(1, "Review") };
        var report = new WorkItemsReport(null, null, stats, items, []);

        Assert.Equal(WorkItemsText.Wip(stats, items), WorkItemsText.Wip(report));
    }

    [Fact]
    public void TheReportOverloadCountsTheItemsWhenTheReportHasNoCounts()
    {
        var report = new WorkItemsReport(null, null, WorkItemsStats.Empty, [Item(1, "Todo"), Item(2, "Blocked")], []);

        var rows = WorkItemsText.Wip(report);

        Assert.Equal(new WorkWip("Todo", 1), rows[0]);
        Assert.Equal(new WorkWip(WorkColumns.Unplaced, 1), rows[^1]);
    }

    [Fact]
    public void ANegativeFileCountForABoardColumnIsPassedThroughAsGiven()
    {
        var rows = WorkItemsText.Wip(Stats(("Todo", -1)), NoItems);

        Assert.Equal(new WorkWip(WorkColumns.Active[0], -1), rows[0]);
    }

    [Fact]
    public void UnplacedFileCountsThatSumToNothingOrLessGiveNoUnplacedRow()
    {
        Assert.DoesNotContain(WorkItemsText.Wip(Stats(("Blocked", -2)), NoItems), row => row.Column == WorkColumns.Unplaced);
        Assert.DoesNotContain(WorkItemsText.Wip(Stats(("Blocked", 2), ("Parked", -2)), NoItems), row => row.Column == WorkColumns.Unplaced);
    }

    [Fact]
    public void UnplacedFileCountsAreSummedNegativesIncluded()
    {
        var rows = WorkItemsText.Wip(Stats(("Blocked", 3), ("Parked", -1)), NoItems);

        Assert.Equal(new WorkWip(WorkColumns.Unplaced, 2), rows[^1]);
    }

    [Fact]
    public void AFileCountAtTheLargestIntIsKeptForItsColumn()
    {
        var rows = WorkItemsText.Wip(Stats(("UAT", int.MaxValue)), NoItems);

        Assert.Equal(new WorkWip(WorkColumns.Active[^1], int.MaxValue), rows[WorkColumns.Active.Count - 1]);
    }

    [Fact]
    public void FileCountsWinEvenWhenEveryOneIsZero()
    {
        var rows = WorkItemsText.Wip(Stats(("Todo", 0)), [Item(1, "Todo"), Item(2, "Blocked")]);

        Assert.All(rows, row => Assert.Equal(0, row.Count));
        Assert.DoesNotContain(rows, row => row.Column == WorkColumns.Unplaced);
    }
}
