using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The <see cref="HealthStates" /> object mother in <c>TestSupport.cs</c>, pinned where it lives: the four states
/// by the word the health tests say them, the heap of answers built from the answers given, the heap of answers
/// that all say the one state, the summary the three counts make, and the summary the states make, counted
/// through <see cref="HealthSummary.Of" />. The mother answers the questions the health tests keep asking in the
/// words the private copies of the ten files that still carry their own aliases and builders answer them -
/// <see cref="EachFilesWithPrivateCopiesSaysWhatTheMotherSays" /> and
/// <see cref="TheFilesPrivateSummaryAndStatesHelpersSayWhatTheMothersSay" /> hold each copy to the shared
/// answer - so a copy may be retired file by file without changing a test's expectation, the way
/// <see cref="EdgeCaseSupportTests" /> holds the <c>Own</c> copies. The sentinels a file casts for an absent
/// state - the <c>Unknown</c> of the serving tests and the <c>Unnamed</c> of the version tests - are the
/// file's own words and are not held here.
/// </summary>
public class HealthStatesObjectMotherTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Static;

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

    // ------------------------------- the private copies, held to the shared answers

    /// <summary>The ten files that still carry their own four aliases of the node's states, in the words they
    /// say them.</summary>
    public static TheoryData<Type> FilesCopied =>
    [
        typeof(FrontDoorAgreementEdgeCaseTests),
        typeof(HealthSummaryEdgeCaseTests),
        typeof(NodeHealthEdgeCaseTests),
        typeof(RotationTests),
        typeof(ServingAssessmentEdgeCaseTests),
        typeof(ServingAssessmentTests),
        typeof(ServingStateBoundaryEdgeCaseTests),
        typeof(ServingStateDecisionEdgeCaseTests),
        typeof(VersionAssessmentEdgeCaseTests),
        typeof(VersionAssessmentTests),
    ];

    /// <summary>Every file that still carries its four private aliases says what the mother says - the
    /// <c>Healthy</c> of every copy stands on <see cref="HealthState.Healthy" />, and so for the other three -
    /// so an alias may be retired file by file without changing a test's expectation.</summary>
    [Theory]
    [MemberData(nameof(FilesCopied))]
    public void EachFilesWithPrivateCopiesSaysWhatTheMotherSays(Type tests)
    {
        Assert.Equal(HealthStates.Healthy, (HealthState)tests.GetField("Healthy", Private)!
            .GetRawConstantValue()!);
        Assert.Equal(HealthStates.Unhealthy, (HealthState)tests.GetField("Unhealthy", Private)!
            .GetRawConstantValue()!);
        Assert.Equal(HealthStates.Pending, (HealthState)tests.GetField("Pending", Private)!
            .GetRawConstantValue()!);
        Assert.Equal(HealthStates.Unreachable, (HealthState)tests.GetField("Unreachable", Private)!
            .GetRawConstantValue()!);
    }

    /// <summary>The one file that still carries the private builders - the <c>Summary</c> of the three counts
    /// and the two <c>States</c> heaps, <see cref="HealthSummaryEdgeCaseTests" /> - says what the mother's
    /// builders say: the same record the same counts stand on, and the same heap the same answers make, whatever
    /// the copy does on its way there. The other nine files carry only the aliases, which
    /// <see cref="EachFilesWithPrivateCopiesSaysWhatTheMotherSays" /> holds.</summary>
    [Fact]
    public void TheFilesPrivateSummaryAndStatesHelpersSayWhatTheMothersSay()
    {
        var copy = typeof(HealthSummaryEdgeCaseTests);

        var summary = copy.GetMethod("Summary", Private, new[] { typeof(int), typeof(int), typeof(int) })!;
        foreach (var (total, healthy, pending) in new[] { (0, 0, 0), (1, 1, 0), (7, 5, 2), (11, 4, 3) })
        {
            Assert.Equal(HealthStates.Summary(total, healthy, pending), Assert.IsType<HealthSummary>(summary.Invoke(null, [total, healthy, pending])));
        }

        var states = copy.GetMethod("States", Private, new[] { typeof(HealthState[]) })!;
        Assert.Equal(HealthStates.States(HealthStates.Healthy, HealthStates.Unreachable),
            Assert.IsType<HealthState[]>(states.Invoke(null, new object?[] { new HealthState[] { HealthState.Healthy, HealthState.Unreachable } })));

        var repeat = copy.GetMethod("States", Private, new[] { typeof(HealthState), typeof(int) })!;
        foreach (var count in new[] { 0, 1, 3, 11 })
        {
            Assert.Equal(HealthStates.States(HealthStates.Pending, count),
                Assert.IsType<HealthState[]>(repeat.Invoke(null, [HealthState.Pending, count])));
        }
    }
}
