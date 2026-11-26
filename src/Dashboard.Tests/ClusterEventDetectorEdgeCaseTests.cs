namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the members of <see cref="ClusterEventDetector"/>: the two
/// words the events stand on and the number of pods it names, the liveness the status file and the collector say
/// between two readings (<see cref="ClusterEventDetector.Liveness"/>), what changed in a status
/// (<see cref="ClusterEventDetector.Status"/>) and in Azure's verdict on the AKS service
/// (<see cref="ClusterEventDetector.Service"/>). <see cref="ClusterEventDetectorTests"/> pins each change the
/// detector makes - the file that stops answering and answers again, the collector that stops writing and writes
/// again, the node that is ready again, the pod that restarts, that becomes unhealthy and that is ready again, the
/// fourteen pods of which ten are named and four are counted - and <see cref="ClusterHelperCoverageTests"/> walks
/// a changed node and a restarted pod through the same status, so what they leave open is pinned here, and only
/// that: the words and the number as the values they are; the detail that ends in a full stop or stands in
/// whitespace alone, and the one a good event does not take; the stale that turns silent, the silent that turns
/// unreadable or stale, the unreadable that turns live, stale or unreadable, and the reading that is Pending on
/// one side or is the same on both; the status said without its parts; the two empty statuses; the tenth and the
/// eleventh pod that change; the restart count that falls; the restart said without a reason, for the pod that is
/// ready and for the one that names none; the change said without an environment, the environment without a
/// namespace, and the two environments of one namespace; the service said without its facts; the round in which
/// the verdict and the power state change at once, in the order they are said; the unknown verdict in full; the
/// power state that is neither Running nor Stopped; and the word Azure writes in other letters, which is the same
/// word.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class ClusterEventDetectorEdgeCaseTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;
    private static readonly DateTimeOffset Now = Generated.AddSeconds(5);

    private static readonly EnvironmentInfo[] Environments = [ClusterFixture.Environment("tdd", "cmdemo3-tdd")];

    private static ClusterStatus With(params ClusterPod[] pods) =>
        ClusterFixture.Status(null, ClusterFixture.Space("cmdemo3-tdd", pods));

    private static IReadOnlyList<DashboardEvent> Between(ClusterStatus before, ClusterStatus after, params EnvironmentInfo[] environments) =>
        ClusterEventDetector.Status(before, after, environments is { Length: > 0 } ? environments : Environments, Now);

    private static DashboardEvent Event(EventLevel level, string? environment, string text, string? place = ClusterEventDetector.ClusterPlace) =>
        new(Now, EventKind.Cluster, level, environment, place, text);

    // ----- The words and the number the detector is built with -----

    /// <summary>
    /// The two places the events of the cluster and of the AKS service stand on, and the number of pods a round
    /// names one by one, to their values: the tests above compare events against the words they stand on, and these
    /// are the words as they are.
    /// </summary>
    [Fact]
    public void ThePlacesAndTheNumberOfNamedPodsAreTheWordsAndTheNumberTheyAre()
    {
        Assert.Equal("cluster", ClusterEventDetector.ClusterPlace);
        Assert.Equal("AKS", ClusterEventDetector.ServicePlace);
        Assert.Equal(10, ClusterEventDetector.NamedPods);
    }

    // ----- Liveness -----

    /// <summary>
    /// The words the liveness says sit next to the edges the tests above do not step across: the detail that ends
    /// in a full stop is said without it, the detail that stands in whitespace alone is not said, and the good
    /// events take no detail at all - the file that answers again does not say why it had stopped. The turn the
    /// status takes is the one the second reading stands on: the stale that turns silent is the file that stops
    /// answering, the silent or the stale that turns unreadable is the collector that stops writing, and the
    /// unreadable that turns live is the file that answers again, not the collector that writes again.
    /// </summary>
    [Theory]
    [InlineData(ClusterLiveness.Live, ClusterLiveness.Unreadable, "the file is not valid JSON.", EventLevel.Problem, "The cluster's status file could not be read: the file is not valid JSON")]
    [InlineData(ClusterLiveness.Live, ClusterLiveness.Silent, "   ", EventLevel.Problem, "The cluster's status file stopped answering")]
    [InlineData(ClusterLiveness.Stale, ClusterLiveness.Silent, "no answer within 10 s", EventLevel.Problem, "The cluster's status file stopped answering: no answer within 10 s")]
    [InlineData(ClusterLiveness.Silent, ClusterLiveness.Unreadable, "the file is not valid JSON.", EventLevel.Problem, "The cluster's status file could not be read: the file is not valid JSON")]
    [InlineData(ClusterLiveness.Stale, ClusterLiveness.Unreadable, null, EventLevel.Problem, "The cluster's status file could not be read")]
    [InlineData(ClusterLiveness.Silent, ClusterLiveness.Stale, null, EventLevel.Problem, "The collector in the cluster stopped writing: the status file is stale")]
    [InlineData(ClusterLiveness.Unreadable, ClusterLiveness.Stale, null, EventLevel.Problem, "The collector in the cluster stopped writing: the status file is stale")]
    [InlineData(ClusterLiveness.Unreadable, ClusterLiveness.Live, "the file is not valid JSON.", EventLevel.Good, "The cluster's status file answers again")]
    public void TheReadingTheOtherTestsDoNotHandIsSaidForTheReadingItStandsOn(
        ClusterLiveness before,
        ClusterLiveness after,
        string? detail,
        EventLevel level,
        string text)
    {
        Assert.Equal(Event(level, null, text), ClusterEventDetector.Liveness(before, after, detail, Now));
    }

    /// <summary>
    /// The readings that are no event, beside the Pending and the unchanged ones the tests above pin: the same
    /// reading on both sides is no turn whatever it is, and a reading that is Pending on the side the turn would
    /// stand on is the first reading, which is no event.
    /// </summary>
    [Theory]
    [InlineData(ClusterLiveness.Pending, ClusterLiveness.Pending)]
    [InlineData(ClusterLiveness.Stale, ClusterLiveness.Stale)]
    [InlineData(ClusterLiveness.Unreadable, ClusterLiveness.Unreadable)]
    [InlineData(ClusterLiveness.Live, ClusterLiveness.Pending)]
    [InlineData(ClusterLiveness.Stale, ClusterLiveness.Pending)]
    [InlineData(ClusterLiveness.Unreadable, ClusterLiveness.Pending)]
    public void AReadingThatIsTheSameOnBothSidesOrPendingOnOneIsNoEvent(ClusterLiveness before, ClusterLiveness after)
    {
        Assert.Null(ClusterEventDetector.Liveness(before, after, "no answer within 10 s", Now));
    }

    // ----- Status, said without its parts -----

    /// <summary>What changed between no status and a status is no change: the rule says so before it says anything
    /// else, naming the argument.</summary>
    [Fact]
    public void AStatusThatIsNotThereHasNoChange()
    {
        var any = ClusterFixture.Status();

        var error = Assert.Throws<ArgumentNullException>(() => ClusterEventDetector.Status(null!, any, Environments, Now));

        Assert.Equal("before", error.ParamName);
    }

    /// <summary>What changed between a status and no status is no change: the rule says so before it says anything
    /// else, naming the argument.</summary>
    [Fact]
    public void TheStatusThePageHasNotReadYetIsSaidWithoutTheChange()
    {
        var any = ClusterFixture.Status();

        var error = Assert.Throws<ArgumentNullException>(() => ClusterEventDetector.Status(any, null!, Environments, Now));

        Assert.Equal("after", error.ParamName);
    }

    /// <summary>
    /// What changed between two statuses is said of the environments of the topology: without them the change is
    /// said of no environment, naming the argument - the names the events stand on are the names the topology
    /// gives, and the rule does not make them up.
    /// </summary>
    [Fact]
    public void TheChangeBetweenTwoStatusesIsSaidWithoutTheEnvironments()
    {
        var ready = With(ClusterFixture.Pod("ui"));
        var failing = With(ClusterFixture.Pod("ui").NotReady());

        var error = Assert.Throws<ArgumentNullException>(() => ClusterEventDetector.Status(ready, failing, null!, Now));

        Assert.Equal("environments", error.ParamName);
    }

    // ----- Status, the boundaries -----

    /// <summary>
    /// The two statuses that hold nothing - no node, no namespace, no pod - change nothing between them: the list
    /// of the change is empty, with or without the environments to say the names of.
    /// </summary>
    [Fact]
    public void TheTwoStatusesThatHoldNothingChangeNothing()
    {
        var empty = ClusterFixture.Status([], []);

        Assert.Empty(Between(empty, empty));
        Assert.Empty(ClusterEventDetector.Status(empty, empty, [], Now));
    }

    /// <summary>
    /// The number of pods a round names one by one, at its value: the ten pods that change are all named, and no
    /// count of the rest is said - the counting starts where the eleventh is not enough, and the test above steps
    /// across it at fourteen.
    /// </summary>
    [Fact]
    public void TheTenPodsThatChangeAreAllNamedAndNoRestIsCounted()
    {
        var names = Enumerable.Range(1, 10).Select(index => $"app{index:00}").ToList();
        var before = With([.. names.Select(name => ClusterFixture.Pod(name))]);
        var after = With([.. names.Select(name => ClusterFixture.Pod(name).NotReady())]);
        var expected = names
            .Select(name => Event(EventLevel.Warning, "tdd", $"{name} in cmdemo3-tdd is unhealthy: not ready"))
            .ToList();

        Assert.Equal(expected, Between(before, after));
    }

    /// <summary>
    /// One pod past the number the round names is the count the round says: ten named, and the one that is not
    /// named counted in one event.
    /// </summary>
    [Fact]
    public void TheEleventhPodThatChangesIsCountedAndNotNamed()
    {
        var names = Enumerable.Range(1, 11).Select(index => $"app{index:00}").ToList();
        var before = With([.. names.Select(name => ClusterFixture.Pod(name))]);
        var after = With([.. names.Select(name => ClusterFixture.Pod(name).NotReady())]);
        var expected = names
            .Take(ClusterEventDetector.NamedPods)
            .Select(name => Event(EventLevel.Warning, "tdd", $"{name} in cmdemo3-tdd is unhealthy: not ready"))
            .Append(Event(EventLevel.Info, null, "1 more pods changed in this check"))
            .ToList();

        Assert.Equal(expected, Between(before, after));
    }

    /// <summary>
    /// The restart count is said for the count that rose: the count that falls between two readings, for the pod
    /// that is not ready on both sides of it, is no event - the words the pod stands in are the same on both
    /// readings, and the count the page shows is the one the second reading stands on.
    /// </summary>
    [Fact]
    public void ARestartCountThatFallsIsNoEvent()
    {
        var before = With(ClusterFixture.Pod("ui", restarts: 7, reason: "CrashLoopBackOff").NotReady());
        var after = With(ClusterFixture.Pod("ui", restarts: 6, reason: "CrashLoopBackOff").NotReady());

        Assert.Empty(Between(before, after));
    }

    /// <summary>
    /// The reason a restart is said with is the one that stands beside the unready it explains: the pod that is
    /// ready again names no reason, with or without one in its facts, and the pod that is not ready but names no
    /// reason is said without one.
    /// </summary>
    [Fact]
    public void ARestartOfAPodThatIsReadyAgainNamesNoReasonEvenWithOne()
    {
        var before = With(ClusterFixture.Pod("ui", restarts: 1));
        var after = With(ClusterFixture.Pod("ui", restarts: 2, reason: "CrashLoopBackOff"));

        Assert.Equal([Event(EventLevel.Warning, "tdd", "ui in cmdemo3-tdd restarted (2 restarts)")], Between(before, after));
    }

    /// <summary>A restart of a pod that is not ready and names no reason is said without one: the reason that is
    /// not there is not said as what it is.</summary>
    [Fact]
    public void ARestartOfAPodThatIsNotReadyAndNamesNoReasonIsSaidWithoutOne()
    {
        var before = With(ClusterFixture.Pod("ui", restarts: 1).NotReady());
        var after = With(ClusterFixture.Pod("ui", restarts: 2).NotReady());

        Assert.Equal([Event(EventLevel.Warning, "tdd", "ui in cmdemo3-tdd restarted (2 restarts)")], Between(before, after));
    }

    // ----- Status, the environments -----

    /// <summary>
    /// The names the events of the pods stand on are the names the topology gives the namespaces: the topology
    /// that names no environment at all says the change of a pod with no environment named, and so does the
    /// environment whose namespace is not there - the namespace the pods stand in is not a name the topology made
    /// up, and no event stands on one.
    /// </summary>
    [Fact]
    public void TheChangeOfAPodIsSaidWithoutAnEnvironmentWithoutANamespace()
    {
        var ready = With(ClusterFixture.Pod("ui"));
        var failing = With(ClusterFixture.Pod("ui").NotReady());

        Assert.Equal(
            [Event(EventLevel.Warning, null, "ui in cmdemo3-tdd is unhealthy: not ready")],
            ClusterEventDetector.Status(ready, failing, [], Now));
        Assert.Equal(
            [Event(EventLevel.Warning, null, "ui in cmdemo3-tdd is unhealthy: not ready")],
            ClusterEventDetector.Status(ready, failing, [ClusterFixture.Environment("tdd", null)], Now));
    }

    /// <summary>
    /// Two environments of one namespace name it by the first: the names the events of the pods stand on are the
    /// first names the topology gives the namespace, and the second stands behind it, unread.
    /// </summary>
    [Fact]
    public void TheTwoEnvironmentsOfOneNamespaceNameItByTheFirst()
    {
        var before = ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui")));
        var after = ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui").NotReady()));
        EnvironmentInfo[] environments = [ClusterFixture.Environment("first", "apps"), ClusterFixture.Environment("second", "apps")];

        Assert.Equal(
            [Event(EventLevel.Warning, "first", "ui in apps is unhealthy: not ready")],
            ClusterEventDetector.Status(before, after, environments, Now));
    }

    // ----- Service, said without its facts -----

    /// <summary>What changed between no facts and facts is no change: the rule says so before it says anything
    /// else, naming the argument.</summary>
    [Fact]
    public void FactsThatAreNotThereHaveNoChange()
    {
        var any = ClusterFixture.SampleService;

        var error = Assert.Throws<ArgumentNullException>(() => ClusterEventDetector.Service(null!, any, Now));

        Assert.Equal("before", error.ParamName);
    }

    /// <summary>The facts the page has not read yet are said without the change, naming the argument.</summary>
    [Fact]
    public void TheFactsThePageHasNotReadYetAreSaidWithoutTheChange()
    {
        var any = ClusterFixture.SampleService;

        var error = Assert.Throws<ArgumentNullException>(() => ClusterEventDetector.Service(any, null!, Now));

        Assert.Equal("after", error.ParamName);
    }

    // ----- Service, the boundaries -----

    /// <summary>
    /// The round in which the verdict and the power state change at once is two events, in the order they are
    /// said: the verdict of Azure first, and the power state of the service after it.
    /// </summary>
    [Fact]
    public void TheRoundInTheWhichTheVerdictAndThePowerStateChangeIsTwoEvents()
    {
        var available = ClusterFixture.SampleService;
        var after = available with
        {
            Availability = new AksAvailability("Degraded", null, null, null),
            PowerState = "Stopped",
        };

        Assert.Equal(
            [
                Event(EventLevel.Warning, null, "Azure's verdict on the AKS service: Available → Degraded", ClusterEventDetector.ServicePlace),
                Event(EventLevel.Info, null, "The power state of the AKS service: Running → Stopped", ClusterEventDetector.ServicePlace),
            ],
            ClusterEventDetector.Service(available, after, Now));
    }

    /// <summary>
    /// The verdict Azure writes in the word it does not write in the other three is said as the word it is:
    /// unknown is a verdict the page says, at the level it says the words it does not know, with the words it
    /// stands on.
    /// </summary>
    [Fact]
    public void TheVerdictAzureDoesNotKnowIsSaidAsTheWordItIs()
    {
        var available = ClusterFixture.SampleService;
        var unknown = available with { Availability = new AksAvailability(AksAvailability.Unknown, null, null, null) };

        Assert.Equal(
            [Event(EventLevel.Info, null, "Azure's verdict on the AKS service: Available → Unknown", ClusterEventDetector.ServicePlace)],
            ClusterEventDetector.Service(available, unknown, Now));
    }

    /// <summary>
    /// The power state the service takes that is neither Running nor Stopped is said as the word it is: the word
    /// Azure writes is the word the event stands on, and the level the turn that is not the one back to Running
    /// stands at is the level of a word the page reports.
    /// </summary>
    [Fact]
    public void ThePowerStateThatIsNeitherRunningNorStoppedIsSaidAsTheWordItIs()
    {
        var available = ClusterFixture.SampleService;
        var stopping = available with { PowerState = "Stopping" };

        Assert.Equal(
            [Event(EventLevel.Info, null, "The power state of the AKS service: Running → Stopping", ClusterEventDetector.ServicePlace)],
            ClusterEventDetector.Service(available, stopping, Now));
    }

    /// <summary>
    /// The word Azure writes in other letters is the same word: the verdict and the power state compared as the
    /// words they are, not as the letters they stand in, and the round in which nothing but the letters change is
    /// no event.
    /// </summary>
    [Fact]
    public void TheWordAzureWritesInOtherLettersIsTheSameWord()
    {
        var available = ClusterFixture.SampleService;

        Assert.Empty(ClusterEventDetector.Service(available, available with { Availability = new AksAvailability("available", null, null, null) }, Now));
        Assert.Empty(ClusterEventDetector.Service(available, available with { PowerState = "running" }, Now));
    }
}
