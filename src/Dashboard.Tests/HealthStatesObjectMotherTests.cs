namespace Dashboard.Tests;

/// <summary>
/// The <see cref="HealthStates" /> object mother in <c>TestSupport.cs</c>, pinned where it lives: the four states
/// by the word the health tests say them, the heap of answers built from the answers given, the heap of answers
/// that all say the one state, the summary the three counts make, and the summary the states make, counted
/// through <see cref="HealthSummary.Of" />. The mother is the one place a <see cref="HealthState" /> is named in
/// the tests: the ten health and version files that used to carry their own four aliases and, in
/// <see cref="HealthSummaryEdgeCaseTests" />, their own <c>Summary</c> and <c>States</c> builders, now name the
/// states through <see cref="HealthStates" /> and the summary and heap builders through it too - so a new
/// <c>HealthState</c>, or a moved default, is written here once and not in every file. The node-version fixtures
/// that stand on the states - the regional builders, the one-past sentinel and the pinned reading - are held by
/// <see cref="Nodes" /> in <c>Fixtures/Nodes.cs</c>, which <see cref="NodesObjectMotherTests" /> pins. The
/// sentinels a file still casts for an absent state - the <c>Unknown</c> of the serving tests - are the file's own
/// words and are not held here; the <c>Unnamed</c> of the version tests is the number one past the four states and
/// is held by <see cref="Nodes" />.
/// </summary>
public class HealthStatesObjectMotherTests
{
    // ------------------------------- the four states, held once

    /// <summary>The mother's four aliases are the four states the enum says, each one the very member of the
    /// enum and not another that spells the same: a test that names <c>HealthStates.Pending</c> stands on
    /// <see cref="HealthState.Pending" />, whatever else an alias might drift to.</summary>
    [Fact]
    public void TheFourAliasesAreTheFourStatesTheEnumSays()
    {
        Assert.Equal(HealthState.Healthy, HealthStates.Healthy);
        Assert.Equal(HealthState.Unhealthy, HealthStates.Unhealthy);
        Assert.Equal(HealthState.Pending, HealthStates.Pending);
        Assert.Equal(HealthState.Unreachable, HealthStates.Unreachable);
    }

    // ------------------------------- the heaps of answers

    /// <summary>A heap of answers is built from the answers given, in the order given: the order the test
    /// named them is the order the heap stands, and the heap of no answers is no answers.</summary>
    [Fact]
    public void TheHeapStandsInTheOrderTheAnswersWereGiven()
    {
        var answers = HealthStates.States(HealthStates.Healthy, HealthStates.Unreachable, HealthStates.Pending, HealthStates.Unhealthy);

        Assert.Equal([HealthState.Healthy, HealthState.Unreachable, HealthState.Pending, HealthState.Unhealthy], answers);
        Assert.Empty(HealthStates.States());
    }

    /// <summary>A heap of the one state is the <paramref name="count" /> answers that all say it - three pending
    /// answers are the three <see cref="HealthState.Pending" /> and nothing else - and the heap of zero of them
    /// is no answers, so a count of nothing says nothing.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(11)]
    public void TheHeapOfTheOneStateSaysItThatManyTimes(int count)
    {
        Assert.Equal([.. Enumerable.Repeat(HealthState.Pending, count)], HealthStates.States(HealthStates.Pending, count));
        Assert.Equal([.. Enumerable.Repeat(HealthState.Unreachable, count)], HealthStates.States(HealthStates.Unreachable, count));
    }

    // ------------------------------- the summaries

    /// <summary>The summary the three counts make is the very record the counts stand on: the total, the healthy
    /// count, the pending count, in the order the record declares them, and the not healthy counted as the
    /// remainder the header weighs.</summary>
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 0)]
    [InlineData(3, 0, 3)]
    [InlineData(7, 5, 2)]
    [InlineData(11, 4, 3)]
    public void TheSummaryTheCountsMakeIsTheRecordTheCountsStandOn(int total, int healthy, int pending)
    {
        var summary = HealthStates.Summary(total, healthy, pending);

        Assert.Equal(new HealthSummary(total, healthy, pending), summary);
        Assert.Equal(total - healthy - pending, summary.NotHealthy);
    }

    /// <summary>The summary the answers make is counted through <see cref="HealthSummary.Of" />: one total for
    /// each answer, the healthy and the pending counted as they say themselves, and the unreachable the
    /// remainder that is not healthy - three pending answers make a pending summary of three, and a heap that
    /// says everything the enum says makes the counts the header weighs.</summary>
    [Fact]
    public void TheSummaryTheAnswersMakeIsCountedThroughOf()
    {
        var pending = HealthStates.Summary(HealthStates.Pending, HealthStates.Pending, HealthStates.Pending);

        Assert.Equal(new HealthSummary(3, 0, 3), pending);
        Assert.Equal(SummaryLevel.Pending, pending.Level);

        var everything = HealthStates.Summary(HealthStates.Healthy, HealthStates.Unhealthy, HealthStates.Pending, HealthStates.Unreachable, HealthStates.Healthy);

        Assert.Equal(HealthSummary.Of([HealthState.Healthy, HealthState.Unhealthy, HealthState.Pending, HealthState.Unreachable, HealthState.Healthy]), everything);
        Assert.Equal((5, 2, 1), (everything.Total, everything.Healthy, everything.Pending));
        Assert.Equal(2, everything.NotHealthy);
        Assert.Equal(SummaryLevel.Problems, everything.Level);
    }
}
