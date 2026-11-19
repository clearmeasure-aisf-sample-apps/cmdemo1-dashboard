namespace Dashboard.Tests;

/// <summary>
/// <see cref="ClusterSample" /> is what the page keeps of one reading of the cluster for its trends: the CPU and
/// the memory the nodes use, or the nothing where no node says a usage. <see cref="ClusterGroupsEdgeCaseTests" />
/// pins the parts the record keeps and the nothing that is not the zero it is not, the two samples that are one
/// while their two numbers are, the words it says its parts in, the status that is not there, and the reading
/// without nodes; <see cref="ClusterGroupsTests" /> pins the sums at two nodes between which one leaves out what
/// the other says and the node that reports no usage at all, and <see cref="ClusterMonitorTests" /> pins the
/// sample the trends are drawn from at the sample status.
/// <para>
/// What is pinned here is the edge each of those sits next to, and only that: the numbers at the two ends of the
/// number and the zero that is a number, the falling apart into the two parts and the copy that is the sample
/// again, the node that says one of the two usages and leaves the other unsaid and the two nodes that split the
/// two usages between them, the usage that is zero and is a sum of zero and not no sum at all, the two nodes
/// whose usages make the sums they say, the node that says the ends of the number, the two nodes whose sum
/// overflows the end the number has, and the namespaces that are no part of the sample at all.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every status is one the test builds, and the
/// sample is said about the status alone, at no moment at all.
/// </para>
/// </summary>
public class ClusterSampleEdgeCaseTests
{
    // ----- the record itself

    /// <summary>The sample keeps the very numbers it is given, at the two ends of the number the number stands
    /// in and at the zero that is a number: the top and the bottom stand where they are given, and the zero that
    /// is a number is kept as the number it is, not the nothing it is not.</summary>
    [Fact]
    public void TheSampleKeepsTheEndsOfTheNumberAndTheZeroItIsGiven()
    {
        var topAndBottom = new ClusterSample(double.MaxValue, double.MinValue);
        Assert.Equal(double.MaxValue, topAndBottom.CpuMillicores);
        Assert.Equal(double.MinValue, topAndBottom.MemoryBytes);

        var zero = new ClusterSample(0, 0);
        Assert.Equal(0d, zero.CpuMillicores);
        Assert.Equal(0d, zero.MemoryBytes);
    }

    /// <summary>The sample falls apart into its two parts - the numbers or the nothing, each as the value it
    /// stands on - and a copy of it is it again: the <c>with</c> that names no part keeps both, and the
    /// <c>with</c> that turns one of them keeps the other it does not name, and not the same reference.</summary>
    [Fact]
    public void TheSampleFallsApartIntoItsPartsAndACopyIsTheSampleAgain()
    {
        var sample = new ClusterSample(812.5, 7046430720d);

        var (cpu, memory) = sample;
        Assert.Equal(812.5, cpu);
        Assert.Equal(7046430720d, memory);

        var (noCpu, noMemory) = new ClusterSample(null, null);
        Assert.Null(noCpu);
        Assert.Null(noMemory);

        var copy = sample with { };
        Assert.Equal(sample, copy);
        Assert.NotSame(sample, copy);

        var withoutCpu = sample with { CpuMillicores = null };
        Assert.Null(withoutCpu.CpuMillicores);
        Assert.Equal(7046430720d, withoutCpu.MemoryBytes);
        Assert.NotEqual(sample, withoutCpu);

        var withoutMemory = sample with { MemoryBytes = null };
        Assert.Equal(812.5, withoutMemory.CpuMillicores);
        Assert.Null(withoutMemory.MemoryBytes);
        Assert.NotEqual(sample, withoutMemory);
    }

    // ----- the ends Of says about

    /// <summary>The sample is said of the nodes' usages alone, and each of the two is said on its own: the node
    /// that says a cpu usage and no memory usage makes a sample that says the cpu and leaves the memory unsaid,
    /// the node that says it the other way round the other way about, and the two nodes that split the two
    /// usages between them make the one sample each says its half of.</summary>
    [Fact]
    public void ANodeThatSaysOneOfItsTwoUsagesLeavesTheOtherUnsaid()
    {
        var cpuOnly = ClusterFixture.Node("aks-a-000001") with
        { Cpu = new ResourceUse(500, 2450, 9100, 3860), Memory = ResourceUse.None };
        var memoryOnly = ClusterFixture.Node("aks-a-000002") with
        { Cpu = ResourceUse.None, Memory = new ResourceUse(7046430720, 6200000000, 11800000000, 13400000000) };

        Assert.Equal(new ClusterSample(500, null), ClusterSample.Of(ClusterFixture.Status([cpuOnly])));
        Assert.Equal(new ClusterSample(null, 7046430720d), ClusterSample.Of(ClusterFixture.Status([memoryOnly])));
        Assert.Equal(new ClusterSample(500, 7046430720d), ClusterSample.Of(ClusterFixture.Status([cpuOnly, memoryOnly])));
    }

    /// <summary>A usage that is zero is a sum of zero and not no sum at all: the node that says it uses nothing
    /// says a number, and the sample says the two zeros it says - the nothing is only where the node says no
    /// usage at all.</summary>
    [Fact]
    public void AUsageThatIsZeroIsASumOfZeroNotNoSumAtAll()
    {
        var node = ClusterFixture.Node() with
        { Cpu = new ResourceUse(0, null, null, null), Memory = new ResourceUse(0, null, null, null) };

        var sample = ClusterSample.Of(ClusterFixture.Status([node]));

        Assert.Equal(0d, sample.CpuMillicores);
        Assert.Equal(0d, sample.MemoryBytes);
        Assert.NotEqual(new ClusterSample(null, null), sample);
    }

    /// <summary>The two nodes that each say their two usages make the sums they say: the cpu of the one and the
    /// cpu of the other make the cpu the sample says, the memory the same - the fixed values the two fixed nodes
    /// stand on.</summary>
    [Fact]
    public void TwoNodesThatEachSayTheirUsagesMakeTheSumsTheySay()
    {
        var status = ClusterFixture.Status(
        [
            ClusterFixture.Node("aks-a-000001"),
            ClusterFixture.Node("aks-a-000002"),
        ]);

        Assert.Equal(new ClusterSample(1624, 2 * 9876543210d), ClusterSample.Of(status));
    }

    /// <summary>The node that says the ends of the number makes the sample that says them: the top of the number
    /// the top, the bottom the bottom, and nothing added to either.</summary>
    [Fact]
    public void ANodeThatSaysTheEndsOfTheNumberMakesASampleThatSaysThem()
    {
        var node = ClusterFixture.Node() with
        { Cpu = new ResourceUse(double.MaxValue, null, null, null), Memory = new ResourceUse(double.MinValue, null, null, null) };

        var sample = ClusterSample.Of(ClusterFixture.Status([node]));

        Assert.Equal(double.MaxValue, sample.CpuMillicores);
        Assert.Equal(double.MinValue, sample.MemoryBytes);
    }

    /// <summary>The two nodes that each say the end of the number make the sum the end overflows to: the two
    /// tops make the top the number has no end to, the two bottoms the bottom it has no end to, and the top and
    /// the bottom evened out make the zero they even to - the usage neither node says stays unsaid through all
    /// of it.</summary>
    [Fact]
    public void TwoNodesThatEachSayTheEndMakeTheSumTheEndOverflowsTo()
    {
        var top = ClusterFixture.Node("aks-a-000001") with { Cpu = new ResourceUse(double.MaxValue, null, null, null), Memory = ResourceUse.None };
        var bottom = ClusterFixture.Node("aks-a-000002") with { Cpu = new ResourceUse(double.MinValue, null, null, null), Memory = ResourceUse.None };

        var twoTops = ClusterSample.Of(ClusterFixture.Status([top, top]));
        Assert.Equal(double.PositiveInfinity, twoTops.CpuMillicores);
        Assert.Null(twoTops.MemoryBytes);

        var twoBottoms = ClusterSample.Of(ClusterFixture.Status([bottom, bottom]));
        Assert.Equal(double.NegativeInfinity, twoBottoms.CpuMillicores);
        Assert.Null(twoBottoms.MemoryBytes);

        var evened = ClusterSample.Of(ClusterFixture.Status([top, bottom]));
        Assert.Equal(0d, evened.CpuMillicores);
        Assert.Null(evened.MemoryBytes);
    }

    /// <summary>The namespaces of a status without nodes are no part of the sample: the pods and the volumes the
    /// namespaces list say nothing the trends are drawn from, and the sample the reading stands on is the one
    /// without a number.</summary>
    [Fact]
    public void TheNamespacesOfAStatusWithoutNodesAreNoPartOfTheSample()
    {
        var status = ClusterFixture.Status(
            [],
            ClusterFixture.Space("apps", ClusterFixture.Pod("ui")),
            ClusterFixture.Space("jobs", ClusterFixture.Pod("backup", ClusterPod.Succeeded, false, reason: "Completed")));

        Assert.Equal(new ClusterSample(null, null), ClusterSample.Of(status));
    }
}
