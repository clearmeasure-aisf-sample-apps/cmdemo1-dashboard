using static Dashboard.Tests.ClusterFixture;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the cluster's states - the types of
/// <c>ClusterAssessment.cs</c>: the six <see cref="ClusterState" />s, the four <see cref="PodState" />s, the five
/// <see cref="ClusterLiveness" />es, the rules that turn a pod into its state (<see cref="PodRules" />) and the
/// <see cref="ClusterAssessment" /> record the page says from them. The behaviour the page shows is pinned where it
/// belongs - <see cref="ClusterAssessmentTests" /> for the readings the page answers,
/// <see cref="ClusterDeliveryHelperTests" /> for the words a problem with a pod and Azure's facts stand in, and
/// <see cref="ClusterHelperCoverageTests" /> for the order of a problem list - and what they leave open is pinned
/// here: the states and the two graces as the numbers they are, a pod the page does not get, a pod that starts
/// after the facts are of and a finished pod that is ready, a reading that is not there, a read that read nothing,
/// a detail that stands as it stands or does not, the list that is empty and the ones of two and of five, a pod
/// that is in no space, a pod in a phase the page does not know among the problems, a status that lists without
/// naming, the collector's moment in the zone the viewer stands in, a stopped service that says nothing without a
/// moment, a stopped service next to a cluster that answers, a starting cluster of four, and a state word that is
/// the empty string.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings, and the
/// one zone that is not UTC is a fixed offset the test builds, not a zone the machine knows.
/// </para>
/// </summary>
public class ClusterStateEdgeCaseTests
{
    // ----- The states, as the numbers they are -----

    /// <summary>The six states of the cluster view, in the order the enum declares them: the order a cast to a
    /// number keeps, and the count the order runs to.</summary>
    [Fact]
    public void TheClusterStateIsOneOfSixInTheOrderItIsDeclared()
    {
        Assert.Equal(0, (int)ClusterState.Checking);
        Assert.Equal(1, (int)ClusterState.Healthy);
        Assert.Equal(2, (int)ClusterState.Warning);
        Assert.Equal(3, (int)ClusterState.Unhealthy);
        Assert.Equal(4, (int)ClusterState.Unreachable);
        Assert.Equal(5, (int)ClusterState.Neutral);
        Assert.Equal(6, Enum.GetValues<ClusterState>().Length);
    }

    /// <summary>The four states of one pod, in the order the enum declares them.</summary>
    [Fact]
    public void ThePodStateIsOneOfFourInTheOrderItIsDeclared()
    {
        Assert.Equal(0, (int)PodState.Ready);
        Assert.Equal(1, (int)PodState.Starting);
        Assert.Equal(2, (int)PodState.Unhealthy);
        Assert.Equal(3, (int)PodState.Finished);
        Assert.Equal(4, Enum.GetValues<PodState>().Length);
    }

    /// <summary>The five livensses of the cluster's own status, in the order the enum declares them.</summary>
    [Fact]
    public void TheClusterLivenessIsOneOfFiveInTheOrderItIsDeclared()
    {
        Assert.Equal(0, (int)ClusterLiveness.Pending);
        Assert.Equal(1, (int)ClusterLiveness.Live);
        Assert.Equal(2, (int)ClusterLiveness.Stale);
        Assert.Equal(3, (int)ClusterLiveness.Silent);
        Assert.Equal(4, (int)ClusterLiveness.Unreadable);
        Assert.Equal(5, Enum.GetValues<ClusterLiveness>().Length);
    }

    // ----- PodRules -----

    /// <summary>The two graces as the values they are - five minutes for a pending pod and two for a running one -
    /// the numbers the boundaries the tests step across are counted from.</summary>
    [Fact]
    public void TheTwoGracesAreFiveMinutesAndTwo()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), PodRules.PendingGrace);
        Assert.Equal(TimeSpan.FromMinutes(2), PodRules.StartingGrace);
        Assert.True(PodRules.PendingGrace > PodRules.StartingGrace);
    }

    /// <summary>A state of no pod is no state: the rule says so before it says anything else.</summary>
    [Fact]
    public void APodThatIsNotThereHasNoState()
    {
        var error = Assert.Throws<ArgumentNullException>(() => PodRules.StateOf(null!, Generated));

        Assert.Equal("pod", error.ParamName);
    }

    /// <summary>A pod whose start stands after the moment the facts are of is younger than no age: two clocks that
    /// disagree make the pod starting, not unhealthy - a running one that is not ready and a pending one alike, and
    /// so is one that starts on the very moment the facts are of.</summary>
    [Fact]
    public void APodThatStartsAfterTheFactsAreOfCountsAsYoung()
    {
        var ahead = ClusterFixture.Pod("ui", ClusterPod.Running, ready: false, age: TimeSpan.FromMinutes(-5));
        var onTheDot = ClusterFixture.Pod("ui", ClusterPod.Running, ready: false, age: TimeSpan.Zero);
        var waiting = ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: TimeSpan.FromMinutes(-5));

        Assert.Equal(PodState.Starting, PodRules.StateOf(ahead, Generated));
        Assert.Equal(PodState.Starting, PodRules.StateOf(onTheDot, Generated));
        Assert.Equal(PodState.Starting, PodRules.StateOf(waiting, Generated));
    }

    /// <summary>The phase the pod says it is in stands before the readiness it says it has: a pod that finished is
    /// finished however ready it is, the readiness the page would call a problem never read.</summary>
    [Fact]
    public void APodThatFinishedIsFinishedHoweverReadyItIs() =>
        Assert.Equal(PodState.Finished, PodRules.StateOf(ClusterFixture.Pod("job", ClusterPod.Succeeded, ready: true), Generated));

    // ----- The record -----

    /// <summary>How many problems a headline names before it counts the rest: three, to the number.</summary>
    [Fact]
    public void AHeadlineNamesThreeProblemsBeforeItCountsTheRest() =>
        Assert.Equal(3, ClusterAssessment.NamedProblems);

    /// <summary>The assessment keeps its four parts as it was given them, each of them the value it stood on: the
    /// full one, the one whose detail is the empty string or spaces alone - kept, not read as absent - and the one
    /// with no detail named, which stands null.</summary>
    [Fact]
    public void TheAssessmentKeepsItsFourPartsAsGiven()
    {
        var withDetail = new ClusterAssessment(ClusterState.Warning, "Degraded", "Azure reports the AKS service degraded", "The API server is slow.");

        Assert.Equal(ClusterState.Warning, withDetail.State);
        Assert.Equal("Degraded", withDetail.Label);
        Assert.Equal("Azure reports the AKS service degraded", withDetail.Headline);
        Assert.Equal("The API server is slow.", withDetail.Detail);

        Assert.Equal(string.Empty, new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every pod is ready", string.Empty).Detail);
        Assert.Equal("   ", new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every pod is ready", "   ").Detail);
        Assert.Null(new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every pod is ready").Detail);
        Assert.Null(new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every pod is ready", null).Detail);
    }

    /// <summary>Two assessments are one while all four parts are - the equality of the operators and of
    /// <c>Equals</c>, and the hash code that follows them - and a difference in any one part, the detail made
    /// shorter or made absent included, is another one.</summary>
    [Fact]
    public void TwoAssessmentsAreOneWhenAllFourPartsAre()
    {
        var one = new ClusterAssessment(ClusterState.Unhealthy, "Unhealthy", "ui in apps: not ready", "Restart it.");
        var two = new ClusterAssessment(ClusterState.Unhealthy, "Unhealthy", "ui in apps: not ready", "Restart it.");

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one == one with { Detail = "Start it." });

        Assert.NotEqual(one, one with { State = ClusterState.Warning });
        Assert.NotEqual(one, one with { Label = "Degraded" });
        Assert.NotEqual(one, one with { Headline = "api in apps: not ready" });
        Assert.NotEqual(one, one with { Detail = "Start it." });
        Assert.NotEqual(one, one with { Detail = null });
        Assert.Equal(one, one with { State = one.State, Label = one.Label, Headline = one.Headline, Detail = one.Detail });
    }

    /// <summary><c>with</c> names the parts it changes and leaves the rest - a copy is equal and not the same - and
    /// the record says its parts in the order they are declared, the name of each with the value it stands on: the
    /// detail the assessment does not have says its name and nothing else.</summary>
    [Fact]
    public void WithChangesOnlyThePartsItNamesAndTheRecordSaysItsPartsInTheirOrder()
    {
        var verdict = new ClusterAssessment(ClusterState.Warning, "Degraded", "Azure reports the AKS service degraded");
        var copy = verdict with { };

        Assert.Equal(verdict, copy);
        Assert.NotSame(verdict, copy);
        Assert.Null(copy.Detail);

        var changed = verdict with { State = ClusterState.Unhealthy, Detail = "The API server does not answer." };

        Assert.Equal(
            new ClusterAssessment(ClusterState.Unhealthy, "Degraded", "Azure reports the AKS service degraded", "The API server does not answer."),
            changed);
        Assert.Equal("Degraded", changed.Label);
        Assert.Equal("Azure reports the AKS service degraded", changed.Headline);

        Assert.Equal(
            "ClusterAssessment { State = Warning, Label = Degraded, Headline = Azure reports the AKS service degraded, Detail =  }",
            verdict.ToString());
        Assert.Equal(
            "ClusterAssessment { State = Unhealthy, Label = Unhealthy, Headline = ui in apps: not ready, Detail = Restart it. }",
            new ClusterAssessment(ClusterState.Unhealthy, "Unhealthy", "ui in apps: not ready", "Restart it.").ToString());
    }

    // ----- ClusterAssessment.LivenessOf -----

    /// <summary>A liveness of no reading is no liveness: the rule says so before it says anything else.</summary>
    [Fact]
    public void AReadingThatIsNotThereHasNoLiveness()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.LivenessOf(null!, Now));

        Assert.Equal("reading", error.ParamName);
    }

    /// <summary>A read that says it read nothing is not a live one and not a stale one: the value the liveness is
    /// read off is not there, so the liveness the page sees is the one of an address that does not answer.</summary>
    [Fact]
    public void AReadThatReadNothingIsSilent() =>
        Assert.Equal(ClusterLiveness.Silent, ClusterAssessment.LivenessOf(new SourceReading<ClusterStatus>(SourceState.Read), Now));

    // ----- ClusterAssessment.Live -----

    /// <summary>A live assessment of no reading is no assessment: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AReadingThatIsNotThereHasNoLiveAssessment()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Live(null!, null, Now, Utc));

        Assert.Equal("reading", error.ParamName);
    }

    /// <summary>The collector's last writing is said in the zone the viewer stands in: the same file at the same
    /// moment, two hours ahead of it, is two hours ahead in the headline, and the age knows no zone.</summary>
    [Fact]
    public void TheCollectorLastWroteIsSaidInTheZoneTheViewerStandsIn()
    {
        var zone = TimeZoneInfo.CreateCustomTimeZone("Two hours ahead", TimeSpan.FromHours(2), "Ahead", "Ahead Standard");
        var reading = ClusterFixture.Read(ClusterFixture.SampleStatus);
        var now = Generated.AddMinutes(12).AddSeconds(4);

        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Unreachable,
                "Stale",
                "The collector in the cluster last wrote 22:15:30 (12 min ago)",
                "What is shown below is as of then."),
            ClusterAssessment.Live(reading, null, now, zone));
    }

    /// <summary>A stopped service says nothing to a cluster that answers: the power state Azure reports is said
    /// where the file does not answer, and while it does the cluster is judged on its own - ready and under no
    /// pressure, it is healthy next to a stopped service as well.</summary>
    [Fact]
    public void AStoppedServiceNextToAClusterThatAnswersSaysNothingAboutThePower()
    {
        var status = ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui")));
        var stopped = ClusterFixture.SampleService with { PowerState = "Stopped" };

        Assert.Equal(
            new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every node is ready and under no pressure, and every pod is ready"),
            ClusterAssessment.Live(ClusterFixture.Read(status), ClusterFixture.Read(stopped), Now, Utc));
    }

    /// <summary>A stopped service that does not say when Azure read it says stopped without the moment: the
    /// sentence under the headline is the one about the pods, and the as of that would come after it is not said.</summary>
    [Fact]
    public void AStoppedServiceWithoutAMomentSaysStoppedWithoutTheAsOf()
    {
        var stopped = ClusterFixture.SampleService with { PowerState = "Stopped", Generated = null };
        var silent = Silent();

        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Neutral,
                "Stopped",
                "The cluster is stopped",
                "The pods and the pages served from inside the cluster do not run while it is stopped."),
            ClusterAssessment.Live(silent, ClusterFixture.Read(stopped), Now, Utc));
    }

    /// <summary>A status file that is not the file, said without a word for why: the detail the reading does not
    /// carry is not said, and the assessment stands with no detail at all.</summary>
    [Fact]
    public void AFileThatIsNotTheFileWithoutAWordForWhyHasNoDetail()
    {
        var malformed = new SourceReading<ClusterStatus>(SourceState.Malformed);

        Assert.Equal(
            new ClusterAssessment(ClusterState.Unhealthy, "Unreadable", "The cluster's status file could not be read"),
            ClusterAssessment.Live(malformed, null, Now, Utc));
    }

    /// <summary>A starting cluster of four names three and counts the rest: the same words the headline of the
    /// problems stands in, said for the pods that are only starting.</summary>
    [Fact]
    public void FourPodsThatAreOnlyStartingAreNamedThreeAndCountedOne()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space(
                "apps",
                ClusterFixture.Pod("one", ready: false, age: TimeSpan.FromSeconds(30)),
                ClusterFixture.Pod("two", ready: false, age: TimeSpan.FromSeconds(30)),
                ClusterFixture.Pod("three", ready: false, age: TimeSpan.FromSeconds(30)),
                ClusterFixture.Pod("four", ready: false, age: TimeSpan.FromSeconds(30))));

        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Checking,
                "Starting",
                "4 pods are starting: one in apps; two in apps; three in apps; and 1 more"),
            ClusterAssessment.Live(ClusterFixture.Read(status), null, Now, Utc));
    }

    // ----- ClusterAssessment.Service -----

    /// <summary>A service assessment of no reading is no assessment: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AServiceReadingThatIsNotThereHasNoServiceAssessment()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Service(null!));

        Assert.Equal("reading", error.ParamName);
    }

    // ----- ClusterAssessment.Problems -----

    /// <summary>The problems of no status are no problems: the rule says so before it says anything else.</summary>
    [Fact]
    public void AStatusThatIsNotThereHasNoProblems()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Problems(null!, Generated));

        Assert.Equal("status", error.ParamName);
    }

    /// <summary>A status that lists without naming - no node, no namespace, nothing - is one problem and no more:
    /// the absence of the nodes is the only thing to say.</summary>
    [Fact]
    public void AStatusThatListsWithoutNamingIsOneProblemAlone()
    {
        var status = ClusterFixture.Status([], []);

        Assert.Equal(["The status file lists no node"], ClusterAssessment.Problems(status, Generated));
    }

    /// <summary>What is not a problem: the node that is ready under no pressure, and the pods that are ready, that
    /// are only starting - running without being ready but young, pending without saying when they started - and
    /// that finished. The list of the status stands empty.</summary>
    [Fact]
    public void PodsThatOnlyStartOrEndAreNoProblems()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space(
                "apps",
                ClusterFixture.Pod("ui", ready: false, age: TimeSpan.FromSeconds(40)),
                ClusterFixture.Pod("db", ClusterPod.Pending, false, started: false),
                ClusterFixture.Pod("job", ClusterPod.Succeeded, false, reason: "Completed"),
                ClusterFixture.Pod("ok")));

        Assert.Empty(ClusterAssessment.Problems(status, Generated));
    }

    /// <summary>A pod in a phase the page does not know, unhealthy in it, stands after the pods the page does
    /// know: the running one that is not ready, then the one pending for too long, then the one whose phase is no
    /// phase the page reads.</summary>
    [Fact]
    public void APodInAPhaseThePageDoesNotKnowComesLastAmongThePods()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space(
                "apps",
                ClusterFixture.Pod("lost", ClusterPod.Unknown, false),
                ClusterFixture.Pod("slow", ClusterPod.Pending, false, age: TimeSpan.FromMinutes(12)),
                ClusterFixture.Pod("api", ready: false, restarts: 2, reason: "CrashLoopBackOff")));

        Assert.Equal(
            [
                "api in apps: CrashLoopBackOff, 2 restarts",
                "slow in apps: Pending for 12 min",
                "lost in apps: Unknown",
            ],
            ClusterAssessment.Problems(status, Generated));
    }

    // ----- ClusterAssessment.PodProblem -----

    /// <summary>A pod in no space is said to be in none: the name of the space that is not there stands as the
    /// nothing it is, and the words of the problem stand where they stand.</summary>
    [Fact]
    public void APodThatIsInNoSpaceIsSaidToBeInNone()
    {
        Assert.Equal("ui in : not ready", ClusterAssessment.PodProblem(ClusterFixture.Pod("ui", ready: false), null!, Generated));
    }

    // ----- ClusterAssessment.Named -----

    /// <summary>A list of names that is not there names nothing: the rule says so before it says anything
    /// else.</summary>
    [Fact]
    public void AListThatIsNotThereNamesNothing()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Named(null!));

        Assert.Equal("items", error.ParamName);
    }

    /// <summary>The list of nothing names nothing - the empty string, not a word for it - and the list of two
    /// names both without a count: the counting starts where the third is not enough.</summary>
    [Fact]
    public void AnEmptyListNamesNothingAndTwoAreNamedWithoutACount()
    {
        Assert.Equal(string.Empty, ClusterAssessment.Named([]));
        Assert.Equal("a; b", ClusterAssessment.Named(["a", "b"]));
    }

    /// <summary>Five problems name three and count the other two: the count is what is not named, not what is
    /// named.</summary>
    [Fact]
    public void FiveProblemsNameThreeAndCountTheOtherTwo() =>
        Assert.Equal("a; b; c; and 2 more", ClusterAssessment.Named(["a", "b", "c", "d", "e"]));

    // ----- ClusterAssessment.AzureReports -----

    /// <summary>A state word that is the empty string is still a state word: it is a string, and the words Azure
    /// reports are the words the facts stand on - the empty one read as what it is, not as the absence of a
    /// state.</summary>
    [Fact]
    public void AStateWordThatIsTheEmptyStringIsStillAStateWord()
    {
        var service = ClusterFixture.SampleService with { Availability = null, PowerState = string.Empty };

        Assert.Equal(
            "Azure reports the AKS service , as of 20:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(ClusterFixture.Read(service), Generated, Utc));
    }
}
