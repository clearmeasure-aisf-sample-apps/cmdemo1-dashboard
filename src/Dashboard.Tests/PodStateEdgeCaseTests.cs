namespace Dashboard.Tests;

/// <summary>
/// <see cref="PodState" /> is the page's whole vocabulary about one pod - four states, no more and no less - and
/// <see cref="PodRules.StateOf" /> is the one rule that lands a pod on one of them, reading a phase, a readiness and
/// an age counted to a moment the caller hands it. The rule's table is pinned where it belongs:
/// <see cref="ClusterAssessmentTests" /> says what a pod of each phase does when it is ready and when it is not, at
/// 5 s, 30 s, 119 s, 120 s, 300 s, 301 s and an hour, and what becomes of a pod that does not say when it started;
/// <see cref="ClusterGroupsTests" /> and <see cref="NamespaceGroupTests" /> pin what the four states are worth in a
/// namespace's counts and in its order, and <see cref="ClusterHelperCoverageTests" /> the words they are said in.
/// <para>
/// What follows is the rest of it, which none of that holds. First the states as values of their own: the number each
/// stands on and the order they are declared in, the first of them being what a pod state that was never set is, and
/// nothing outside the four being no state however it is arrived at - cast from a number, read out of text, or made by
/// adding two states together, which this enum neither offers nor forbids. Then the two graces as the lengths they
/// are rather than as the ages a theory happens to try, and the rule stepped across each of them a tick at a time: a
/// running pod that is not ready turns on the very tick its grace ends, a pending one on the tick after it. Then the
/// edges the table walks past - the phase that decides before the readiness and the readiness before the age, so that
/// a pending pod which says its containers are ready still runs out its grace; the empty phase, the phase of spaces
/// and the phase in a case the file does not write; the name, the workload, the reason and the restart count that are
/// no part of a state at all, empty or full; a pod that started at the bottom of the clock; an age that came out under
/// zero because the reading was counted from a moment before the pod's own start; and the same pod, saying nothing
/// about its start, at either end of <see cref="DateTimeOffset" />. Last the one argument the rule refuses by name: no
/// pod.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="PodRules.StateOf" /> takes
/// its moment as an argument, so every age below is one the test brought and each boundary can be stood on to the
/// tick. The states themselves decide nothing, which is why they are pinned as the numbers and the words they are, and
/// the only member of the type besides the two graces is that one method.
/// </para>
/// </summary>
public class PodStateEdgeCaseTests
{
    /// <summary>The moment the pods below are read at: every age is counted back from it.</summary>
    private static readonly DateTimeOffset AsOf = new(2026, 10, 6, 20, 15, 30, TimeSpan.Zero);

    /// <summary>A pod of a phase and a readiness, started so long before <see cref="AsOf" /> - or, for
    /// <see langword="null" />, saying nothing about when it started: the readiness and the start are the
    /// pod-level steps the pod carries rather than the flags a factory used to gate.</summary>
    private static ClusterPod Aged(string phase, bool ready, TimeSpan? age)
    {
        var pod = ClusterFixture.Pod("ui", phase);
        pod = ready ? pod.Ready() : pod.NotReady();
        return pod.StartedAt(age is { } span ? AsOf - span : null);
    }

    // ------------------------------------------------------------------------ the four states

    /// <summary>The four words the page has for a pod, in the order they stand in the enum, on the four numbers they
    /// stand in: nothing else is a state, and a fifth would be a state no badge, no count and no sentence of the page
    /// knows. The order is the order of the declaration, which is also the order of the underlying numbers.</summary>
    [Fact]
    public void TheFourStatesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Ready", "Starting", "Unhealthy", "Finished"], Enum.GetNames<PodState>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<PodState>().Select(state => (int)state));
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(PodState)));
    }

    /// <summary>Each state on its own number and on its own word, both ways round: the number is the one the name
    /// stands on, the name is the one that number answers to, and the state is its own equal and no other's.</summary>
    [Theory]
    [InlineData(PodState.Ready, 0, "Ready")]
    [InlineData(PodState.Starting, 1, "Starting")]
    [InlineData(PodState.Unhealthy, 2, "Unhealthy")]
    [InlineData(PodState.Finished, 3, "Finished")]
    public void EachStateStandsOnItsOwnNumberAndItsOwnWord(PodState state, int number, string name)
    {
        Assert.Equal(number, (int)state);
        Assert.Equal(state, (PodState)number);
        Assert.Equal(name, state.ToString());
        Assert.Equal(name, Enum.GetName(state));
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>The first state is what a pod state that was never set is - and it is the one that says nothing is
    /// wrong, which is why a parse that failed and left its answer on the default may never be shown as a fact.
    /// Outside the four there is nothing: neither one below the first nor one above the last nor the two ends of
    /// <see cref="int" /> is a defined state, and a value the enum does not name has no name of its own - it says the
    /// number it stands on.</summary>
    [Fact]
    public void AnUnsetStateIsTheFirstOneAndNothingOutsideTheFourIsAState()
    {
        Assert.Equal(PodState.Ready, default);
        Assert.Equal(PodState.Ready, (PodState)0);

        Assert.True(Enum.IsDefined(PodState.Ready));
        Assert.True(Enum.IsDefined(PodState.Finished));
        Assert.False(Enum.IsDefined((PodState)(-1)));
        Assert.False(Enum.IsDefined((PodState)4));
        Assert.False(Enum.IsDefined((PodState)int.MinValue));
        Assert.False(Enum.IsDefined((PodState)int.MaxValue));

        Assert.Null(Enum.GetName((PodState)4));
        Assert.Null(Enum.GetName((PodState)(-1)));
        Assert.Null(Enum.GetName((PodState)int.MinValue));
        Assert.Equal("4", ((PodState)4).ToString());
        Assert.Equal("-2147483648", ((PodState)int.MinValue).ToString());
    }

    /// <summary>The four states apart from one another - each is the state of one pod alone, and each is equal to
    /// itself in the box as out of it, an <see cref="int" /> standing on the same number being no state's equal - and
    /// comparing by the numbers they stand on, which is the order of the declaration and nothing to do with how much
    /// they worry the page (that order is <c>ClusterGroups</c>' own, and puts the unhealthy pod first).</summary>
    [Fact]
    public void TheFourStatesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        PodState[] states = [PodState.Ready, PodState.Starting, PodState.Unhealthy, PodState.Finished];

        Assert.All(states, state => Assert.Single(states, other => other == state));

        Assert.True(PodState.Ready < PodState.Starting);
        Assert.True(PodState.Starting < PodState.Unhealthy);
        Assert.True(PodState.Unhealthy < PodState.Finished);
        Assert.False(PodState.Finished < PodState.Unhealthy);
        Assert.Equal(1, PodState.Finished.CompareTo(PodState.Unhealthy));
        Assert.Equal(-1, PodState.Ready.CompareTo(PodState.Finished));
        Assert.Equal(0, PodState.Unhealthy.CompareTo((object)PodState.Unhealthy));

        Assert.Equal((object)PodState.Unhealthy, (object)PodState.Unhealthy);
        Assert.False(((object)PodState.Unhealthy).Equals(null));
        Assert.False(PodState.Unhealthy.Equals(null));
        Assert.NotEqual((object)PodState.Unhealthy, (object)2);
        Assert.Equal(PodState.Unhealthy.GetHashCode(), ((object)PodState.Unhealthy).GetHashCode());
    }

    /// <summary>The states are a choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so the
    /// operators work on the numbers underneath rather than on the states - and because those numbers are the four
    /// that stand from 0 to 3, every union of two states lands on a state again while the intersection of two of them
    /// is <see cref="PodState.Ready" />, the word that says nothing is wrong. A starting pod unioned with an unhealthy
    /// one is a <c>Finished</c> pod, which is what the page would show were a state ever combined this way; the
    /// numbers are pinned here so that the day somebody reaches for <c>|</c>, the tests say what it costs.</summary>
    [Fact]
    public void TwoStatesCombinedAreAnotherStateRatherThanASet()
    {
        Assert.False(typeof(PodState).IsDefined(typeof(FlagsAttribute), false));

        Assert.Equal(PodState.Starting, PodState.Ready | PodState.Starting);
        Assert.Equal(PodState.Unhealthy, PodState.Unhealthy | PodState.Ready);
        Assert.Equal(PodState.Finished, PodState.Unhealthy | PodState.Starting);
        Assert.Equal(PodState.Finished, PodState.Finished | PodState.Unhealthy);

        Assert.Equal(PodState.Unhealthy, PodState.Unhealthy & PodState.Finished);
        Assert.Equal(PodState.Ready, PodState.Starting & PodState.Unhealthy);
        Assert.Equal(PodState.Ready, PodState.Finished & PodState.Ready);
    }

    // ------------------------------------------------------------------------ a state out of text

    /// <summary>A state read out of text is the state the text names, in the case the enum spells it, or the state the
    /// number it holds stands on. A name is read whole and its surrounding spaces are walked past - which is all the
    /// trimming the page may ever count on - and a number with a cipher in front of it is still the small number it
    /// says.</summary>
    [Theory]
    [InlineData("Ready", PodState.Ready)]
    [InlineData("Starting", PodState.Starting)]
    [InlineData("Unhealthy", PodState.Unhealthy)]
    [InlineData("Finished", PodState.Finished)]
    [InlineData("  Ready  ", PodState.Ready)]
    [InlineData("0", PodState.Ready)]
    [InlineData("3", PodState.Finished)]
    [InlineData("01", PodState.Starting)]
    public void AStateTextIsTheStateItNamesOrTheStateItNumbers(string text, PodState expected)
    {
        Assert.True(Enum.TryParse<PodState>(text, out var state));
        Assert.Equal(expected, state);
        Assert.True(Enum.IsDefined(state));
    }

    /// <summary>Text that is no state is refused - <see langword="null" />, the empty string, a blank of spaces or of
    /// a tab, a name in another case, a name with something behind it, a fraction, a hexadecimal, a word - and a
    /// refused parse leaves its answer standing on the first state rather than on nothing: the <c>false</c> is the
    /// whole answer, and a caller that reads the value anyway is shown a pod as ready as a pod can be.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("ready")]
    [InlineData("FINISHED")]
    [InlineData("Ready?")]
    [InlineData("2.0")]
    [InlineData("0x0")]
    [InlineData("One")]
    public void TextThatIsNoStateIsRefusedAndLeavesTheFirstStateBehind(string? text)
    {
        Assert.False(Enum.TryParse<PodState>(text, out var state));
        Assert.Equal(PodState.Ready, state);
        Assert.False(Enum.TryParse<PodState>(text, ignoreCase: false, out _));
    }

    /// <summary>The one text that is no name and is not refused is a number outside the four: the parse answers truly
    /// and hands back a value that is no state at all, which is why a read number is worth nothing until
    /// <see cref="Enum.IsDefined(Enum)" /> has been asked about it as well. A name in a case of its own, on the other
    /// hand, is a state as soon as the reader is told to forgive the case - and is still nothing until it is.</summary>
    [Fact]
    public void ANumberOutsideTheFourIsReadAsAStateThatIsNoState()
    {
        Assert.True(Enum.TryParse<PodState>("4", out var above));
        Assert.Equal((PodState)4, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<PodState>("-1", out var below));
        Assert.Equal((PodState)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<PodState>("unhealthy", out _));
        Assert.True(Enum.TryParse<PodState>("unhealthy", ignoreCase: true, out var forgiven));
        Assert.Equal(PodState.Unhealthy, forgiven);
    }

    // ------------------------------------------------------------------------ the two graces

    /// <summary>The two lengths a pod is given before its youth stops excusing it, as the numbers they are: five
    /// minutes of pending, two of running without readiness - the pending the longer of the two, since a pod that has
    /// not been placed waits on the cluster rather than on its own containers. Both are held to the tick, so that a
    /// threshold that moves is a decision the tests see rather than a drift.</summary>
    [Fact]
    public void TheTwoGracesAreFixedLengthsWithThePendingOneTheLonger()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), PodRules.PendingGrace);
        Assert.Equal(3_000_000_000L, PodRules.PendingGrace.Ticks);
        Assert.Equal(TimeSpan.FromMinutes(2), PodRules.StartingGrace);
        Assert.Equal(1_200_000_000L, PodRules.StartingGrace.Ticks);

        Assert.True(PodRules.StartingGrace < PodRules.PendingGrace);
        Assert.Equal(PodRules.PendingGrace, PodRules.PendingGrace);
        Assert.Equal(PodRules.StartingGrace, PodRules.StartingGrace);
    }

    /// <summary>A running pod that is not ready stops being starting on the very tick its grace ends: the exact grace
    /// is already too old, and one tick less is not. The table elsewhere stands on whole seconds, which leaves this
    /// tick said by nothing but the code.</summary>
    [Theory]
    [InlineData(-1, PodState.Starting)]
    [InlineData(0, PodState.Unhealthy)]
    [InlineData(1, PodState.Unhealthy)]
    public void ARunningPodNotReadyTurnsOnTheTickItsGraceEnds(int ticks, PodState expected)
    {
        var age = PodRules.StartingGrace + TimeSpan.FromTicks(ticks);

        Assert.Equal(expected, PodRules.StateOf(Aged(ClusterPod.Running, ready: false, age), AsOf));
    }

    /// <summary>A pending pod is starting to the tick its grace ends and on that tick as well - the comparison is a
    /// strict one, so the exact five minutes is still grace - and unhealthy from the tick after it.</summary>
    [Theory]
    [InlineData(-1, PodState.Starting)]
    [InlineData(0, PodState.Starting)]
    [InlineData(1, PodState.Unhealthy)]
    public void APendingPodTurnsOnTheTickAfterItsGrace(int ticks, PodState expected)
    {
        var age = PodRules.PendingGrace + TimeSpan.FromTicks(ticks);

        Assert.Equal(expected, PodRules.StateOf(Aged(ClusterPod.Pending, ready: false, age), AsOf));
    }

    /// <summary>The zero of an age: a pod that started at the very moment it is read is the youngest pod of all to the
    /// rule, starting rather than unhealthy - while a pod of a phase the page does not know is never starting, young
    /// or old, and a ready one is ready at that moment as at every other.</summary>
    [Fact]
    public void APodThatStartedAtTheMomentItIsReadIsYoungAndNeverOld()
    {
        Assert.Equal(PodState.Starting, PodRules.StateOf(Aged(ClusterPod.Running, ready: false, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Starting, PodRules.StateOf(Aged(ClusterPod.Pending, ready: false, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Aged(ClusterPod.Running, ready: true, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Finished, PodRules.StateOf(Aged(ClusterPod.Succeeded, ready: false, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(ClusterPod.Failed, ready: false, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(ClusterPod.Unknown, ready: false, TimeSpan.Zero), AsOf));
    }

    // ------------------------------------------------------------------------ the moment a pod is read at

    /// <summary>An age that comes out under zero - the reading counted from a moment before the pod's own start, which
    /// is the page's clock behind the cluster's - is the youngest age of all to the rule and never the oldest: nothing
    /// under zero reaches a grace, so neither the pending pod nor the running one has run out anything it could be
    /// blamed for.</summary>
    [Theory]
    [InlineData(ClusterPod.Pending)]
    [InlineData(ClusterPod.Running)]
    public void APodThatStartedAfterTheMomentItIsReadIsYoungRatherThanOld(string phase)
    {
        Assert.Equal(PodState.Starting, PodRules.StateOf(Aged(phase, ready: false, -TimeSpan.FromMinutes(10)), AsOf));
        Assert.Equal(PodState.Starting, PodRules.StateOf(Aged(phase, ready: false, -PodRules.PendingGrace), AsOf));
    }

    /// <summary>A pod whose start stands at the bottom of the clock is as old as the rule can be told: pending far
    /// past its grace and running far past its own, ready or finished whatever the arithmetic.</summary>
    [Fact]
    public void APodThatStartedAtTheBottomOfTheClockIsOld()
    {
        var oldest = Aged(ClusterPod.Pending, ready: false, null) with { StartedAt = DateTimeOffset.MinValue };

        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(oldest, AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(oldest with { Phase = ClusterPod.Running }, AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(oldest with { Phase = ClusterPod.Running, Ready = true }, AsOf));
        Assert.Equal(PodState.Finished, PodRules.StateOf(oldest with { Phase = ClusterPod.Succeeded }, AsOf));
    }

    /// <summary>The same pod, told when it started once, read at five moments: young at the one it was written for and
    /// the second before its grace, and old at the ones from it on - the rule counts to the moment it is handed and to
    /// no other, so one pod is a starting pod now and an unhealthy one three minutes on. A moment before the pod's own
    /// start is young again, which is the clock behind rather than the pod.</summary>
    [Fact]
    public void APodsAgeIsCountedToTheMomentItIsGivenAndNotToAnyOther()
    {
        var pod = Aged(ClusterPod.Running, ready: false, TimeSpan.FromSeconds(30));
        var started = pod.StartedAt!.Value;

        Assert.Equal(PodState.Starting, PodRules.StateOf(pod, started));
        Assert.Equal(PodState.Starting, PodRules.StateOf(pod, started + PodRules.StartingGrace - TimeSpan.FromSeconds(1)));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(pod, started + PodRules.StartingGrace));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(pod, started + PodRules.PendingGrace));
        Assert.Equal(PodState.Starting, PodRules.StateOf(pod, started - TimeSpan.FromDays(1)));
    }

    /// <summary>A pod that says nothing about when it started is measured against no age at all, so it is the same at
    /// either end of the clock as it is in the middle of it: the pending pod starting, the running pod that is not
    /// ready unhealthy at once, the ready pod ready, the finished one finished and the failed one failed - and no
    /// subtraction of two moments the test cannot reach is ever attempted.</summary>
    [Fact]
    public void APodThatSaysNothingAboutItsStartIsTheSameAtEitherEndOfTheClock()
    {
        foreach (var at in new[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue })
        {
            var silent = Aged(ClusterPod.Running, ready: false, null);

            Assert.Null(silent.StartedAt);
            Assert.Equal(PodState.Unhealthy, PodRules.StateOf(silent, at));
            Assert.Equal(PodState.Starting, PodRules.StateOf(silent with { Phase = ClusterPod.Pending }, at));
            Assert.Equal(PodState.Ready, PodRules.StateOf(silent with { Ready = true }, at));
            Assert.Equal(PodState.Finished, PodRules.StateOf(silent with { Phase = ClusterPod.Succeeded }, at));
            Assert.Equal(PodState.Unhealthy, PodRules.StateOf(silent with { Phase = ClusterPod.Failed }, at));
            Assert.Equal(PodState.Unhealthy, PodRules.StateOf(silent with { Phase = "" }, at));
        }
    }

    // ------------------------------------------------------------------------ what decides first

    /// <summary>The phase before the readiness and the readiness before the age: a job that finished is finished young
    /// as old, whatever its containers say; a pod that failed is unhealthy even while it says it is ready; a pending
    /// pod runs out its grace whether or not it says ready, for the phase is the pod's own word and the readiness its
    /// containers'; and a running pod that says ready is ready on both graces and past them.</summary>
    [Fact]
    public void ThePhaseDecidesBeforeTheReadinessAndTheReadinessBeforeTheAge()
    {
        var young = TimeSpan.FromSeconds(1);
        var old = TimeSpan.FromMinutes(10);

        Assert.Equal(PodState.Finished, PodRules.StateOf(Aged(ClusterPod.Succeeded, ready: true, young), AsOf));
        Assert.Equal(PodState.Finished, PodRules.StateOf(Aged(ClusterPod.Succeeded, ready: true, old), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(ClusterPod.Failed, ready: true, young), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(ClusterPod.Pending, ready: true, old), AsOf));
        Assert.Equal(PodState.Starting, PodRules.StateOf(Aged(ClusterPod.Pending, ready: true, young), AsOf));
        Assert.Equal(PodState.Starting, PodRules.StateOf(Aged(ClusterPod.Pending, ready: true, PodRules.PendingGrace), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Aged(ClusterPod.Running, ready: true, PodRules.StartingGrace), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Aged(ClusterPod.Running, ready: true, PodRules.PendingGrace), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Aged(ClusterPod.Unknown, ready: true, old), AsOf));
    }

    // ------------------------------------------------------------------------ the words a pod carries

    /// <summary>The phase is measured against the five words of <see cref="ClusterPod" /> and against nothing else: an
    /// empty phase, a phase of spaces, a phase with a space behind it, a phase in a case the file does not write and a
    /// phase that is some other word are all of them phases the page does not know - and an unknown phase is ready when
    /// the pod says so and unhealthy otherwise, its age deciding nothing, so the same phase read with no start at all
    /// comes back the same way. <c>Pending</c> is the phase of the grace; <c>PENDING</c> is not a phase.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("running")]
    [InlineData("Running ")]
    [InlineData("PENDING")]
    [InlineData("Crashed")]
    public void APhaseThePageDoesNotKnowIsSaidByReadinessAlone(string phase)
    {
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(phase, ready: false, TimeSpan.FromMinutes(30)), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(phase, ready: false, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(phase, ready: false, -TimeSpan.FromHours(1)), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Aged(phase, ready: false, null), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Aged(phase, ready: true, TimeSpan.FromMinutes(30)), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Aged(phase, ready: true, null), AsOf));
    }

    /// <summary>What a pod is called, what owns it and what it is waiting on are no part of its state, and neither is
    /// how often it came back: the rule reads a phase, a readiness and a start alone. The empty name, the absent
    /// workload, the reason that is spaces and the reason that is nothing, a restart count at either end of
    /// <see cref="int" /> - all of it is the same pod to the rule, which is what keeps the sentence about a state and
    /// the state itself apart.</summary>
    [Theory]
    [InlineData("", "", "", 0)]
    [InlineData("ui-0", null, null, 0)]
    [InlineData("ui-0", "ui", "   ", 1)]
    [InlineData("ui-0", "ui", "CrashLoopBackOff", int.MaxValue)]
    public void ANameAWorkloadAReasonAndARestartCountAreNoPartOfAState(string name, string? workload, string? reason, int restarts)
    {
        ClusterPod Say(string phase, bool ready, TimeSpan? age) =>
            Aged(phase, ready, age) with { Name = name, Workload = workload, Reason = reason, Restarts = restarts };

        Assert.Equal(PodState.Starting, PodRules.StateOf(Say(ClusterPod.Running, ready: false, TimeSpan.FromSeconds(30)), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Say(ClusterPod.Running, ready: false, TimeSpan.FromMinutes(6)), AsOf));
        Assert.Equal(PodState.Starting, PodRules.StateOf(Say(ClusterPod.Pending, ready: false, TimeSpan.FromMinutes(4)), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Say(ClusterPod.Pending, ready: false, TimeSpan.FromMinutes(6)), AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(Say(ClusterPod.Failed, ready: true, TimeSpan.Zero), AsOf));
        Assert.Equal(PodState.Finished, PodRules.StateOf(Say(ClusterPod.Succeeded, ready: false, null), AsOf));
        Assert.Equal(PodState.Ready, PodRules.StateOf(Say(ClusterPod.Running, ready: true, null), AsOf));
    }

    // ------------------------------------------------------------------------ the pod that is not there

    /// <summary>The rule's one refusal, named: no pod is not a pod that is starting, and the answer says which
    /// argument was wanted. The moment it is read at is never refused - it is a value of the caller's, not a clock of
    /// the rule's own, which is what lets every boundary above be stood on to the tick.</summary>
    [Fact]
    public void APodThatIsNotThereIsRefusedByName()
    {
        var error = Assert.Throws<ArgumentNullException>(() => PodRules.StateOf(null!, AsOf));

        Assert.Equal("pod", error.ParamName);
    }
}
