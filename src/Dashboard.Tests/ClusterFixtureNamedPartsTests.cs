namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterFixture.Node(string, bool, string[])" />, <see cref="ClusterFixture.Pod(string, string, int, string?, string?)" />
/// and the steps of <see cref="PodSteps" /> build their records positionally: the node's pool, size, zone and kubelet
/// version are four words in a row, the pod's two resources are two in a row - a swap of two of them compiles and
/// says nothing. Pinned here: what each factory builds, part for part, against the same record written out with
/// every argument named, so a swap inside the shared ObjectMother shows as the swap it is; each of the pod
/// factory's parameters, named by its caller, landing on the one part it stands for and nowhere else; and each of
/// the pod-level steps - the readiness and the start the factory no longer gates by a flag - landing on the one
/// part it sets and keeping the rest.
/// </summary>
public class ClusterFixtureNamedPartsTests
{
    private static readonly ResourceUse PodCpu = new(Usage: 10, Requests: 100, Limits: 500);
    private static readonly ResourceUse PodMemory = new(Usage: 100 * 1024 * 1024, Requests: 268435456, Limits: 536870912);

    /// <summary>The node the fixture builds is the node written out with every part named.</summary>
    [Fact]
    public void TheFixtureNodeIsTheNodeWithEveryPartNamed()
    {
        string[] pressures = ["MemoryPressure"];

        var node = ClusterFixture.Node("aks-user-0", false, pressures);

        var named = new ClusterNode(
            Name: "aks-user-0",
            Ready: false,
            Pressures: pressures,
            Unschedulable: false,
            Pool: "system",
            Size: "Standard_D4as_v6",
            Zone: null,
            KubeletVersion: "v1.33.3",
            CreatedAt: ClusterFixture.Generated.AddDays(-3),
            Cpu: new ResourceUse(Usage: 812, Requests: 2450, Limits: 9100, Allocatable: 3860),
            Memory: new ResourceUse(Usage: 9876543210, Requests: 6200000000, Limits: 11800000000, Allocatable: 13400000000),
            Pods: 63,
            PodCapacity: 110);
        Assert.Equal(named, node);
        Assert.Same(pressures, node.Pressures);
    }

    /// <summary>The node the fixture builds with no argument at all: the system node, ready and under no pressure.</summary>
    [Fact]
    public void TheDefaultFixtureNodeIsTheReadySystemNode()
    {
        var node = ClusterFixture.Node();

        Assert.Equal("aks-system-12148983-vmss000000", node.Name);
        Assert.True(node.Ready);
        Assert.Empty(node.Pressures);
        Assert.Equal("system", node.Pool);
        Assert.Equal("Standard_D4as_v6", node.Size);
        Assert.Null(node.Zone);
        Assert.Equal("v1.33.3", node.KubeletVersion);
        Assert.Equal(812, node.Cpu.Usage);
        Assert.Equal(9876543210, node.Memory.Usage);
        Assert.Equal(63, node.Pods);
        Assert.Equal(110, node.PodCapacity);
    }

    /// <summary>The pod the fixture builds from its workload alone is the pod written out with every part named:
    /// ready, its one container ready, and started an hour before the moment the facts are of.</summary>
    [Fact]
    public void TheDefaultFixturePodIsThePodWithEveryPartNamed()
    {
        var named = new ClusterPod(
            Name: "ui-0",
            Workload: "ui",
            Kind: "Deployment",
            Node: "aks-system-12148983-vmss000000",
            Phase: ClusterPod.Running,
            Ready: true,
            Containers: 1,
            ContainersReady: 1,
            Restarts: 0,
            Reason: null,
            StartedAt: ClusterFixture.Generated - TimeSpan.FromHours(1),
            Cpu: PodCpu,
            Memory: PodMemory);

        Assert.Equal(named, ClusterFixture.Pod("ui"));
    }

    /// <summary>Every parameter named by its caller lands on the one part it stands for: the pod built with all of
    /// them - and not ready, the readiness the steps set rather than a flag the factory gates - is the pod written
    /// out with every part named.</summary>
    [Fact]
    public void EveryNamedOptionOfTheFixturePodLandsOnItsOwnPart()
    {
        var pod = ClusterFixture.Pod(
            workload: "db",
            phase: ClusterPod.Failed,
            restarts: 7,
            reason: "CrashLoopBackOff",
            name: "db-7f6b5c4d3-m8n2p").NotReady();

        var named = new ClusterPod(
            Name: "db-7f6b5c4d3-m8n2p",
            Workload: "db",
            Kind: "Deployment",
            Node: "aks-system-12148983-vmss000000",
            Phase: ClusterPod.Failed,
            Ready: false,
            Containers: 1,
            ContainersReady: 0,
            Restarts: 7,
            Reason: "CrashLoopBackOff",
            StartedAt: ClusterFixture.Generated - TimeSpan.FromHours(1),
            Cpu: PodCpu,
            Memory: PodMemory);
        Assert.Equal(named, pod);
    }

    /// <summary>Each of the pod-level steps lands on the one part it sets and keeps the rest: the readiness the
    /// containers stand in for, the start counted back from the moment the facts are of, and the moment said
    /// outright - the pod's name, phase and restarts standing where they stood before the step.</summary>
    [Fact]
    public void EachStepOfTheFixturePodLandsOnTheOnePartItSets()
    {
        var ready = ClusterFixture.Pod("ui").Ready();
        var notReady = ClusterFixture.Pod("ui").NotReady();
        var started = ClusterFixture.Pod("ui").Started(TimeSpan.FromMinutes(5));
        var startedAt = ClusterFixture.Pod("ui").StartedAt(DateTimeOffset.MinValue);

        Assert.True(ready.Ready);
        Assert.Equal(1, ready.ContainersReady);
        Assert.False(notReady.Ready);
        Assert.Equal(0, notReady.ContainersReady);
        Assert.Equal(ClusterFixture.Generated - TimeSpan.FromMinutes(5), started.StartedAt);
        Assert.Equal(DateTimeOffset.MinValue, startedAt.StartedAt);

        Assert.Equal("ui-0", notReady.Name);
        Assert.Equal(ClusterPod.Running, started.Phase);
        Assert.Equal(0, startedAt.Restarts);
        Assert.Equal(PodCpu, ready.Cpu);
        Assert.Equal(PodMemory, notReady.Memory);
    }

    /// <summary>A pod told no start moment says no moment, whatever age it was started with: the step that says
    /// nothing is the one that stands, and no age reaches a pod whose start is the nothing it is.</summary>
    [Fact]
    public void AFixturePodToldNoStartSaysNoMomentWhateverItsAge() =>
        Assert.Null(ClusterFixture.Pod("ui").Started(TimeSpan.FromDays(2)).StartedAt(null).StartedAt);

    /// <summary>The kind and the node are the two words the fixture writes itself, and neither is the workload or the
    /// name: a swap of the three words in a row would put one where the other stands.</summary>
    [Fact]
    public void TheFixturePodsThreeWordsInARowStandApart()
    {
        var pod = ClusterFixture.Pod("ui", name: "ui-1");

        Assert.Equal("ui-1", pod.Name);
        Assert.Equal("ui", pod.Workload);
        Assert.Equal("Deployment", pod.Kind);
        Assert.Equal("aks-system-12148983-vmss000000", pod.Node);
        Assert.Equal(100, pod.Cpu.Requests);
        Assert.Equal(268435456, pod.Memory.Requests);
    }
}
