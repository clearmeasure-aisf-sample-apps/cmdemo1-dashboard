namespace Dashboard.Tests;

/// <summary>
/// A pod's reason is joined by the cluster's one rule (#1433): the spaces around it and the full stops it ends in
/// are not said, wherever it is said - the restart event, the row's words, the problem line and the unhealthy event.
/// </summary>
public class ClusterReasonJoiningTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;
    private static readonly DateTimeOffset Now = Generated.AddSeconds(5);

    private static readonly EnvironmentInfo[] Environments = [ClusterFixture.Environment("tdd", "cmdemo3-tdd")];

    private static IReadOnlyList<DashboardEvent> Between(ClusterStatus before, ClusterStatus after) =>
        ClusterEventDetector.Status(before, after, Environments, Now);

    private static ClusterStatus With(params ClusterPod[] pods) => ClusterFixture.Status(null, ClusterFixture.Space("cmdemo3-tdd", pods));

    [Theory]
    [InlineData("CrashLoopBackOff. ")]
    [InlineData(" CrashLoopBackOff.")]
    [InlineData("CrashLoopBackOff..")]
    public void ARestartSaysAReasonTrimmed(string reason)
    {
        var before = With(ClusterFixture.Pod("ui", ready: false, restarts: 6, reason: reason));
        var after = With(ClusterFixture.Pod("ui", ready: false, restarts: 7, reason: reason));

        Assert.Equal(
            [new DashboardEvent(Now, EventKind.Cluster, EventLevel.Warning, "tdd", "cluster", "ui in cmdemo3-tdd restarted (7 restarts): CrashLoopBackOff")],
            Between(before, after));
    }

    [Fact]
    public void ARestartWithAReasonOfOnlySpacesSaysNoReason()
    {
        var before = With(ClusterFixture.Pod("ui", ready: false, restarts: 6, reason: "  "));
        var after = With(ClusterFixture.Pod("ui", ready: false, restarts: 7, reason: "  "));

        Assert.Equal(
            [new DashboardEvent(Now, EventKind.Cluster, EventLevel.Warning, "tdd", "cluster", "ui in cmdemo3-tdd restarted (7 restarts)")],
            Between(before, after));
    }

    [Fact]
    public void AnUnhealthyPodSaysItsReasonTrimmed()
    {
        var ready = With(ClusterFixture.Pod("ui"));
        var failing = With(ClusterFixture.Pod("ui", ready: false, reason: "ImagePullBackOff. "));

        Assert.Equal(
            [new DashboardEvent(Now, EventKind.Cluster, EventLevel.Warning, "tdd", "cluster", "ui in cmdemo3-tdd is unhealthy: ImagePullBackOff")],
            Between(ready, failing));
    }

    [Fact]
    public void ThePodsWordsSayTheReasonTrimmed()
    {
        Assert.Equal(
            "CrashLoopBackOff",
            ClusterText.PodWords(ClusterFixture.Pod("ui", ready: false, restarts: 7, reason: "CrashLoopBackOff. "), PodState.Unhealthy, Generated));
        Assert.Equal(
            "Starting: ContainerCreating",
            ClusterText.PodWords(ClusterFixture.Pod("ui", ready: false, reason: " ContainerCreating."), PodState.Starting, Generated));
        Assert.Equal(
            "Starting",
            ClusterText.PodWords(ClusterFixture.Pod("ui", ready: false, reason: " . "), PodState.Starting, Generated));
        Assert.Equal(
            "Pending for 90 min: Unschedulable",
            ClusterText.PodWords(ClusterFixture.Pod("new", ClusterPod.Pending, false, reason: "Unschedulable. ", age: TimeSpan.FromMinutes(90)), PodState.Unhealthy, Generated));
        Assert.Equal(
            "Evicted",
            ClusterText.PodWords(ClusterFixture.Pod("ui", ClusterPod.Failed, false, reason: "Evicted."), PodState.Unhealthy, Generated));
    }

    [Fact]
    public void TheProblemLineSaysTheReasonTrimmed()
    {
        Assert.Equal(
            "ui in cmdemo3-tdd: CrashLoopBackOff, 7 restarts",
            ClusterAssessment.PodProblem(ClusterFixture.Pod("ui", ready: false, restarts: 7, reason: "CrashLoopBackOff. "), "cmdemo3-tdd", Generated));
    }
}
