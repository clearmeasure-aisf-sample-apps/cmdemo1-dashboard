using static Dashboard.Tests.ClusterFixture;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the members of <see cref="ClusterAssessment" /> that no other
/// file pins yet. What those files already hold is left alone: <see cref="ClusterAssessmentTests" /> and
/// <see cref="ClusterStateEdgeCaseTests" /> pin the states of a reading, the record's four parts and their equality,
/// <see cref="ClusterLivenessEdgeCaseTests" /> the tick a status turns stale at and the readings that are not there
/// at all, <see cref="ClusterHelperCoverageTests" /> the order of a problem list, and
/// <see cref="ClusterDeliveryHelperTests" /> the words <see cref="ClusterAssessment.PodProblem" /> makes of a pod
/// in trouble and <see cref="ClusterAssessment.AzureReports" /> makes of every reading of Azure's facts.
/// <para>
/// What is pinned here is the rest: a silence about why the file did not answer, which leaves the assessment with no
/// detail at all - and the single letter, the spaces alone and the full stop already there that make a sentence or
/// make none; a
/// status that never says when it was written, which no clock can make stale; the node that is named once although
/// it is under pressure; what is wrong said before what is only starting; Azure's sentence left out of the detail
/// where Azure says none; the state word that is the empty string and the one that is only another case; the names
/// that say nothing kept in the list they are named from; a problem with a pod that is not a problem at all; and
/// the verdict that is not there, which is left out where an empty one stands.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings, and the
/// only zone the words are asked in is UTC.
/// </para>
/// </summary>
public class ClusterAssessmentEdgeCaseTests
{
    /// <summary>Azure's own sentence about the sample's verdict, the detail the service card stands under.</summary>
    private const string AzureSays = "There aren't any known Azure platform problems affecting this managed cluster.";

    // ----- ClusterAssessment.Live: a silence about why -----

    /// <summary>
    /// A status file that does not answer and says nothing for why is said without a detail: the reason that is not
    /// there, the one that is the empty string and the one that is spaces alone are all the silence itself, and no
    /// sentence is built out of them - the assessment stands with its four parts and the last of them null.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AStatusFileThatDoesNotAnswerWithoutAWordForWhySaysNothingMore(string? detail)
    {
        var assessment = ClusterAssessment.Live(Silent(detail: detail), null, Now, Utc);

        Assert.Equal(
            new ClusterAssessment(ClusterState.Unreachable, "Unreachable", "The cluster's status file does not answer"),
            assessment);
        Assert.Null(assessment.Detail);
    }

    /// <summary>
    /// The reason a reading carries becomes a sentence of its own at its shortest: the one letter of it stands in
    /// the capital it is given and nothing follows it, so a detail of one character is a word and a full stop, not
    /// an argument that runs past the end of the string.
    /// </summary>
    [Fact]
    public void AReasonOfASingleLetterIsASentenceOfItsOwn() =>
        Assert.Equal("X.", ClusterAssessment.Live(Silent(detail: "x"), null, Now, Utc).Detail);

    /// <summary>
    /// A reason that already ends in its full stop is said with one: the stop that stands there is taken off before
    /// the one that closes the sentence is put on, so the words do not end in two.
    /// </summary>
    [Fact]
    public void AReasonThatAlreadyEndsInAFullStopIsSaidWithOne() =>
        Assert.Equal(
            "The address answered HTTP 404.",
            ClusterAssessment.Live(Silent(detail: "the address answered HTTP 404."), null, Now, Utc).Detail);

    /// <summary>
    /// A reason is said with one closing stop whatever shape its end takes: no stop, one stop, a stop and the space a
    /// JSON field or a trimmed message leaves after it, two stops, and spaces around the words - the words the page
    /// shows stay whole and never end in ". .".
    /// </summary>
    [Theory]
    [InlineData("the file is empty")]
    [InlineData("the file is empty.")]
    [InlineData("the file is empty. ")]
    [InlineData("the file is empty.\n")]
    [InlineData("the file is empty..")]
    [InlineData("the file is empty. .")]
    [InlineData("  the file is empty.  ")]
    public void AReasonIsSaidWithOneClosingStopWhateverItsEnd(string detail)
    {
        Assert.Equal("The file is empty.", ClusterAssessment.Live(Silent(detail: detail), null, Now, Utc).Detail);
        Assert.Equal(
            "The file is empty.",
            ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Malformed, Detail: detail)).Detail);
        Assert.Equal(
            "The file is empty. The page reads them again with every check.",
            ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Unavailable, Detail: detail)).Detail);
    }

    // ----- ClusterAssessment.Live: what is wrong before what is only starting -----

    /// <summary>
    /// The pods that are only starting are counted where nothing is wrong: while a problem stands in the file the
    /// headline names the problem alone, and the young pod that is not ready is not mentioned - the same status read
    /// without it is the starting one the page waits on.
    /// </summary>
    [Fact]
    public void WhatIsWrongIsSaidBeforeWhatIsOnlyStarting()
    {
        var starting = ClusterFixture.Pod("ui", ready: false, age: TimeSpan.FromSeconds(40));
        var broken = ClusterFixture.Pod("api", ready: false, restarts: 2, reason: "CrashLoopBackOff");

        Assert.Equal(
            new ClusterAssessment(ClusterState.Unhealthy, "Unhealthy", "api in apps: CrashLoopBackOff, 2 restarts"),
            ClusterAssessment.Live(ClusterFixture.Read(ClusterFixture.Status(null, ClusterFixture.Space("apps", starting, broken))), null, Now, Utc));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Checking, "Starting", "1 pod is starting: ui in apps"),
            ClusterAssessment.Live(ClusterFixture.Read(ClusterFixture.Status(null, ClusterFixture.Space("apps", starting))), null, Now, Utc));
    }

    // ----- ClusterAssessment.LivenessOf and Live: a file that never says when -----

    /// <summary>
    /// A status that does not say when the collector wrote it is never stale, and so is live at any clock however
    /// far the page's stands after the moment the fixture was written: the age the staleness is counted from has no
    /// start, and the cluster is judged on what the file holds.
    /// </summary>
    [Fact]
    public void AStatusThatNeverSaysWhenItWasWrittenIsLiveAtAnyClock()
    {
        var reading = ClusterFixture.Read(ClusterFixture.Status() with { Generated = null });
        var far = Generated.AddDays(30);

        Assert.Equal(ClusterLiveness.Live, ClusterAssessment.LivenessOf(reading, far));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Healthy, "Healthy", "Every node is ready and under no pressure, and every pod is ready"),
            ClusterAssessment.Live(reading, null, far, Utc));
    }

    // ----- ClusterAssessment.Problems: one node, one line -----

    /// <summary>
    /// A node that is not ready is named once: the pressures it stands under are said of the ready ones alone, so
    /// the node that cannot be reached does not fill the headline twice over - and the ready node under the same
    /// pressures is named for them.
    /// </summary>
    [Fact]
    public void ANodeThatIsNotReadyIsNamedOnceAndNotAgainForThePressuresItIsUnder()
    {
        var down = ClusterFixture.Node("aks-system-12148983-vmss000000", ready: false, "MemoryPressure", "DiskPressure", "PIDPressure");

        Assert.Equal(
            ["Node aks-…000000 is not ready"],
            ClusterAssessment.Problems(ClusterFixture.Status([down]), Generated));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Unhealthy, "Unhealthy", "Node aks-…000000 is not ready"),
            ClusterAssessment.Live(ClusterFixture.Read(ClusterFixture.Status([down])), null, Now, Utc));

        Assert.Equal(
            ["Node aks-…000000: memory pressure, disk pressure, PID pressure"],
            ClusterAssessment.Problems(ClusterFixture.Status([down with { Ready = true }]), Generated));
    }

    // ----- ClusterAssessment.Service: the sentences Azure does not say -----

    /// <summary>
    /// Where Azure says no sentence there is no detail: the healthy verdict, the stopped one and the degraded one
    /// all stand on the headline alone when Azure's <c>summary</c> is not there - and the sentence that is there and
    /// says nothing is kept as the empty string it is, not read as the absence of one.
    /// </summary>
    [Fact]
    public void AzuresVerdictWithoutItsSentenceLeavesTheDetailOut()
    {
        var silence = new AksAvailability(AksAvailability.Available, null, null, null);

        Assert.Equal(
            new ClusterAssessment(ClusterState.Healthy, "Available", "Azure reports the AKS service available and running"),
            Service(ClusterFixture.SampleService with { Availability = silence }));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Neutral, "Stopped", "The AKS service is stopped"),
            Service(ClusterFixture.SampleService with { Availability = silence with { State = AksAvailability.Unavailable }, PowerState = AksService.Stopped }));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Warning, "Degraded", "Azure reports the AKS service degraded"),
            Service(ClusterFixture.SampleService with { Availability = silence with { State = AksAvailability.Degraded } }));
        Assert.Equal(
            string.Empty,
            Service(ClusterFixture.SampleService with { Availability = silence with { Summary = string.Empty } }).Detail);
    }

    /// <summary>
    /// A state word that is the empty string is still a state word the service says: an empty provisioning state is
    /// not <c>Succeeded</c>, so it is a warning labelled with the nothing it is - while <c>Succeeded</c> written in
    /// another case is <c>Succeeded</c> and no warning at all, and the service is judged on its verdict.
    /// </summary>
    [Fact]
    public void AProvisioningWordThatIsTheEmptyStringIsAWarningAndOneInAnotherCaseIsNot()
    {
        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Warning,
                string.Empty,
                "The provisioning state of the AKS service is , not Succeeded",
                AzureSays),
            Service(ClusterFixture.SampleService with { ProvisioningState = string.Empty }));
        Assert.Equal(
            new ClusterAssessment(ClusterState.Healthy, "Available", "Azure reports the AKS service available and running", AzureSays),
            Service(ClusterFixture.SampleService with { ProvisioningState = "SUCCEEDED" }));
    }

    /// <summary>
    /// A verdict that is the empty word is none of the four the page reads: the service is neutral under it, its
    /// label is the nothing Azure wrote, and the headline is the sentence that names no verdict - the empty word
    /// read as a word, exactly as <see cref="ClusterStateEdgeCaseTests" /> pins it of Azure's own report.
    /// </summary>
    [Fact]
    public void AVerdictThatIsTheEmptyWordIsNeutralAndSaidAsItIs() =>
        Assert.Equal(
            new ClusterAssessment(ClusterState.Neutral, string.Empty, "Azure reports the AKS service "),
            Service(ClusterFixture.SampleService with { Availability = new AksAvailability(string.Empty, null, null, null) }));

    /// <summary>
    /// A service file that could not be read and says nothing for why: the reason that is spaces alone is no
    /// sentence, so a malformed file stands with its headline alone, and an unreadable one is left with the calm
    /// word that follows - the promise that the page reads them again, which is said whether the file gave a reason
    /// or not.
    /// </summary>
    [Fact]
    public void AServiceFileThatCannotBeReadWithoutAWordForWhySaysNothingMore()
    {
        Assert.Equal(
            new ClusterAssessment(ClusterState.Warning, "Unreadable", "Azure's facts about the AKS service could not be read"),
            ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Malformed, Detail: "   ")));
        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Neutral,
                "Not known",
                "Azure's facts about the AKS service could not be read",
                "The page reads them again with every check."),
            ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Unavailable, Detail: "   ")));
    }

    /// <summary>
    /// A service reading marked read that holds no facts is no service to assess: it is said as the reading that
    /// could not be read - not known, with the promise that the page reads them again - the way the cluster's
    /// liveness counts the status reading that holds nothing as silent and Azure's words say it could not be read.
    /// </summary>
    [Fact]
    public void AServiceReadingMarkedReadThatHoldsNothingIsNotKnown() =>
        Assert.Equal(
            new ClusterAssessment(
                ClusterState.Neutral,
                "Not known",
                "Azure's facts about the AKS service could not be read",
                "The page reads them again with every check."),
            ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Read)));

    // ----- ClusterAssessment.AzureReports -----

    /// <summary>
    /// The verdict that is not there is left out of Azure's words, where the verdict that is the empty word stands
    /// as a word of its own: an availability that names no state leaves the power state to be said alone, joined to
    /// no empty first word.
    /// </summary>
    [Fact]
    public void AVerdictThatIsNotThereIsLeftOutOfAzuresWords() =>
        Assert.Equal(
            "Azure reports the AKS service Running, as of 20:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(
                ClusterFixture.Read(ClusterFixture.SampleService with { Availability = new AksAvailability(null!, null, null, null) }), Generated, Utc));

    // ----- ClusterAssessment.Named -----

    /// <summary>
    /// A list of names is named as it stands: a name that says nothing is a name, kept in the list between the
    /// separators that hold them apart, and the count of what is not named counts them too - three of them name
    /// three and count none, four of them name three and count the fourth.
    /// </summary>
    [Fact]
    public void ANamedListKeepsTheNamesThatSayNothing()
    {
        Assert.Equal(string.Empty, ClusterAssessment.Named([string.Empty]));
        Assert.Equal("; ", ClusterAssessment.Named([string.Empty, string.Empty]));
        Assert.Equal("; ; ", ClusterAssessment.Named([string.Empty, string.Empty, string.Empty]));
        Assert.Equal("; ; ; and 1 more", ClusterAssessment.Named([string.Empty, string.Empty, string.Empty, string.Empty]));
    }

    // ----- ClusterAssessment.PodProblem -----

    /// <summary>
    /// A problem with a pod is said in the words of a problem whatever the pod is: the rule that decides a pod's
    /// state is never consulted, so the pod that runs ready is <c>not ready</c>, the one that finished is said the
    /// reason it ended for, and the pending one that says it is ready is pending for its age - the words the page
    /// only ever stands over a pod that is not ready.
    /// </summary>
    [Fact]
    public void AProblemWithAPodSaysTheWordsOfAProblemForAnyPod()
    {
        Assert.Equal("ui in apps: not ready", ClusterAssessment.PodProblem(ClusterFixture.Pod("ui"), "apps", Generated));
        Assert.Equal(
            "job in apps: Completed",
            ClusterAssessment.PodProblem(ClusterFixture.Pod("job", ClusterPod.Succeeded, false, reason: "Completed"), "apps", Generated));
        Assert.Equal(
            "new in apps: Pending for 90 min",
            ClusterAssessment.PodProblem(ClusterFixture.Pod("new", ClusterPod.Pending, ready: true, age: TimeSpan.FromMinutes(90)), "apps", Generated));
    }
}
