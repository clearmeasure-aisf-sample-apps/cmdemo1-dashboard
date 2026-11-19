namespace Dashboard.Tests;

/// <summary>
/// The small helpers around the big ones: where a pod is said to be and what a problem with it reads like
/// (<see cref="ClusterText.PodIn"/>, <see cref="ClusterAssessment.PodProblem"/>), what Azure says where the cluster
/// itself says nothing (<see cref="ClusterAssessment.AzureReports"/>), what the delivery card's two little words do
/// (<see cref="DeliveryText.BehindHelp"/>, <see cref="DeliveryReport.IsSystem"/>), and how a line on a node is cut
/// (<see cref="DeploymentActivity.Cut"/>).
/// </summary>
public class ClusterDeliveryHelperTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;
    private static readonly DateTimeOffset AzureRead = ClusterFixture.SampleService.Generated!.Value;
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    // ----- ClusterText.PodIn -----

    [Theory]
    [InlineData("cmdemo3-tdd", "ui in cmdemo3-tdd")]
    [InlineData("apps", "ui in apps")]
    [InlineData("kube-system", "ui in kube-system")]
    public void APodIsNamedWithTheSpaceItIsIn(string space, string expected)
    {
        Assert.Equal(expected, ClusterText.PodIn(ClusterFixture.Pod("ui"), space));
    }

    [Fact]
    public void APodIsCalledByItsWorkloadAndByItsOwnNameWithoutOne()
    {
        Assert.Equal("ui in apps", ClusterText.PodIn(ClusterFixture.Pod("ui", name: "ui-7f9c4-2xk4"), "apps"));
        Assert.Equal("ui-7f9c4-2xk4 in apps", ClusterText.PodIn(ClusterFixture.Pod("ui", name: "ui-7f9c4-2xk4") with { Workload = null }, "apps"));
    }

    [Fact]
    public void APodThatIsNotThereIsNotSaidToBeAnywhere() =>
        Assert.Throws<ArgumentNullException>(() => ClusterText.PodIn(null!, "apps"));

    // ----- ClusterAssessment.AzureReports -----

    [Fact]
    public void WithoutAnAddressForAzuresFactsOrWithoutAReadingTheySayNothing()
    {
        Assert.Null(ClusterAssessment.AzureReports(null, Generated, Utc));
        Assert.Null(ClusterAssessment.AzureReports(new SourceReading<AksService>(SourceState.Pending), Generated, Utc));
    }

    [Fact]
    public void AzuresFactsThatAreNotPublishedYetAreSaidCalmly() =>
        Assert.Equal(
            "Azure's facts about the AKS service are not published yet.",
            ClusterAssessment.AzureReports(new SourceReading<AksService>(SourceState.Missing), Generated, Utc));

    [Theory]
    [InlineData(SourceState.Unavailable)]
    [InlineData(SourceState.Malformed)]
    public void AzuresFactsThatCouldNotBeReadAreSaidToHaveFailedToo(SourceState state) =>
        Assert.Equal(
            "Azure's facts about the AKS service could not be read either.",
            ClusterAssessment.AzureReports(new SourceReading<AksService>(state, Detail: "no answer within 10 s"), Generated, Utc));

    [Fact]
    public void AReadingThatSaysItReadNothingIsOneThatCouldNotBeRead() =>
        Assert.Equal(
            "Azure's facts about the AKS service could not be read either.",
            ClusterAssessment.AzureReports(new SourceReading<AksService>(SourceState.Read), Generated, Utc));

    [Fact]
    public void AzureNamesItsVerdictAndItsPowerStateTogether()
    {
        // The sample file: availability Available, power state Running, read at 20:10:04.
        Assert.Equal(
            "Azure reports the AKS service Available and Running, as of 20:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(ClusterFixture.Read(ClusterFixture.SampleService), Generated, Utc));

        // Only one of the two: the other is left out, not said as an empty word.
        Assert.Equal(
            "Azure reports the AKS service Available, as of 20:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(ClusterFixture.Read(ClusterFixture.SampleService with { PowerState = null }), Generated, Utc));
        Assert.Equal(
            "Azure reports the AKS service Running, as of 20:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(ClusterFixture.Read(ClusterFixture.SampleService with { Availability = null }), Generated, Utc));
    }

    [Fact]
    public void AzureFallsBackToTheProvisioningStateAndOtherwiseSaysNothing()
    {
        Assert.Equal(
            "Azure reports the AKS service Updating, as of 20:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(
                ClusterFixture.Read(ClusterFixture.SampleService with { Availability = null, PowerState = null, ProvisioningState = "Updating" }), Generated, Utc));
        Assert.Null(
            ClusterAssessment.AzureReports(
                ClusterFixture.Read(ClusterFixture.SampleService with { Availability = null, PowerState = null, ProvisioningState = null }), Generated, Utc));
    }

    [Fact]
    public void AzureNamesTheMomentItsFactsAreOfInTheViewersZoneAndNotAtAllWithoutOne()
    {
        var service = ClusterFixture.SampleService;

        // The same facts at the same moment, in Chicago: 20:10:04 UTC is 15:10:04 there, and the age knows no zone.
        Assert.Equal(
            "Azure reports the AKS service Available and Running, as of 15:10:04 (5 min ago).",
            ClusterAssessment.AzureReports(ClusterFixture.Read(service), Generated, TimeZoneInfo.FindSystemTimeZoneById("America/Chicago")));
        Assert.Equal(
            "Azure reports the AKS service Available and Running.",
            ClusterAssessment.AzureReports(ClusterFixture.Read(service with { Generated = null }), Generated, Utc));
        Assert.Equal(
            "Azure reports the AKS service Available and Running, as of 20:10:04 (3 d ago).",
            ClusterAssessment.AzureReports(ClusterFixture.Read(service), AzureRead.AddDays(3), Utc));
    }

    // ----- ClusterAssessment.PodProblem -----

    [Theory]
    [InlineData(ClusterPod.Running, null, 0, 3600, "api in apps: not ready")]
    [InlineData(ClusterPod.Running, "CrashLoopBackOff", 0, 3600, "api in apps: CrashLoopBackOff")]
    [InlineData(ClusterPod.Running, "CrashLoopBackOff", 1, 3600, "api in apps: CrashLoopBackOff, 1 restart")]
    [InlineData(ClusterPod.Running, "CrashLoopBackOff", 7, 3600, "api in apps: CrashLoopBackOff, 7 restarts")]
    [InlineData(ClusterPod.Failed, "Evicted", 0, 3600, "api in apps: Evicted")]
    [InlineData(ClusterPod.Failed, null, 3, 3600, "api in apps: Failed, 3 restarts")]
    [InlineData(ClusterPod.Pending, null, 0, 720, "api in apps: Pending for 12 min")]
    [InlineData(ClusterPod.Pending, "ImagePullBackOff", 0, 720, "api in apps: Pending for 12 min: ImagePullBackOff")]
    [InlineData(ClusterPod.Pending, null, 0, 10800, "api in apps: Pending for 3 h")]
    [InlineData(ClusterPod.Unknown, null, 0, 3600, "api in apps: Unknown")]
    [InlineData(ClusterPod.Unknown, "ContainerStatusUnknown", 2, 86400, "api in apps: ContainerStatusUnknown, 2 restarts")]
    public void AProblemWithAPodNamesThePodWhatItSaysAndHowOftenItRestarted(string phase, string? reason, int restarts, int ageSeconds, string expected)
    {
        var pod = ClusterFixture.Pod("api", phase, ready: false, restarts: restarts, reason: reason, age: TimeSpan.FromSeconds(ageSeconds));

        Assert.Equal(expected, ClusterAssessment.PodProblem(pod, "apps", Generated));
    }

    [Fact]
    public void APendingPodThatDoesNotSayWhenItStartedIsPendingWithoutAnAge()
    {
        var pod = ClusterFixture.Pod("new", ClusterPod.Pending, ready: false, started: false);

        Assert.Equal("new in apps: Pending", ClusterAssessment.PodProblem(pod, "apps", Generated));
        Assert.Equal("new in cmdemo3-tdd: Pending", ClusterAssessment.PodProblem(pod, "cmdemo3-tdd", Generated));
    }

    [Fact]
    public void APodsAgeInAProblemIsCountedToTheMomentTheFactsAreOf()
    {
        var pod = ClusterFixture.Pod("slow", ClusterPod.Pending, ready: false, age: TimeSpan.FromMinutes(12));

        // It started 12 min before the file was written: at that moment the pod is 12 min old (minutes are said up
        // to two hours), with a status a shift later it is 12 h old (hours are said up to two days).
        Assert.Equal("slow in apps: Pending for 12 min", ClusterAssessment.PodProblem(pod, "apps", Generated));
        Assert.Equal("slow in apps: Pending for 12 h", ClusterAssessment.PodProblem(pod, "apps", Generated.AddHours(12)));
    }

    [Fact]
    public void AProblemWithNoPodNamesNoPod() =>
        Assert.Throws<ArgumentNullException>(() => ClusterAssessment.PodProblem(null!, "apps", Generated));

    // ----- DeliveryText.BehindHelp and DeliveryReport.IsSystem -----

    [Theory]
    [InlineData("tdd", "Versions: the distance in the project's list of releases. Days: since this environment and tdd last ran the same release.")]
    [InlineData("pre-prod", "Versions: the distance in the project's list of releases. Days: since this environment and pre-prod last ran the same release.")]
    public void TheTitleOfComparedSaysWhatTheTwoNumbersCount(string first, string expected) =>
        Assert.Equal(expected, DeliveryText.BehindHelp(first));

    [Fact]
    public void TheTitleNamesTheEnvironmentItComparesWith() =>
        Assert.Equal(DeliveryText.BehindHelp("tdd").Replace("tdd", "uat", StringComparison.Ordinal), DeliveryText.BehindHelp("uat"));

    [Theory]
    [InlineData("system", true)]
    [InlineData("System", true)]
    [InlineData("SYSTEM", true)]
    [InlineData("sYsTeM", true)]
    [InlineData("ui", false)]
    [InlineData("dashboard", false)]
    [InlineData("", false)]
    [InlineData(" system", false)]
    [InlineData("system ", false)]
    [InlineData("systems", false)]
    [InlineData("demo-system", false)]
    [InlineData(null, false)]
    public void TheSystemProjectIsNamedInAnyCaseAndNothingElseIs(string? name, bool isSystem) =>
        Assert.Equal(isSystem, DeliveryReport.IsSystem(name!));

    [Fact]
    public void TheNameOfTheSystemProjectIsTheOneItsEntryCarries()
    {
        Assert.Equal("system", DeliveryReport.SystemName);
        Assert.True(DeliveryReport.IsSystem(DeliveryReport.SystemName));
        Assert.Equal("the system (infrastructure and pipeline) in uat", DeliveryText.Context(DeliveryReport.SystemName, "uat"));
    }

    // ----- DeploymentActivity.Cut -----

    private const string Line = "deploying 2.4.44, 3 min";

    [Theory]
    [InlineData(40, Line)]
    [InlineData(23, Line)]
    [InlineData(22, "deploying 2.4.44, 3 m…")]
    [InlineData(10, "deploying…")]
    [InlineData(2, "d…")]
    [InlineData(1, "…")]
    public void ALineIsCutToWhatFitsWithAnEllipsis(int length, string expected) =>
        Assert.Equal(expected, DeploymentActivity.Cut(Line, length));

    [Fact]
    public void ALineThatFitsIsKeptWholeAndALineOfTheLengthItselfToo()
    {
        Assert.Equal(Line, DeploymentActivity.Cut(Line, Line.Length));
        Assert.Equal(Line, DeploymentActivity.Cut(Line, Line.Length + 1));
        Assert.NotEqual(Line, DeploymentActivity.Cut(Line, Line.Length - 1));
        Assert.Equal("d…", DeploymentActivity.Cut(Line, 2));
    }

    [Fact]
    public void ASpaceTheCutLeavesBehindIsNotLeftInFrontOfTheEllipsis()
    {
        // Six of nine characters, the sixth of them a space: the ellipsis takes the room the space had.
        Assert.Equal("abcd…", DeploymentActivity.Cut("abcd efgh", 6));
        Assert.Equal("2.4.43 waits for a…", DeploymentActivity.Cut("2.4.43 waits for a sign-off", 20));
    }

    [Fact]
    public void ANodeLineIsCutToTheLengthANodeHoldsAndWithOneCharacter()
    {
        const string Responsible = "demo approvers, Space Managers";

        var cut = DeploymentActivity.Cut($"2.4.43 waits for {Responsible}", DeploymentActivity.NodeLength);

        Assert.Equal("2.4.43 waits for demo approvers, Sp…", cut);
        Assert.Equal(DeploymentActivity.NodeLength, cut.Length);
        Assert.EndsWith("…", cut, StringComparison.Ordinal);
        Assert.DoesNotContain("...", cut, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1, "")]
    [InlineData(36, "")]
    public void NothingToSayIsCutIntoNothing(int length, string expected) => Assert.Equal(expected, DeploymentActivity.Cut(string.Empty, length));

    [Fact]
    public void ALineThatIsNotThereIsNotCutAtAll() => Assert.Throws<ArgumentNullException>(() => DeploymentActivity.Cut(null!, 10));

    [Fact]
    public void OneCharacterIsTheEllipsisAlone() => Assert.Equal("…", DeploymentActivity.Cut("abcdef", 1));

    [Fact]
    public void ALineGivenTheLengthItItselfHasIsKeptWhole() => Assert.Equal("abcdef", DeploymentActivity.Cut("abcdef", "abcdef".Length));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ALineThatHoldsNothingAtAllIsRefused(int length)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => DeploymentActivity.Cut("abc", length));

        Assert.Equal("length", error.ParamName);
        // A line that is empty is refused for the same reason: there is no length below 1 to cut it to.
        Assert.Throws<ArgumentOutOfRangeException>(() => DeploymentActivity.Cut(string.Empty, length));
    }

    [Fact]
    public void TheSpacesTheCutFallsOnAreTrimmedBeforeTheEllipsis() => Assert.Equal("abc…", DeploymentActivity.Cut("abc   def", 5));
}
