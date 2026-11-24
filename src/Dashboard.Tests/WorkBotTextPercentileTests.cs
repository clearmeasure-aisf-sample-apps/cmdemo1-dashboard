namespace Dashboard.Tests;

/// <summary>
/// The percentiles and rounding that <see cref="WorkBotText"/> uses for all duration measurements: the median
/// (nearest-rank, lower middle for even counts) and the 85th percentile, both rounded to two decimal places
/// of an hour. These tests use the public entry points to pin the behavior without depending on private methods.
/// </summary>
public class WorkBotTextPercentileTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 22, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset At(int day, int hour = 0, int minute = 0) => new(2026, 11, day, hour, minute, 0, TimeSpan.Zero);

    private static WorkItem Item(int number, string? column = null, IReadOnlyList<ColumnVisit>? history = null, IReadOnlyList<WorkPullRequest>? pulls = null) =>
        new(number, $"Item {number}", null, null, column, history ?? [], null, null, [], pulls ?? [], null, WorkItemEnvironments.None, null);

    /// <summary>An item that completed after <paramref name="hours"/> of cycle time (Todo→InProgress→Done).</summary>
    private static WorkItem DoneAfterHours(double hours) =>
        Item(1, "Done", [
            new ColumnVisit(WorkColumns.Todo, At(1, 8)),
            new ColumnVisit(WorkColumns.InProgress, At(1, 8)),
            new ColumnVisit(WorkColumns.Done, At(1, 8).AddHours(hours)),
        ]);

    /// <summary>A pull request merged at <paramref name="mergedAt"/>.</summary>
    private static WorkPullRequest MergedPull(int number, DateTimeOffset mergedAt) =>
        new(number, null, "merged", mergedAt, "sha");

    [Fact]
    public void ASingleItemHasMedianEqualToP85EqualToItsDuration()
    {
        var stats = WorkBotText.FromItems([DoneAfterHours(4.5)]);

        Assert.NotNull(stats.CycleTimeHours);
        Assert.Equal(4.5, stats.CycleTimeHours!.Median);
        Assert.Equal(4.5, stats.CycleTimeHours!.P85);
        Assert.Equal(1, stats.CycleTimeHours!.N);
    }

    [Fact]
    public void AnEvenCountMedianIsTheLowerMiddleNotTheAverage()
    {
        // 4 items with cycle times 1, 2, 3, 4 hours
        // Nearest-rank median at 0.5: rank = ceil(4 * 0.5) = 2, index = 1 → value = 2 (lower middle)
        var items = new List<WorkItem>
        {
            DoneAfterHours(1),
            DoneAfterHours(2),
            DoneAfterHours(3),
            DoneAfterHours(4),
        };

        var stats = WorkBotText.FromItems(items);

        Assert.NotNull(stats.CycleTimeHours);
        // Lower middle (2), not the average (2.5)
        Assert.Equal(2.0, stats.CycleTimeHours!.Median);
        // P85: rank = ceil(4 * 0.85) = 4, index = 3 → value = 4
        Assert.Equal(4.0, stats.CycleTimeHours!.P85);
        Assert.Equal(4, stats.CycleTimeHours!.N);
    }

    [Fact]
    public void P85OfTwentyValuesLandsAtRank17NotTheMaximum()
    {
        // 20 items with cycle times 1..20 hours
        // P85: rank = ceil(20 * 0.85) = 17, index = 16 → value = 17
        var items = Enumerable.Range(1, 20).Select(i => DoneAfterHours(i)).ToList();

        var stats = WorkBotText.FromItems(items);

        Assert.NotNull(stats.CycleTimeHours);
        // Median: rank = ceil(20 * 0.5) = 10, index = 9 → value = 10
        Assert.Equal(10.0, stats.CycleTimeHours!.Median);
        // P85: rank = ceil(20 * 0.85) = 17, index = 16 → value = 17 (not the max 20)
        Assert.Equal(17.0, stats.CycleTimeHours!.P85);
        Assert.Equal(20, stats.CycleTimeHours!.N);
    }

    [Fact]
    public void DurationsAreRoundedToTwoDecimalPlaces()
    {
        // 1 hour + 23 minutes + 20 seconds = 1.38888... hours
        var hours = 1 + 23.0 / 60.0 + 20.0 / 3600.0;
        var stats = WorkBotText.FromItems([DoneAfterHours(hours)]);

        Assert.NotNull(stats.CycleTimeHours);
        // Rounded to 2 decimal places: 1.39
        Assert.Equal(1.39, stats.CycleTimeHours!.Median);
        Assert.Equal(1.39, stats.CycleTimeHours!.P85);
    }

    [Fact]
    public void ColumnWaitsUseTheSamePercentilesAndRounding()
    {
        // Two items in UAT: 1 hour and 2 hours
        var item1 = Item(1, WorkColumns.Done,
        [
            new ColumnVisit(WorkColumns.Uat, At(5, 8), At(5, 9), 1.0),
            new ColumnVisit(WorkColumns.Done, At(5, 9)),
        ]);
        var item2 = Item(2, WorkColumns.Done,
        [
            new ColumnVisit(WorkColumns.Uat, At(6, 8), At(6, 10), 2.0),
            new ColumnVisit(WorkColumns.Done, At(6, 10)),
        ]);

        var waits = WorkBotText.ColumnWaits(null, [item1, item2], Now);

        var uatWait = waits.First(w => w.Column == WorkColumns.Uat);
        // Median of 2 values: rank = ceil(2 * 0.5) = 1, index = 0 → value = 1 (lower middle)
        Assert.Equal(1.0, uatWait.Duration.Median);
        // P85 of 2 values: rank = ceil(2 * 0.85) = 2, index = 1 → value = 2
        Assert.Equal(2.0, uatWait.Duration.P85);
        Assert.Equal(2, uatWait.Duration.N);
    }

    [Fact]
    public void MergeToProdUsesTheSamePercentilesAndRounding()
    {
        // One item merged then deployed 4 hours later
        var item = new WorkItem(
            1, "Item 1", null, null, WorkColumns.Done,
            [
                new ColumnVisit(WorkColumns.Todo, At(1, 8)),
                new ColumnVisit(WorkColumns.Done, At(2, 8)),
            ],
            null, null, [],
            [MergedPull(1, At(2, 12))],
            null,
            new WorkItemEnvironments(null, null, new WorkDeployment("2.4.1", At(2, 16))),
            null);

        var mergeToProd = WorkBotText.MergeToProd(null, [item]);

        Assert.NotNull(mergeToProd);
        Assert.Equal(4.0, mergeToProd.Median);
        Assert.Equal(4.0, mergeToProd.P85);
        Assert.Equal(1, mergeToProd.N);
    }
}