using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The one "earliest of the times that are there" rule behind lead time, cycle time and merge to production, pinned
/// through <see cref="WorkBotText.FromItems"/> and <see cref="WorkBotText.MergeToProd"/>.
/// </summary>
public class WorkBotTextEarliestTests
{
    private static DateTimeOffset At(int day, int hour = 0) => new(2026, 11, day, hour, 0, 0, TimeSpan.Zero);

    private static WorkItem Item(
        int number,
        IReadOnlyList<ColumnVisit> history,
        IReadOnlyList<WorkPullRequest>? pulls = null,
        DateTimeOffset? deployedProd = null) =>
        new(
            number,
            $"Item {number}",
            null,
            null,
            "Done",
            history,
            null,
            null,
            [],
            pulls ?? [],
            null,
            deployedProd is { } at ? new WorkItemEnvironments(null, null, new WorkDeployment("1.0.0", at)) : WorkItemEnvironments.None,
            null,
            WorkWorker.Unknown,
            null);

    private static WorkPullRequest Pull(int number, DateTimeOffset? mergedAt, string? state = null) =>
        new(number, null, state, mergedAt, null);

    [Fact]
    public void LeadTimeStartsAtTheEarliestEnteredAtEvenWhenTheHistoryIsOutOfOrder()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var stats = WorkBotText.FromItems(
        [
            Item(1, [
                new ColumnVisit(WorkColumns.Review, At(3)),
                new ColumnVisit(WorkColumns.Done, At(5)),
                new ColumnVisit(WorkColumns.Todo, At(1)),
                new ColumnVisit(null, null),
            ]),
        ]);

        Assert.Equal(new WorkDuration(96, 96, 1), stats.LeadTimeHours);
        Assert.Equal(new WorkDuration(48, 48, 1), stats.CycleTimeHours);
    }

    [Fact]
    public void CycleTimeSkipsTheTodoVisitAndVisitsWithNoColumn()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var stats = WorkBotText.FromItems(
        [
            Item(1, [
                new ColumnVisit(null, At(1)),
                new ColumnVisit("todo", At(2)),
                new ColumnVisit(WorkColumns.Review, At(4)),
                new ColumnVisit(WorkColumns.Done, At(6)),
            ]),
        ]);

        Assert.Equal(new WorkDuration(48, 48, 1), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(120, 120, 1), stats.LeadTimeHours);
    }

    [Fact]
    public void CycleTimeWithOnlyTodoVisitsBeforeDoneStartsAtTheEarliestTimedVisitThatIsNotTodo()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var stats = WorkBotText.FromItems(
        [
            Item(1, [
                new ColumnVisit(WorkColumns.Todo, At(3)),
                new ColumnVisit(WorkColumns.Todo, At(2)),
                new ColumnVisit(WorkColumns.Review, null),
                new ColumnVisit(null, null),
                new ColumnVisit(WorkColumns.Done, At(4)),
            ]),
        ]);

        // A counted item always has a timed Done visit, and Done is not Todo, so the working start is Done itself
        // (zero cycle time) rather than the fallback to the earliest Todo visit; lead time takes the earliest Todo.
        Assert.Equal(new WorkDuration(0, 0, 1), stats.CycleTimeHours);
        Assert.Equal(new WorkDuration(48, 48, 1), stats.LeadTimeHours);
    }

    [Fact]
    public void AnItemWhoseDoneVisitHasNoTimeIsNotCountedEvenThoughTodoVisitsAre()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var stats = WorkBotText.FromItems(
        [
            Item(1, [
                new ColumnVisit(WorkColumns.Todo, At(3)),
                new ColumnVisit(WorkColumns.Todo, At(2)),
                new ColumnVisit(WorkColumns.Review, null),
                new ColumnVisit(WorkColumns.Done, null),
            ]),
        ]);

        Assert.Null(stats.CycleTimeHours);
        Assert.Null(stats.LeadTimeHours);
    }

    [Fact]
    public void MergeToProdUsesTheEarliestMergedAtAndIgnoresPullsThatWereNotMerged()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var items = new[]
        {
            Item(
                1,
                [],
                [Pull(11, At(3)), Pull(12, null, "open"), Pull(13, At(2)), Pull(14, null, "merged"), Pull(15, At(4))],
                At(5)),
        };

        Assert.Equal(new WorkDuration(72, 72, 1), WorkBotText.MergeToProd(null, items));
    }

    [Fact]
    public void AnItemWhoseEarliestMergeIsAfterItsProductionDeployIsLeftOut()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var items = new[]
        {
            Item(1, [], [Pull(11, At(6)), Pull(12, At(7))], At(5)),
            Item(2, [], [Pull(21, At(1)), Pull(22, At(9))], At(2)),
        };

        Assert.Equal(new WorkDuration(24, 24, 1), WorkBotText.MergeToProd(null, items));
        Assert.Null(WorkBotText.MergeToProd(null, [items[0]]));
    }

    [Fact]
    public void AnItemWithOnlyUnmergedPullsHasNoMergeToProd()
    {
        using var scope = new CultureScope(CultureInfo.InvariantCulture);
        var items = new[] { Item(1, [], [Pull(11, null, "merged"), Pull(12, null, "open")], At(5)) };

        Assert.Null(WorkBotText.MergeToProd(null, items));
    }
}
