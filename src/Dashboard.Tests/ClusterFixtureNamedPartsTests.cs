namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterFixture.Node(string, bool, string[])" /> and
/// <see cref="ClusterFixture.Pod(string, string, bool, int, string?, TimeSpan?, string?, bool)" /> build their records
/// positionally: the node's pool, size, zone and kubelet version are four words in a row, the pod's workload, kind and
/// node three, the pod's two container counts two numbers in a row, and both records' cpu and memory two resources in
/// a row - a swap of two of them compiles and says nothing. Pinned here: what each factory builds, part for part,
/// against the same record written out with every argument named, so a swap inside the shared ObjectMother shows as
/// the swap it is; and each of the pod factory's optional parameters, named by its caller, landing on the one part it
/// stands for and nowhere else.
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

    /// <summary>The pod the fixture builds from its workload alone is the pod written out with every part named.</summary>
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

    /// <summary>Every optional parameter named by its caller lands on the one part it stands for: the pod built with
    /// all of them is the pod written out with every part named.</summary>
    [Fact]
    public void EveryNamedOptionOfTheFixturePodLandsOnItsOwnPart()
    {
        var pod = ClusterFixture.Pod(
            workload: "db",
            phase: ClusterPod.Failed,
            ready: false,
            restarts: 7,
            reason: "CrashLoopBackOff",
            age: TimeSpan.FromMinutes(5),
            name: "db-7f6b5c4d3-m8n2p",
            started: true);

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
            StartedAt: ClusterFixture.Generated - TimeSpan.FromMinutes(5),
            Cpu: PodCpu,
            Memory: PodMemory);
        Assert.Equal(named, pod);
    }

    /// <summary>A pod that was not started says no moment, whatever age it is handed.</summary>
    [Fact]
    public void AFixturePodNotStartedSaysNoMomentWhateverItsAge() =>
        Assert.Null(ClusterFixture.Pod("ui", age: TimeSpan.FromDays(2), started: false).StartedAt);

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

    /// <summary>The fixture's least file names an empty list of nodes and nothing else: no namespaces, no moment, no
    /// interval and no version, so a test that reads it starts from nothing but the list.</summary>
    [Fact]
    public void TheFileWithNoNodesReadsAsAStatusOfNothing()
    {
        var status = ClusterFixture.Read(ClusterFixture.NoNodes);

        Assert.Empty(status.Nodes);
        Assert.Empty(status.Namespaces);
        Assert.Null(status.Generated);
        Assert.Null(status.IntervalSeconds);
        Assert.Null(status.KubernetesVersion);
    }

    /// <summary>The fixture's file of the three facts says the fixture's own moment, the fifteen-second interval and the
    /// version the fixture's status is built with, over an empty list of nodes.</summary>
    [Fact]
    public void TheFileWithTheThreeFactsReadsAsTheFixturesThreeFacts()
    {
        var status = ClusterFixture.Read(ClusterFixture.FileWithTheThreeFacts);

        Assert.Equal(ClusterFixture.Generated, status.Generated);
        Assert.Equal(15, status.IntervalSeconds);
        Assert.Equal("v1.33.3", status.KubernetesVersion);
        Assert.Empty(status.Nodes);
        Assert.Empty(status.Namespaces);
    }

    /// <summary>A file the reader refuses fails the fixture's read, and the failure a person reads carries the
    /// reader's own problem, word for word.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("{ not json")]
    [InlineData("""{ "generated": "2026-10-06T20:15:30Z" }""")]
    public void AFileTheReaderRefusesFailsTheReadWithTheProblemToRead(string json)
    {
        var problem = ClusterStatus.Parse(json).Problem;

        Assert.NotNull(problem);

        var failure = Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => ClusterFixture.Read(json));

        Assert.Contains(problem, failure.Message, StringComparison.Ordinal);
    }
}
