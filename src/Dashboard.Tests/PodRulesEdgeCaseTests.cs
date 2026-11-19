namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="PodRules" />, the rules that turn a pod's phase,
/// readiness and age into its state. The readings the page answers are pinned where they belong:
/// <see cref="ClusterAssessmentTests" /> pins a pod of each phase, ready or not, at the seconds of each grace, and
/// <see cref="ClusterStateEdgeCaseTests" /> pins the two graces as the minutes they are, a pod that is not there,
/// a pod that starts after the facts are of and a finished pod that is ready. What is pinned here is the edge each
/// of those readings sits next to: the graces to the tick, the tick on each side of each grace with the point on
/// the grace itself, the phases that never ask for an age on a pod that does not say when it started, a phase the
/// page does not know - even the empty string - that stands before the age, the age counted from the instant the
/// moments stand on rather than from the wall clocks the offsets write, and the two ends of the clock as moments
/// the rules count from.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class PodRulesEdgeCaseTests
{
    /// <summary>The moment the facts are of: the moment the sample's collector wrote, the age every pod below is
    /// counted against.</summary>
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    // ----- The graces, to the tick they are -----

    /// <summary>
    /// The two graces to the tick: the seconds <see cref="ClusterAssessmentTests" /> steps across are counted from
    /// these values, so a grace that moves by a tick is a boundary the page draws in a different place. The
    /// minutes they are stand in <see cref="ClusterStateEdgeCaseTests" />; this is where they stand to the
    /// tick, and the first is two and a half of the second.
    /// </summary>
    [Fact]
    public void TheGracesStandAtTheirTicks()
    {
        Assert.Equal(3_000_000_000, PodRules.PendingGrace.Ticks);
        Assert.Equal(1_200_000_000, PodRules.StartingGrace.Ticks);
        Assert.Equal(TimeSpan.FromSeconds(300), PodRules.PendingGrace);
        Assert.Equal(TimeSpan.FromSeconds(120), PodRules.StartingGrace);
    }

    // ----- The boundary of the pending grace -----

    /// <summary>
    /// A pending pod counts as unhealthy from the tick past the grace, and not the tick before it: the point on
    /// the grace itself is still starting. The seconds on each side (<c>ClusterAssessmentTests</c>: 300 and 301)
    /// leave the ticks between them open; this closes them.
    /// </summary>
    [Fact]
    public void APendingPodTurnsUnhealthyOneTickPastTheGraceAndNotOneTickBefore()
    {
        var oneTickYoung = ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: PodRules.PendingGrace - TimeSpan.FromTicks(1));
        var onTheDot = ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: PodRules.PendingGrace);
        var oneTickOld = ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: PodRules.PendingGrace + TimeSpan.FromTicks(1));

        Assert.Equal(PodState.Starting, PodRules.StateOf(oneTickYoung, Generated));
        Assert.Equal(PodState.Starting, PodRules.StateOf(onTheDot, Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(oneTickOld, Generated));
    }

    // ----- The boundary of the running grace -----

    /// <summary>
    /// A running pod that is not ready counts as unhealthy from the very tick of the grace - younger is the
    /// starting side, the grace itself is not - and so is the tick after it: the comparison is strictly less than,
    /// and the seconds on each side (<c>ClusterAssessmentTests</c>: 119 and 120) leave the ticks between them open.
    /// </summary>
    [Fact]
    public void ARunningPodTurnsUnhealthyAtTheGraceItselfAndStaysThereOneTickAfter()
    {
        var oneTickYoung = ClusterFixture.Pod("ui", ClusterPod.Running, false, age: PodRules.StartingGrace - TimeSpan.FromTicks(1));
        var onTheDot = ClusterFixture.Pod("ui", ClusterPod.Running, false, age: PodRules.StartingGrace);
        var oneTickOld = ClusterFixture.Pod("ui", ClusterPod.Running, false, age: PodRules.StartingGrace + TimeSpan.FromTicks(1));

        Assert.Equal(PodState.Starting, PodRules.StateOf(oneTickYoung, Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(onTheDot, Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(oneTickOld, Generated));
    }

    // ----- A pod that does not say when it started -----

    /// <summary>
    /// The phases the rule decides without an age: a finished pod is finished and a failed one is unhealthy
    /// however young or however unstarted it is, and a running pod that is ready is ready for the same reason.
    /// A start that is not there is no age, and no age is none of these ages - the rule never gets to compare it.
    /// The other two, the pending and the running pod that is not ready, are the ones that do read the age, and
    /// they are pinned in <see cref="ClusterAssessmentTests" />.
    /// </summary>
    [Fact]
    public void APhaseThatDoesNotAskForAnAgeIsDecidedWithoutOne()
    {
        Assert.Equal(PodState.Finished, PodRules.StateOf(ClusterFixture.Pod("job", ClusterPod.Succeeded, started: false), Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(ClusterFixture.Pod("ui", ClusterPod.Failed, started: false), Generated));
        Assert.Equal(PodState.Ready, PodRules.StateOf(ClusterFixture.Pod("ui", ClusterPod.Running, true, started: false), Generated));
        Assert.Equal(PodState.Ready, PodRules.StateOf(ClusterFixture.Pod("ui", ClusterPod.Unknown, true, started: false), Generated));
    }

    // ----- A phase the page does not know -----

    /// <summary>
    /// A phase the page does not know - one of the five words Kubernetes writes is not the only phase a file may
    /// hold - is decided by the readiness alone, and the empty string is a phase the page does not know: ready
    /// when it says so, unhealthy otherwise. Young, a pod in such a phase is not starting the way the pending and
    /// the running pod of the same age are; old, it is unhealthy for the phase; and a start after the moment the
    /// facts are of changes nothing, since the age is never read.
    /// </summary>
    [Fact]
    public void APhaseThePageDoesNotKnowStandsBeforeTheAge()
    {
        var youngReady = ClusterFixture.Pod("ui", "Terminating", true, age: TimeSpan.FromSeconds(30));
        var youngNotReady = ClusterFixture.Pod("ui", "Terminating", false, age: TimeSpan.FromSeconds(30));

        Assert.Equal(PodState.Ready, PodRules.StateOf(youngReady, Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(youngNotReady, Generated));

        var youngReadyAlone = ClusterFixture.Pod("ui", string.Empty, true, age: TimeSpan.FromSeconds(30));
        var youngNotReadyAlone = ClusterFixture.Pod("ui", string.Empty, false, age: TimeSpan.FromSeconds(30));

        Assert.Equal(PodState.Ready, PodRules.StateOf(youngReadyAlone, Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(youngNotReadyAlone, Generated));

        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(ClusterFixture.Pod("ui", "Terminating", false, age: TimeSpan.FromHours(2)), Generated));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(ClusterFixture.Pod("ui", "Terminating", false, age: TimeSpan.FromHours(-2)), Generated));
    }

    // ----- The moment the age is counted from -----

    /// <summary>
    /// The age of a pod is the time between two instants, not between the wall clocks the offsets write: the pod's
    /// start written two hours ahead of the moment the facts are of stands on the instant two minutes before it -
    /// on the running grace itself, unhealthy, where a subtraction of the wall clocks would have found the pod
    /// almost two hours young and still starting - and a start written twelve hours behind it stands on the instant
    /// six minutes before, over the pending grace, where the wall clocks say the pod starts almost two hours from
    /// now.
    /// </summary>
    [Fact]
    public void TheAgeIsCountedFromTheInstantNotFromTheWallClocks()
    {
        var pod = ClusterFixture.Pod("ui", ClusterPod.Running, false, age: TimeSpan.FromMinutes(2));
        var onTheDot = pod with { StartedAt = pod.StartedAt!.Value.ToOffset(TimeSpan.FromHours(2)) };

        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(onTheDot, Generated));

        var pending = ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: TimeSpan.FromMinutes(6));
        var overTheGrace = pending with { StartedAt = pending.StartedAt!.Value.ToOffset(TimeSpan.FromHours(-12)) };
        var asOf = Generated.ToOffset(TimeSpan.FromHours(10));

        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(overTheGrace, asOf));
    }

    /// <summary>
    /// The two ends of the clock are moments the rules count from: a pod that does not say when it started has no
    /// age at either end, so a pending one is still only starting at both; a pod that started at the very
    /// beginning of the clock and whose facts are of the very end of it is ten thousand years old, over both
    /// graces; and readiness still stands before the age at the end of the clock as it does anywhere else.
    /// </summary>
    [Fact]
    public void TheEndsOfTheClockAreMomentsTheRulesCountFrom()
    {
        var neverStarted = ClusterFixture.Pod("ui", ClusterPod.Pending, false, started: false);

        Assert.Equal(PodState.Starting, PodRules.StateOf(neverStarted, DateTimeOffset.MinValue));
        Assert.Equal(PodState.Starting, PodRules.StateOf(neverStarted, DateTimeOffset.MaxValue));

        var ancientPending = ClusterFixture.Pod("ui", ClusterPod.Pending, false) with { StartedAt = DateTimeOffset.MinValue };
        var ancientRunning = ClusterFixture.Pod("ui", ClusterPod.Running, false) with { StartedAt = DateTimeOffset.MinValue };
        var ancientReady = ClusterFixture.Pod("ui", ClusterPod.Running, true) with { StartedAt = DateTimeOffset.MinValue };

        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(ancientPending, DateTimeOffset.MaxValue));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(ancientRunning, DateTimeOffset.MaxValue));
        Assert.Equal(PodState.Ready, PodRules.StateOf(ancientReady, DateTimeOffset.MaxValue));
    }
}
