namespace Dashboard.Tests;

/// <summary>
/// The words the two cluster tables put in a node's pod cell and a pod's state tooltip, on
/// <see cref="ClusterText"/> where every other cell's words live: <see cref="ClusterText.NodePods" /> for the
/// node's pods of its capacity, and <see cref="ClusterText.PodContainers" /> for the pod's phase and containers
/// ready. Both rules were private in the markup (<c>ClusterNodes.razor</c>'s <c>Pods</c>, <c>PodTable.razor</c>'s
/// <c>Containers</c>), where no unit test could call them; they are pinned arm for arm here, and the output must
/// stay the same for every input: <c>x of y</c> in <c>ClusterText.Of</c>'s invariant digits with no separator, a
/// lone count in <c>BuildText.Count</c>'s <c>N0</c> with a separator, and the null answer without a count.
/// </summary>
public class ClusterTextNodePodsTests
{
    // ----- NodePods: the node's pods of how many it can take -----

    /// <summary>Both numbers are said against each other: <c>12 of 110</c>, invariant digits with no separator.</summary>
    [Fact]
    public void BothNumbersAreSaidAgainstEachOther()
    {
        var node = ClusterFixture.Node() with { Pods = 12, PodCapacity = 110 };
        Assert.Equal("12 of 110", ClusterText.NodePods(node));
    }

    /// <summary>The fixture node's own numbers, read through the fixture as the table reads them.</summary>
    [Fact]
    public void TheFixtureNodeSaysItsPodsOfItsCapacity()
    {
        Assert.Equal("63 of 110", ClusterText.NodePods(ClusterFixture.Node()));
    }

    /// <summary>Only the count is said, in the count's format: a lone count of 1,234 is <c>N0</c>-grouped (<c>1,234</c>).</summary>
    [Fact]
    public void ACountWithoutACapacityIsSaidAsACount()
    {
        var node = ClusterFixture.Node() with { PodCapacity = null };
        Assert.Equal("63", ClusterText.NodePods(node));

        var big = ClusterFixture.Node() with { Pods = 1234, PodCapacity = null };
        Assert.Equal("1,234", ClusterText.NodePods(big));
    }

    /// <summary>A capacity without a count is nothing to say.</summary>
    [Fact]
    public void ACapacityWithoutACountIsNothingToSay()
    {
        var node = ClusterFixture.Node() with { Pods = null };
        Assert.Null(ClusterText.NodePods(node));
    }

    /// <summary>Neither number is nothing to say.</summary>
    [Fact]
    public void NeitherNumberIsNothingToSay()
    {
        var node = ClusterFixture.Node() with { Pods = null, PodCapacity = null };
        Assert.Null(ClusterText.NodePods(node));
    }

    /// <summary>The null argument is named at the throw.</summary>
    [Fact]
    public void ANullNodeIsNamed()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.NodePods(null!));
        Assert.Equal("node", thrown.ParamName);
    }

    // ----- PodContainers: the pod's phase and containers ready -----

    /// <summary>Both counts are said after the phase: <c>Phase Running; 2 of 2 containers ready</c>.</summary>
    [Fact]
    public void BothCountsAreSaidAfterThePhase()
    {
        var pod = ClusterFixture.Pod("ui") with { Containers = 2, ContainersReady = 2 };
        Assert.Equal("Phase Running; 2 of 2 containers ready", ClusterText.PodContainers(pod));
    }

    /// <summary>Only the ready count, only the total, or neither: the phase is said on its own.</summary>
    [Fact]
    public void WithoutBothCountsThePhaseIsSaidOnItsOwn()
    {
        var onlyReady = ClusterFixture.Pod("ui") with { Containers = null, ContainersReady = 1 };
        Assert.Equal("Phase Running", ClusterText.PodContainers(onlyReady));

        var onlyAll = ClusterFixture.Pod("ui") with { Containers = 3, ContainersReady = null };
        Assert.Equal("Phase Running", ClusterText.PodContainers(onlyAll));

        Assert.Equal("Phase Running", ClusterText.PodContainers(ClusterFixture.SilentPod("bare")));
    }

    /// <summary>The phase is the pod's own word, whatever it is.</summary>
    [Fact]
    public void ThePhaseIsThePodsOwnWord()
    {
        var pod = ClusterFixture.Pod("job", ClusterPod.Succeeded, ready: false) with { Containers = 1, ContainersReady = 0 };
        Assert.Equal("Phase Succeeded; 0 of 1 containers ready", ClusterText.PodContainers(pod));
    }

    /// <summary>The null argument is named at the throw.</summary>
    [Fact]
    public void ANullPodIsNamed()
    {
        var thrown = Assert.Throws<ArgumentNullException>(() => ClusterText.PodContainers(null!));
        Assert.Equal("pod", thrown.ParamName);
    }
}
