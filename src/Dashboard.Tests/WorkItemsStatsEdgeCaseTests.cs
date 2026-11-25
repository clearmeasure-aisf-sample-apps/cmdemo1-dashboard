using Dashboard.Work;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemsStats" />, the counts a
/// <c>workitems.json</c> gives for the whole board, declared in <c>src/Dashboard/Work/WorkItemsReport.cs</c>.
/// <para>
/// What the other files already pin is left alone: which word of the file each part is read from is pinned in
/// <see cref="WorkItemsStatsPartsTests" />, and an empty or a populated board read through
/// <see cref="WorkItemsReport.Parse" /> is pinned in <see cref="WorkItemsReportEdgeCaseTests" />. What follows is the
/// record itself: the nine parts in the order they are declared, the three a caller may leave out,
/// <see cref="WorkItemsStats.Empty" /> as a block of nothing, which single part is enough for
/// <see cref="WorkItemsStats.HasAny" />, an empty-but-present collection that is not a count, equality part by part,
/// <c>with</c>, deconstruction and the one line the record says about itself. Nothing here reaches the network, the
/// disk or a clock.
/// </para>
/// </summary>
public class WorkItemsStatsEdgeCaseTests
{
    // The collection parts are reference types, and a record compares them by reference, so the tests share one
    // instance of each; the record parts (durations, the week, rework, cost, throughput) compare by value.
    private static readonly Dictionary<string, int> Wip = new() { ["Todo"] = 3 };
    private static readonly IReadOnlyList<WorkWeek> Weeks = [new(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero), 4, 2)];
    private static readonly Dictionary<string, WorkDuration> ColumnTimes = new() { ["Review"] = new(21, 22, 23) };
    private static readonly IReadOnlyList<WorkThroughput> Rows = [new("cursor", 9, 3, 1)];

    private static WorkDuration Cycle => new(1, 2, 3);

    private static WorkDuration Lead => new(11, 12, 13);

    private static WorkDuration MergeToProd => new(21, 22, 23);

    private static WorkWeek Week => new(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero), 4, 2);

    private static WorkRework Rework => new(5, 6, 0.25);

    private static WorkCost Cost => new(7.5, "metered");

    private static WorkThroughput Throughput => new("cursor", 9, 3, 1);

    /// <summary>A stats with every part set to a distinct value, in the order the record declares them.</summary>
    private static WorkItemsStats Full => new(Wip, Cycle, Lead, Weeks, Rework, Cost, ColumnTimes, Rows, MergeToProd);

    // -------------------------------------------------------------------------- the constructor and Empty

    /// <summary>The nine parts, in the order the record declares them, each come back as the property of their name.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var stats = Full;

        Assert.Same(Wip, stats.WipByColumn);
        Assert.Equal(Cycle, stats.CycleTimeHours);
        Assert.Equal(Lead, stats.LeadTimeHours);
        Assert.Same(Weeks, stats.PerWeek);
        Assert.Equal(Rework, stats.Rework);
        Assert.Equal(Cost, stats.Cost);
        Assert.Same(ColumnTimes, stats.TimePerColumnHours);
        Assert.Same(Rows, stats.Throughput);
        Assert.Equal(MergeToProd, stats.MergeToProdHours);
    }

    /// <summary>The last three parts are optional: left out, they are null, and the block is the very block that says
    /// null for them out loud.</summary>
    [Fact]
    public void TheLastThreePartsDefaultToNothing()
    {
        var wip = new Dictionary<string, int> { ["Todo"] = 1 };
        var perWeek = new List<WorkWeek> { Week };
        var stats = new WorkItemsStats(wip, Cycle, Lead, perWeek, Rework, Cost);

        Assert.Null(stats.TimePerColumnHours);
        Assert.Null(stats.Throughput);
        Assert.Null(stats.MergeToProdHours);
        Assert.Same(wip, stats.WipByColumn);
        Assert.Same(perWeek, stats.PerWeek);
        Assert.Equal(new WorkItemsStats(wip, Cycle, Lead, perWeek, Rework, Cost, null, null, null), stats);
    }

    /// <summary><see cref="WorkItemsStats.Empty" /> is a block of nothing: no column counts, no durations, no weeks, no
    /// rework, no cost, and none of the three optional parts either. It is the same block every time it is asked for.</summary>
    [Fact]
    public void EmptyIsTheSameBlockOfNothingEveryTime()
    {
        var stats = WorkItemsStats.Empty;

        Assert.Same(WorkItemsStats.Empty, stats);
        Assert.Empty(stats.WipByColumn);
        Assert.Null(stats.CycleTimeHours);
        Assert.Null(stats.LeadTimeHours);
        Assert.Empty(stats.PerWeek);
        Assert.Null(stats.Rework);
        Assert.Null(stats.Cost);
        Assert.Null(stats.TimePerColumnHours);
        Assert.Null(stats.Throughput);
        Assert.Null(stats.MergeToProdHours);
        Assert.False(stats.HasAny);
    }

    // ----------------------------------------------------------------------------------------------- HasAny

    /// <summary>Each part on its own is a count, so each of the nine parts by itself is enough for the board to say
    /// something.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void ASinglePartAloneIsEnough(int part)
    {
        var stats = part switch
        {
            0 => new WorkItemsStats(new Dictionary<string, int> { ["Todo"] = 1 }, null, null, [], null, null),
            1 => new WorkItemsStats(new Dictionary<string, int>(), Cycle, null, [], null, null),
            2 => new WorkItemsStats(new Dictionary<string, int>(), null, Lead, [], null, null),
            3 => new WorkItemsStats(new Dictionary<string, int>(), null, null, [Week], null, null),
            4 => new WorkItemsStats(new Dictionary<string, int>(), null, null, [], Rework, null),
            5 => new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, Cost),
            6 => new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, null, new Dictionary<string, WorkDuration> { ["Review"] = MergeToProd }),
            7 => new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, null, null, [Throughput]),
            _ => new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, null, null, null, MergeToProd),
        };

        Assert.True(stats.HasAny);
    }

    /// <summary><see cref="WorkItemsStats.HasAny" /> asks whether a count is there, not how large it is: a zero count,
    /// a duration of zero hours, a week of zero and a rework of zero are each a part that is there.</summary>
    [Fact]
    public void AZeroIsACountNotAnAbsence()
    {
        Assert.True(new WorkItemsStats(new Dictionary<string, int> { ["Todo"] = 0 }, null, null, [], null, null).HasAny);
        Assert.True(new WorkItemsStats(new Dictionary<string, int>(), new WorkDuration(0, 0, 0), null, [], null, null).HasAny);
        Assert.True(new WorkItemsStats(new Dictionary<string, int>(), null, null, [new WorkWeek(null, 0, 0)], null, null).HasAny);
        Assert.True(new WorkItemsStats(new Dictionary<string, int>(), null, null, [], new WorkRework(0, 0, 0), null).HasAny);
        Assert.True(new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, new WorkCost(0, null)).HasAny);
    }

    /// <summary>A collection that is present but holds nothing is not a count: an empty board of nothing says nothing,
    /// whatever the shape of its emptiness.</summary>
    [Fact]
    public void AnEmptyCollectionIsNotACount()
    {
        var stats = new WorkItemsStats(
            new Dictionary<string, int>(),
            null,
            null,
            [],
            null,
            null,
            new Dictionary<string, WorkDuration>(),
            [],
            null);

        Assert.Empty(stats.WipByColumn);
        Assert.Empty(stats.PerWeek);
        Assert.Empty(stats.TimePerColumnHours!);
        Assert.Empty(stats.Throughput!);
        Assert.False(stats.HasAny);
    }

    // ----------------------------------------------------------------------------------------- equality

    /// <summary>Two stats built from the same nine parts are one stats and hash alike; each part on its own tells two
    /// apart, and a part that is present-but-empty is not the same as one that is absent.</summary>
    [Fact]
    public void TwoStatsAreOneWhenEveryPartIs()
    {
        var one = Full;
        var again = new WorkItemsStats(Wip, Cycle, Lead, Weeks, Rework, Cost, ColumnTimes, Rows, MergeToProd);

        Assert.Equal(one, again);
        Assert.True(one == again);
        Assert.False(one != again);
        Assert.Equal(one.GetHashCode(), again.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.Equal(one, one with { WipByColumn = Wip });

        WorkItemsStats[] different =
        [
            one with { WipByColumn = new Dictionary<string, int>() },
            one with { WipByColumn = new Dictionary<string, int> { ["Todo"] = 4 } },
            one with { CycleTimeHours = null },
            one with { LeadTimeHours = null },
            one with { PerWeek = [] },
            one with { Rework = null },
            one with { Cost = null },
            one with { TimePerColumnHours = null },
            one with { Throughput = null },
            one with { MergeToProdHours = null },
            one with { CycleTimeHours = new WorkDuration(1.5, 2, 3) },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.True(one != other);
        }

        // Present-but-empty is not the same as absent: a stats that lists no columns is not the Empty block.
        Assert.NotEqual(WorkItemsStats.Empty, one with { WipByColumn = new Dictionary<string, int>(), PerWeek = [] });
        Assert.NotEqual(WorkItemsStats.Empty, new WorkItemsStats(new Dictionary<string, int>(), null, null, [], null, null, new Dictionary<string, WorkDuration>(), [], null));
    }

    /// <summary><c>with</c> changes the part it names and leaves the eight others, and the original, as they were.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var one = Full;
        var changed = one with { Cost = null, Throughput = [new WorkThroughput("beta", 1, 2, 3)] };

        Assert.Null(changed.Cost);
        Assert.Equal([new WorkThroughput("beta", 1, 2, 3)], changed.Throughput);
        Assert.Equal(Full, one);
        Assert.Same(Full.WipByColumn, changed.WipByColumn);
        Assert.Equal(Full.CycleTimeHours, changed.CycleTimeHours);
        Assert.Equal(Full.LeadTimeHours, changed.LeadTimeHours);
        Assert.Same(Full.PerWeek, changed.PerWeek);
        Assert.Equal(Full.Rework, changed.Rework);
        Assert.Same(Full.TimePerColumnHours, changed.TimePerColumnHours);
        Assert.Equal(Full.MergeToProdHours, changed.MergeToProdHours);
        Assert.True(changed.HasAny);
    }

    // --------------------------------------------------------------------- deconstruction and the line

    /// <summary>The record deconstructs into its nine parts in the order they are declared.</summary>
    [Fact]
    public void TheStatsDeconstructInTheOrderItIsDeclared()
    {
        var (wip, cycle, lead, perWeek, rework, cost, timePerColumn, throughput, mergeToProd) = Full;

        Assert.Same(Wip, wip);
        Assert.Equal(Cycle, cycle);
        Assert.Equal(Lead, lead);
        Assert.Same(Weeks, perWeek);
        Assert.Equal(Rework, rework);
        Assert.Equal(Cost, cost);
        Assert.Same(ColumnTimes, timePerColumn);
        Assert.Same(Rows, throughput);
        Assert.Equal(MergeToProd, mergeToProd);
    }

    /// <summary>The one line the record says about itself names the type, then its parts in declared order, then
    /// <see cref="WorkItemsStats.HasAny" />; a part that is nothing says nothing.</summary>
    [Fact]
    public void TheRecordSaysWhatItIsInOneLineOfItsOwnParts()
    {
        var text = new WorkItemsStats(new Dictionary<string, int> { ["Todo"] = 1 }, null, null, [], null, null).ToString();

        Assert.StartsWith("WorkItemsStats { ", text, StringComparison.Ordinal);
        Assert.Contains("WipByColumn = ", text, StringComparison.Ordinal);
        Assert.Contains("CycleTimeHours = ,", text, StringComparison.Ordinal);
        Assert.Contains("MergeToProdHours = ,", text, StringComparison.Ordinal);
        Assert.Contains("HasAny = True", text, StringComparison.Ordinal);
        Assert.True(
            text.IndexOf("WipByColumn =", StringComparison.Ordinal) < text.IndexOf("MergeToProdHours =", StringComparison.Ordinal),
            text);
        Assert.True(
            text.IndexOf("MergeToProdHours =", StringComparison.Ordinal) < text.IndexOf("HasAny =", StringComparison.Ordinal),
            text);
    }
}
