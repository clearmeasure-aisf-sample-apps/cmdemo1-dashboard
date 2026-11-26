namespace Dashboard.Tests;

/// <summary>
/// A <see cref="NamespaceGroup"/> is one namespace as the view shows it: its <see cref="PodRow"/>s in the order of the
/// table and its counts. The groups come from <see cref="ClusterGroups.Of"/>: one per environment's namespace, in the
/// topology's order, and one per every other namespace, by name.
/// </summary>
public class NamespaceGroupTests
{
    private static readonly DateTimeOffset Generated = ClusterFixture.Generated;

    [Fact]
    public void EachNamespaceBecomesOneGroupThatHoldsOnlyItsOwnPodsAndItsVolumes()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space("cmdemo3-prod", ClusterFixture.Pod("ui"), ClusterFixture.Pod("db")),
            new ClusterNamespace("kube-system", [ClusterFixture.Pod("coredns")], [new ClusterVolume("data", 8589934592d, "Bound")]),
            ClusterFixture.Space("argocd", ClusterFixture.Pod("server")));

        var groups = ClusterGroups.Of(status, [ClusterFixture.Environment("prod", "cmdemo3-prod", "prod")], Generated);

        var prod = Assert.Single(groups.Environments);
        Assert.Equal("cmdemo3-prod", prod.Name);
        Assert.Equal(("prod", "prod"), (prod.Environment!.Name, prod.Environment.Tier));
        Assert.True(prod.Found);
        Assert.Equal(["db-0", "ui-0"], prod.Pods.Select(row => row.Pod.Name));
        Assert.All(prod.Pods, row => Assert.Equal(PodState.Ready, row.State));
        Assert.Empty(prod.Volumes);

        Assert.Equal(2, groups.Platform.Count);
        var (argocd, kubeSystem) = (groups.Platform[0], groups.Platform[1]);
        Assert.All(groups.Platform, group => Assert.Null(group.Environment));
        Assert.All(groups.Platform, group => Assert.True(group.Found));
        Assert.Equal("argocd", argocd.Name);
        Assert.Equal(["server-0"], argocd.Pods.Select(row => row.Pod.Name));
        Assert.Empty(argocd.Volumes);
        Assert.Equal("kube-system", kubeSystem.Name);
        Assert.Equal(["coredns-0"], kubeSystem.Pods.Select(row => row.Pod.Name));
        Assert.Equal(new ClusterVolume("data", 8589934592d, "Bound"), Assert.Single(kubeSystem.Volumes));
    }

    [Fact]
    public void TheGroupsComeInTheOrderOfTheTopologyForTheEnvironmentsAndByNameForThePlatform()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space("zeta-prod", ClusterFixture.Pod("ui")),
            ClusterFixture.Space("alpha-tdd", ClusterFixture.Pod("ui")),
            ClusterFixture.Space("m-tool", ClusterFixture.Pod("m")),
            ClusterFixture.Space("A-tool", ClusterFixture.Pod("a")));

        var groups = ClusterGroups.Of(
            status,
            [
                ClusterFixture.Environment("prod", "zeta-prod", "prod"),
                ClusterFixture.Environment("tdd", "alpha-tdd"),
            ],
            Generated);

        Assert.Equal(["zeta-prod", "alpha-tdd"], groups.Environments.Select(group => group.Name));
        Assert.Equal(["prod", "tdd"], groups.Environments.Select(group => group.Environment!.Name));
        Assert.Equal(["A-tool", "m-tool"], groups.Platform.Select(group => group.Name));
    }

    [Fact]
    public void ThePodRowsOfAGroupComeUnhealthyFirstThenStartingThenReadyThenFinishedAndEachStateCounts()
    {
        var status = ClusterFixture.Status(
            null,
            ClusterFixture.Space(
                "apps",
                ClusterFixture.Pod("web"),
                ClusterFixture.Pod("backup", ClusterPod.Succeeded, reason: "Completed").NotReady(),
                ClusterFixture.Pod("new").NotReady().Started(TimeSpan.FromSeconds(30)),
                ClusterFixture.Pod("api", name: "api-b"),
                ClusterFixture.Pod("api", name: "api-a"),
                ClusterFixture.Pod("broken", restarts: 4, reason: "CrashLoopBackOff").NotReady(),
                ClusterFixture.Pod("late", ClusterPod.Pending).NotReady().Started(TimeSpan.FromMinutes(10)),
                ClusterFixture.Pod("Zeta")));

        var group = Assert.Single(ClusterGroups.Of(status, [], Generated).Platform);

        Assert.Equal(["broken-0", "late-0", "new-0", "api-a", "api-b", "web-0", "Zeta-0", "backup-0"], group.Pods.Select(row => row.Pod.Name));
        Assert.Equal(
            [PodState.Unhealthy, PodState.Unhealthy, PodState.Starting, PodState.Ready, PodState.Ready, PodState.Ready, PodState.Ready, PodState.Finished],
            group.Pods.Select(row => row.State));
        Assert.Equal((4, 7, 1, 2, 4), (group.Ready, group.Total, group.Finished, group.Unhealthy, group.Restarts));
        Assert.Equal(8 * 10d, group.CpuUsage);
        Assert.Equal(8 * 104857600d, group.MemoryUsage);
    }

    [Fact]
    public void TheGroupCountsItsRowsByStateAndSumsTheirRestartsAndUsageLeavingOutWhatAPodDoesNotReport()
    {
        var group = new NamespaceGroup(
            "apps",
            null,
            true,
            [
                new PodRow(ClusterFixture.Pod("web"), PodState.Ready),
                new PodRow(ClusterFixture.Pod("api", restarts: 2) with { Cpu = new ResourceUse(25, 100, 500) }, PodState.Ready),
                new PodRow(ClusterFixture.Pod("new", restarts: 1).NotReady().Started(TimeSpan.FromSeconds(30)) with { Cpu = new ResourceUse(5, 100, 500) }, PodState.Starting),
                new PodRow(ClusterFixture.Pod("broken", restarts: 4).NotReady() with { Cpu = ResourceUse.None }, PodState.Unhealthy),
                new PodRow(ClusterFixture.Pod("backup", ClusterPod.Succeeded).NotReady() with { Cpu = new ResourceUse(0.5, 0, null) }, PodState.Finished),
            ],
            []);

        Assert.Equal((2, 4, 1, 1), (group.Ready, group.Total, group.Finished, group.Unhealthy));
        Assert.Equal(7, group.Restarts);
        Assert.Equal(40.5, group.CpuUsage);
        Assert.Equal(5 * 104857600d, group.MemoryUsage);
    }

    [Fact]
    public void AGroupWhosePodsReportNoUsageHasNoSumOfIt()
    {
        var group = new NamespaceGroup(
            "jobs",
            null,
            true,
            [new PodRow(ClusterFixture.Pod("backup", ClusterPod.Succeeded).NotReady() with { Cpu = ResourceUse.None, Memory = ResourceUse.None }, PodState.Finished)],
            []);

        Assert.Equal((0, 0, 1), (group.Ready, group.Total, group.Finished));
        Assert.Null(group.CpuUsage);
        Assert.Null(group.MemoryUsage);
    }

    [Fact]
    public void ThePlatformNeedsALookOnlyWhenOneOfItsGroupsHasAnUnhealthyPod()
    {
        var readyAndFinished = ClusterGroups.Of(
            ClusterFixture.Status(
                null,
                ClusterFixture.Space("a", ClusterFixture.Pod("web")),
                ClusterFixture.Space("b", ClusterFixture.Pod("job", ClusterPod.Succeeded, reason: "Completed").NotReady())),
            [],
            Generated);
        var starting = ClusterGroups.Of(
            ClusterFixture.Status(null, ClusterFixture.Space("a", ClusterFixture.Pod("new").NotReady().Started(TimeSpan.FromSeconds(30)))),
            [],
            Generated);
        var broken = ClusterGroups.Of(
            ClusterFixture.Status(null, ClusterFixture.Space("a", ClusterFixture.Pod("broken", restarts: 3, reason: "CrashLoopBackOff").NotReady())),
            [],
            Generated);

        Assert.Equal(0, readyAndFinished.Platform.Sum(space => space.Unhealthy));
        Assert.Equal(0, starting.Platform.Sum(space => space.Unhealthy));
        Assert.Equal(1, broken.Platform.Sum(space => space.Unhealthy));
        Assert.False(readyAndFinished.PlatformNeedsALook);
        Assert.False(starting.PlatformNeedsALook);
        Assert.True(broken.PlatformNeedsALook);
    }

    [Fact]
    public void AnEmptyStatusHasNoGroupsAndAGroupWithoutPodsCountsNothing()
    {
        var groups = ClusterGroups.Of(ClusterFixture.Status(), [], Generated);

        Assert.Empty(groups.Environments);
        Assert.Empty(groups.Platform);
        Assert.False(groups.PlatformNeedsALook);

        var empty = new NamespaceGroup("apps", null, true, [], []);
        Assert.Equal((0, 0, 0, 0, 0), (empty.Ready, empty.Total, empty.Finished, empty.Unhealthy, empty.Restarts));
        Assert.Null(empty.CpuUsage);
        Assert.Null(empty.MemoryUsage);
    }

    [Fact]
    public void ANEnvironmentWhoseNamespaceTheEmptyStatusDoesNotListIsNotFoundAndCountsNothing()
    {
        var groups = ClusterGroups.Of(ClusterFixture.Status(), [ClusterFixture.Environment("tdd", "cmdemo3-tdd")], Generated);

        var missing = Assert.Single(groups.Environments);
        Assert.Equal("cmdemo3-tdd", missing.Name);
        Assert.Equal("tdd", missing.Environment!.Name);
        Assert.False(missing.Found);
        Assert.Empty(missing.Pods);
        Assert.Empty(missing.Volumes);
        Assert.Equal((0, 0, 0, 0, 0), (missing.Ready, missing.Total, missing.Finished, missing.Unhealthy, missing.Restarts));
        Assert.Null(missing.CpuUsage);
        Assert.Null(missing.MemoryUsage);
        Assert.Empty(groups.Platform);
    }
}
