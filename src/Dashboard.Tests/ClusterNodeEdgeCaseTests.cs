namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterNode" />: the parts the node keeps of
/// what it is given, the parts it falls apart into, the words it says its parts in, and the parts that are
/// one and the parts that are not. <see cref="ParsedEdgeCaseTests" /> pins <see cref="ClusterNode.IsHealthy" />
/// at every corner of the ready and the pressure and the cordon that does not make it otherwise, and the
/// readiness, the pressure and the cordon that turn two nodes apart; <see cref="ClusterParserTests" /> pins
/// the sample file's node at the values it stands on, and the node the parser builds where the file does not
/// say; <see cref="ClusterGroupsTests" />, <see cref="ClusterTotalsEdgeCaseTests" /> and
/// <see cref="ClusterSampleEdgeCaseTests" /> pin the sums and the samples the node's parts make.
/// <para>
/// What is pinned here is what they leave open, and only that: the node keeps the very parts it is given -
/// the words as the words, the moment as the moment, the numbers at the ends of the number they stand in and
/// the zero that is a number, the lists as the very lists they are - and the nothing it is given stands the
/// nothing it is, and the empty word it is given stands the empty word it is, not the nothing it is not, and
/// the spaces about a word are kept as the spaces they are; the thirteen parts it falls apart into, each as
/// the value it stands on; the words the record says its parts in, the nothing and the empty word said the
/// way it says them; the copy that turns one of the parts it keeps and keeps the rest it does not; and the
/// parts that turn two nodes apart - the name, the pool, the size, the zone, the kubelet, the created
/// moment, the cpu, the memory, the pods and the pod capacity - and the pressures the list the node keeps:
/// the same list the nodes keep is one, and the different list is another node while its words are the same
/// - the word lost, the order turned about, the list made empty, and the empty word kept.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class ClusterNodeEdgeCaseTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 3, 14, 2, 11, TimeSpan.Zero);

    // ----- The parts the node keeps -----

    /// <summary>
    /// The node keeps the very parts it is given: the words as the words, the moment as the moment, the
    /// numbers as the numbers, and the lists as the very lists it was given - the cpu, the memory and the
    /// pressures the same instances, not copies of them - the order of the pressures the order it was given.
    /// </summary>
    [Fact]
    public void ANodeKeepsTheVeryPartsItIsGiven()
    {
        IReadOnlyList<string> pressures = ["MemoryPressure", "DiskPressure"];
        var cpu = new ResourceUse(812, 2450, 9100, 3860);
        var memory = new ResourceUse(9876543210, 6200000000, 11800000000, 13400000000);
        var node = new ClusterNode(
            "aks-system-12148983-vmss000000",
            false,
            pressures,
            true,
            "system",
            "Standard_D4as_v6",
            "2",
            "v1.33.3",
            CreatedAt,
            cpu,
            memory,
            63,
            110);

        Assert.Equal("aks-system-12148983-vmss000000", node.Name);
        Assert.False(node.Ready);
        Assert.Same(pressures, node.Pressures);
        Assert.Equal(["MemoryPressure", "DiskPressure"], node.Pressures);
        Assert.True(node.Unschedulable);
        Assert.Equal("system", node.Pool);
        Assert.Equal("Standard_D4as_v6", node.Size);
        Assert.Equal("2", node.Zone);
        Assert.Equal("v1.33.3", node.KubeletVersion);
        Assert.Equal(CreatedAt, node.CreatedAt);
        Assert.Same(cpu, node.Cpu);
        Assert.Same(memory, node.Memory);
        Assert.Equal(63, node.Pods);
        Assert.Equal(110, node.PodCapacity);
    }

    /// <summary>
    /// The node keeps the nothing it is given as the nothing it is: the pool, the size, the zone, the
    /// kubelet, the created moment, the pods and the pod capacity stand the nothing they are given, the
    /// pressures stand the empty list, and the cpu and the memory stand the resource that says no number at
    /// all.
    /// </summary>
    [Fact]
    public void ANodeKeepsTheNothingItIsGiven()
    {
        var node = new ClusterNode("node-1", false, [], false, null, null, null, null, null, ResourceUse.None, ResourceUse.None, null, null);

        Assert.Equal("node-1", node.Name);
        Assert.False(node.Ready);
        Assert.Empty(node.Pressures);
        Assert.False(node.Unschedulable);
        Assert.Null(node.Pool);
        Assert.Null(node.Size);
        Assert.Null(node.Zone);
        Assert.Null(node.KubeletVersion);
        Assert.Null(node.CreatedAt);
        Assert.Equal(ResourceUse.None, node.Cpu);
        Assert.Equal(ResourceUse.None, node.Memory);
        Assert.Null(node.Pods);
        Assert.Null(node.PodCapacity);
    }

    /// <summary>
    /// The empty word the node is given stands the empty word it is: the name, the pool, the size, the zone
    /// and the kubelet that are the empty word keep it - the node is not the parser, and does not drop what
    /// is empty and does not trim what has spaces about it.
    /// </summary>
    [Fact]
    public void AnEmptyWordStandsTheEmptyWordItIsAndTheSpacesAboutAWordAreKept()
    {
        var empty = new ClusterNode("", false, [], false, "", "", "", "", CreatedAt, ResourceUse.None, ResourceUse.None, 0, 0);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Pool);
        Assert.Equal(string.Empty, empty.Size);
        Assert.Equal(string.Empty, empty.Zone);
        Assert.Equal(string.Empty, empty.KubeletVersion);
        Assert.Equal(0, empty.Pods);
        Assert.Equal(0, empty.PodCapacity);

        var padded = ClusterFixture.Node() with { Name = "  node-1  ", Zone = "  " };
        Assert.Equal("  node-1  ", padded.Name);
        Assert.Equal("  ", padded.Zone);
    }

    /// <summary>
    /// The pods and the pod capacity stand where they are given, at the two ends of the number the number
    /// stands in and at the zero that is a number: the most pods a node can say the top, the least it can
    /// say the bottom, and the zero that is a number kept as the number it is, not the nothing it is not.
    /// </summary>
    [Theory]
    [InlineData(int.MinValue, int.MinValue)]
    [InlineData(0, 0)]
    [InlineData(63, 110)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void ThePodsAndThePodCapacityStandWhereTheyAreGiven(int pods, int podCapacity)
    {
        var node = ClusterFixture.Node() with { Pods = pods, PodCapacity = podCapacity };

        Assert.Equal(pods, node.Pods);
        Assert.Equal(podCapacity, node.PodCapacity);
    }

    /// <summary>
    /// The created moment stands where it is given, at the two ends the moment stands in: the oldest moment
    /// a node can say and the newest it can say, kept as the moments they are, beside the moment the test
    /// brings.
    /// </summary>
    [Fact]
    public void TheCreatedAtStandsWhereItIsGivenAtTheEndsTheMomentStandsIn()
    {
        var oldest = ClusterFixture.Node() with { CreatedAt = DateTimeOffset.MinValue };
        var newest = ClusterFixture.Node() with { CreatedAt = DateTimeOffset.MaxValue };
        var at = ClusterFixture.Node() with { CreatedAt = CreatedAt };

        Assert.Equal(DateTimeOffset.MinValue, oldest.CreatedAt);
        Assert.Equal(DateTimeOffset.MaxValue, newest.CreatedAt);
        Assert.Equal(CreatedAt, at.CreatedAt);
    }

    // ----- The parts the node falls apart into -----

    /// <summary>
    /// The node falls apart into its thirteen parts, each as the value it stands on, in the order the node
    /// names them - the words and the moments or the nothing, the lists as the very lists they are - and
    /// the parts the node was not given come out the nothing they are.
    /// </summary>
    [Fact]
    public void ANodeFallsApartIntoItsThirteenParts()
    {
        IReadOnlyList<string> pressures = ["MemoryPressure", "DiskPressure"];
        var cpu = new ResourceUse(812, 2450, 9100, 3860);
        var memory = new ResourceUse(9876543210, 6200000000, 11800000000, 13400000000);
        var node = new ClusterNode(
            "node-1",
            false,
            pressures,
            true,
            "system",
            "Standard_D4as_v6",
            "2",
            "v1.33.3",
            CreatedAt,
            cpu,
            memory,
            63,
            110);

        var (name, ready, nodePressures, unschedulable, pool, size, zone, kubeletVersion, createdAt, nodeCpu, nodeMemory, pods, podCapacity) = node;

        Assert.Equal("node-1", name);
        Assert.False(ready);
        Assert.Same(pressures, nodePressures);
        Assert.True(unschedulable);
        Assert.Equal("system", pool);
        Assert.Equal("Standard_D4as_v6", size);
        Assert.Equal("2", zone);
        Assert.Equal("v1.33.3", kubeletVersion);
        Assert.Equal(CreatedAt, createdAt);
        Assert.Same(cpu, nodeCpu);
        Assert.Same(memory, nodeMemory);
        Assert.Equal(63, pods);
        Assert.Equal(110, podCapacity);

        var (noName, noReady, noPressures, noUnschedulable, noPool, noSize, noZone, noKubeletVersion, noCreatedAt, noCpu, noMemory, noPods, noPodCapacity) =
            new ClusterNode("node-2", true, [], false, null, null, null, null, null, ResourceUse.None, ResourceUse.None, null, null);

        Assert.Equal("node-2", noName);
        Assert.True(noReady);
        Assert.Empty(noPressures);
        Assert.False(noUnschedulable);
        Assert.Null(noPool);
        Assert.Null(noSize);
        Assert.Null(noZone);
        Assert.Null(noKubeletVersion);
        Assert.Null(noCreatedAt);
        Assert.Equal(ResourceUse.None, noCpu);
        Assert.Equal(ResourceUse.None, noMemory);
        Assert.Null(noPods);
        Assert.Null(noPodCapacity);
    }

    // ----- The words the node says its parts in -----

    /// <summary>
    /// The words the node says its parts in: the record's words, in the order the parts are named, the
    /// node's own name at the front, each part said as the word it stands on - the words and the numbers as
    /// the words and the numbers they are, the nested resource as the words it stands on, the list the node
    /// was given as the list it is, the health the parts make at the end, and the nothing and the empty word
    /// the way it says them: a size that is the nothing is said as a size that is the empty word, and the
    /// words it says them with are the same.
    /// </summary>
    [Fact]
    public void ANodeSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        var node = new ClusterNode(
            "node-1",
            false,
            [],
            true,
            "system",
            null,
            string.Empty,
            "v1.33.3",
            null,
            new ResourceUse(10, 100, 500, 2000),
            new ResourceUse(null, null, null, null),
            63,
            null);

        Assert.Equal(
            "ClusterNode { Name = node-1, Ready = False, Pressures = System.String[], Unschedulable = True, Pool = system, Size = , Zone = , KubeletVersion = v1.33.3, CreatedAt = , Cpu = ResourceUse { Usage = 10, Requests = 100, Limits = 500, Allocatable = 2000 }, Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Pods = 63, PodCapacity = , IsHealthy = False }",
            node.ToString());

        var withEmptySize = node with { Size = string.Empty };
        Assert.Equal(node.ToString(), withEmptySize.ToString());
    }

    // ----- The copies -----

    /// <summary>
    /// The copy of a node that turns one of the parts it keeps keeps the rest: the name turned alone keeps
    /// the twelve parts it does not turn, and the pool, the size, the zone, the kubelet, the created
    /// moment, the cpu, the memory, the pods and the pod capacity each turned is turned alone - the parts
    /// the copy does not turn stand as they stood, and the copy is not the node it is the copy of.
    /// </summary>
    [Fact]
    public void ACopyThatTurnsOneOfThePartsItKeepsKeepsTheRest()
    {
        var node = ClusterFixture.Node();

        var renamed = node with { Name = "node-2" };
        Assert.Equal("node-2", renamed.Name);
        Assert.Equal(node.Ready, renamed.Ready);
        Assert.Equal(node.Pressures, renamed.Pressures);
        Assert.Equal(node.Unschedulable, renamed.Unschedulable);
        Assert.Equal(node.Pool, renamed.Pool);
        Assert.Equal(node.Size, renamed.Size);
        Assert.Equal(node.Zone, renamed.Zone);
        Assert.Equal(node.KubeletVersion, renamed.KubeletVersion);
        Assert.Equal(node.CreatedAt, renamed.CreatedAt);
        Assert.Equal(node.Cpu, renamed.Cpu);
        Assert.Equal(node.Memory, renamed.Memory);
        Assert.Equal(node.Pods, renamed.Pods);
        Assert.Equal(node.PodCapacity, renamed.PodCapacity);
        Assert.NotSame(node, renamed);

        var noPool = node with { Pool = null };
        Assert.Null(noPool.Pool);
        Assert.Equal(node.Name, noPool.Name);
        Assert.Equal(node.Size, noPool.Size);

        var noSize = node with { Size = null };
        Assert.Null(noSize.Size);
        Assert.Equal(noSize.Pool, node.Pool);
        Assert.Equal(noSize.Zone, node.Zone);

        var noZone = node with { Zone = null };
        Assert.Null(noZone.Zone);
        Assert.Equal(noZone.KubeletVersion, node.KubeletVersion);

        var noKubelet = node with { KubeletVersion = null };
        Assert.Null(noKubelet.KubeletVersion);
        Assert.Equal(noKubelet.Size, node.Size);

        var noCreatedAt = node with { CreatedAt = null };
        Assert.Null(noCreatedAt.CreatedAt);
        Assert.Equal(noCreatedAt.KubeletVersion, node.KubeletVersion);

        var noCpu = node with { Cpu = ResourceUse.None };
        Assert.Equal(ResourceUse.None, noCpu.Cpu);
        Assert.Equal(noCpu.Memory, node.Memory);

        var noMemory = node with { Memory = ResourceUse.None };
        Assert.Equal(ResourceUse.None, noMemory.Memory);
        Assert.Equal(noMemory.Cpu, node.Cpu);

        var noCounts = node with { Pods = null, PodCapacity = null };
        Assert.Null(noCounts.Pods);
        Assert.Null(noCounts.PodCapacity);
        Assert.Equal(noCounts.Memory, node.Memory);
    }

    // ----- The nodes that are one and the nodes that are not -----

    /// <summary>
    /// The parts that turn two nodes apart: the name, the pool, the size, the zone, the kubelet, the
    /// created moment, the cpu, the memory, the pods and the pod capacity each turned is another node - the
    /// readiness, the pressure and the cordon the other tests pin - and the node asked of as an object is
    /// the one its parts make it, and the nothing it is not asked of is not it, and the nodes that are one
    /// are not the one reference they are one with.
    /// </summary>
    [Fact]
    public void APartTurnedIsAnotherNodeAndTheEqualsAskedOfItFollows()
    {
        var one = ClusterFixture.Node();
        var two = ClusterFixture.Node();

        Assert.True(one.Equals(two as object));
        Assert.False(one.Equals(null));
        Assert.False(ReferenceEquals(one, two));
        Assert.True(one != one with { Name = "node-2" });

        Assert.NotEqual(one, one with { Name = "node-2" });
        Assert.NotEqual(one, one with { Pool = null });
        Assert.NotEqual(one, one with { Pool = "apps" });
        Assert.NotEqual(one, one with { Size = null });
        Assert.NotEqual(one, one with { Zone = "2" });
        Assert.NotEqual(one, one with { KubeletVersion = "v9.9.9" });
        Assert.NotEqual(one, one with { KubeletVersion = null });
        Assert.NotEqual(one, one with { CreatedAt = null });
        Assert.NotEqual(one, one with { Cpu = ResourceUse.None });
        Assert.NotEqual(one, one with { Memory = ResourceUse.None });
        Assert.NotEqual(one, one with { Pods = 64 });
        Assert.NotEqual(one, one with { PodCapacity = null });
    }

    /// <summary>
    /// The pressures are the list the node keeps: the two nodes that keep the same list are one, and the
    /// hash code that follows them, while the different list is another node while its words are the same -
    /// the word lost, the order turned about, the list made empty, and the empty word kept, each a list the
    /// node is not given, and a node the node is not one with.
    /// </summary>
    [Fact]
    public void ThePressuresAreTheListTheNodeKeepsAndADifferentListIsAnotherNode()
    {
        string[] same = ["MemoryPressure", "DiskPressure"];
        var one = ClusterFixture.Node() with { Pressures = same };
        var withTheSameList = ClusterFixture.Node() with { Pressures = same };

        Assert.Same(same, one.Pressures);
        Assert.Same(same, withTheSameList.Pressures);
        Assert.Equal(one, withTheSameList);
        Assert.Equal(one.GetHashCode(), withTheSameList.GetHashCode());

        Assert.NotEqual(one, one with { Pressures = ["MemoryPressure", "DiskPressure"] });
        Assert.NotEqual(one, one with { Pressures = ["MemoryPressure"] });
        Assert.NotEqual(one, one with { Pressures = ["DiskPressure", "MemoryPressure"] });
        Assert.NotEqual(one, one with { Pressures = [] });
        Assert.NotEqual(one, one with { Pressures = [""] });
    }

    /// <summary>A node whose memory part is the nothing it was given says the one word the all-nothing use says:
    /// the constant that every edge-case file names instead of repeating the string twice. The test builds a node
    /// with no memory part said and checks that its words carry the constant where the memory part stands.</summary>
    [Fact]
    public void ANodeWithNoMemoryPartSaidSaysTheEmptyResourceUseConstant()
    {
        var node = ClusterFixture.Node() with { Memory = ResourceUse.None };

        Assert.Contains($"Memory = {RecordWords.EmptyResourceUse}", node.ToString(), StringComparison.Ordinal);
    }
}
