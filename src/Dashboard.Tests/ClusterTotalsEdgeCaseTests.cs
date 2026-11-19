namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterTotals" /> is the cluster card's numbers: the nodes ready of the nodes, the pods ready of the
/// pods that count, the finished jobs counted on their own, the sums of the nodes' resources, the pods the nodes run
/// of how many they can take, and the restarts over every pod. <see cref="ClusterGroupsTests" /> pins them at the
/// sample status and at two nodes between which one leaves out what the other says, and
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> pins them at one node with one ready pod and one
/// finished job; the same sums at the level of one namespace stand in <see cref="NamespaceGroupTests" />.
/// <para>
/// What is pinned here is the edge each of those sits next to, and only that: the status that is not there, named;
/// the status that lists no node and no namespace; the cluster without nodes whose pods are still counted, the empty
/// namespace among them adding nothing; the nodes that are not ready; the nodes that each report a different one of
/// the four numbers of a resource, the field no node reports and the sum that comes out of none; the usage that is
/// zero, which is a sum of zero and not no sum at all; the restart count at the two ends of the count; the pod whose
/// state turns between starting and unhealthy between the two moments it is read at, which the totals do not tell
/// apart; and the record itself - the parts it keeps and their order, the words it says them in, the two totals that
/// are one while the parts are, the falling apart and the copy, and the totals that are not there.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every status is one the test builds, and every
/// moment a total is read at is one the test brings.
/// </para>
/// </summary>
public class ClusterTotalsEdgeCaseTests
{
    private static readonly DateTimeOffset AsOf = ClusterFixture.Generated;

    private static readonly ResourceUse Cpu = new(812, 2450, null, 3860);
    private static readonly ResourceUse Memory = new(null, 268435456, null, 13400000000);

    // ----- the status the totals are said about

    /// <summary>The totals are said about a status: without one it is said so, naming the argument - the moment the
    /// facts are of is never refused, it is a value of the caller's.</summary>
    [Fact]
    public void TheTotalsAreSaidAboutAStatus()
    {
        var error = Assert.Throws<ArgumentNullException>(() => ClusterTotals.Of(null!, AsOf));

        Assert.Equal("status", error.ParamName);
    }

    /// <summary>A status that lists no node and no namespace says no number at all: every count is the zero that
    /// counts nothing, every sum is the sum that no node reported, and the pods the nodes run and the pods the nodes
    /// can take are not said - there is no node that says them.</summary>
    [Fact]
    public void AStatusThatListsNothingLeavesNoTotals()
    {
        var totals = ClusterTotals.Of(ClusterFixture.Status([]), AsOf);

        Assert.Equal((0, 0, 0, 0, 0), (totals.NodesReady, totals.Nodes, totals.PodsReady, totals.Pods, totals.PodsFinished));
        Assert.Equal(0, totals.Restarts);
        Assert.Equal(ResourceUse.None, totals.Cpu);
        Assert.Equal(ResourceUse.None, totals.Memory);
        Assert.Null(totals.PodsOnNodes);
        Assert.Null(totals.PodCapacity);
    }

    /// <summary>A cluster without a node is not a cluster without pods: the pods are counted, the finished ones on
    /// their own and the restarts added, while every sum of the nodes is the sum no node reported - the namespace
    /// that lists no pod adds nothing to any of it.</summary>
    [Fact]
    public void AClusterWithoutNodesStillCountsItsPods()
    {
        var status = ClusterFixture.Status(
            [],
            ClusterFixture.Space("empty"),
            ClusterFixture.Space(
                "apps",
                ClusterFixture.Pod("ui"),
                ClusterFixture.Pod("backup", ClusterPod.Succeeded, false, restarts: 3, reason: "Completed")));

        var totals = ClusterTotals.Of(status, AsOf);

        Assert.Equal((0, 0), (totals.NodesReady, totals.Nodes));
        Assert.Equal((1, 1, 1), (totals.PodsReady, totals.Pods, totals.PodsFinished));
        Assert.Equal(3, totals.Restarts);
        Assert.Equal(ResourceUse.None, totals.Cpu);
        Assert.Equal(ResourceUse.None, totals.Memory);
        Assert.Null(totals.PodsOnNodes);
        Assert.Null(totals.PodCapacity);
    }

    /// <summary>A node that is not ready is still a node: it counts in the nodes and not in the nodes ready - the
    /// whole cluster not ready, and its pods and its pod capacity said on, since readiness is the node's own word and
    /// no part of the count of its pods.</summary>
    [Fact]
    public void NodesThatAreNotReadyCountAsNodesAndNotAsReady()
    {
        var status = ClusterFixture.Status(
            [
                ClusterFixture.Node("aks-a-000001", false),
                ClusterFixture.Node("aks-a-000002", false),
            ],
            ClusterFixture.Space("apps", ClusterFixture.Pod("ui")));

        var totals = ClusterTotals.Of(status, AsOf);

        Assert.Equal((0, 2), (totals.NodesReady, totals.Nodes));
        Assert.Equal((1, 1, 0), (totals.PodsReady, totals.Pods, totals.PodsFinished));
        Assert.Equal(126, totals.PodsOnNodes);
        Assert.Equal(220, totals.PodCapacity);
    }

    /// <summary>The nodes' resources are summed field by field: the usage of one node and the usage of the other
    /// make one usage, the requests of one node stand alone while the other says none, and a field neither node
    /// reports is not in the sum at all - no sum of none, not a sum of zero. What a node does not say about its pods
    /// is not said by the totals either: a capacity said by no node is a capacity that is not said.</summary>
    [Fact]
    public void ANodeSaysOnlyWhatItSaysAndTheFieldNoNodeSaysIsNotSaid()
    {
        var status = ClusterFixture.Status(
            [
                ClusterFixture.Node("aks-a-000001") with { Cpu = new ResourceUse(0.5, 500, null, null), Memory = new ResourceUse(null, null, null, 4096), Pods = null, PodCapacity = null },
                ClusterFixture.Node("aks-a-000002") with { Cpu = new ResourceUse(0.25, null, 100, null), Memory = ResourceUse.None, Pods = null, PodCapacity = null },
            ]);

        var totals = ClusterTotals.Of(status, AsOf);

        Assert.Equal(new ResourceUse(0.75, 500, 100, null), totals.Cpu);
        Assert.Null(totals.Memory.Usage);
        Assert.Null(totals.Memory.Requests);
        Assert.Null(totals.Memory.Limits);
        Assert.Equal(4096d, totals.Memory.Allocatable);
        Assert.Null(totals.PodsOnNodes);
        Assert.Null(totals.PodCapacity);
    }

    /// <summary>A usage that is zero is a sum of zero and not no sum at all: the node that says it uses nothing
    /// says a number, and the totals say the number it says, with the fields it does not say left unsaid.</summary>
    [Fact]
    public void AUsageThatIsZeroIsASumOfZeroNotNoSumAtAll()
    {
        var node = ClusterFixture.Node() with { Cpu = new ResourceUse(0, null, null, null), Memory = new ResourceUse(0, null, null, null) };
        var totals = ClusterTotals.Of(ClusterFixture.Status([node]), AsOf);

        Assert.Equal(0d, totals.Cpu.Usage);
        Assert.Equal(0d, totals.Memory.Usage);
        Assert.Null(totals.Cpu.Requests);
        Assert.Null(totals.Cpu.Limits);
        Assert.Null(totals.Cpu.Allocatable);
        Assert.Null(totals.Memory.Requests);
        Assert.Null(totals.Memory.Limits);
        Assert.Null(totals.Memory.Allocatable);
    }

    /// <summary>The restarts over every pod stand in the totals at the two ends of the count the restart count
    /// stands in: one pod that says the top of the count is the top of the count in the totals, and the bottom the
    /// bottom, with nothing added to either.</summary>
    [Fact]
    public void TheRestartsStandInTheTotalsAtTheEndsOfTheCount()
    {
        var top = ClusterTotals.Of(ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui", restarts: int.MaxValue))), AsOf);
        var bottom = ClusterTotals.Of(ClusterFixture.Status(null, ClusterFixture.Space("apps", ClusterFixture.Pod("ui", restarts: int.MinValue))), AsOf);

        Assert.Equal(int.MaxValue, top.Restarts);
        Assert.Equal(int.MinValue, bottom.Restarts);
    }

    /// <summary>The moment the totals are read at is the moment the pods' ages are counted to - and yet the state a
    /// pod turns on between the two moments it is read at is no part of the totals' counts: the pod that is starting
    /// at the first and unhealthy at the second is a pod that is not ready and not finished at both, so the ready,
    /// the counting and the finished are the same at both. The flip is the rule's, not the totals': both states are
    /// said by the rule itself.</summary>
    [Fact]
    public void TheTotalsDoNotTellApartTheStartingPodFromTheUnhealthyOne()
    {
        var pod = ClusterFixture.Pod("ui", ready: false, age: TimeSpan.FromSeconds(90));
        var started = pod.StartedAt!.Value;
        var status = ClusterFixture.Status(null, ClusterFixture.Space("apps", pod));

        Assert.Equal(PodState.Starting, PodRules.StateOf(pod, AsOf));
        Assert.Equal(PodState.Unhealthy, PodRules.StateOf(pod, started + PodRules.StartingGrace));

        var starting = ClusterTotals.Of(status, AsOf);
        var unhealthy = ClusterTotals.Of(status, started + PodRules.StartingGrace);

        Assert.Equal((0, 1, 0), (starting.PodsReady, starting.Pods, starting.PodsFinished));
        Assert.Equal((0, 1, 0), (unhealthy.PodsReady, unhealthy.Pods, unhealthy.PodsFinished));
    }

    // ----- the record itself

    /// <summary>The totals keep the very parts they are given, each in the place it is given in, and say them in the
    /// order the record declares them: the counts first, the two resources inside them, the pods the nodes run and
    /// can take left unsaid where they are not said, and the restarts last - a total built at the ends of the count
    /// says the ends.</summary>
    [Fact]
    public void TheTotalsKeepThePartsTheyAreGivenAndSayThemInTheirOrder()
    {
        var totals = new ClusterTotals(1, 2, 0, 1, 1, Cpu, Memory, null, 110, int.MaxValue);

        Assert.Equal(1, totals.NodesReady);
        Assert.Equal(2, totals.Nodes);
        Assert.Equal(0, totals.PodsReady);
        Assert.Equal(1, totals.Pods);
        Assert.Equal(1, totals.PodsFinished);
        Assert.Equal(Cpu, totals.Cpu);
        Assert.Equal(Memory, totals.Memory);
        Assert.Null(totals.PodsOnNodes);
        Assert.Equal(110, totals.PodCapacity);
        Assert.Equal(int.MaxValue, totals.Restarts);

        Assert.Equal(
            "ClusterTotals { NodesReady = 1, Nodes = 2, PodsReady = 0, Pods = 1, PodsFinished = 1, " +
                "Cpu = ResourceUse { Usage = 812, Requests = 2450, Limits = , Allocatable = 3860 }, " +
                "Memory = ResourceUse { Usage = , Requests = 268435456, Limits = , Allocatable = 13400000000 }, " +
                "PodsOnNodes = , PodCapacity = 110, Restarts = 2147483647 }",
            totals.ToString());
    }

    /// <summary>Two totals are one while their ten parts are - the counts' own equality and the resources', each a
    /// record's - and a difference in any one of them, a count turned, a number of a resource changed or a field that
    /// was said left unsaid, is another one. The hash code follows the equality, and the totals a status comes out
    /// of are the totals it comes out of again; neither no totals nor a word is a total's equal.</summary>
    [Fact]
    public void TwoTotalsAreOneWhileTheirPartsAre()
    {
        var one = new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, 63, 110, 7);
        var two = new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, 63, 110, 7);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ClusterTotals"));

        Assert.NotEqual(one, new ClusterTotals(2, 2, 3, 4, 5, Cpu, Memory, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 3, 3, 4, 5, Cpu, Memory, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 4, 4, 5, Cpu, Memory, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 5, 5, Cpu, Memory, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 6, Cpu, Memory, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 5, Cpu with { Usage = 813 }, Memory, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory with { Requests = null }, 63, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, 64, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, null, 110, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, 63, 111, 7));
        Assert.NotEqual(one, new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, 63, 110, 8));

        var status = ClusterFixture.Status();
        Assert.Equal(ClusterTotals.Of(status, AsOf), ClusterTotals.Of(status, AsOf));
    }

    /// <summary>The totals fall apart into the ten parts they hold - the same resources, the same references - and a
    /// copy of the totals is the totals again: the <c>with</c> that names no part and the <c>with</c> that names the
    /// part it turns keep the rest, and not the same reference.</summary>
    [Fact]
    public void TheTotalsFallApartIntoTheirPartsAndACopyIsTheTotalsAgain()
    {
        var totals = new ClusterTotals(1, 2, 3, 4, 5, Cpu, Memory, 63, 110, 7);

        var (nodesReady, nodes, podsReady, pods, podsFinished, cpu, memory, podsOnNodes, podCapacity, restarts) = totals;
        Assert.Equal((1, 2, 3, 4, 5, 63, 110, 7), (nodesReady, nodes, podsReady, pods, podsFinished, podsOnNodes, podCapacity, restarts));
        Assert.Same(Cpu, cpu);
        Assert.Same(Memory, memory);

        var copy = totals with { };
        Assert.Equal(totals, copy);
        Assert.NotSame(totals, copy);
        Assert.Same(totals.Cpu, copy.Cpu);

        var turned = totals with { Restarts = 0 };
        Assert.Equal(0, turned.Restarts);
        Assert.Equal((1, 2, 3, 4, 5, 63, 110), (turned.NodesReady, turned.Nodes, turned.PodsReady, turned.Pods, turned.PodsFinished, turned.PodsOnNodes, turned.PodCapacity));
        Assert.Same(totals.Cpu, turned.Cpu);
        Assert.NotEqual(totals, turned);

        var repack = totals with { NodesReady = totals.NodesReady, Restarts = totals.Restarts };
        Assert.Equal(totals, repack);
        Assert.NotSame(totals, repack);
    }

    /// <summary>Totals that are not there are no totals: the record's default is no totals, and totals are never
    /// equal to no totals however the question is asked - the operators, and <c>Equals</c> asked about the no
    /// totals - while the totals that are there are totals to both of them.</summary>
    [Fact]
    public void TotalsThatAreNotThereAreNoTotals()
    {
        Assert.Null(default(ClusterTotals));

        ClusterTotals? none = null;
        var there = new ClusterTotals(0, 0, 0, 0, 0, ResourceUse.None, ResourceUse.None, null, null, 0);

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);
    }
}
