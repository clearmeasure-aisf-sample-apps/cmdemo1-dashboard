using System.Collections;

namespace Dashboard.Tests;

/// <summary>
/// A <see cref="PodTally" /> folds the rows of a namespace in one walk. The page reads a group's seven numbers off
/// one tally, so they agree with the group's own members, and the restart count past an <see cref="int" /> still
/// throws when it is read and never earlier.
/// </summary>
public sealed class PodTallyTests
{
    private static readonly PodRow[] Mixed =
    [
        Row("web", PodState.Ready, restarts: 1, cpu: 10, memory: 1024),
        Row("api", PodState.Ready, cpu: 2.5),
        Row("new", PodState.Starting, restarts: 2),
        Row("broken", PodState.Unhealthy, restarts: 3, memory: 2048),
        Row("backup", PodState.Finished, restarts: 4, cpu: 0.25),
    ];

    [Fact]
    public void ItWalksTheRowsOnce()
    {
        var rows = new CountingRows(Mixed);

        var tally = new PodTally(rows);

        Assert.Equal(1, rows.Walks);
        Assert.Equal((5, 2, 1, 1, 1, 4), (tally.Rows, tally.Ready, tally.Starting, tally.Unhealthy, tally.Finished, tally.Total));
        Assert.Equal(10, tally.Restarts);
        Assert.Equal(12.75, tally.CpuUsage);
        Assert.Equal(3072d, tally.MemoryUsage);
        Assert.Equal(1, rows.Walks);
    }

    [Fact]
    public void ItGivesTheSameNumbersAsTheGroup()
    {
        var group = new NamespaceGroup("apps", null, true, Mixed, []);
        var tally = new PodTally(group.Pods);

        Assert.Equal(
            (group.Ready, group.Total, group.Finished, group.Unhealthy, group.Restarts, group.CpuUsage, group.MemoryUsage),
            (tally.Ready, tally.Total, tally.Finished, tally.Unhealthy, tally.Restarts, tally.CpuUsage, tally.MemoryUsage));
    }

    [Fact]
    public void TheStatesAddUpToTheRowsAndTheTotalIsEveryRowButTheFinished()
    {
        var tally = new PodTally(Mixed);

        Assert.Equal(tally.Rows, tally.Ready + tally.Starting + tally.Unhealthy + tally.Finished);
        Assert.Equal(tally.Rows - tally.Finished, tally.Total);
    }

    [Fact]
    public void NoRowsIsZeroCountsAndNoSums()
    {
        var tally = new PodTally([]);

        Assert.Equal((0, 0, 0, 0, 0, 0, 0), (tally.Rows, tally.Ready, tally.Starting, tally.Unhealthy, tally.Finished, tally.Total, tally.Restarts));
        Assert.Null(tally.CpuUsage);
        Assert.Null(tally.MemoryUsage);
    }

    [Fact]
    public void NoRowsAtAllIsRefused() =>
        Assert.Throws<ArgumentNullException>(() => new PodTally(null!));

    [Fact]
    public void RestartsPastAnIntThrowOnlyWhenRead()
    {
        PodRow[] over = [Row("web", PodState.Ready, restarts: int.MaxValue), Row("api", PodState.Starting, restarts: 1)];

        var tally = new PodTally(over);
        var group = new NamespaceGroup("apps", null, true, over, []);

        Assert.Equal((1, 1, 2), (tally.Ready, tally.Starting, tally.Total));
        Assert.Throws<OverflowException>(() => tally.Restarts);
        Assert.Equal((1, 2, 0, 0), (group.Ready, group.Total, group.Finished, group.Unhealthy));
        Assert.Throws<OverflowException>(() => group.Restarts);
        Assert.Throws<OverflowException>(() => ClusterText.Summary(group));
    }

    /// <summary>As a checked <c>Sum</c> does, a count that goes past an <see cref="int" /> on the way throws even
    /// when a later negative count would bring it back.</summary>
    [Fact]
    public void RestartsThatPassAnIntOnTheWayThrowLikeACheckedSum()
    {
        PodRow[] rows =
        [
            Row("a", PodState.Ready, restarts: int.MaxValue),
            Row("b", PodState.Ready, restarts: 1),
            Row("c", PodState.Ready, restarts: -5),
        ];

        Assert.Throws<OverflowException>(() => rows.Sum(row => row.Pod.Restarts));
        Assert.Throws<OverflowException>(() => new PodTally(rows).Restarts);
    }

    [Fact]
    public void TheSummaryOfAGroupReadsItsRowsOnce()
    {
        var rows = new CountingRows(Mixed);
        var group = new NamespaceGroup("apps", null, true, rows, []);

        var summary = ClusterText.Summary(group);

        Assert.Equal(1, rows.Walks);
        Assert.Equal("2 of 4 ready · 1 finished · 10 restarts · CPU 13 m · memory 3 KiB", summary);
    }

    private static PodRow Row(string workload, PodState state, int restarts = 0, double? cpu = null, double? memory = null) =>
        new(
            ClusterFixture.Pod(workload, restarts: restarts) with
            {
                Cpu = new ResourceUse(cpu, null, null),
                Memory = new ResourceUse(memory, null, null),
            },
            state);

    /// <summary>A list of rows that counts how often it is walked.</summary>
    private sealed class CountingRows(IReadOnlyList<PodRow> rows) : IReadOnlyList<PodRow>
    {
        public int Walks { get; private set; }

        public int Count => rows.Count;

        public PodRow this[int index] => rows[index];

        public IEnumerator<PodRow> GetEnumerator()
        {
            Walks++;
            return rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
