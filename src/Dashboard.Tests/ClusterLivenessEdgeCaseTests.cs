namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterLiveness" /> and of the members of
/// <see cref="ClusterAssessment" /> that turn a reading into one: <see cref="ClusterAssessment.LivenessOf"/>, the
/// state each liveness takes in <see cref="ClusterAssessment.Live"/> and <see cref="ClusterAssessment.Service"/>,
/// and the words the problems and the starting ones are said in (<see cref="ClusterAssessment.Problems"/>,
/// <see cref="ClusterAssessment.Named"/>, <see cref="PodRules"/>). <see cref="ClusterAssessmentTests" /> pins each
/// state at a moment of its choosing - a sample status, a status twelve minutes past its moment - and
/// <see cref="ClusterDeliveryHelperTests" /> pins the words <see cref="ClusterAssessment.PodProblem"/> and
/// <see cref="ClusterAssessment.AzureReports"/> make.
/// <para>
/// What is pinned here is the edge each of those readings sits next to, and only that: the five words of the
/// liveness and their order, the two graces and the number of problems a headline names, to their value; the tick
/// <see cref="ClusterAssessment.Live"/> turns a fresh status stale at, and the two reads that sit exactly on the
/// Live side and the Silent side of it - a clock that stands at the cluster's, or behind it, and a reading marked
/// Read that holds nothing; and the reading, the status, the pod and the list that are not there at all, each
/// answered with the argument it is missing named.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class ClusterLivenessEdgeCaseTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;
    private static readonly DateTimeOffset Now = Generated.AddSeconds(20);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    // ----- The liveness itself -----

    /// <summary>
    /// The five words of the liveness, in the order they are declared: the page and the events between readings
    /// are compared on these values, so their names and their order are the contract, and no sixth word stands
    /// behind them.
    /// </summary>
    [Fact]
    public void TheFiveWordsOfTheLivenessAreTheFiveThePageKnows()
    {
        Assert.Equal(0, (int)ClusterLiveness.Pending);
        Assert.Equal(1, (int)ClusterLiveness.Live);
        Assert.Equal(2, (int)ClusterLiveness.Stale);
        Assert.Equal(3, (int)ClusterLiveness.Silent);
        Assert.Equal(4, (int)ClusterLiveness.Unreadable);

        Assert.Equal(["Pending", "Live", "Stale", "Silent", "Unreadable"], Enum.GetNames<ClusterLiveness>());
        Assert.Equal(ClusterLiveness.Unreadable, Enum.Parse<ClusterLiveness>("Unreadable"));
    }

    // ----- The rules -----

    /// <summary>
    /// The two graces the rules count ages in, to their value: the boundary tests of
    /// <see cref="ClusterAssessmentTests" /> step across them at 300/301 s and 119/120 s, but none of them pins
    /// the minutes they are.
    /// </summary>
    [Fact]
    public void TheTwoGracesAreTheMinutesTheRulesCount()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), PodRules.PendingGrace);
        Assert.Equal(TimeSpan.FromMinutes(2), PodRules.StartingGrace);
    }

    /// <summary>The state of a pod is said about a pod: without one the rule says so, naming the argument.</summary>
    [Fact]
    public void APodsStateIsSaidWithoutAPod()
    {
        var error = Assert.Throws<ArgumentNullException>(() => PodRules.StateOf(null!, Generated));

        Assert.Equal("pod", error.ParamName);
    }

    /// <summary>
    /// A pod the clock has not reached is younger than any grace: a running pod that is not ready and a pending
    /// pod whose start stands in the future are both starting, and one that started at the moment the facts are of
    /// is the youngest of all.
    /// </summary>
    [Fact]
    public void APodThatStartsAfterTheFactsAreOfIsYoungerThanAnyGrace()
    {
        var future = ClusterFixture.Pod("ui", ready: false, age: TimeSpan.FromMinutes(-5));
        var pending = ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: TimeSpan.FromMinutes(-5));
        var justNow = ClusterFixture.Pod("ui", ready: false, age: TimeSpan.Zero);

        Assert.Equal(PodState.Starting, PodRules.StateOf(future, Generated));
        Assert.Equal(PodState.Starting, PodRules.StateOf(pending, Generated));
        Assert.Equal(PodState.Starting, PodRules.StateOf(justNow, Generated));
    }

    // ----- LivenessOf -----

    /// <summary>The liveness of a reading is said about a reading: without one it is said so, naming the argument.</summary>
    [Fact]
    public void TheLivenessIsSaidWithoutAReading()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.LivenessOf(null!, Now));

        Assert.Equal("reading", error.ParamName);
    }

    /// <summary>
    /// A reading marked Read that holds nothing is one that does not answer: the value the reading would have held
    /// is null, with or without the reason that stands in its place, and the liveness is Silent, not Live.
    /// </summary>
    [Fact]
    public void AReadingThatIsMarkedReadButHoldsNothingIsSilent()
    {
        Assert.Equal(ClusterLiveness.Silent, ClusterAssessment.LivenessOf(new SourceReading<ClusterStatus>(SourceState.Read), Now));
        Assert.Equal(
            ClusterLiveness.Silent,
            ClusterAssessment.LivenessOf(new SourceReading<ClusterStatus>(SourceState.Read, Detail: "the file is not valid JSON"), Now));
    }

    /// <summary>
    /// The age of the file runs back from the page's clock to the moment it was written; the two ends of that run
    /// are the moments the file cannot be older than: the moment it was written itself, and a clock that stands
    /// behind the cluster's - both say Live, whatever the interval.
    /// </summary>
    [Fact]
    public void AClockAtTheClustersOrBehindItSaysLive()
    {
        var reading = ClusterFixture.Read(ClusterFixture.Status());

        Assert.Equal(ClusterLiveness.Live, ClusterAssessment.LivenessOf(reading, Generated));
        Assert.Equal(ClusterLiveness.Live, ClusterAssessment.LivenessOf(reading, Generated.AddSeconds(-1)));
        Assert.Equal(ClusterLiveness.Live, ClusterAssessment.LivenessOf(reading, Generated.AddMinutes(-10)));
    }

    // ----- The cluster, live -----

    /// <summary>The state of the cluster is said about a reading: without one it is said so, naming the argument.</summary>
    [Fact]
    public void TheClusterIsSaidWithoutAReading()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Live(null!, null, Now, Utc));

        Assert.Equal("reading", error.ParamName);
    }

    /// <summary>
    /// The tick <see cref="ClusterAssessment.Live"/> turns a fresh status stale: the fixture's status writes every
    /// 15 s, so its <c>StaleAfter</c> is four intervals - the minute - and at the minute itself it is still
    /// healthy, one tick past it it is stale. The stale words are the collector's moment and its age, in the
    /// page's words, and what is shown below is as of then.
    /// </summary>
    [Fact]
    public void AStatusIsStillLiveAtItsStaleAfterAndStaleOneTickPastIt()
    {
        var reading = ClusterFixture.Read(ClusterFixture.Status());

        Assert.Equal(
            new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every node is ready and under no pressure, and every pod is ready"),
            ClusterAssessment.Live(reading, null, Generated.AddSeconds(60), Utc));
        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Unreachable,
                "Stale",
                "The collector in the cluster last wrote 20:15:30 (1 min ago)",
                "What is shown below is as of then."),
            ClusterAssessment.Live(reading, null, Generated.AddSeconds(61), Utc));
    }

    /// <summary>
    /// Azure's facts marked Read but holding nothing are facts that could not be read, beside the two words the
    /// cluster says where it says them: the silence of the status file, and the age of the collector's last write.
    /// </summary>
    [Fact]
    public void AzuresFactsThatAreMarkedReadButHoldNothingAreFactsThatCouldNotBeRead()
    {
        var reading = new SourceReading<AksService>(SourceState.Read);

        Assert.Equal(
            "No answer within 10 s. Azure's facts about the AKS service could not be read either.",
            ClusterAssessment.Live(new SourceReading<ClusterStatus>(SourceState.Unavailable, Detail: "no answer within 10 s"), reading, Now, Utc).Detail);
        Assert.Equal(
            "What is shown below is as of then. Azure's facts about the AKS service could not be read either.",
            ClusterAssessment.Live(ClusterFixture.Read(ClusterFixture.Status()), reading, Generated.AddSeconds(61), Utc).Detail);
    }

    // ----- The AKS service -----

    /// <summary>The state of the service is said about a reading: without one it is said so, naming the argument.</summary>
    [Fact]
    public void TheServiceIsSaidWithoutAReading()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Service(null!));

        Assert.Equal("reading", error.ParamName);
    }

    // ----- The problems, named -----

    /// <summary>The problems of a status are said about a status: without one they are said so, naming the argument.</summary>
    [Fact]
    public void TheProblemsAreSaidWithoutAStatus()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Problems(null!, Generated));

        Assert.Equal("status", error.ParamName);
    }

    /// <summary>
    /// A status that names neither node nor namespace says exactly one thing - that it names no node - and nothing
    /// else: the list of its problems is that line alone.
    /// </summary>
    [Fact]
    public void AStatusThatNamesNothingNamesOnlyThatItNamesNoNode()
    {
        Assert.Equal(["The status file lists no node"], ClusterAssessment.Problems(ClusterFixture.Status([]), Generated));
    }

    /// <summary>The number of problems a headline names before it counts the rest, to its value: three.</summary>
    [Fact]
    public void AHeadlineNamesThreeProblemsBeforeItCountsTheRest()
    {
        Assert.Equal(3, ClusterAssessment.NamedProblems);
    }

    /// <summary>
    /// A list is named at its ends: the empty list says no word at all, and five is the count the four-item test
    /// of <see cref="ClusterAssessmentTests" /> leaves open - the rest are two. A list that is not there is said
    /// so, naming the argument.
    /// </summary>
    [Fact]
    public void ANamedListIsSaidAtItsEnds()
    {
        Assert.Equal(string.Empty, ClusterAssessment.Named([]));
        Assert.Equal("a; b; c; and 2 more", ClusterAssessment.Named(["a", "b", "c", "d", "e"]));

        var error = Assert.Throws<ArgumentNullException>(() => ClusterAssessment.Named(null!));
        Assert.Equal("items", error.ParamName);
    }
}
